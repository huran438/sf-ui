using System.Collections.Generic;
using SFramework.Configs.Runtime;
using SFramework.Core.Runtime;

namespace SFramework.UI.Runtime
{
    public sealed class SFUIConfig : SFConfig, ISFConfigsGenerator
    {
        public SFScreenGroupNode[] Groups;

        public override ISFConfigNode[] Nodes => Groups;

        public void GetGenerationData(out SFGenerationData[] generationData)
        {
            var screens = new HashSet<string>();

            foreach (var layer0 in Groups)
            {
                foreach (var layer1 in layer0.Screens)
                {
                    screens.Add($"{Name}/{layer0.Name}/{layer1.Name}");
                }
            }

            var widgets = new HashSet<string>();

            foreach (var layer0 in Groups)
            {
                foreach (var layer1 in layer0.Screens)
                {
                    foreach (var layer2 in layer1.Widgets)
                    {
                        widgets.Add($"{Name}/{layer0.Name}/{layer1.Name}/{layer2.Name}");
                    }
                }
            }

            generationData = new[]
            {
                new SFGenerationData
                {
                    FileName = "SFScreens",
                    Properties = screens
                },
                new SFGenerationData
                {
                    FileName = "SFWidgets",
                    Properties = widgets
                }
            };
        }
    }
}