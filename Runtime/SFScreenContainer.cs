using System;
using SFramework.Core.Runtime;
using UnityEngine;
using UnityEngine.Serialization;

namespace SFramework.UI.Runtime
{
    [Serializable]
    public sealed class SFScreenContainer : SFDatabaseNode
    {
        
        [SerializeField]
        private SFWidgetContainer[] _widgetContainers;
        
        public override ISFDatabaseNode[] Children => _widgetContainers;
    }
}