using System;
using SFramework.Configs.Runtime;
using SFramework.Core.Runtime;
using UnityEngine;

namespace SFramework.UI.Runtime
{
    [Serializable]
    public sealed class SFWidgetNode : SFConfigNode
    {
        public override ISFConfigNode[] Children => null;
    }
}