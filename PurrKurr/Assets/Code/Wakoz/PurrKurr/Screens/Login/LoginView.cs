using Code.Wakoz.PurrKurr.Views;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Code.Wakoz.PurrKurr.Screens.Login
{
    public class LoginView : View<LoginModel>
    {
        public event Action OnOfflinePlay;
        public event Action OnAnonymousAuth;
        public event Action OnMoveToUsernamePasswordSignUp;
        public event Action OnGoogleAppleClicked;
        public event Action OnFacebookAuth;
        public event Action OnPreviousClicked;
        public event Action OnUsernamePasswordSignUp;
        public event Action OnUsernamePasswordSignIn;

        public InputField UsernameInput;
        public InputField PasswordInput;

        [SerializeField] private CanvasGroupFaderView _welcomeWindowFader;

        [SerializeField] private CanvasGroupFaderView _userFeedFader;
        [SerializeField] private RectTransformScalerView _userFeedScaler;
        [SerializeField] private TMP_Text _userFeedText;

        [SerializeField] private CanvasGroupFaderView _credetialsFormFader;
        [SerializeField] private RectTransformScalerView _credetialsFormScaler;

        [SerializeField] private TextMeshProUGUI _versionText;

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

            if (_welcomeWindowFader != null) {
                _welcomeWindowFader.CanvasTarget.alpha = 0;
            }

            if (_versionText != null) {
                _versionText.SetText($"Version: {Application.version}");
            }
        }

        protected override void ModelChanged() {

            ChangePage(Model.PageIndex);
            UpdateUserFeed("");
            UpdateCredentialsForm(Model.IsFormAvailable());
            UpdateWelcomeWindow(Model.IsWelcomeAvailable());
        }

        public void ChangePage(int newState) {

            if (_pageStateView == null) {
                return;
            }

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
                _credetialsFormScaler.TargetRectTransform.localScale = new Vector3(0, 0, 1);
                _credetialsFormScaler.StartTransition();

            } else if (_credetialsFormFader.CanvasTarget.alpha > 0) {
                _credetialsFormFader.EndTransition();
                _credetialsFormScaler.EndTransition();
            }
        }

        private void UpdateWelcomeWindow(bool isAvailable) {

            if (_welcomeWindowFader == null) return;
            _welcomeWindowFader.EndTransition(Convert.ToInt32(isAvailable));
        }

        public void ClickedOffline() => OnOfflinePlay?.Invoke();
        public void ClickedEmail() => OnMoveToUsernamePasswordSignUp?.Invoke();
        public void ClickeGoogleApple() => OnGoogleAppleClicked?.Invoke();
        public void ClickedFacebook() => OnFacebookAuth?.Invoke();
        public void ClickedAnonymous() => OnAnonymousAuth?.Invoke();
        public void ClickedPrevious() => OnPreviousClicked?.Invoke();
        public void ClickedEmailSignUp() => OnUsernamePasswordSignUp?.Invoke();
        public void ClickedEmailSignIn() => OnUsernamePasswordSignIn?.Invoke();
    }
}
