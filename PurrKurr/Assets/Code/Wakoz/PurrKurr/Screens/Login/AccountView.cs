using UnityEngine;
using Code.Wakoz.PurrKurr.Views;
using System;
using TMPro;
using UnityEngine.UI;

namespace Code.Wakoz.PurrKurr.Screens.Login 
{
    public class AccountView : View<AccountModel> {

        public event Action OnPreviousClicked;

        public event Action OnChangePlayerNameClicked;
        public event Action OnLinkAccountClicked;
        public event Action OnUpdateCredentialsClicked;
        public event Action OnDisplayAllAccountInfo;

        public event Action OnLogOutClicked;
        public event Action OnLogOutConfirmClicked;
        public event Action OnLogOutCancelClicked;
        public event Action OnToggleForceDelete;

        public InputField UsernameInput;
        public InputField PasswordInput;
        public InputField ChangeNameInput;

        public MultiStateView _pageStateView;
        [SerializeField] private TMP_Text _playerNameText;
        [SerializeField] private TextMeshProUGUI _versionText;
        [SerializeField] private TextMeshProUGUI _playerInfoText;
        
        [Header("Change Name")]
        [SerializeField] private CanvasGroupFaderView _changeNameFader;
        [SerializeField] private RectTransformScalerView _changeNameScaler;
        [SerializeField] private Button _changeNameButton;

        [Header("Logout Window")]
        [SerializeField] private CanvasGroupFaderView _logoutWindowFader;
        [SerializeField] private TextMeshProUGUI _logoutWindowText;
        [SerializeField] private Toggle _forceDeleteToggle;

        [Header("Userfeed Window")]
        [SerializeField] private CanvasGroupFaderView _userFeedFader;
        [SerializeField] private RectTransformScalerView _userFeedScaler;
        [SerializeField] private TMP_Text _userFeedText;

        [Header("Credetials Form")]
        [SerializeField] private CanvasGroupFaderView _credetialsFormFader;
        [SerializeField] private RectTransformScalerView _credetialsFormScaler;


        protected override void ModelReplaced() {

            if (_userFeedFader != null) {
                _userFeedFader.CanvasTarget.alpha = 0;
            }

            if (_userFeedScaler != null) {
                _userFeedScaler.TargetRectTransform.localScale = new Vector3(0, 0, 1);
            }

            if (_credetialsFormFader != null) {
                _credetialsFormFader.CanvasTarget.alpha = 0;
            }

            if (_credetialsFormScaler != null) {
                _credetialsFormScaler.TargetRectTransform.localScale = new Vector3(0, 0, 1);
            }

            if (_changeNameFader != null) {
                _changeNameFader.CanvasTarget.alpha = 0;
            }

            if (_changeNameScaler != null) {
                _changeNameScaler.TargetRectTransform.localScale = new Vector3(0, 0, 1);
            }

            if (_versionText != null) {
                _versionText.SetText($"Version: {Application.version}");
            }

            UpdateUserFeed("");
            SetInteractableButtonChangeName(false);
        }

        protected override void ModelChanged() {

            _logoutWindowFader?.EndTransition(Convert.ToInt32(Model.IsLogOutConfirmationPopup));
            if (Model.IsLogOutConfirmationPopup) {
                UpdateforceDeleteToggle();
                _logoutWindowText?.SetText(
                    Model.IsGuest ? "Logging out as guest will <color=#FFFFFF;\"><b>erase all progress</b></color>" :
                    Model.ForceDeleteOnLogOut ? "<color=#FFFFFF;\"><b>Delete Account?</b></color><br>Progress will be lost!" : $"<color=#FFFFFF;\"><b>Log out?</b></color><br>Progress is saved"
                );
            }

            UpdateDisplayName();
            UpdatePlayerInfo();
            ChangePage(Model.PageIndex);
            UpdateCredentialsForm(Model.IsCredentialsAvailable);
            UpdateChangeNameForm(Model.IsChangeNameAvailable);
        }

        private void UpdateforceDeleteToggle() {

            if (_forceDeleteToggle == null) return;

            _forceDeleteToggle.interactable = !Model.IsGuest;
            if (_forceDeleteToggle.isOn != Model.ForceDeleteOnLogOut) {
                _forceDeleteToggle.isOn = Model.ForceDeleteOnLogOut;
            }
        }

        private void UpdateDisplayName() {

            if (_playerNameText == null)
                return;

            if (_playerNameText.text == Model.PlayerDisplayName)
                return;

            _playerNameText.SetText(Model.PlayerDisplayName);
        }

        private void UpdatePlayerInfo() {

            if (_playerInfoText == null)
                return;

            var userIdText = Model.DisplayUserId;

            if (_playerInfoText.text == userIdText)
                return;

            _playerInfoText.SetText(userIdText);
        }

        public void ChangePage(int newState) {

            if (_pageStateView == null) return;

            if (Model.PageIndex == _pageStateView.CurrentState) return;

            _pageStateView.ChangeState(newState);
        }

        public void UpdateUserFeed(string userFeedText) {

            if (_userFeedText == null) {
                return;
            }

            var hasText = !string.IsNullOrEmpty(userFeedText);

            if (hasText) {
                _userFeedFader.StartTransition();
                _userFeedText.SetText(userFeedText);
                _userFeedScaler.TargetRectTransform.localScale = new Vector3(0, 0, 1);
                _userFeedScaler.StartTransition();

            } else if (_userFeedFader.CanvasTarget.alpha > 0) {
                _userFeedFader.EndTransition();
                _userFeedScaler.EndTransition();
            }
        }

        public void SetInteractableButtonChangeName(bool isInteractive) {

            if (_changeNameButton == null) return;
            _changeNameButton.interactable = isInteractive;
        }

        private void UpdateCredentialsForm(bool isAvailable) {

            if (isAvailable) {
                _credetialsFormFader.StartTransition();
                _credetialsFormScaler.StartTransition();

            } else if (_credetialsFormFader.CanvasTarget.alpha > 0) {
                _credetialsFormFader.EndTransition();
                _credetialsFormScaler.EndTransition();
            }
        }

        private void UpdateChangeNameForm(bool isAvailable) {

            if (isAvailable) {
                _changeNameFader.StartTransition();
                _changeNameScaler.StartTransition();

            } else if (_changeNameFader.CanvasTarget.alpha > 0) {
                _changeNameFader.EndTransition();
                _changeNameScaler.EndTransition();
            }
        }

        public void ClickedPrevious() => OnPreviousClicked?.Invoke();

        public void ClickedChangePlayerName() => OnChangePlayerNameClicked?.Invoke();
        public void ClickedLinkAccount() => OnLinkAccountClicked?.Invoke();
        public void ClickedUpdateCredentials() => OnUpdateCredentialsClicked?.Invoke();
        public void ClickedAccountInfo() => OnDisplayAllAccountInfo?.Invoke();

        public void ClickedLogout() => OnLogOutClicked?.Invoke();
        public void ClickedLogoutConfirm() => OnLogOutConfirmClicked?.Invoke();
        public void ClickedLogoutCancel() => OnLogOutCancelClicked?.Invoke();
        public void ClickedForceDelete() => OnToggleForceDelete?.Invoke();
    }
}
