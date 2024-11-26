using System;
using SFramework.Configs.Runtime;

namespace SFramework.UI.Runtime
{
    [Serializable]
    public class SFScreenModel : SFNodeModel<SFScreenNode>
    {
        public SFScreenState State { get; internal set; }
        public bool IsLoaded { get; internal set; }

        public SFScreenModel(SFScreenNode node) : base(node)
        {
            State = SFScreenState.Closed;
        }

        public override void Dispose()
        {
            
        }
    }
}