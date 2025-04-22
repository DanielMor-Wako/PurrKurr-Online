using System.Threading.Tasks;
using UnityEngine;

namespace Code.Wakoz.PurrKurr.Screens.MainMenu
{
    [DefaultExecutionOrder(11)]
    public class MainMenuController : SingleController
    {

        [SerializeField] private MainMenuView _view;

        private MainMenuModel _model;

        protected override void Clean() {
            _view.OnPlayCampaignClicked -= HandlePlayCampaignClicked;
            _view.OnPlayEventsClicked -= HandleEventsClicked;
            _view.OnPlayArenaClicked -= HandlePlayArenaClicked;
            _view.OnPlayHuntClicked -= HandlePlayHuntClicked;
        }

        protected override Task Initialize() {

            _model = new MainMenuModel();

            _view.OnPlayCampaignClicked += HandlePlayCampaignClicked;
            _view.OnPlayEventsClicked += HandleEventsClicked;
            _view.OnPlayArenaClicked += HandlePlayArenaClicked;
            _view.OnPlayHuntClicked += HandlePlayHuntClicked;

            _view.SetModel(_model);

            return Task.CompletedTask;
        }

        public void ShowMenu(bool isOpen) {
            _model.SetOpenState(isOpen);
        }

        private void HandlePlayCampaignClicked() {
            ShowMenu(false);
        }

        private void HandlePlayArenaClicked() {

        }

        private void HandlePlayHuntClicked() {

        }

        private void HandleEventsClicked() {
            
        }

    }
}
