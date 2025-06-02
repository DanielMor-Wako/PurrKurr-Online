using Code.Core.Auth;
using Code.Wakoz.PurrKurr.Screens.SceneTransition;
using System;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using UnityEngine;
using Unity.Services.Core;
using Code.Core;

namespace Code.Wakoz.PurrKurr.Screens.Login
{
    [DefaultExecutionOrder(11)]
    public class AccountController : SingleController
    {
        [SerializeField] private AccountView _view;

        private AccountModel _model;

        private AuthenticationManager _authService;
        private RegexValidator _regexValidator;

        protected override void Clean() {

            _view.OnPreviousClicked -= HandlePrevious;

            _view.OnLinkAccountClicked -= HandleLinkProviders;
            _view.OnUpdateCredentialsClicked -= HandleUpdateCredentials;

            _view.OnLogOutClicked -= HandleLogOutPopup;
            _view.OnLogOutConfirmClicked -= HandleLogOutConfirm;
            _view.OnLogOutCancelClicked -= HandleLogOutCancel;

            _authService?.Dispose();
            _authService = null;
        }

        protected override async Task Initialize() {

            _model = new AccountModel();

            _view.OnPreviousClicked += HandlePrevious;

            _view.OnLinkAccountClicked += HandleLinkProviders;
            _view.OnUpdateCredentialsClicked += HandleUpdateCredentials;

            _view.OnLogOutClicked += HandleLogOutPopup;
            _view.OnLogOutConfirmClicked += HandleLogOutConfirm;
            _view.OnLogOutCancelClicked += HandleLogOutCancel;

            _view.SetModel(_model);

            _authService = new AuthenticationManager();

            if (UnityServices.State is ServicesInitializationState.Uninitialized 
                || await _authService.IsGuest()) {
                // Guest or unsigned user -> Show available providers
                ChangePageToGuestUser();

            } else {
                // Registered user -> Show Linked providers
                ChangePageToRegisteredUser();

                var playerInfo = await AuthenticationService.Instance.GetPlayerInfoAsync();

                UpdateRegisteredUserInfo(playerInfo);
            }
        }

        private void UpdateRegisteredUserInfo(PlayerInfo playerInfo) {

            _model.ChangeCredentialsAvailability(string.IsNullOrEmpty(playerInfo.Username), true);
            SetDisplayName();

            _view.UpdateUserFeed($"ID:{playerInfo.Id}<br>UnityID:<br>{playerInfo.GetUnityId()}");
        }

        private void SetDisplayName() {
            var gameManager = GetController<GameManager>();
            var displayName = gameManager != null ? gameManager?.DisplayName : "";
            _model.SetDisplayName(displayName);
        }

        private void ChangePageToGuestUser() {
            _model.SetGuestUser(true);
            _model.ChangePageIndex(1);
            SetDisplayName();
        }

        private void ChangePageToRegisteredUser() {
            _model.SetGuestUser(false);
            _model.ChangePageIndex(2);
        }

        private void HandlePrevious() {

            _model.ChangeCredentialsAvailability(false, true);
            _model.ChangeLogOutConfirmationState(false);

            GetController<SceneTransitionController>().LoadSceneByIndex(1, "Main Menu");
            //_model.ChangeOpenState(!_model.IsOpen);
            //_model.ChangeLogOutConfirmationState(false);
        }

        private void HandleLogOutPopup() {

            _model.ChangeLogOutConfirmationState(!_model.IsLogOutConfirmationPopup);

            /*var modalData = isGuest ?
                new OverlayWindowData() {
                    Title = "Warning",
                    BodyContent = "Logging out as guest will erase all data",
                    ButtonsRawData = new List<GenericButtonData>(new List<GenericButtonData>() { new GenericButtonData(GenericButtonType.Confirm, "Log out", () => DeleteGuestAccount()) })
                }
                : new OverlayWindowData() {
                    Title = "Warning",
                    BodyContent = $"Log out <br>{(await AuthenticationService.Instance.GetPlayerInfoAsync()).Username} ?",
                    ButtonsRawData = new List<GenericButtonData>(new List<GenericButtonData>() { new GenericButtonData(GenericButtonType.Confirm, "Log out", () => LogOut()) })
                };*/
        }

        private async void HandleLogOutConfirm() {

            if (UnityServices.State is ServicesInitializationState.Uninitialized) {
                LoadLoginScene("Restart");
                return;
            }

            if (_authService == null) {
                _authService = new AuthenticationManager();
                await _authService.InitAsync();
            }

            var isLoggedIn = _authService.IsLoggedIn();
            if (!isLoggedIn) {
                LoadLoginScene("");
                return;
            }

            var isGuest = await _authService.IsGuest();
            if (isGuest) {
                await DeleteAccount();
                return;
            }

            LogOut();
        }

