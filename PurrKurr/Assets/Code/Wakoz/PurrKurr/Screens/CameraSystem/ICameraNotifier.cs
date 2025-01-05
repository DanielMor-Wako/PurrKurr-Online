namespace Code.Wakoz.PurrKurr.Screens.CameraSystem
{
    public interface ICameraNotifier
    {
        void NotifyCameraChange(BaseCamera camera);
    }
    public interface IBindable
    {
        // todo: move thos class to more common namespace
        void Bind();
        void Unbind();
    }
}
