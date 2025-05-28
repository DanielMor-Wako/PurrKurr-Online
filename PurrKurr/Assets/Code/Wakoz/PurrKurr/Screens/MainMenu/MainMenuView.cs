using Code.Wakoz.PurrKurr.Views;
using System;
using TMPro;
using UnityEngine;

namespace Code.Wakoz.PurrKurr.Screens.MainMenu
{
    public class MainMenuView : View<MainMenuModel>
    {
        [SerializeField] CanvasGroupFaderView _fader;
        [SerializeField] private TMP_Text _playerNameText;

        [SerializeField] private MultiStateView _menuAvailableState;
        [SerializeField] private ImageFillerView _loadingProgressBar;
        [SerializeField] private TMP_Text _loadingProgressText;

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

        /// <summary>
        /// Update progress of text and image filler by the targetValue,
        /// value must be between 0 to 1, progress is visible when value is above 0
        /// </summary>
        /// <param name="targetValue"></param>
        public void UpdateLoadingBarProgress(float targetValue, string progressText = "") {

            _loadingProgressBar.gameObject.SetActive(targetValue > 0);

            if (targetValue <= 0)
                return;

            _loadingProgressBar?.StartTransition(targetValue);
            _loadingProgressText?.SetText(progressText);
        }

        protected override void ModelReplaced() {

            if (_loadingProgressBar == null) return;
            _loadingProgressBar.ImageTarget.fillAmount = 0;
        }

        protected override void ModelChanged() {

            _fader?.EndTransition(Convert.ToInt32(Model.IsOpen));
            UpdateDisplayName();
            UpdateButtonsAvailabeState();
        }

        private void UpdateDisplayName() {

            if (_playerNameText == null)
                return;

            _playerNameText?.SetText(Model.PlayerDisplayName);
        }

        private void UpdateButtonsAvailabeState() {

            if (_menuAvailableState == null) return;

            _menuAvailableState.ChangeState(Convert.ToInt32(Model.AreButtonsAvailable));
        }

    }
}
