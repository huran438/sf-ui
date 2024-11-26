namespace SFramework.UI.Runtime
{
    public enum SFScreenState : byte
    {
        Show = 1 << 0,
        Shown = 1 << 1,
        Close = 1 << 2,
        Closed = 1 << 3
    }
}