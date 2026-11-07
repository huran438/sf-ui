using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using SFramework.Configs.Runtime;
using SFramework.Core.Runtime;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.EventSystems;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace SFramework.UI.Runtime
{
    public sealed partial class SFUIService : ISFUIService
    {
        public event Action<string> OnUnloadScreen = _ => { };
        public event Action<string> OnScreenUnloaded = _ => { };
        public event Action<string, bool, object[]> OnShowScreen = (_, _, _) => { };
        public event Action<string, bool, bool> OnCloseScreen = (_, _, _) => { };
        public event Action<string> OnScreenShown = _ => { };
        public event Action<string> OnScreenClosed = _ => { };
        public event Action<string, SFBaseEventType, BaseEventData> OnWidgetBaseEvent = (_, _, _) => { };

        public event Action<string, int, SFPointerEventType, PointerEventData> OnWidgetPointerEvent = (_, _, _, _) =>
        {
        };

        public SFScreenModel[] ScreenModels => _screenModels.Values.ToArray();
        public SFWidgetModel[] WidgetModels => _widgetModelById.Values.ToArray();

        private readonly Dictionary<string, SFScreenModel> _screenModels = new();
        private readonly Dictionary<string, SFWidgetModel> _widgetModelById = new();
        private readonly Dictionary<string, ScreenLoad> _screenLoads = new();
        private readonly Dictionary<string, SFScreenView> _screenViews = new();
        private readonly Dictionary<string, SFScreenNode> _screenNodes = new();
        private readonly Dictionary<string, SFWidgetNode> _widgetNodes = new();
        private readonly Dictionary<string, List<SFWidgetView>> _widgetViews = new();

        private Transform _parentTransform;
        private Transform _loadingTransform;
        private readonly ISFConfigsService _configsService;
        private int _pendingLoads;
        private bool _disposed;


        SFUIService(ISFConfigsService configsService)
        {
            _parentTransform = new GameObject("SFUI").transform;
            _configsService = configsService;
        }

        private Transform GetLoadingRoot()
        {
            if (_loadingTransform != null) return _loadingTransform;
            var root = new GameObject("SFUI Loading Screens");
            root.SetActive(false);
            // Addressables can finish after a Single scene change. Keep late prefabs inactive.
            if (Application.isPlaying) UnityEngine.Object.DontDestroyOnLoad(root);
            return _loadingTransform = root.transform;
        }

        public async UniTask Init(CancellationToken cancellationToken)
        {
            if (_disposed) throw new ObjectDisposedException(nameof(SFUIService));
            cancellationToken.ThrowIfCancellationRequested();
            if (_configsService.TryGetConfigs(out SFUIConfig[] configs))
            {
                foreach (var repository in configs)
                {
                    if (repository?.Groups == null) continue;

                    foreach (var groupNode in repository.Groups)
                    {
                        if (groupNode?.Screens == null) continue;

                        foreach (var screenNode in groupNode.Screens)
                        {
                            if (screenNode == null || !_screenNodes.TryAdd(screenNode.FullId, screenNode)) continue;
                            _screenModels.Add(screenNode.FullId, new SFScreenModel(screenNode));
                            foreach (var widgetNode in screenNode.Widgets ?? Array.Empty<SFWidgetNode>())
                            {
                                if (widgetNode == null) continue;
                                _widgetNodes.TryAdd(widgetNode.FullId, widgetNode);
                                _widgetModelById.TryAdd(widgetNode.FullId, new SFWidgetModel(widgetNode));
                            }
                        }
                    }
                }
            }

            var preloadTasks = new List<UniTask>();
            foreach (var screen in _screenNodes.Values)
                if (screen.Preload)
                    preloadTasks.Add(LoadScreen(screen.FullId, cancellationToken: cancellationToken));
            await UniTask.WhenAll(preloadTasks).AttachExternalCancellation(cancellationToken);
        }


        public async UniTask LoadScreen(string screen, bool show = false, bool force = false,
            IProgress<float> progress = null,
            CancellationToken cancellationToken = default, params object[] parameters)
        {
            if (_disposed) throw new ObjectDisposedException(nameof(SFUIService));
            if (string.IsNullOrEmpty(screen))
            {
                SFDebug.Log(LogType.Warning, "[SFUI] - Unable to load screen. Id is empty!");
                return;
            }

            if (!_screenNodes.TryGetValue(screen, out var screenNode))
            {
                throw new KeyNotFoundException($"Unknown screen '{screen}'.");
            }

            if (show)
            {
                await ShowScreen(screen, force, progress, cancellationToken, parameters);
                return;
            }

            await EnsureLoaded(screen, screenNode, progress, cancellationToken);
        }

        private async UniTask EnsureLoaded(string screen, SFScreenNode node, IProgress<float> progress,
            CancellationToken cancellationToken)
        {
            if (_disposed) throw new ObjectDisposedException(nameof(SFUIService));
            cancellationToken.ThrowIfCancellationRequested();

            if (_screenLoads.TryGetValue(screen, out var load) && !load.Loading && load.View == null)
            {
                RemoveLoad(screen, load);
                load = null;
            }

            if (load?.Unloading == true) throw new OperationCanceledException();

            if (load == null)
            {
                if (TryGetScreenView(screen, out var view) && view != null) return;
                load = new ScreenLoad();
                _screenLoads.Add(screen, load);
                InstantiateScreen(screen, node.Prefab, load).Forget();
            }

            load.Waiters++;
            try
            {
                if (progress != null)
                {
                    while (load.Loading && OwnsLoad(screen, load))
                    {
                        if (load.Handle.IsValid()) progress.Report(load.Handle.PercentComplete);
                        await UniTask.Yield(cancellationToken);
                    }
                }

                await load.Completion.Task.AttachExternalCancellation(cancellationToken);
                cancellationToken.ThrowIfCancellationRequested();
                if (!OwnsLoad(screen, load)) throw new OperationCanceledException();
                progress?.Report(1f);
            }
            finally
            {
                if (--load.Waiters == 0 && load.Loading && OwnsLoad(screen, load))
                    RemoveLoad(screen, load);
            }
        }

        private async UniTask InstantiateScreen(string screen, string key, ScreenLoad load)
        {
            _pendingLoads++;
            try
            {
                if (_parentTransform == null) _parentTransform = new GameObject("SFUI").transform;
                // A canceled load can finish later. Keep its prefab inactive until it still owns the slot.
                load.Handle = Addressables.InstantiateAsync(key, GetLoadingRoot());
                while (!load.Handle.IsDone) await UniTask.Yield();
                if (!OwnsLoad(screen, load)) return;
                if (load.Handle.Status != AsyncOperationStatus.Succeeded || load.Handle.Result == null)
                    throw new InvalidOperationException($"Failed to load screen '{screen}' from '{key}'.",
                        load.Handle.OperationException);

                var instance = load.Handle.Result;
                load.View = instance.GetComponentInChildren<SFScreenView>(true);
                if (load.View == null || load.View.Screen != screen)
                    throw new InvalidOperationException($"Prefab '{key}' must contain a screen view with ID '{screen}'.");
                if (_parentTransform == null)
                    throw new InvalidOperationException($"The UI root was destroyed while loading screen '{screen}'.");

                instance.transform.SetParent(_parentTransform, false);
                if (!OwnsLoad(screen, load)) return;
                if (!TryGetScreenView(screen, out var registered) || registered != load.View)
                    throw new InvalidOperationException($"Screen '{screen}' did not register its active view.");

                load.Loading = false;
                load.Completion.TrySetResult();
            }
            catch (Exception error)
            {
                if (OwnsLoad(screen, load))
                {
                    RemoveLoad(screen, load, error);
                }
            }
            finally
            {
                load.Loading = false;
                if (!OwnsLoad(screen, load)) ReleaseHandle(load);
                if (--_pendingLoads == 0 && _disposed) DestroyRoot(_loadingTransform);
            }
        }

        private bool OwnsLoad(string screen, ScreenLoad load) =>
            _screenLoads.TryGetValue(screen, out var current) && current == load;

        private void RemoveLoad(string screen, ScreenLoad load, Exception error = null)
        {
            _screenLoads.Remove(screen);
            if (load.View != null) load.View.DetachFromService();
            if (TryGetScreenView(screen, out var view) && ReferenceEquals(view, load.View))
                UnregisterScreen(screen);

            if (!load.Loading) ReleaseHandle(load);
            if (error == null) load.Completion.TrySetCanceled();
            else load.Completion.TrySetException(error);
        }

        private static void ReleaseHandle(ScreenLoad load)
        {
            var handle = load.Handle;
            load.Handle = default;
            if (handle.IsValid()) Addressables.Release(handle);
        }

        public void UnloadScreen(string screen)
        {
            if (string.IsNullOrEmpty(screen))
                throw new ArgumentNullException(nameof(screen));

            if (!_screenLoads.TryGetValue(screen, out var load))
                throw new Exception("Screen not loaded: " + screen);
            if (load.Unloading) return;
            load.Unloading = true;

            var model = _screenModels[screen];
            model.ShowRevision++;
            try
            {
                if (model.State != SFScreenState.Closed)
                {
                    model.State = SFScreenState.Closed;
                    OnScreenClosed.Invoke(screen);
                }

                OnUnloadScreen.Invoke(screen);
            }
            finally
            {
                if (OwnsLoad(screen, load)) RemoveLoad(screen, load);
            }

            OnScreenUnloaded.Invoke(screen);
        }

        public UniTask ShowScreen(string screen, params object[] parameters)
        {
            return ShowScreen(screen, false, null, CancellationToken.None, parameters);
        }

        public UniTask ShowScreen(string screen, bool force, params object[] parameters)
        {
            return ShowScreen(screen, force, null, CancellationToken.None, parameters);
        }

        public async UniTask ShowScreen(string screen, bool force, IProgress<float> progress = null,
            CancellationToken cancellationToken = default,
            params object[] parameters)
        {
            if (string.IsNullOrEmpty(screen))
            {
                SFDebug.Log(LogType.Warning, "[SFUI] - Unable to show screen. Id is empty!");
                return;
            }

            if (!_screenModels.TryGetValue(screen, out var screenModel)) return;
            if (screenModel.State == SFScreenState.Show || screenModel.State == SFScreenState.Shown) return;
            var revision = screenModel.ShowRevision;
            await EnsureLoaded(screen, screenModel.Node, progress, cancellationToken);
            if (_disposed || revision != screenModel.ShowRevision ||
                screenModel.State == SFScreenState.Show || screenModel.State == SFScreenState.Shown) return;

            screenModel.State = SFScreenState.Show;
            try
            {
                OnShowScreen.Invoke(screen, force, parameters);
            }
            catch
            {
                if (screenModel.State == SFScreenState.Show) screenModel.State = SFScreenState.Closed;
                throw;
            }
        }

        public void SetParameters(string screen, params object[] parameters)
        {
            if (parameters == null)
            {
                SFDebug.Log("Parameters NULL");
                return;
            }

            if (_screenViews.TryGetValue(screen, out var view))
            {
                view.SetParameters(parameters);
            }
            else
            {
                SFDebug.Log("Cant find view");
            }
        }

        public void CloseScreen(string screen, bool force, bool unload)
        {
            if (string.IsNullOrEmpty(screen))
            {
                SFDebug.Log(LogType.Warning, "[SFUI] - Unable to close screen. Id is empty!");
                return;
            }

            if (!_screenModels.TryGetValue(screen, out var screenModel)) return;
            screenModel.ShowRevision++;
            if (screenModel.State == SFScreenState.Closed)
            {
                if (unload && _screenLoads.TryGetValue(screen, out var load) && load.Loading) UnloadScreen(screen);
                return;
            }
            if (screenModel.State == SFScreenState.Close) return;
            screenModel.State = SFScreenState.Close;
            OnCloseScreen.Invoke(screen, force, unload);
        }

        public bool TryGetScreenView(string screen, out SFScreenView screenView)
        {
            return _screenViews.TryGetValue(screen, out screenView);
        }

        public bool TryGetScreenView<T>(string screen, out T screenView) where T : SFScreenView
        {
            if (_screenViews.TryGetValue(screen, out var view))
            {
                screenView = view as T;
                return screenView != null;
            }

            screenView = null;
            return false;
        }

        public bool TryGetScreenModel(string screen, out SFScreenModel screenModel)
        {
            return _screenModels.TryGetValue(screen, out screenModel);
        }

        public void RegisterScreen(string screen, SFScreenView root)
        {
            if (_disposed) return;
            if (string.IsNullOrEmpty(screen))
            {
                SFDebug.Log(LogType.Warning, "[SFUI] - Unable to register screen. Id is empty!");
                return;
            }

            if (root == null || !TryGetScreenModel(screen, out var model)) return;
            if (_screenViews.TryGetValue(screen, out var existing) && existing != null) return;

            model.State = SFScreenState.Closed;
            _screenViews[screen] = root;
            model.IsLoaded = true;
        }

        public void RegisterWidget(string widget, SFWidgetView widgetView)
        {
            if (_disposed) return;
            if (string.IsNullOrEmpty(widget))
            {
                SFDebug.Log(LogType.Warning, "[SFUI] - Unable to register widget. Id is empty!");
                return;
            }

            if (_widgetViews.TryGetValue(widget, out var widgetViews))
            {
                widgetViews.Add(widgetView);
            }
            else
            {
                var initialWidgetViews = new List<SFWidgetView>() { widgetView };
                if (!_widgetViews.TryAdd(widget, initialWidgetViews))
                {
                    SFDebug.Log(LogType.Error, "Can't add WidgetView to widget {0}", widget);
                }
            }

            if (TryGetWidgetModel(widget, out var model))
            {
                model.RegisterView(widgetView);
            }
        }

        public void UnregisterWidget(string widget, SFWidgetView widgetView)
        {
            if (_disposed) return;
            if (string.IsNullOrEmpty(widget))
            {
                SFDebug.Log(LogType.Warning, "[SFUI] - Unable to unregister widget. Id is empty!");
                return;
            }

            if (_widgetViews.TryGetValue(widget, out var views))
            {
                views.Remove(widgetView);
                if (views.Count == 0)
                {
                    _widgetViews.Remove(widget);
                }
            }

            if (TryGetWidgetModel(widget, out var model))
            {
                model.UnregisterView(widgetView);
            }
        }

        public bool TryGetWidgetView(string widget, int index, out SFWidgetView view)
        {
            view = null;
            if (string.IsNullOrEmpty(widget)) return false;
            if (!_widgetViews.TryGetValue(widget, out var widgets)) return false;
            if (widgets.Count == 0) return false;
            index = Mathf.Clamp(index, 0, widgets.Count - 1);
            view = widgets[index];
            return true;
        }

        public bool TryGetWidgetView<T>(string widget, int index, out T view) where T : SFWidgetView
        {
            if (TryGetWidgetView(widget, index, out var widgetView))
            {
                view = widgetView as T;
                return view != null;
            }

            view = null;
            return false;
        }

        public bool TryGetWidgetNode(string widget, out SFWidgetNode widgetNode)
        {
            return _widgetNodes.TryGetValue(widget, out widgetNode);
        }

        public bool TryGetWidgetModel(string widget, out SFWidgetModel widgetModel)
        {
            if (string.IsNullOrWhiteSpace(widget))
            {
                widgetModel = null;
                return false;
            }

            if (_widgetModelById.TryGetValue(widget, out widgetModel)) return true;
            SFDebug.Log(LogType.Warning, "Can't get widget model for widget {0}", widget);
            return false;
        }

        public void UnregisterScreen(string screen)
        {
            if (string.IsNullOrEmpty(screen))
            {
                SFDebug.Log(LogType.Warning, "[SFUI] - Unable to unregister screen. Id is empty!");
                return;
            }

            var wasRegistered = _screenViews.Remove(screen, out var view);
            if (TryGetScreenModel(screen, out var model))
            {
                model.State = SFScreenState.Closed;
                model.IsLoaded = false;
            }

            // Unity can destroy this view during a scene change while the service stays alive.
            if (wasRegistered && _screenLoads.TryGetValue(screen, out var load) && ReferenceEquals(view, load.View))
                RemoveLoad(screen, load);
        }

        public void ScreenShownCallback(string screen)
        {
            if (string.IsNullOrWhiteSpace(screen)) return;
            if (!_screenModels.TryGetValue(screen, out var screenModel)) return;
            if (screenModel.State == SFScreenState.Shown) return;
            screenModel.State = SFScreenState.Shown;
            OnScreenShown.Invoke(screen);
        }

        public void ScreenClosedCallback(string screen, bool unload)
        {
            if (string.IsNullOrWhiteSpace(screen)) return;
            if (!_screenModels.TryGetValue(screen, out var screenModel)) return;
            if (screenModel.State == SFScreenState.Closed) return;
            screenModel.State = SFScreenState.Closed;
            OnScreenClosed.Invoke(screen);

            if (unload && screenModel.State == SFScreenState.Closed && _screenLoads.ContainsKey(screen))
            {
                UnloadScreen(screen);
            }
        }

        public void WidgetEventCallback(string widget, SFBaseEventType eventType, BaseEventData eventData)
        {
            if (string.IsNullOrWhiteSpace(widget)) return;
            OnWidgetBaseEvent.Invoke(widget, eventType, eventData);
        }

        public void WidgetEventCallback(string widget, int index, SFPointerEventType eventType,
            PointerEventData eventData)
        {
            if (string.IsNullOrWhiteSpace(widget)) return;
            OnWidgetPointerEvent.Invoke(widget, index, eventType, eventData);
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            foreach (var entry in _screenLoads.ToArray()) RemoveLoad(entry.Key, entry.Value);
            foreach (var view in _screenViews.Values.ToArray())
                if (view != null) view.DetachFromService();
            foreach (var model in _widgetModelById.Values) model.Dispose();

            _screenViews.Clear();
            _screenModels.Clear();
            _screenNodes.Clear();
            _widgetViews.Clear();
            _widgetModelById.Clear();
            _widgetNodes.Clear();
            DestroyRoot(_parentTransform);
            if (_pendingLoads == 0) DestroyRoot(_loadingTransform);
        }

        private static void DestroyRoot(Transform root)
        {
            if (root == null) return;
            if (Application.isPlaying) UnityEngine.Object.Destroy(root.gameObject);
            else UnityEngine.Object.DestroyImmediate(root.gameObject);
        }

        private sealed class ScreenLoad
        {
            public readonly UniTaskCompletionSource Completion = new();
            public AsyncOperationHandle<GameObject> Handle;
            public SFScreenView View;
            public bool Loading = true;
            public bool Unloading;
            public int Waiters;
        }
    }
}
