using System;
using System.Collections.Generic;
using SFramework.Core.Runtime;
using UnityEngine;
using UnityEngine.EventSystems;

namespace SFramework.UI.Runtime
{
    public sealed class SFUIService : ISFUIService
    {
        public bool IsLoaded => _isLoaded;

        public event Action<string> OnShowScreen = _ => { };
        public event Action<string> OnCloseScreen = _ => { };
        public event Action<string> OnScreenShown = _ => { };
        public event Action<string> OnScreenClosed = _ => { };
        public event Action<string, SFBaseEventType, BaseEventData> OnWidgetBaseEvent = (_, _, _) => { };
        public event Action<string, SFPointerEventType, PointerEventData> OnWidgetPointerEvent = (_, _, _) => { };

        private Dictionary<string, SFScreenState> _screenStates = new();
        private Dictionary<string, GameObject> _screenRootGameObjects = new();

        private bool _isLoaded;

        [SFInject]
        public void Init(SFUIDatabase database)
        {
            foreach (SFScreenGroupContainer screenGroupContainer in database.Nodes)
            {
                foreach (SFScreenContainer screenContainer in screenGroupContainer.Children)
                {
                    foreach (SFWidgetContainer widgetContainer in screenContainer.Children)
                    {
                    }
                }
            }
        }

        public void ShowScreen(string screen)
        {
            if (string.IsNullOrWhiteSpace(screen)) return;
            _screenStates[screen] = SFScreenState.Showing;
            OnShowScreen.Invoke(screen);
        }

        public void CloseScreen(string screen)
        {
            if (string.IsNullOrWhiteSpace(screen)) return;
            _screenStates[screen] = SFScreenState.Closing;
            OnCloseScreen.Invoke(screen);
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
    }
}