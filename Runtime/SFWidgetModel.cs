using System;
using System.Collections.Generic;
using SFramework.Configs.Runtime;

namespace SFramework.UI.Runtime
{
    [Serializable]
    public class SFWidgetModel : SFNodeModel<SFWidgetNode>
    {
        public IReadOnlyList<SFWidgetView> Views => _views;

        private List<SFWidgetView> _views;

        public SFWidgetModel(SFWidgetNode node) : base(node)
        {
            _views = new List<SFWidgetView>();
        }

        public void RegisterView(SFWidgetView widgetView)
        {
            _views.Add(widgetView);
        }

        public void UnregisterView(SFWidgetView widgetView)
        {
            _views.Remove(widgetView);
        }

        public bool IsLoadedViewByIndex(int index)
        {
            if (index < 0) index = 0;
            return _views.Count > 0 && index < _views.Count;
        }

        public override void Dispose()
        {
            _views.Clear();
        }
    }
}