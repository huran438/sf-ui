using System;
using SFramework.Core.Runtime;

namespace SFramework.UI.Runtime
{
    public sealed class SFWidgetAttribute : SFTypeAttribute
    {
        public SFWidgetAttribute() : base(typeof(SFUIDatabase))
        {
            
        }
    }
}