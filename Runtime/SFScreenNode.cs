using System;
using SFramework.Configs.Runtime;
using SFramework.Core.Runtime;
using UnityEngine;
using UnityEngine.Serialization;

namespace SFramework.UI.Runtime
{
    [Serializable]
    public sealed class SFScreenNode : SFConfigNode
    {
        public string Prefab;
        public SFWidgetNode[] Widgets;
        public override ISFConfigNode[] Nodes => Widgets;
    }
}