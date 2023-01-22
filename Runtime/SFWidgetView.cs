using SFramework.Core.Runtime;
using UnityEngine;
using UnityEngine.EventSystems;

namespace SFramework.UI.Runtime
{
    public abstract class SFWidgetView : SFView, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler,
        IPointerUpHandler, IPointerClickHandler, IInitializePotentialDragHandler, IBeginDragHandler, IDragHandler,
        IEndDragHandler, IDropHandler, IScrollHandler, IUpdateSelectedHandler, ISelectHandler, IDeselectHandler,
        IMoveHandler, ISubmitHandler, ICancelHandler
    {
        [SFInject]
        protected readonly ISFUIService _uiService;

        public string Widget => _widget;

        [SFWidget]
        [SerializeField]
        private string _widget;

        #region Virtual

        protected virtual void PointerEnter(PointerEventData eventData) { }
        protected virtual void PointerExit(PointerEventData eventData) { }
        protected virtual void PointerDown(PointerEventData eventData) { }
        protected virtual void PointerUp(PointerEventData eventData) { }
        protected virtual void PointerClick(PointerEventData eventData) { }
        protected virtual void InitializePotentialDrag(PointerEventData eventData) { }
        protected virtual void BeginDrag(PointerEventData eventData) { }
        protected virtual void Drag(PointerEventData eventData) { }
        protected virtual void EndDrag(PointerEventData eventData) { }
        protected virtual void Drop(PointerEventData eventData) { }
        protected virtual void Scroll(PointerEventData eventData) { }

        #endregion
        
        #region Handlers

        // POINTER EVENTS
        public void OnPointerEnter(PointerEventData eventData)
        {
            _uiService.WidgetEventCallback(_widget, SFPointerEventType.Enter, eventData);
            PointerEnter(eventData);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _uiService.WidgetEventCallback(_widget, SFPointerEventType.Exit, eventData);
            PointerExit(eventData);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            _uiService.WidgetEventCallback(_widget, SFPointerEventType.Down, eventData);
            PointerDown(eventData);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            _uiService.WidgetEventCallback(_widget, SFPointerEventType.Up, eventData);
            PointerUp(eventData);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            _uiService.WidgetEventCallback(_widget, SFPointerEventType.Click, eventData);
            PointerClick(eventData);
        }

        public void OnInitializePotentialDrag(PointerEventData eventData)
        {
            _uiService.WidgetEventCallback(_widget, SFPointerEventType.InitializePotentialDrag, eventData);
            InitializePotentialDrag(eventData);
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            _uiService.WidgetEventCallback(_widget, SFPointerEventType.BeginDrag, eventData);
            BeginDrag(eventData);
        }

        public void OnDrag(PointerEventData eventData)
        {
            _uiService.WidgetEventCallback(_widget, SFPointerEventType.Drag, eventData);
            Drag(eventData);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            _uiService.WidgetEventCallback(_widget, SFPointerEventType.EndDrag, eventData);
            EndDrag(eventData);
        }

        public void OnDrop(PointerEventData eventData)
        {
            _uiService.WidgetEventCallback(_widget, SFPointerEventType.Drop, eventData);
            Drop(eventData);
        }

        public void OnScroll(PointerEventData eventData)
        {
            _uiService.WidgetEventCallback(_widget, SFPointerEventType.Scroll, eventData);
            Scroll(eventData);
        }

        // BASE EVENTS

        public void OnUpdateSelected(BaseEventData eventData)
        {
            _uiService.WidgetEventCallback(_widget, SFBaseEventType.UpdateSelected, eventData);
        }

        public void OnSelect(BaseEventData eventData)
        {
            _uiService.WidgetEventCallback(_widget, SFBaseEventType.Select, eventData);
        }

        public void OnDeselect(BaseEventData eventData)
        {
            _uiService.WidgetEventCallback(_widget, SFBaseEventType.Deselect, eventData);
        }

        public void OnMove(AxisEventData eventData)
        {
            _uiService.WidgetEventCallback(_widget, SFBaseEventType.Move, eventData);
        }

        public void OnSubmit(BaseEventData eventData)
        {
            _uiService.WidgetEventCallback(_widget, SFBaseEventType.Submit, eventData);
        }

        public void OnCancel(BaseEventData eventData)
        {
            _uiService.WidgetEventCallback(_widget, SFBaseEventType.Cancel, eventData);
        }

        #endregion
    }
}