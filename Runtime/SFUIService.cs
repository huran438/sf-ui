using System;
using System.Collections.Generic;
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
    public class SFUIServiceAssetNotFoundException : Exception
    {
    }

    public sealed class SFUIService : ISFUIService

    {
        public bool IsLoaded => _isLoaded;

        public event Action<string> OnShowScreen = _ => { };
        public event Action<string> OnCloseScreen = _ => { };
        public event Action<string> OnScreenShown = _ => { };
        public event Action<string> OnScreenClosed = _ => { };
        public event Action<string, SFBaseEventType, BaseEventData> OnWidgetBaseEvent = (_, _, _) => { };
        public event Action<string, SFPointerEventType, PointerEventData> OnWidgetPointerEvent = (_, _, _) => { };

        private Dictionary<string, AsyncOperationHandle> _loadedScreens = new();
        private Dictionary<string, SFScreenState> _screenStates = new();
        private Dictionary<string, GameObject> _screenRootGameObjects = new();
        private Dictionary<string, SFScreenNode> _screenNodes = new();
        private Dictionary<string, SFWidgetNode> _widgetNodes = new();

        private bool _isLoaded;
        readonly Transform _parentTransform;


        SFUIService(ISFContainer container, ISFConfigsService provider)
        {
            var repositories = provider.GetRepositories<SFUIConfig>();

            foreach (var repository in repositories)
            {
                foreach (SFScreenGroupNode groupNode in repository.Nodes)
                {
                    foreach (SFScreenNode screenNode in groupNode.Nodes)
                    {
                        var screen = SFConfigsExtensions.GetSFId(repository.Name, groupNode.Name, screenNode.Name);
                        _screenNodes.TryAdd(screen, screenNode);
                        foreach (SFWidgetNode widgetNode in screenNode.Nodes)
                        {
                            var widget = SFConfigsExtensions.GetSFId(repository.Name, groupNode.Name, screenNode.Name,
                                widgetNode.Name);
                            _widgetNodes.TryAdd(widget, widgetNode);
                        }
                    }
                }
            }

            _parentTransform = new GameObject("SFUI").GetComponent<Transform>();
            _parentTransform.SetParent(container.Root, true);
        }

        public async UniTask LoadScreen(string screen, bool show = false, IProgress<float> progress = null,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(screen))
                throw new ArgumentNullException(nameof(screen));

            if (_loadedScreens.TryGetValue(screen, out _)) return;

            if (!_screenNodes.TryGetValue(screen, out var screenNode))
            {
                throw new NullReferenceException("Failed to load screen node: " + screen);
            }

            var handle = Addressables.InstantiateAsync(screenNode.Prefab, _parentTransform);

            _loadedScreens[screen] = handle;

            while (!handle.IsDone && !cancellationToken.IsCancellationRequested)
            {
                progress?.Report(handle.PercentComplete);
                await UniTask.Yield(cancellationToken);
            }

            if (cancellationToken.IsCancellationRequested)
            {
                Addressables.Release(handle);
                _loadedScreens.Remove(screen);
                throw new OperationCanceledException();
            }

            if (handle.Status == AsyncOperationStatus.Failed)
            {
                Addressables.Release(handle);
                _loadedScreens.Remove(screen);
                throw new NullReferenceException("Failed to load screen: " + screen);
            }

            if (show)
            {
                _screenStates[screen] = SFScreenState.Showing;
                OnShowScreen.Invoke(screen);
            }
        }

        public void UnloadScreen(string screen)
        {
            if (string.IsNullOrEmpty(screen))
                throw new ArgumentNullException(nameof(screen));

            if (!_loadedScreens.ContainsKey(screen))
                throw new Exception("Screen not loaded: " + screen);

            Addressables.Release(_loadedScreens[screen]);
            _loadedScreens.Remove(screen);
        }

        public async UniTask ShowScreen(string screen, IProgress<float> progress = null, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(screen)) return;

            if (!_loadedScreens.ContainsKey(screen))
            {
                await LoadScreen(screen, true, progress, cancellationToken);
                return;
            }

            _screenStates[screen] = SFScreenState.Showing;
            OnShowScreen.Invoke(screen);
        }

        public void CloseScreen(string screen, bool unload = false)
        {
            if (string.IsNullOrWhiteSpace(screen)) return;
            _screenStates[screen] = SFScreenState.Closing;
            OnCloseScreen.Invoke(screen);
            if (unload)
            {
                UnloadScreen(screen);
            }
        }

        public SFScreenState GetScreenState(string screen)
        {
            return !_screenStates.ContainsKey(screen) ? SFScreenState.Closed : _screenStates[screen];
        }

        public GameObject GetScreenRootGO(string screen)
        {
            return !_screenRootGameObjects.ContainsKey(screen) ? null : _screenRootGameObjects[screen];
        }

        public void Register(string screen, GameObject root)
        {
            if (!_isLoaded)
            {
                _isLoaded = true;
            }

            _screenStates[screen] = SFScreenState.Closed;
            _screenRootGameObjects[screen] = root;
        }

        public void Unregister(string screen)
        {
            if (_screenStates.ContainsKey(screen))
                _screenStates.Remove(screen);

            if (_screenRootGameObjects.ContainsKey(screen))
                _screenRootGameObjects.Remove(screen);
        }

        public void ScreenShownCallback(string sfScreen)
        {
            if (string.IsNullOrWhiteSpace(sfScreen)) return;
            _screenStates[sfScreen] = SFScreenState.Shown;
            OnScreenShown.Invoke(sfScreen);
        }

        public void ScreenClosedCallback(string sfScreen)
        {
            if (string.IsNullOrWhiteSpace(sfScreen)) return;
            _screenStates[sfScreen] = SFScreenState.Closed;
            OnScreenClosed.Invoke(sfScreen);
        }

        public void WidgetEventCallback(string widget, SFBaseEventType eventType, BaseEventData eventData)
        {
            if (string.IsNullOrWhiteSpace(widget)) return;
            OnWidgetBaseEvent.Invoke(widget, eventType, eventData);
        }

        public void WidgetEventCallback(string widget, SFPointerEventType eventType, PointerEventData eventData)
        {
            if (string.IsNullOrWhiteSpace(widget)) return;
            OnWidgetPointerEvent.Invoke(widget, eventType, eventData);
        }

        public void Dispose()
        {
            // TODO release managed resources here
        }
    }
}
