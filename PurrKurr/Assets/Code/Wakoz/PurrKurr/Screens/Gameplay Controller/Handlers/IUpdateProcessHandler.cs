using System;

namespace Code.Wakoz.PurrKurr.Screens.Gameplay_Controller.Handlers
{
    public interface IUpdateProcessHandler : IHandler
    {
        void UpdateProcess(float deltaTime = 0);
    }

}