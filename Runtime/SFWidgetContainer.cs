using System;
using SFramework.Core.Runtime;
using UnityEngine;

namespace SFramework.UI.Runtime
{
    [Serializable]
    public sealed class SFWidgetContainer : SFDatabaseNode
    {
        public override ISFDatabaseNode[] Children => null;
    }
}