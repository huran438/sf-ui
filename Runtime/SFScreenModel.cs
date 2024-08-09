using System;
using SFramework.Configs.Runtime;

namespace SFramework.UI.Runtime
{
    [Serializable]
    public class SFScreenModel : SFNodeModel<SFScreenNode>
    {
        public SFScreenState State { get; set; }

        public SFScreenModel(SFScreenNode node) : base(node)
        {
        }

        public override void Dispose()
        {
            
        }
    }
}