using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using SFramework.Configs.Runtime;
using SFramework.UI.Runtime;
using UnityEngine;
using UnityEngine.EventSystems;
using Object = UnityEngine.Object;

namespace SFramework.UI.Editor.Tests
{
    public sealed class SFUIServiceTests
    {
        private const string ScreenId = "ui/main/inventory";
        private const string WidgetId = ScreenId + "/item";
        private SFUIService _service;
        private readonly List<GameObject> _objects = new();

        [SetUp]
        public void SetUp()
        {
            var config = new SFUIConfig
            {
                Id = "ui",
                Groups = new[]
                {
                    new SFScreenGroupNode
                    {
                        Id = "main",
                        Screens = new[]
                        {
                            new SFScreenNode
                            {
                                Id = "inventory", Preload = false,
                                Widgets = new[] { new SFWidgetNode { Id = "item" } }
                            }
                        }
                    }
                }
            };
            config.BuildTree();
            _service = (SFUIService)Activator.CreateInstance(typeof(SFUIService),
                BindingFlags.Instance | BindingFlags.NonPublic, null,
                new object[] { new MemoryConfigsService(config) }, null);
            var parent = (Transform)typeof(SFUIService)
                .GetField("_parentTransform", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(_service);
            _objects.Add(parent.gameObject);
            _service.Init(CancellationToken.None).GetAwaiter().GetResult();
        }

        [TearDown]
        public void TearDown()
        {
            foreach (var model in _service.WidgetModels) model.Dispose();
            _service.Dispose();
            foreach (var gameObject in _objects)
                if (gameObject != null) Object.DestroyImmediate(gameObject);
            _objects.Clear();
        }

        [Test]
        public void Init_BuildsScreenAndWidgetModelsWithParentRelationship()
        {
            Assert.That(_service.TryGetScreenModel(ScreenId, out var screen), Is.True);
            Assert.That(_service.TryGetWidgetModel(WidgetId, out var widget), Is.True);
            Assert.That(_service.TryGetWidgetNode(WidgetId, out var node), Is.True);
            Assert.That(widget.Node, Is.SameAs(node));
            Assert.That(node.Parent, Is.SameAs(screen.Node));
            Assert.That(screen.State, Is.EqualTo(SFScreenState.Closed));
            Assert.That(screen.IsLoaded, Is.False);
            Assert.That(_service.ScreenModels, Has.Length.EqualTo(1));
            Assert.That(_service.WidgetModels, Has.Length.EqualTo(1));
        }

        [Test]
        public void Dispose_DestroysTheServiceRootAndCanBeRepeated()
        {
            var root = _objects[0];

            _service.Dispose();
            _service.Dispose();

            Assert.That(root == null, Is.True);
        }

        [Test]
        public void ParameterLookup_MatchesExactKeysAndPreservesTheRestOfTheValue()
        {
            var parameters = new[] { null, "showDetails=true", "show", "url=a=b", "empty=" };

            Assert.That(parameters.HasParameter("show"), Is.True);
            Assert.That(parameters.HasParameter("sho"), Is.False);
            Assert.That(parameters.TryGetParameterValue("show", out _), Is.False);
            Assert.That(parameters.TryGetParameterValue("url", out var url), Is.True);
            Assert.That(url, Is.EqualTo("a=b"));
            Assert.That(parameters.TryGetParameterValue("empty", out var empty), Is.True);
            Assert.That(empty, Is.Empty);
        }

        [Test]
        public void ShowScreen_WhilePreloading_WaitsAndSendsOneShowRequest()
        {
            var (load, completion) = AddPendingLoad();
            var calls = 0;
            _service.OnShowScreen += (_, _, _) => calls++;

            var first = _service.ShowScreen(ScreenId, false);
            var second = _service.ShowScreen(ScreenId, true);
            Assert.That(calls, Is.Zero);

            var view = AddView<QuietScreenView>();
            _service.RegisterScreen(ScreenId, view);
            load.GetType().GetField("View").SetValue(load, view);
            load.GetType().GetField("Loading").SetValue(load, false);
            completion.TrySetResult();
            first.GetAwaiter().GetResult();
            second.GetAwaiter().GetResult();

            Assert.That(calls, Is.EqualTo(1));
        }

        [Test]
        public void ShowScreen_CanceledWaiterDoesNotCancelAnotherRequest()
        {
            using var cancellation = new CancellationTokenSource();
            var (load, completion) = AddPendingLoad();
            var calls = 0;
            _service.OnShowScreen += (_, _, _) => calls++;

            var canceled = _service.ShowScreen(ScreenId, false, cancellationToken: cancellation.Token);
            var remaining = _service.ShowScreen(ScreenId, false);
            cancellation.Cancel();
            Assert.That(() => canceled.GetAwaiter().GetResult(), Throws.TypeOf<OperationCanceledException>());

            var view = AddView<QuietScreenView>();
            _service.RegisterScreen(ScreenId, view);
            load.GetType().GetField("View").SetValue(load, view);
            load.GetType().GetField("Loading").SetValue(load, false);
            completion.TrySetResult();
            remaining.GetAwaiter().GetResult();

            Assert.That(calls, Is.EqualTo(1));
        }

        [Test]
        public void UnregisterScreen_RemovesTheLoadOwnedByThatView()
        {
            var (load, completion) = AddPendingLoad();
            var view = AddView<QuietScreenView>();
            _service.RegisterScreen(ScreenId, view);
            load.GetType().GetField("View").SetValue(load, view);
            load.GetType().GetField("Loading").SetValue(load, false);
            completion.TrySetResult();

            _service.UnregisterScreen(ScreenId);

            Assert.That(ScreenLoads.Contains(ScreenId), Is.False);
            Assert.That(_service.TryGetScreenView(ScreenId, out _), Is.False);
        }

        private IDictionary ScreenLoads => (IDictionary)typeof(SFUIService)
            .GetField("_screenLoads", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(_service);

        private (object Load, UniTaskCompletionSource Completion) AddPendingLoad()
        {
            var serviceType = typeof(SFUIService);
            var load = Activator.CreateInstance(serviceType.GetNestedType("ScreenLoad", BindingFlags.NonPublic), true);
            ScreenLoads.Add(ScreenId, load);
            return (load, (UniTaskCompletionSource)load.GetType().GetField("Completion").GetValue(load));
        }

        [Test]
        public void RegisterAndUnregisterScreen_UpdatesViewLookupAndLoadedState()
        {
            var view = AddView<QuietScreenView>();
            _service.TryGetScreenModel(ScreenId, out var model);

            _service.RegisterScreen(ScreenId, view);

            Assert.That(_service.TryGetScreenView(ScreenId, out SFScreenView registered), Is.True);
            Assert.That(registered, Is.SameAs(view));
            Assert.That(model.IsLoaded, Is.True);
            _service.ScreenShownCallback(ScreenId);

            _service.UnregisterScreen(ScreenId);

            Assert.That(_service.TryGetScreenView(ScreenId, out SFScreenView removed), Is.False);
            Assert.That(removed, Is.Null);
            Assert.That(model.IsLoaded, Is.False);
            Assert.That(model.State, Is.EqualTo(SFScreenState.Closed));
        }

        [Test]
        public void ScreenShownCallback_UpdatesStateBeforeEventAndSuppressesDuplicateCallback()
        {
            var events = new List<string>();
            _service.TryGetScreenModel(ScreenId, out var model);
            _service.OnScreenShown += id =>
            {
                Assert.That(model.State, Is.EqualTo(SFScreenState.Shown));
                events.Add(id);
            };

            _service.ScreenShownCallback(ScreenId);
            _service.ScreenShownCallback(ScreenId);

            Assert.That(events, Is.EqualTo(new[] { ScreenId }));
        }

        [Test]
        public void CloseScreenAndClosedCallback_EmitOrderedTransitionsOnlyOnce()
        {
            var events = new List<string>();
            _service.TryGetScreenModel(ScreenId, out var model);
            _service.ScreenShownCallback(ScreenId);
            _service.OnCloseScreen += (id, force, unload) =>
            {
                Assert.That(id, Is.EqualTo(ScreenId));
                Assert.That(force, Is.True);
                Assert.That(unload, Is.False);
                Assert.That(model.State, Is.EqualTo(SFScreenState.Close));
                events.Add("close");
            };
            _service.OnScreenClosed += id =>
            {
                Assert.That(id, Is.EqualTo(ScreenId));
                Assert.That(model.State, Is.EqualTo(SFScreenState.Closed));
                events.Add("closed");
            };

            _service.CloseScreen(ScreenId, true, false);
            _service.CloseScreen(ScreenId, true, false);
            _service.ScreenClosedCallback(ScreenId, false);
            _service.ScreenClosedCallback(ScreenId, false);

            Assert.That(events, Is.EqualTo(new[] { "close", "closed" }));
        }

        [Test]
        public void CloseScreen_AlreadyClosed_DoesNotRequestAnotherClose()
        {
            var calls = 0;
            _service.OnCloseScreen += (_, _, _) => calls++;

            _service.CloseScreen(ScreenId, false, false);

            Assert.That(calls, Is.Zero);
            _service.TryGetScreenModel(ScreenId, out var model);
            Assert.That(model.State, Is.EqualTo(SFScreenState.Closed));
        }

        [Test]
        public void ShowScreen_AlreadyShown_DoesNotLoadOrEmitAnotherShowRequest()
        {
            var calls = 0;
            _service.OnShowScreen += (_, _, _) => calls++;
            _service.ScreenShownCallback(ScreenId);

            _service.ShowScreen(ScreenId, true, new object[] { "parameter" }).GetAwaiter().GetResult();

            Assert.That(calls, Is.Zero);
            _service.TryGetScreenModel(ScreenId, out var model);
            Assert.That(model.State, Is.EqualTo(SFScreenState.Shown));
        }

        [Test]
        public void UnknownScreenCallbacks_DoNotAffectKnownScreenOrEmitEvents()
        {
            var calls = 0;
            _service.OnScreenShown += _ => calls++;
            _service.OnScreenClosed += _ => calls++;
            _service.OnCloseScreen += (_, _, _) => calls++;

            _service.ScreenShownCallback("missing");
            _service.ScreenClosedCallback("missing", false);
            _service.CloseScreen("missing", true, false);
            _service.ShowScreen("missing", false).GetAwaiter().GetResult();

            Assert.That(calls, Is.Zero);
            _service.TryGetScreenModel(ScreenId, out var model);
            Assert.That(model.State, Is.EqualTo(SFScreenState.Closed));
        }

        [Test]
        public void RegisterWidgets_PreservesViewOrderInLookupAndModel()
        {
            var first = AddView<QuietWidgetView>();
            var second = AddView<QuietWidgetView>();
            _service.RegisterWidget(WidgetId, first);
            _service.RegisterWidget(WidgetId, second);

            Assert.That(_service.TryGetWidgetView(WidgetId, 1, out SFWidgetView found), Is.True);
            Assert.That(found, Is.SameAs(second));
            _service.TryGetWidgetModel(WidgetId, out var model);
            Assert.That(model.Views, Is.EqualTo(new SFWidgetView[] { first, second }));
        }

        [TestCase(-10, 0)]
        [TestCase(-1, 0)]
        [TestCase(0, 0)]
        [TestCase(1, 1)]
        [TestCase(2, 1)]
        [TestCase(100, 1)]
        public void TryGetWidgetView_ClampsIndexToRegisteredViewRange(int index, int expectedIndex)
        {
            var views = new SFWidgetView[] { AddView<QuietWidgetView>(), AddView<QuietWidgetView>() };
            foreach (var view in views) _service.RegisterWidget(WidgetId, view);

            Assert.That(_service.TryGetWidgetView(WidgetId, index, out SFWidgetView found), Is.True);
            Assert.That(found, Is.SameAs(views[expectedIndex]));
        }

        [Test]
        public void UnregisterWidget_PreservesOtherViewInLookupAndModel()
        {
            var first = AddView<QuietWidgetView>();
            var second = AddView<QuietWidgetView>();
            _service.RegisterWidget(WidgetId, first);
            _service.RegisterWidget(WidgetId, second);

            _service.UnregisterWidget(WidgetId, first);

            Assert.That(_service.TryGetWidgetView(WidgetId, 0, out SFWidgetView remaining), Is.True);
            Assert.That(remaining, Is.SameAs(second));
            _service.TryGetWidgetModel(WidgetId, out var model);
            Assert.That(model.Views, Is.EqualTo(new SFWidgetView[] { second }));
        }

        [Test]
        public void UnregisterLastWidget_MakesViewUnavailableAndEmptiesModel()
        {
            var view = AddView<QuietWidgetView>();
            _service.RegisterWidget(WidgetId, view);

            _service.UnregisterWidget(WidgetId, view);

            Assert.That(_service.TryGetWidgetView(WidgetId, 0, out SFWidgetView remaining), Is.False);
            Assert.That(remaining, Is.Null);
            _service.TryGetWidgetModel(WidgetId, out var model);
            Assert.That(model.Views, Is.Empty);
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("missing")]
        public void TryGetWidgetView_UnregisteredId_ReturnsFalseAndNull(string id)
        {
            Assert.That(_service.TryGetWidgetView(id, 0, out SFWidgetView view), Is.False);
            Assert.That(view, Is.Null);
        }

        [Test]
        public void WidgetModel_UnregistersSpecificViewAndDisposeClearsTheRest()
        {
            _service.TryGetWidgetModel(WidgetId, out var model);
            var first = AddView<QuietWidgetView>();
            var second = AddView<QuietWidgetView>();
            model.RegisterView(first);
            model.RegisterView(second);

            model.UnregisterView(first);

            Assert.That(model.Views, Is.EqualTo(new SFWidgetView[] { second }));
            Assert.That(model.IsLoadedViewByIndex(-1), Is.True);
            Assert.That(model.IsLoadedViewByIndex(1), Is.False);

            model.Dispose();

            Assert.That(model.Views, Is.Empty);
            Assert.That(model.IsLoadedViewByIndex(0), Is.False);
        }

        [Test]
        public void WidgetBaseEventCallback_ForwardsIdTypeAndOriginalEventData()
        {
            var data = new BaseEventData(null);
            var calls = 0;
            _service.OnWidgetBaseEvent += (id, type, forwarded) =>
            {
                calls++;
                Assert.That(id, Is.EqualTo(WidgetId));
                Assert.That(type, Is.EqualTo(SFBaseEventType.Submit));
                Assert.That(forwarded, Is.SameAs(data));
            };

            _service.WidgetEventCallback(WidgetId, SFBaseEventType.Submit, data);

            Assert.That(calls, Is.EqualTo(1));
        }

        [Test]
        public void WidgetPointerEventCallback_ForwardsIdIndexTypeAndOriginalEventData()
        {
            var data = new PointerEventData(null) { position = new Vector2(12f, 34f) };
            var calls = 0;
            _service.OnWidgetPointerEvent += (id, index, type, forwarded) =>
            {
                calls++;
                Assert.That(id, Is.EqualTo(WidgetId));
                Assert.That(index, Is.EqualTo(3));
                Assert.That(type, Is.EqualTo(SFPointerEventType.Drag));
                Assert.That(forwarded, Is.SameAs(data));
            };

            _service.WidgetEventCallback(WidgetId, 3, SFPointerEventType.Drag, data);

            Assert.That(calls, Is.EqualTo(1));
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("  ")]
        public void WidgetEventCallbacks_BlankId_DoNotEmitEvents(string id)
        {
            var calls = 0;
            _service.OnWidgetBaseEvent += (_, _, _) => calls++;
            _service.OnWidgetPointerEvent += (_, _, _, _) => calls++;

            _service.WidgetEventCallback(id, SFBaseEventType.Submit, new BaseEventData(null));
            _service.WidgetEventCallback(id, 0, SFPointerEventType.Click, new PointerEventData(null));

            Assert.That(calls, Is.Zero);
        }

        private T AddView<T>() where T : Component
        {
            var gameObject = new GameObject(typeof(T).Name, typeof(RectTransform));
            gameObject.SetActive(false);
            _objects.Add(gameObject);
            return gameObject.AddComponent<T>();
        }

        private sealed class MemoryConfigsService : ISFConfigsService
        {
            private readonly ISFConfig[] _configs;
            public MemoryConfigsService(params ISFConfig[] configs) => _configs = configs;
            public IEnumerable<ISFConfig> Configs => _configs;
            public bool TryGetConfigs<T>(out T[] configs) where T : ISFConfig
            {
                configs = _configs.OfType<T>().ToArray();
                return configs.Length != 0;
            }
            public IEnumerable<T> GetConfigs<T>() where T : ISFConfig => _configs.OfType<T>();
            public bool TryGetGlobalConfig<T>(out T config) where T : ISFGlobalConfig
            {
                config = default;
                return false;
            }
            public UniTask Init(CancellationToken cancellationToken) => UniTask.CompletedTask;
            public void Dispose() { }
        }
    }

    // Registration tests manage lifecycle explicitly; no container or scene callbacks are needed.
    public sealed class QuietWidgetView : SFWidgetView
    {
        protected override void Awake() { }
        protected override void Start() { }
        protected override void OnDestroy() { }
    }

    public sealed class QuietScreenView : SFScreenView
    {
        protected override void Awake() { }
        protected override void OnShowScreen(bool force, object[] parameters) { }
        protected override void OnScreenShown() { }
        protected override void OnCloseScreen(bool force, bool unload) { }
        protected override void OnScreenClosed() { }
        protected override void OnScreenDestroy() { }
        protected override void OnDestroy() { }
    }
}
