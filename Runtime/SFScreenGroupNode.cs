using System;
using SFramework.Core.Runtime;
using SFramework.Repositories.Runtime;
using UnityEngine;

namespace SFramework.UI.Runtime
{
    [Serializable]
    public sealed class SFScreenGroupNode : SFNode
    {
        public SFScreenNode[] Screens;

        public override ISFNode[] Nodes => Screens;
    }
}