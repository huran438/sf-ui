using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using SFramework.Core.Runtime;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Scripting;



namespace SFramework.UI.Runtime
{
    public interface ISFUIService : ISFService
    {
        bool IsLoaded { get; }
        event Action<string, string[]> OnShowScreen;
        event Action<string> OnCloseScreen;
        event Action<string> OnScreenShown;
        event Action<string> OnScreenClosed;
        event Action<string, SFBaseEventType, BaseEventData> OnWidgetBaseEvent;
        event Action<string, SFPointerEventType, PointerEventData> OnWidgetPointerEvent;
        UniTask LoadScreen(string screen, bool show = false, IProgress<float> progress = null, CancellationToken cancellationToken = default, params string[] parameters);
        void UnloadScreen(string screen);
        UniTask ShowScreen(string screen, params string[] parameters);
        UniTask ShowScreen(string screen, IProgress<float> progress = null, CancellationToken cancellationToken = default, params string[] parameters);
        void CloseScreen(string screen, bool unload = false);
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
