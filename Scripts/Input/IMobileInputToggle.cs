namespace Insthync.CameraAndInput
{
    public interface IMobileInputToggle
    {
        string ToggleGroupName { get; }
        bool IsToggled { get; }
        void Toggle();
        void UnToggle();
    }
}
