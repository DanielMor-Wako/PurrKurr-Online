using Code.Wakoz.PurrKurr.Views;
using System;
using UnityEngine;

namespace Code.Wakoz.PurrKurr.Screens.MainMenu
{
    public class MainMenuView : View<MainMenuModel>
    {
        [SerializeField] CanvasGroupFaderView _fader;

        public event Action OnAccountClicked;
        public event Action OnPlayCampaignClicked;
        public event Action OnPlayEventsClicked;
        public event Action OnPlayArenaClicked;
        public event Action OnPlayHuntClicked;

        public void ClickedAccount() => OnAccountClicked?.Invoke();
        public void ClickedPlayCampaign() => OnPlayCampaignClicked?.Invoke();
        public void ClickedPlayEvents() => OnPlayEventsClicked?.Invoke();
        public void ClickedPlayArena() => OnPlayArenaClicked?.Invoke();
        public void ClickedPlayHunt() => OnPlayHuntClicked?.Invoke();

        protected override void ModelChanged() {

            _fader?.EndTransition(Convert.ToInt32(Model.IsOpen));
        }

    }
}
