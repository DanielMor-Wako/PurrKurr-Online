using Code.Core;
using Code.Core.Auth;
using Code.Core.Services.DataManagement;
using Code.Wakoz.PurrKurr.DataClasses.Enums;
using Code.Wakoz.PurrKurr.Screens.Gameplay_Controller;
using Code.Wakoz.PurrKurr.Screens.SceneTransition;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Code.Wakoz.PurrKurr.Screens.MainMenu
{
    [DefaultExecutionOrder(11)]
    public class MainMenuController : SingleController {

        [SerializeField] private MainMenuView _view;

        private MainMenuModel _model;
        private AuthenticationManager _authService;

        private GameManager _gameManager;

        protected override void Clean() {

            _view.OnAccountClicked -= HandleAccountClicked;

            _view.OnPlayCampaignClicked -= HandlePlayCampaignClicked;
            _view.OnPlayEventsClicked -= HandleEventsClicked;
            _view.OnPlayArenaClicked -= HandlePlayArenaClicked;
            _view.OnPlayHuntClicked -= HandlePlayHuntClicked;

            _authService?.Dispose();
            _authService = null;
        }

        protected override Task Initialize() {

            _model = new MainMenuModel();

            _view.OnAccountClicked += HandleAccountClicked;

            _view.OnPlayCampaignClicked += HandlePlayCampaignClicked;
            _view.OnPlayEventsClicked += HandleEventsClicked;
            _view.OnPlayArenaClicked += HandlePlayArenaClicked;
            _view.OnPlayHuntClicked += HandlePlayHuntClicked;

            _view.SetModel(_model);

            WaitForPlayerInfoAndUpdateView();

            return Task.CompletedTask;
        }

        private async void WaitForPlayerInfoAndUpdateView() {

            _gameManager ??= GetController<GameManager>();
            if (_gameManager == null) {
                return;
            }

            _model.SetButtonsAvailability(false, true);
            _model.SetDisplayName(_gameManager.DisplayName);
            _view.UpdateLoadingBarProgress(0);

            if (_gameManager.DataProgressInPercent < 1) {
                _view.UpdateLoadingBarProgress(0.01f);
                await _gameManager.WaitUntilLoadComplete(() => UpdateLoadingProgress());
                _view.UpdateLoadingBarProgress(1, "Ready");
                await Task.Delay(TimeSpan.FromSeconds(1.5f));
                _view.UpdateLoadingBarProgress(0);
            }

            _model.SetButtonsAvailability(true);
            /*if (_gameManager.DisplayName != _model.PlayerDisplayName) {
                _model.SetDisplayName(_gameManager.DisplayName);
            } else {
                Debug.LogWarning("Seems that displayName is already set , if reocurring consider erasing the second update and not wait for load complete");
            }*/

        }

        private void UpdateLoadingProgress() {
            _view.UpdateLoadingBarProgress(_gameManager.DataProgressInPercent, $"{Mathf.CeilToInt(_gameManager.DataProgressInPercent * 100)}% Loaded");
        }

        private void HandleAccountClicked() {
            var controller = GetController<GameplayController>();
            if (controller == null || !controller.IsGameRunning()) {
                Debug.LogWarning("Cant start when game is not running");
                return;
            }
            GetController<SceneTransitionController>().LoadSceneByIndex(2, "Profile");
        }

        public void ShowMenu(bool isOpen) {
            _model.ChangeState(isOpen);
        }

        private void HandlePlayCampaignClicked() {
            
            var controller = GetController<GameplayController>();
            if (controller == null || !controller.IsGameRunning()) {
                Debug.LogWarning("Cant start when game is not running");
                return;
            }
            // todo: use objectivesData to set up the player abilities
            List <Definitions.ActionType> abilities = controller.Handlers.GetHandler<GameStateHandler>().GetCharacterAbilitiesFromCollectedObjectives();
            //UnityEngine.Debug.Log("Player abilities retrieved -> " + string.Join(",", abilities));
            controller.SetUnlockedAbilities(abilities);

            ShowMenu(false);
        }

        private void HandleEventsClicked() {

            var controller = GetController<GameplayController>();
            if (controller == null || !controller.IsGameRunning()) {
                Debug.LogWarning("Cant start when game is not running");
                return;
            }

            controller.SetUnlockedAbilities(new List<Definitions.ActionType>() { Definitions.ActionType.Jump, Definitions.ActionType.Projectile, Definitions.ActionType.Grab, Definitions.ActionType.Attack, Definitions.ActionType.Special, Definitions.ActionType.Block, Definitions.ActionType.Rope });
            //controller.SetUnlockedAbilities(null);

            ShowMenu(false);
        }

        private void HandlePlayArenaClicked() {

        }

        private void HandlePlayHuntClicked() {

        }

    }
}
