using SFramework.Configs.Runtime;

namespace SFramework.UI.Runtime
{
    public sealed class SFUIConfig : SFNodesConfig
    {
        public SFScreenGroupNode[] Groups;

        public override ISFConfigNode[] Children => Groups;
    }
}