﻿using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using SFramework.Core.Runtime;
using UnityEngine;
using UnityEngine.UI;

namespace SFramework.UI.Runtime
{
    [RequireComponent(typeof(Canvas))]
    [RequireComponent(typeof(CanvasGroup))]
    [RequireComponent(typeof(GraphicRaycaster))]
    public abstract class SFScreenView : SFView
    {
        public Canvas Canvas => _canvas;

        public CanvasGroup CanvasGroup => _canvasGroup;

        public GraphicRaycaster GraphicRaycaster => _graphicRaycaster;

        public string Screen => _screen;

        protected SFScreenState State => _uiServiceInternal.TryGetScreenModel(_screen, out var model) ? model.State : SFScreenState.Closed;

        [SFScreen]
        [SerializeField]
        private string _screen;
        
        [SerializeField]
        private bool _visibleByDefault;
        
        private Canvas _canvas;
        private CanvasGroup _canvasGroup;
        private GraphicRaycaster _graphicRaycaster;
        private ISFUIService _uiServiceInternal;

        protected IEnumerable<SFWidgetView> Widgets => _widgets;

        private SFWidgetView[] _widgets = Array.Empty<SFWidgetView>();

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

        [SFInject]
        public void _InitializeScreenInternal(ISFUIService uiController)
        {
            _uiServiceInternal = uiController;
            _uiServiceInternal.RegisterScreen(_screen, this);
            _uiServiceInternal.OnShowScreen += _onShowScreen;
            _uiServiceInternal.OnCloseScreen += _onCloseScreen;
            _uiServiceInternal.OnScreenShown += _onScreenShown;
            _uiServiceInternal.OnScreenClosed += _onScreenClosed;
        }


        protected abstract void OnShowScreen([CanBeNull] object[] parameters);
        protected abstract void OnScreenShown();
        protected abstract void OnCloseScreen();
        protected abstract void OnScreenClosed();

        public void ShowScreen()
        {
            _uiServiceInternal.ShowScreen(Screen);
        }

        public void CloseScreen()
        {
            _uiServiceInternal.CloseScreen(Screen);
        }

        public void ScreenShownCallback()
        {
            _uiServiceInternal.ScreenShownCallback(_screen);
        }

        public void ScreenClosedCallback()
        {
            _uiServiceInternal.ScreenClosedCallback(_screen);
        }

        private void _onShowScreen(string screen, object[] parameters)
        {
            if (screen != _screen) return;
            OnShowScreen(parameters);
        }

        private void _onScreenShown(string screen)
        {
            if (screen != _screen) return;
            OnScreenShown();
        }

        private void _onCloseScreen(string screen)
        {
            if (screen != _screen) return;
            OnCloseScreen();
        }

        private void _onScreenClosed(string screen)
        {
            if (screen != _screen) return;
            OnScreenClosed();
        }

        protected virtual void OnDestroy()
        {
            if (!Application.isPlaying) return;
            _uiServiceInternal.UnregisterScreen(_screen);
            _uiServiceInternal.OnShowScreen -= _onShowScreen;
            _uiServiceInternal.OnCloseScreen -= _onCloseScreen;
            _uiServiceInternal.OnScreenShown -= _onScreenShown;
            _uiServiceInternal.OnScreenClosed -= _onScreenClosed;
        }
    }
}