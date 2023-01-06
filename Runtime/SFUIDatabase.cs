using SFramework.Core.Runtime;
using UnityEngine;
using UnityEngine.Serialization;

namespace SFramework.UI.Runtime
{
    [CreateAssetMenu(menuName = "SFramework/UI Database")]
    public sealed class SFUIDatabase : SFDatabase
    {
        [SerializeField]
        private SFScreenGroupContainer[] _screenGroupsContainers;

        public override string Title => "UI";
        public override ISFDatabaseNode[] Nodes => _screenGroupsContainers;

        // protected override void Generate(out SFGenerationData[] generationData)
        // {
        //     var screens = new Dictionary<string, string>();
        //
        //     foreach (var layer0 in screenGroupsContainers)
        //     {
        //         foreach (var layer1 in layer0.ScreenContainers)
        //         {
        //             screens[layer1._Id] = $"{layer0._Name}/{layer1._Name}";
        //         }
        //     }
        //
        //     var buttons = new Dictionary<string, string>();
        //
        //     foreach (var layer0 in screenGroupsContainers)
        //     {
        //         foreach (var layer1 in layer0.ScreenContainers)
        //         {
        //             foreach (var layer2 in layer1.ButtonContainers)
        //             {
        //                 buttons[layer2._Id] = $"{layer0._Name}/{layer1._Name}/{layer2._Name}";
        //             }
        //         }
        //     }
        //     
        //     var toggles = new Dictionary<string, string>();
        //
        //     foreach (var layer0 in screenGroupsContainers)
        //     {
        //         foreach (var layer1 in layer0.ScreenContainers)
        //         {
        //             foreach (var layer2 in layer1.ToggleContainers)
        //             {
        //                 toggles[layer2._Id] = $"{layer0._Name}/{layer1._Name}/{layer2._Name}";
        //             }
        //         }
        //     }
        //
        //     generationData = new[]
        //     {
        //         new SFGenerationData
        //         {
        //             FileName = "SFScreens",
        //             Usings = new[]
        //             {
        //                 "using SFramework.UI.Runtime;",
        //             },
        //             SFType = typeof(SFScreen),
        //             Properties = screens
        //         },
        //         new SFGenerationData
        //         {
        //             FileName = "SFButtons",
        //             Usings = new[]
        //             {
        //                 "using SFramework.UI.Runtime;",
        //             },
        //             SFType = typeof(SFButton),
        //             Properties = buttons
        //         },
        //         new SFGenerationData
        //         {
        //             FileName = "SFToggles",
        //             Usings = new[]
        //             {
        //                 "using SFramework.UI.Runtime;",
        //             },
        //             SFType = typeof(SFToggle),
        //             Properties = toggles
        //         }
        //     };
        // }
    }
}