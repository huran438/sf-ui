using System;
using SFramework.Core.Runtime;
using SFramework.Repositories.Runtime;
using UnityEngine;
using UnityEngine.Serialization;

namespace SFramework.UI.Runtime
{
    [Serializable]
    public sealed class SFScreenNode : SFNode
    {
        public SFWidgetNode[] Widgets;
        public override ISFNode[] Nodes => Widgets;
    }
}