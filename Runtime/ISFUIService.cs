using System;
using UnityEngine;

namespace SFramework.UI.Runtime
{
    public interface ISFUIService
    {
        bool IsLoaded { get; }
        event Action<string> OnShowScreen;
        event Action<string> OnCloseScreen;
        event Action<string> OnScreenShown;
        event Action<string> OnScreenClosed;
        void ShowScreen(string screen);
        void CloseScreen(string screen);
        SFScreenState GetScreenState(string screen);
        GameObject GetScreenRootGO(string screen);
        void Register(string screen, GameObject root);
        void Unregister(string screen);
        void ScreenShownCallback(string sfScreen);
        void ScreenClosedCallback(string sfScreen);
    }
}