namespace Code.Wakoz.PurrKurr.Screens.CameraSystem
{
    public interface ICameraCharacterMediator
    {
        void NotifyCameraChange(BaseCamera camera);
        void BindEvents();
        void UnbindEvents();
    }
}
