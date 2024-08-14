using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using SFramework.Core.Runtime;
using UnityEngine.EventSystems;


namespace SFramework.UI.Runtime
{
    public interface ISFUIService : ISFService
    {
        event Action<string, object[]> OnShowScreen;
        event Action<string> OnCloseScreen;
        event Action<string> OnScreenShown;
        event Action<string> OnScreenClosed;
        event Action<string, SFBaseEventType, BaseEventData> OnWidgetBaseEvent;
        event Action<string, int,SFPointerEventType, PointerEventData> OnWidgetPointerEvent;
        SFScreenModel[] ScreenModels { get; }
        SFWidgetModel[] WidgetModels { get; }

        UniTask LoadScreen(string screen, bool show = false, IProgress<float> progress = null,
            CancellationToken cancellationToken = default, params object[] parameters);

        void UnloadScreen(string screen);
        UniTask ShowScreen(string screen, params object[] parameters);

        UniTask ShowScreen(string screen, IProgress<float> progress = null, CancellationToken cancellationToken = default,
            params object[] parameters);

        void CloseScreen(string screen, bool unload = false);
        bool TryGetScreenView(string screen, out SFScreenView screenView);
        bool TryGetScreenModel(string screen, out SFScreenModel screenModel);
        void RegisterScreen(string screen, SFScreenView root);
        void RegisterWidget(string widget, SFWidgetView widgetView);
        void UnregisterWidget(string widget, SFWidgetView widgetView);
        bool TryGetWidgetView(string widget, int index, out SFWidgetView view);
        bool TryGetWidgetNode(string widget, out SFWidgetNode widgetNode);
        bool TryGetWidgetModel(string widget, out SFWidgetModel widgetModel);
        void UnregisterScreen(string screen);
        void ScreenShownCallback(string screen);
        void ScreenClosedCallback(string screen);
        void WidgetEventCallback(string widget, SFBaseEventType eventType, BaseEventData eventData);
        void WidgetEventCallback(string widget, int index, SFPointerEventType eventType, PointerEventData eventData);
    }
}