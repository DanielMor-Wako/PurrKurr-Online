using Code.Core;
using Code.Core.Auth;
using Code.Wakoz.PurrKurr.Screens.Gameplay_Controller;
using Code.Wakoz.PurrKurr.Screens.SceneTransition;
using System;
using System.Threading;
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

        private CancellationTokenSource _loadPlayerDataCTS;

        protected override void Clean() {

            _view.OnAccountClicked -= HandleAccountClicked;

            _view.OnPlayCampaignClicked -= HandlePlayCampaignClicked;
            _view.OnPlayEventsClicked -= HandleEventsClicked;
            _view.OnPlayArenaClicked -= HandlePlayArenaClicked;
            _view.OnPlayHuntClicked -= HandlePlayHuntClicked;

            _authService?.Dispose();
            _authService = null;

            CancelLoadingPlayerData();
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

            _model.SetButtonsAvailability(_gameManager.DataProgressInPercent == 1, true);
            _model.SetDisplayName(_gameManager.DisplayName != "" ? _gameManager.DisplayName : "New Player");
            _view.UpdateLoadingBarProgress(0);

            if (_gameManager.DataProgressInPercent < 1) {
                var hasCache = _gameManager.TryGetCacheFilePath(out var _);
                _view.UpdateLoadingBarProgress(.01f, $"{Mathf.CeilToInt(_view.GetLoadingBarProgress() * 100)}%");
                
                await _gameManager.WaitUntilLoadComplete(() => UpdateProgress());
                await WaitUntilFullProgressBar(() => UpdateProgress());

                _view.UpdateLoadingBarProgress(1, "<b><color=#FFFFFF;\"><b>Ready</b></color>");
                await Task.Delay(TimeSpan.FromSeconds(.75f));
                _view.UpdateLoadingBarProgress(0);

                _model.SetButtonsAvailability(true, true);
                _model.SetDisplayName(_gameManager.DisplayName);
            }
        }
        
        public async Task WaitUntilFullProgressBar(Action onCallback = null) {

            if (_view.GetLoadingBarProgress() < 1) {

                _loadPlayerDataCTS ??= new CancellationTokenSource();
                await WaitUntil(() => Mathf.CeilToInt(_view.GetLoadingBarProgress() * 100) == 100, onCallback, _loadPlayerDataCTS.Token);
            }
        }

        private async Task WaitUntil(Func<bool> condition, Action onCallback = null, CancellationToken cancellationToken = default) {

            while (!condition() && !cancellationToken.IsCancellationRequested) {
                await Task.Delay(TimeSpan.FromSeconds(0.1f), cancellationToken);
                onCallback?.Invoke();
            }
        }

        private void UpdateProgress() {
            _view.UpdateLoadingBarProgress(_gameManager.DataProgressInPercent, $"{Mathf.CeilToInt(_view.GetLoadingBarProgress() * 100)}%");
        }
        
        private void CancelLoadingPlayerData() {
            _loadPlayerDataCTS?.Cancel();
            _loadPlayerDataCTS = null;
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

            // Todo: Mode Specific Strategy
            controller.UnlockCharacterAbilitiesByObjectives();
            //controller.UnlockAllAbilities();

            ShowMenu(false);
        }

        private void HandleEventsClicked() {

            var controller = GetController<GameplayController>();
            if (controller == null || !controller.IsGameRunning()) {
                Debug.LogWarning("Cant start when game is not running");
                return;
            }

            var hasCompletedTutorial = controller.Handlers.GetHandler<GameStateHandler>().HasCompletedTutorial();
            if (!hasCompletedTutorial) {
                Debug.Log("Locked: You must completed Campaign");
                return;
            }

            Debug.Log("Events is currently offline");
            return;

            ShowMenu(false);
        }

        private void HandlePlayArenaClicked() {

        }

        private void HandlePlayHuntClicked() {

        }

    }
}
