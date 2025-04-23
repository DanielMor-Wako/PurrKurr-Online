using Code.Wakoz.PurrKurr.DataClasses.Enums;
using Code.Wakoz.PurrKurr.Screens.CameraSystem;
using Code.Wakoz.PurrKurr.Screens.Gameplay_Controller;
using System.Collections.Generic;
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
            GetController<GameplayController>().UnlockHeroAbilities(null);
            ShowMenu(false);
        }

        private void HandleEventsClicked() {
            GetController<GameplayController>().UnlockHeroAbilities(new List<Definitions.ActionType>() { Definitions.ActionType.Jump, Definitions.ActionType.Projectile, Definitions.ActionType.Grab, Definitions.ActionType.Attack, Definitions.ActionType.Special, Definitions.ActionType.Block, Definitions.ActionType.Rope});
            ShowMenu(false);
        }

        private void HandlePlayArenaClicked() {

        }

        private void HandlePlayHuntClicked() {

        }

    }
}
