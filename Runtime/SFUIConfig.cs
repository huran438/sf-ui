using System.Collections.Generic;
using SFramework.Configs.Runtime;
using SFramework.Core.Runtime;

namespace SFramework.UI.Runtime
{
    public sealed class SFUIConfig : SFConfig, ISFConfigsGenerator
    {
        public SFScreenGroupNode[] Groups;

        public override ISFConfigNode[] Children => Groups;

        public void GetGenerationData(out SFGenerationData[] generationData)
        {
            var screens = new HashSet<string>();

            foreach (var layer0 in Groups)
            {
                foreach (var layer1 in layer0.Screens)
                {
                    screens.Add($"{Id}/{layer0.Id}/{layer1.Id}");
                }
            }

            var widgets = new HashSet<string>();

            foreach (var layer0 in Groups)
            {
                foreach (var layer1 in layer0.Screens)
                {
                    foreach (var layer2 in layer1.Widgets)
                    {
                        widgets.Add($"{Id}/{layer0.Id}/{layer1.Id}/{layer2.Id}");
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