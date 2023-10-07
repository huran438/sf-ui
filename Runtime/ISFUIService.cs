using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;

namespace SFramework.UI.Runtime
{
    public interface ISFUIService
    {
        bool IsLoaded { get; }
        event Action<string> OnShowScreen;
        event Action<string> OnCloseScreen;
        event Action<string> OnScreenShown;
        event Action<string> OnScreenClosed;
        event Action<string, SFBaseEventType, BaseEventData> OnWidgetBaseEvent;
        event Action<string, SFPointerEventType, PointerEventData> OnWidgetPointerEvent;


        UniTask LoadScreen(string screen, bool show = false,Action onResult = null, IProgress<float> progress = null,
            CancellationToken cancellationToken = default);

        void UnloadScreen(string screen, Action onResult = null);

        void ShowScreen(string screen);
        void CloseScreen(string screen);
        SFScreenState GetScreenState(string screen);
        GameObject GetScreenRootGO(string screen);
        void Register(string screen, GameObject root);
        void Unregister(string screen);
        void ScreenShownCallback(string screen);
        void ScreenClosedCallback(string screen);
        void WidgetEventCallback(string widget, SFBaseEventType eventType, BaseEventData eventData);
        void WidgetEventCallback(string widget, SFPointerEventType eventType, PointerEventData eventData);
    }
}