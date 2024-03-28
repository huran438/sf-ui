using System;
using SFramework.Configs.Runtime;
using SFramework.Core.Runtime;
using UnityEngine;

namespace SFramework.UI.Runtime
{
    [Serializable]
    public sealed class SFScreenGroupNode : SFConfigNode
    {
        public SFScreenNode[] Screens;

        public override ISFConfigNode[] Children => Screens;
    }
}