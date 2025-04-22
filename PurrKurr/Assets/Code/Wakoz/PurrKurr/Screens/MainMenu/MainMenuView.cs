using Code.Wakoz.PurrKurr.Views;
using System;
using UnityEngine;

namespace Code.Wakoz.PurrKurr.Screens.MainMenu
{
    public class MainMenuView : View<MainMenuModel>
    {
        [SerializeField] CanvasGroupFaderView _fader;

        public event Action OnPlayCampaignClicked;
        public event Action OnPlayEventsClicked;
        public event Action OnPlayArenaClicked;
        public event Action OnPlayHuntClicked;

        protected override void ModelChanged() {

            _fader?.EndTransition(Model.IsOpen ? 1 : 0);
        }

        public void ClickedPlayCampaign() => OnPlayCampaignClicked?.Invoke();
        public void ClickedPlayEvents() => OnPlayEventsClicked?.Invoke();
        public void ClickedPlayAreba() => OnPlayArenaClicked?.Invoke();
        public void ClickedPlayHunt() => OnPlayHuntClicked?.Invoke();
    }
}
