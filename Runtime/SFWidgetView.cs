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

        public string Widget
        {
            get
            {
                return _widget;
            }
        }

        [SFWidget]
        [SerializeField]
        private string _widget;

        #region Virtual

        virtual protected void PointerEnter(PointerEventData eventData) { }
        virtual protected void PointerExit(PointerEventData eventData) { }
        virtual protected void PointerDown(PointerEventData eventData) { }
        virtual protected void PointerUp(PointerEventData eventData) { }
        virtual protected void PointerClick(PointerEventData eventData) { }
        virtual protected void InitializePotentialDrag(PointerEventData eventData) { }
        virtual protected void BeginDrag(PointerEventData eventData) { }
        virtual protected void Drag(PointerEventData eventData) { }
        virtual protected void EndDrag(PointerEventData eventData) { }
        virtual protected void Drop(PointerEventData eventData) { }
        virtual protected void Scroll(PointerEventData eventData) { }

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
        
        public bool IsNull<T>(T instance) where T : IEventSystemHandler
        {
            if (instance is Object unityObject)
                return unityObject == null;

            return instance == null;
        }
    }
}