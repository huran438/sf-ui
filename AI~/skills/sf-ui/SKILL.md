---
name: sf-ui
description: Use when adding SFramework screens, widgets, inventory/menu UI, screen transitions, or UI EventSystem routing.
---

# sf-ui

Read [the package guide](references/package-readme.md) and [installed source](SOURCE.md) for complete screen/view examples and overloads.

Register ISFConfigsService before ISFUIService and open screens after service initialization. Add the screen to SFUIConfig, set its Addressable prefab key, and match serialized FullId values on screen and widget views. Ensure the screen view is active in the prefab so it can register after loading.

Provide a concrete SFScreenView and implement all abstract callbacks. The subclass controls canvas visibility and input. Complete transitions through ScreenShownCallback and ScreenClosedCallback; awaiting ShowScreen does not wait for the shown animation callback.

Use the actual ShowScreen/LoadScreen signatures, including required force/show booleans and the cancellation overload when needed. CloseScreen requires force and unload. SFUICloseBehaviour metadata is not applied automatically by the current service.

Widget IDs may have multiple views. Filter OnWidgetPointerEvent by full ID, index, and event type; unsubscribe host-owned listeners at shutdown. Preserve base screen/widget lifecycle cleanup and include an EventSystem.

SFWidgetView is abstract. Attach a concrete subclass, which may be empty, to the close-widget GameObject. For pointer input, provide a raycastable UI Graphic and ensure the screen's GraphicRaycaster and canvas input settings permit events.

Concurrent requests for the same screen share one load. A canceled request stops waiting independently. Failed loads can be retried. Dispose releases owned screens, cancels pending requests, and destroys the SFUI parent. Pending prefabs stay inactive until their Addressables operations settle.

Validate config IDs, Addressables setup, show/close completion, input and unloading in the consuming project's test scene. Run package tests for service changes.
