using System;
using SFramework.Core.Runtime;
using SFramework.Repositories.Runtime;

namespace SFramework.UI.Runtime
{
    public sealed class SFScreenAttribute : SFIdAttribute
    {
        public SFScreenAttribute() : base( typeof(SFUIRepository),2)
        {
        }
    }
}