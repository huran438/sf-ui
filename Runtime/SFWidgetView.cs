using System;
using SFramework.Core.Runtime;
using UnityEngine;
using UnityEngine.EventSystems;
using Object = UnityEngine.Object;

namespace SFramework.UI.Runtime
{
    public abstract class SFWidgetView : SFView, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler,
        IPointerUpHandler, IPointerClickHandler, IInitializePotentialDragHandler, IBeginDragHandler, IDragHandler,
        IEndDragHandler, IDropHandler, IScrollHandler, IUpdateSelectedHandler, ISelectHandler, IDeselectHandler,
        IMoveHandler, ISubmitHandler, ICancelHandler
    {
        [SFInject]
        protected readonly ISFUIService UIService;

        public string Widget => _widget;

        public int Index { get; set; }

        public RectTransform RectTransform { get; private set; }

        [SFWidget]
        [SerializeField]
        private string _widget;

        #region Virtual

        virtual protected void PointerEnter(PointerEventData eventData)
        {
        }

        virtual protected void PointerExit(PointerEventData eventData)
        {
        }

        virtual protected void PointerDown(PointerEventData eventData)
        {
        }

        virtual protected void PointerUp(PointerEventData eventData)
        {
        }

        virtual protected void PointerClick(PointerEventData eventData)
        {
        }

        virtual protected void InitializePotentialDrag(PointerEventData eventData)
        {
        }

        virtual protected void BeginDrag(PointerEventData eventData)
        {
        }

        virtual protected void Drag(PointerEventData eventData)
        {
        }

        virtual protected void EndDrag(PointerEventData eventData)
        {
        }

        virtual protected void Drop(PointerEventData eventData)
        {
        }

        virtual protected void Scroll(PointerEventData eventData)
        {
        }

        #endregion

        #region Handlers

        // POINTER EVENTS
        public virtual void OnPointerEnter(PointerEventData eventData)
        {
            if (string.IsNullOrEmpty(_widget))
            {
                OnWidgetPointerEvent(_widget, Index, SFPointerEventType.Enter, eventData);
                return;
            }

            UIService.WidgetEventCallback(_widget, Index, SFPointerEventType.Enter, eventData);
        }

        public virtual void OnPointerExit(PointerEventData eventData)
        {
            if (string.IsNullOrEmpty(_widget))
            {
                OnWidgetPointerEvent(_widget, Index, SFPointerEventType.Exit, eventData);
                return;
            }

            UIService.WidgetEventCallback(_widget, Index, SFPointerEventType.Exit, eventData);
        }

        public virtual void OnPointerDown(PointerEventData eventData)
        {
            if (string.IsNullOrEmpty(_widget))
            {
                OnWidgetPointerEvent(_widget, Index, SFPointerEventType.Down, eventData);
                return;
            }

            UIService.WidgetEventCallback(_widget, Index, SFPointerEventType.Down, eventData);
        }

        public virtual void OnPointerUp(PointerEventData eventData)
        {
            if (string.IsNullOrEmpty(_widget))
            {
                OnWidgetPointerEvent(_widget, Index, SFPointerEventType.Up, eventData);
                return;
            }

            UIService.WidgetEventCallback(_widget, Index, SFPointerEventType.Up, eventData);
        }

        public virtual void OnPointerClick(PointerEventData eventData)
        {
            if (string.IsNullOrEmpty(_widget))
            {
                OnWidgetPointerEvent(_widget, Index, SFPointerEventType.Click, eventData);
                return;
            }

            UIService.WidgetEventCallback(_widget, Index, SFPointerEventType.Click, eventData);
        }

        public virtual void OnInitializePotentialDrag(PointerEventData eventData)
        {
            if (string.IsNullOrEmpty(_widget))
            {
                OnWidgetPointerEvent(_widget, Index, SFPointerEventType.InitializePotentialDrag, eventData);
                return;
            }

            UIService.WidgetEventCallback(_widget, Index, SFPointerEventType.InitializePotentialDrag, eventData);
        }

        public virtual void OnBeginDrag(PointerEventData eventData)
        {
            if (string.IsNullOrEmpty(_widget))
            {
                OnWidgetPointerEvent(_widget, Index, SFPointerEventType.BeginDrag, eventData);
                return;
            }

            UIService.WidgetEventCallback(_widget, Index, SFPointerEventType.BeginDrag, eventData);
        }

        public virtual void OnDrag(PointerEventData eventData)
        {
            if (string.IsNullOrEmpty(_widget))
            {
                OnWidgetPointerEvent(_widget, Index, SFPointerEventType.Drag, eventData);
                return;
            }

            UIService.WidgetEventCallback(_widget, Index, SFPointerEventType.Drag, eventData);
        }

        public virtual void OnEndDrag(PointerEventData eventData)
        {
            if (string.IsNullOrEmpty(_widget))
            {
                OnWidgetPointerEvent(_widget, Index, SFPointerEventType.EndDrag, eventData);
                return;
            }

            UIService.WidgetEventCallback(_widget, Index, SFPointerEventType.EndDrag, eventData);
        }

        public virtual void OnDrop(PointerEventData eventData)
        {
            if (string.IsNullOrEmpty(_widget))
            {
                OnWidgetPointerEvent(_widget, Index, SFPointerEventType.Drop, eventData);
                return;
            }

            UIService.WidgetEventCallback(_widget, Index, SFPointerEventType.Drop, eventData);
        }

        public virtual void OnScroll(PointerEventData eventData)
        {
            if (string.IsNullOrEmpty(_widget))
            {
                OnWidgetPointerEvent(_widget, Index, SFPointerEventType.Scroll, eventData);
                return;
            }

            UIService.WidgetEventCallback(_widget, Index, SFPointerEventType.Scroll, eventData);
        }

        // BASE EVENTS

        public virtual void OnUpdateSelected(BaseEventData eventData)
        {
            UIService.WidgetEventCallback(_widget, SFBaseEventType.UpdateSelected, eventData);
        }

        public virtual void OnSelect(BaseEventData eventData)
        {
            UIService.WidgetEventCallback(_widget, SFBaseEventType.Select, eventData);
        }

        public virtual void OnDeselect(BaseEventData eventData)
        {
            UIService.WidgetEventCallback(_widget, SFBaseEventType.Deselect, eventData);
        }

        public virtual void OnMove(AxisEventData eventData)
        {
            UIService.WidgetEventCallback(_widget, SFBaseEventType.Move, eventData);
        }

        public virtual void OnSubmit(BaseEventData eventData)
        {
            UIService.WidgetEventCallback(_widget, SFBaseEventType.Submit, eventData);
        }

        public virtual void OnCancel(BaseEventData eventData)
        {
            UIService.WidgetEventCallback(_widget, SFBaseEventType.Cancel, eventData);
        }

        #endregion

        protected override void Awake()
        {
            RectTransform = transform as RectTransform;
            base.Awake();
        }

        protected virtual void Start()
        {
            UIService.RegisterWidget(_widget, this);
            UIService.OnWidgetPointerEvent += OnWidgetPointerEvent;
            UIService.OnShowScreen += OnShowScreen;
            UIService.OnCloseScreen += OnCloseScreen;
        }

        private void OnShowScreen(string screen, object[] parameters)
        {
            if (!_widget.StartsWith(screen)) return;
            ShowScreenEvent(parameters);
        }

        private void OnCloseScreen(string screen)
        {
            if (!_widget.StartsWith(screen)) return;
            CloseScreenEvent();
        }

        private void OnWidgetPointerEvent(string widget, int index, SFPointerEventType sfPointerEventType, PointerEventData eventData)
        {
            if (widget != _widget) return;
            if (index != Index) return;

            switch (sfPointerEventType)
            {
                case SFPointerEventType.Enter:
                    PointerEnter(eventData);
                    break;
                case SFPointerEventType.Exit:
                    PointerExit(eventData);
                    break;
                case SFPointerEventType.Down:
                    PointerDown(eventData);
                    break;
                case SFPointerEventType.Up:
                    PointerUp(eventData);
                    break;
                case SFPointerEventType.Click:
                    PointerClick(eventData);
                    break;
                case SFPointerEventType.InitializePotentialDrag:
                    InitializePotentialDrag(eventData);
                    break;
                case SFPointerEventType.BeginDrag:
                    BeginDrag(eventData);
                    break;
                case SFPointerEventType.Drag:
                    Drag(eventData);
                    break;
                case SFPointerEventType.EndDrag:
                    EndDrag(eventData);
                    break;
                case SFPointerEventType.Drop:
                    Drop(eventData);
                    break;
                case SFPointerEventType.Scroll:
                    Scroll(eventData);
                    break;
            }
        }

        protected virtual void ShowScreenEvent(object[] parameters)
        {
        }

        protected virtual void CloseScreenEvent()
        {
        }

        protected virtual void OnDestroy()
        {
            UIService.OnShowScreen -= OnShowScreen;
            UIService.OnCloseScreen -= OnCloseScreen;
            UIService.OnWidgetPointerEvent -= OnWidgetPointerEvent;
            UIService.UnregisterWidget(_widget, this);
        }

        public bool IsNull<T>(T instance) where T : IEventSystemHandler
        {
            if (instance is Object unityObject)
                return unityObject == null;

            return instance == null;
        }
    }
}