﻿using System;
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
    public sealed class SFUIService : ISFUIService
    {
        public event Action<string> OnUnloadScreen = _ => { };
        public event Action<string> OnScreenUnloaded = _ => { };
        public event Action<string, object[]> OnShowScreen = (_, _) => { };
        public event Action<string, bool> OnCloseScreen = (_, _) => { };
        public event Action<string> OnScreenShown = _ => { };
        public event Action<string> OnScreenClosed = _ => { };
        public event Action<string, SFBaseEventType, BaseEventData> OnWidgetBaseEvent = (_, _, _) => { };

        public event Action<string, int, SFPointerEventType, PointerEventData> OnWidgetPointerEvent = (_, _, _, _) => { };

        public SFScreenModel[] ScreenModels => _screenModels.Values.ToArray();
        public SFWidgetModel[] WidgetModels => _widgetModelById.Values.ToArray();

        private readonly Dictionary<string, SFScreenModel> _screenModels = new();
        private readonly Dictionary<string, SFWidgetModel> _widgetModelById = new();
        private readonly Dictionary<string, AsyncOperationHandle> _operationHandleByScreen = new();
        private readonly Dictionary<string, SFScreenView> _screenViews = new();
        private readonly Dictionary<string, SFScreenNode> _screenNodes = new();
        private readonly Dictionary<string, SFWidgetNode> _widgetNodes = new();
        private readonly Dictionary<string, List<SFWidgetView>> _widgetViews = new();
        
        readonly Transform _parentTransform;
        private readonly ISFConfigsService _configsService;


        SFUIService(ISFContainer container, ISFConfigsService configsService)
        {
            _parentTransform = new GameObject("SFUI").GetComponent<Transform>();
            _parentTransform.SetParent(container.Root, true);
            _configsService = configsService;
        }

        public UniTask Init(CancellationToken cancellationToken)
        {
            if (_configsService.TryGetConfigs(out SFUIConfig[] configs))
            {
                foreach (var repository in configs)
                {
                    foreach (var groupNode in repository.Groups)
                    {
                        foreach (var screenNode in groupNode.Screens)
                        {
                            _screenNodes.TryAdd(screenNode.FullId, screenNode);
                            _screenModels.TryAdd(screenNode.FullId, new SFScreenModel(screenNode));
                            foreach (var widgetNode in screenNode.Widgets)
                            {
                                _widgetNodes.TryAdd(widgetNode.FullId, widgetNode);
                                _widgetModelById.TryAdd(widgetNode.FullId, new SFWidgetModel(widgetNode));
                            }
                        }
                    }
                }
            }


            return UniTask.CompletedTask;
        }


        public async UniTask LoadScreen(string screen, bool show = false, IProgress<float> progress = null,
            CancellationToken cancellationToken = default, params object[] parameters)
        {
            if (string.IsNullOrEmpty(screen))
                throw new ArgumentNullException(nameof(screen));

            if (_operationHandleByScreen.TryGetValue(screen, out _)) return;

            if (!_screenNodes.TryGetValue(screen, out var screenNode))
            {
                throw new NullReferenceException("Failed to load screen node: " + screen);
            }

            var handle = Addressables.InstantiateAsync(screenNode.Prefab, _parentTransform);

            _operationHandleByScreen[screen] = handle;

            while (!handle.IsDone && !cancellationToken.IsCancellationRequested)
            {
                progress?.Report(handle.PercentComplete);
                await UniTask.Yield(cancellationToken);
            }

            if (cancellationToken.IsCancellationRequested)
            {
                Addressables.Release(handle);
                _operationHandleByScreen.Remove(screen);
                throw new OperationCanceledException();
            }

            if (handle.Status == AsyncOperationStatus.Failed)
            {
                Addressables.Release(handle);
                _operationHandleByScreen.Remove(screen);
                throw new NullReferenceException("Failed to load screen: " + screen);
            }

            if (show)
            {
                _screenModels[screen].State = SFScreenState.Show;
                OnShowScreen.Invoke(screen, parameters);
            }
        }

        public void UnloadScreen(string screen)
        {
            if (string.IsNullOrEmpty(screen))
                throw new ArgumentNullException(nameof(screen));

            if (!_operationHandleByScreen.ContainsKey(screen))
                throw new Exception("Screen not loaded: " + screen);


            if (_screenModels[screen].State != SFScreenState.Closed)
            {
                _screenModels[screen].State = SFScreenState.Closed;
                OnScreenClosed.Invoke(screen);
            }

            OnUnloadScreen.Invoke(screen);
            Addressables.Release(_operationHandleByScreen[screen]);
            _operationHandleByScreen.Remove(screen);
            OnScreenUnloaded.Invoke(screen);
        }

        public UniTask ShowScreen(string screen, params object[] parameters)
        {
            return ShowScreen(screen, null, default, parameters);
        }

        public async UniTask ShowScreen(string screen, IProgress<float> progress = null,
            CancellationToken cancellationToken = default,
            params object[] parameters)
        {
            if (string.IsNullOrWhiteSpace(screen)) return;
            if(!_screenModels.TryGetValue(screen, out var screenModel)) return;
            if(screenModel.State == SFScreenState.Show || screenModel.State == SFScreenState.Shown) return;
            screenModel.State = SFScreenState.Show;
            if (!_operationHandleByScreen.ContainsKey(screen))
            {
                await LoadScreen(screen, true, progress, cancellationToken, parameters);
                return;
            }

            OnShowScreen.Invoke(screen, parameters);
        }

        public void CloseScreen(string screen, bool unload = false)
        {
            if (string.IsNullOrWhiteSpace(screen)) return;
            if(!_screenModels.TryGetValue(screen, out var screenModel)) return;
            if(screenModel.State == SFScreenState.Close || screenModel.State == SFScreenState.Closed) return;
            screenModel.State = SFScreenState.Close;
            OnCloseScreen.Invoke(screen, unload);
        }

        public bool TryGetScreenView(string screen, out SFScreenView screenView)
        {
            return _screenViews.TryGetValue(screen, out screenView);
        }

        public bool TryGetScreenModel(string screen, out SFScreenModel screenModel)
        {
            return _screenModels.TryGetValue(screen, out screenModel);
        }

        public void RegisterScreen(string screen, SFScreenView root)
        {
            _screenModels[screen].State = SFScreenState.Closed;
            _screenViews[screen] = root;
            if (TryGetScreenModel(screen, out var model))
            {
                model.IsLoaded = true;
            }
        }

        public void RegisterWidget(string widget, SFWidgetView widgetView)
        {
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
            if (_widgetViews.ContainsKey(widget))
            {
                _widgetViews.Remove(widget);
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
            index = Mathf.Clamp(index, 0, widgets.Count);
            view = widgets[index];
            return true;
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
            _screenModels[screen].State = SFScreenState.Closed;

            if (_screenViews.ContainsKey(screen))
            {
                _screenViews.Remove(screen);
            }

            if (TryGetScreenModel(screen, out var model))
            {
                model.IsLoaded = false;
            }
        }

        public void ScreenShownCallback(string screen)
        {
            if (string.IsNullOrWhiteSpace(screen)) return;
            if(!_screenModels.TryGetValue(screen, out var screenModel)) return;
            if(screenModel.State == SFScreenState.Shown) return;
            screenModel.State = SFScreenState.Shown;
            OnScreenShown.Invoke(screen);
        }

        public void ScreenClosedCallback(string screen, bool unload)
        {
            if (string.IsNullOrWhiteSpace(screen)) return;
            if(!_screenModels.TryGetValue(screen, out var screenModel)) return;
            if(screenModel.State == SFScreenState.Closed) return;
            screenModel.State = SFScreenState.Closed;
            OnScreenClosed.Invoke(screen);

            if (unload)
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
            // TODO release managed resources here
        }
    }
}