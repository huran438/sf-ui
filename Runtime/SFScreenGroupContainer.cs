using System;
using SFramework.Core.Runtime;
using UnityEngine;

namespace SFramework.UI.Runtime
{
    [Serializable]
    public sealed class SFScreenGroupContainer : SFDatabaseNode
    {
        [SerializeField]
        private SFScreenContainer[] _screenContainers;

        public override ISFDatabaseNode[] Children => _screenContainers;
    }
}