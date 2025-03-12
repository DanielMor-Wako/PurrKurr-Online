namespace Code.Wakoz.PurrKurr.Screens.Gameplay_Controller.Handlers
{
    // todo: perhaps remove IDisposible and just not ref the gameEvents
    public interface IBindableHandler : IHandler
    {
        void Bind();
        void Unbind();
    }

}