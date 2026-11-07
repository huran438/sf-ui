using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using SFramework.Core.Runtime;
using UnityEngine;
using UnityEngine.UI;

namespace SFramework.UI.Runtime
{
    [RequireComponent(typeof(Canvas))]
    [RequireComponent(typeof(CanvasGroup))]
    [RequireComponent(typeof(GraphicRaycaster))]
    public abstract partial class SFScreenView : SFView
    {
        public Canvas Canvas => _canvas;

        public CanvasGroup CanvasGroup => _canvasGroup;

        public GraphicRaycaster GraphicRaycaster => _graphicRaycaster;

        public string Screen => _screen;

        protected SFScreenState State => _uiServiceInternal.TryGetScreenModel(_screen, out var model)
            ? model.State
            : SFScreenState.Closed;

        [SFScreen]
        [SerializeField]
        private string _screen;

        [SerializeField]
        private bool _visibleByDefault;

        private Canvas _canvas;
        private CanvasGroup _canvasGroup;
        private GraphicRaycaster _graphicRaycaster;
        private ISFUIService _uiServiceInternal;
        private ISFUIService RegisteredService { get; set; }

        protected IEnumerable<SFWidgetView> Widgets => _widgets;

        private SFWidgetView[] _widgets = Array.Empty<SFWidgetView>();

        private bool _unloadOnClose;

        protected override void Awake()
        {
            _canvas = GetComponent<Canvas>();
            _canvasGroup = GetComponent<CanvasGroup>();
            _graphicRaycaster = GetComponent<GraphicRaycaster>();
            _canvasGroup.alpha = _visibleByDefault ? 1f : 0f;
            _canvasGroup.interactable = _visibleByDefault;
            _canvasGroup.blocksRaycasts = _visibleByDefault;
            _widgets = GetComponentsInChildren<SFWidgetView>(true);
            base.Awake();
        }

        public void _InitializeScreenInternal(ISFUIService uiController)
        {
            if (RegisteredService == uiController && RegisteredService != null) return;
            DetachFromService();
            _uiServiceInternal = uiController;
            if (uiController == null) return;

            uiController.RegisterScreen(_screen, this);
            if (string.IsNullOrEmpty(_screen) ||
                !uiController.TryGetScreenView(_screen, out var registered) || registered != this) return;

            RegisteredService = uiController;
            uiController.OnShowScreen += _onShowScreen;
            uiController.OnCloseScreen += _onCloseScreen;
            uiController.OnScreenShown += _onScreenShown;
            uiController.OnScreenClosed += _onScreenClosed;
        }

        protected override void OnDependenciesInjected()
        {
            _InitializeScreenInternal(_uiServiceInternal);
            base.OnDependenciesInjected();
        }


        protected abstract void OnShowScreen(bool force, [CanBeNull] object[] parameters);
        protected abstract void OnScreenShown();
        protected abstract void OnCloseScreen(bool force, bool unload);
        protected abstract void OnScreenClosed();
        protected abstract void OnScreenDestroy();

        public void ShowScreen(bool force)
        {
            _uiServiceInternal.ShowScreen(Screen, force).Forget();
        }

        public void ShowScreen()
        {
            ShowScreen(false);
        }

        public void CloseScreen(bool force, bool unload)
        {
            _uiServiceInternal.CloseScreen(Screen, force, unload);
        }

        public void CloseScreen()
        {
            _uiServiceInternal.CloseScreen(Screen, false, false);
        }

        public void ScreenShownCallback()
        {
            if (RegisteredService != null && State == SFScreenState.Show)
                RegisteredService.ScreenShownCallback(_screen);
        }

        public void ScreenClosedCallback()
        {
            if (RegisteredService != null && State == SFScreenState.Close)
                RegisteredService.ScreenClosedCallback(_screen, _unloadOnClose);
        }

        private void _onShowScreen(string screen, bool force, object[] parameters)
        {
            if (screen != _screen) return;
            OnShowScreen(force, parameters);
        }

        private void _onScreenShown(string screen)
        {
            if (screen != _screen) return;
            OnScreenShown();
        }

        private void _onCloseScreen(string screen, bool force, bool unload)
        {
            if (screen != _screen) return;
            _unloadOnClose = unload;
            OnCloseScreen(force, unload);
        }

        private void _onScreenClosed(string screen)
        {
            if (screen != _screen) return;
            OnScreenClosed();
        }

        protected virtual void OnDestroy()
        {
            if (!Application.isPlaying) return;
            try
            {
                OnScreenDestroy();
            }
            finally
            {
                DetachFromService();
            }
        }

        internal void DetachFromService()
        {
            var service = RegisteredService;
            if (service == null) return;
            RegisteredService = null;
            service.OnShowScreen -= _onShowScreen;
            service.OnCloseScreen -= _onCloseScreen;
            service.OnScreenShown -= _onScreenShown;
            service.OnScreenClosed -= _onScreenClosed;
            if (service.TryGetScreenView(_screen, out var registered) && ReferenceEquals(registered, this))
                service.UnregisterScreen(_screen);
        }
        public virtual void SetParameters(object[] parameters)
        {
            
        }
  
    }
}
