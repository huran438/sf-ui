using System;
using SFramework.Core.Runtime;
using SFramework.Repositories.Runtime;

namespace SFramework.UI.Runtime
{
    public sealed class SFWidgetAttribute : SFIdAttribute
    {
        public SFWidgetAttribute() : base( typeof(SFUIRepository), 3)
        {
        }
    }
}