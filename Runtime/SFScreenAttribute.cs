using System;
using SFramework.Core.Runtime;

namespace SFramework.UI.Runtime
{
    public sealed class SFScreenAttribute : SFTypeAttribute
    {
        public SFScreenAttribute() : base(typeof(SFUIDatabase), 2)
        {
        }
    }
}