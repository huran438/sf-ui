using System;
using SFramework.Core.Runtime;
using UnityEngine;

namespace SFramework.UI.Runtime
{
    [Serializable]
    public sealed class SFScreenContainer : SFDatabaseNode
    {

        [SerializeField]
        private SFWidgetContainer[] _buttonContainers;


        public override ISFDatabaseNode[] Children => _buttonContainers;
    }
}