        private void HandleLogOutCancel() {
            _model.ChangeLogOutConfirmationState(false);
        }

        private void LogOut() {

            var gameManager = GetController<GameManager>();
            if (gameManager != null) {
                gameManager.ClearCachedFiles();
            }

            _authService.LogOut();
            
            LoadLoginScene("Logged Out");
        }

        private async Task DeleteAccount() {

            var gameManager = GetController<GameManager>();
            if (gameManager != null) {
                gameManager.ClearCachedFiles();
                await gameManager.DeleteGameData();
            }

            await _authService.DeleteAccount();

            LoadLoginScene("Account Deleted");
        }

        private void LoadLoginScene(string titleText) {
            GetController<SceneTransitionController>().LoadSceneByIndex(0, titleText);
        }

        private async void HandleLinkProviders() {

            _model.ChangeCredentialsAvailability(false, true);
            _model.ChangePageIndex(0);


            if (AuthenticationService.Instance.IsSignedIn) {
                try {
                    Debug.Log("user is signed in, trying to logout before re-signing");
                    _authService.LogOut(true);
                    AuthenticationService.Instance.ClearSessionToken();

                    await _authService.InitAsync();
                    await _authService.SignInCachedUserAsync();

                } catch (Exception ex) {
                    ChangePageToGuestUser();
                }
            }

            _authService.SetAuthStrategy(new UnityAccountAuthStrategy());
            _authService.Authenticate(OnAuthSuccess, OnAuthFailure);


            var playerInfo = await AuthenticationService.Instance.GetPlayerInfoAsync();

            UpdateRegisteredUserInfo(playerInfo);

        }


        // link account methods
        private async void HandleLinkFb() {
            var linkedProviders = await AuthenticationService.Instance.GetPlayerInfoAsync();
            _view.UpdateUserFeed("Facebook id :" + linkedProviders.GetFacebookId());

            Debug.Log("Not Available, Try again later");
        }

        private void OnAuthFailure(string obj) {
            Debug.Log("Failed to link account");
            ChangePageToGuestUser();
        }

        private async void OnAuthSuccess(string obj) {
            Debug.Log("Account link start");
            
            await Task.Delay(TimeSpan.FromSeconds(5));

            if (_authService.IsLoggedIn() && !(await _authService.IsGuest())) {
                
                ChangePageToRegisteredUser();
                _view.UpdateUserFeed("Account linked");
                Debug.Log("Account linked");

                var playerInfo = await AuthenticationService.Instance.GetPlayerInfoAsync();
                _model.ForceRefresh();

            } else {

                ChangePageToGuestUser();
                _view.UpdateUserFeed("");
                Debug.Log("not registered or offline");
            }
            
        }

        private async void HandleUpdateCredentials() {

            try {
                var playerInfo = await AuthenticationService.Instance.GetPlayerInfoAsync();
                if (string.IsNullOrEmpty(playerInfo.Username)) {
                    OnConfirmUpdateCredentials();
                } else {
                    _view.UpdateUserFeed($"Username is {playerInfo.Username}<br>Update on {playerInfo.LastPasswordUpdate}");
                }

            } catch (Exception ex) {
                _view.UpdateUserFeed(ex.Message);
            }
            
        }

        private void OnConfirmUpdateCredentials() {

            string username = _view.UsernameInput.text;
            string password = _view.PasswordInput.text;

            _regexValidator ??= new RegexValidator();
            if (!IsValidUsername(username)) {
                _view.UpdateUserFeed("Username is not valid");
            } else {
                AddCredentialsAsync(
                    username, password,
                    OnSuccessAddCredentials,
                    onFailureAddCredentials
                );
            }
        }

        private bool IsValidUsername(string name) {
            return !string.IsNullOrEmpty(name) && name.Length >= 3 && name.Length <= 20;
        }

        private void AddCredentialsAsync(
            string email, string password,
            Action<string> onSuccess, Action<string> onFailure
        ) {
            _authService.HandleAddCredentialsAsync(email, password, onSuccess, onFailure);
        }

        private void onFailureAddCredentials(string obj) {
            _view.UpdateUserFeed($"Failed: Credentials Error {obj}");
        }

        private async void OnSuccessAddCredentials(string obj) {

            ChangePageToRegisteredUser();

            var playerInfo = await AuthenticationService.Instance.GetPlayerInfoAsync();

            _view.UpdateUserFeed($"Account: {playerInfo.Username}<br>Last update on {playerInfo.LastPasswordUpdate}");
        }
    }
}
