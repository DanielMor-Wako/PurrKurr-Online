using UnityEngine;
using Code.Wakoz.PurrKurr.Views;
using System;
using TMPro;
using UnityEngine.UI;

namespace Code.Wakoz.PurrKurr.Screens.Login 
{
    public class AccountView : View<AccountModel> {

        public event Action OnPreviousClicked;

        public event Action OnLinkAccountClicked;
        public event Action OnUpdateCredentialsClicked;

        public event Action OnLogOutClicked;
        public event Action OnLogOutConfirmClicked;
        public event Action OnLogOutCancelClicked;

        public InputField UsernameInput;
        public InputField PasswordInput;

        [SerializeField] private TMP_Text _usernameTitle;

        [SerializeField] private CanvasGroupFaderView _logoutWindowFader;

        [SerializeField] private CanvasGroupFaderView _userFeedFader;
        [SerializeField] private RectTransformScalerView _userFeedScaler;
        [SerializeField] private TMP_Text _userFeedText;

        [SerializeField] private CanvasGroupFaderView _credetialsFormFader;
        [SerializeField] private RectTransformScalerView _credetialsFormScaler;

        [Space(10)]
        public MultiStateView _pageStateView;

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
        }

        protected override void ModelChanged() {

            _logoutWindowFader?.EndTransition(Convert.ToInt32(Model.IsLogOutConfirmationPopup));

            UpdateUsernameInfo();
            ChangePage(Model.PageIndex);
            UpdateUserFeed("");
            UpdateCredentialsForm(Model.IsCredentialsAvailable);
        }

        private void UpdateUsernameInfo() {

            if (_usernameTitle == null || Model.Player == null)
                return;

            _usernameTitle?.SetText(Model.Player.Username);
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

        private void UpdateCredentialsForm(bool isAvailable) {

            if (isAvailable) {
                _credetialsFormFader.StartTransition();
                _credetialsFormScaler.StartTransition();

            } else if (_credetialsFormFader.CanvasTarget.alpha > 0) {
                _credetialsFormFader.EndTransition();
                _credetialsFormScaler.EndTransition();
            }
        }

        public void ClickedPrevious() => OnPreviousClicked?.Invoke();

        public void ClickedLinkAccount() => OnLinkAccountClicked?.Invoke();
        public void ClickedUpdateCredentials() => OnUpdateCredentialsClicked?.Invoke();

        public void ClickedLogout() => OnLogOutClicked?.Invoke();
        public void ClickedLogoutConfirm() => OnLogOutConfirmClicked?.Invoke();
        public void ClickedLogoutCancel() => OnLogOutCancelClicked?.Invoke();


    }
}
