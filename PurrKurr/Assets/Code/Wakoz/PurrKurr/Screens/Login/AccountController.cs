using Code.Core.Auth;
using Code.Wakoz.PurrKurr.Screens.SceneTransition;
using System;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using UnityEngine;
using Unity.Services.Core;
using Code.Core;
using System.Threading;
using Unity.Services.Authentication.PlayerAccounts;

namespace Code.Wakoz.PurrKurr.Screens.Login
{
    [DefaultExecutionOrder(11)]
    public class AccountController : SingleController
    {
        [SerializeField] private AccountView _view;

        private AccountModel _model;

        private AuthenticationManager _authService;
        private RegexValidator _regexValidator;

        private CancellationTokenSource _cancellationTokenSource;

        protected override void Clean() {

            _view.ChangeNameInput.onEndEdit.RemoveAllListeners();

            _view.OnPreviousClicked -= HandlePrevious;

            _view.OnChangePlayerNameClicked -= HandlePlayerRename;
            _view.OnLinkAccountClicked -= HandleLinkProviders;
            _view.OnUpdateCredentialsClicked -= HandleUpdateCredentials;
            _view.OnDisplayAllAccountInfo -= HandleDisplayAllInfo;

            _view.OnLogOutClicked -= HandleLogOutPopup;
            _view.OnLogOutConfirmClicked -= HandleLogOutConfirm;
            _view.OnLogOutCancelClicked -= HandleLogOutCancel;
            _view.OnToggleForceDelete -= HandleToggleForceDelete;

            _authService?.Dispose();
            _authService = null;
        }

        protected override async Task Initialize() {

            _model = new AccountModel();

            _view.ChangeNameInput.onEndEdit.AddListener(text => {
                _view.SetInteractableButtonChangeName(IsValidNewPlayername(text));
            });

            _view.OnPreviousClicked += HandlePrevious;

            _view.OnChangePlayerNameClicked += HandlePlayerRename;
            _view.OnLinkAccountClicked += HandleLinkProviders;
            _view.OnUpdateCredentialsClicked += HandleUpdateCredentials;
            _view.OnDisplayAllAccountInfo += HandleDisplayAllInfo;

            _view.OnLogOutClicked += HandleLogOutPopup;
            _view.OnLogOutConfirmClicked += HandleLogOutConfirm;
            _view.OnLogOutCancelClicked += HandleLogOutCancel;
            _view.OnToggleForceDelete += HandleToggleForceDelete;

            _view.SetModel(_model);

            _authService = new AuthenticationManager();
            SetDisplayUserId();

            await SetPageAsGuestOrRegistered();
        }

        private async Task SetPageAsGuestOrRegistered() {

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

        private bool IsValidNewPlayername(string playerName)
            => !string.IsNullOrEmpty(playerName.Trim()) && playerName != _model.PlayerDisplayName;

        private void UpdateRegisteredUserInfo(PlayerInfo playerInfo) {
            SetDisplayName();
        }

        private async void HandleDisplayAllInfo() {
            var playerInfo = await AuthenticationService.Instance.GetPlayerInfoAsync();
            var id = playerInfo.Id;
            GUIUtility.systemCopyBuffer = id;
            _view.UpdateUserFeed($"User id copied to clipboard!<br>{playerInfo.Username}<br>{id}");
        }

        private void SetDisplayName() {
            var gameManager = GetController<GameManager>();
            var displayName = gameManager != null ? gameManager.DisplayName : "";
            _model.SetDisplayName(displayName);
        }

        private void SetDisplayUserId() {
            var gameManager = GetController<GameManager>();
            var userId = gameManager != null ? gameManager.UserId : "";
            _model.SetUserId(userId);
        }

        private void ChangePageToGuestUser() {

            _model.SetGuestUser(true);
            _model.ChangeNameAvailability(false, true);
            _model.SetForceDeleteOnLogOut(true, true);
            _model.ChangePageIndex(1);
            SetDisplayName();
        }

        private void ChangePageToRegisteredUser() {

            _model.SetGuestUser(false);
            _model.ChangeNameAvailability(true, true);
            _model.ChangeCredentialsAvailability(false, true);
            _model.SetForceDeleteOnLogOut(false, true);
            _model.ChangePageIndex(2);
        }

        private void HandlePrevious() {

            _model.ChangeCredentialsAvailability(false, true);
            _model.ChangeNameAvailability(false, true);
            _model.ChangeLogOutConfirmationState(false);
            _view.UpdateUserFeed("");

            GetController<SceneTransitionController>().LoadSceneByIndex(1, "Main Menu");
            //_model.ChangeOpenState(!_model.IsOpen);
        }

        private void HandleLogOutPopup() {

            _model.ChangeLogOutConfirmationState(!_model.IsLogOutConfirmationPopup);
            _view.UpdateUserFeed("");
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

            if (_model.ForceDeleteOnLogOut || await _authService.IsGuest()) {
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

        private void HandleToggleForceDelete() {
            if (_model.IsGuest) 
                return;
            _model.SetForceDeleteOnLogOut(!_model.ForceDeleteOnLogOut);
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

        private async void HandlePlayerRename() {

            var input = _view.ChangeNameInput.text;

            // check sufficient currency

            if (!IsValidUsername(input)) {
                _view.UpdateUserFeed("Name must be between 3–50 characters.<br>No spaces or special characters (e.g., <, >, /, \", ;).");
                return;
            }

            _view.SetInteractableButtonChangeName(false);
            if (await TryPersonalizePlayerName(input)) {

                var gameManager = GetController<GameManager>();
                gameManager?.UpdatePlayerIdAndDisplayName();
                gameManager?.ForceCache();
                SetDisplayName();
                _view.SetInteractableButtonChangeName(IsValidNewPlayername(input));
            }
        }

        private async void HandleLinkProviders() {

            if (!AuthenticationService.Instance.IsSignedIn) {
                Debug.LogWarning("User is not signed in");
                return;
            }

            if (await _authService.IsUnityIdAvailable()) {
                Debug.LogWarning($"User is already linked");
                _view.UpdateUserFeed($"User is already linked to another player");
                return;
            }

            _model.ChangePageIndex(0);

            Debug.Log($"Trying to Link (isSignedIn?){PlayerAccountService.Instance.IsSignedIn} with accessToken {PlayerAccountService.Instance.AccessToken}");
            if (string.IsNullOrEmpty(PlayerAccountService.Instance.AccessToken)) {
                // If the player has no AccessToken, proceed to the Unity Authentication sign-in.
                await PlayerAccountService.Instance.StartSignInAsync();
            }

            _cancellationTokenSource ??= new CancellationTokenSource();
            await WaitUntilPlayerAccessTokenExist(10, _cancellationTokenSource.Token);

            _authService.SetAuthStrategy(new UnityAccountAuthStrategy());
            _authService.Register(OnAuthSuccess, OnAuthFailure);
        }

        // todo: move to authentication utils
        private async Task<bool> TryPersonalizePlayerName(string personlizedName) {

            try {
                await AuthenticationService.Instance.UpdatePlayerNameAsync(personlizedName);
                _view.UpdateUserFeed($"Saved Name:<br>{personlizedName}");
                Debug.Log($"Player name set as '{personlizedName}'");
                return true;
            }
            catch (RequestFailedException ex) {
                _view.UpdateUserFeed(ex.Message);
                Debug.LogError($"Failed to set player name: {ex.Message}");
            }
            catch (Exception ex) {
                _view.UpdateUserFeed(ex.Message);
                Debug.LogError($"Exception setting player name {ex.Message}");
            }

            return false;
        }

        // link account methods
        private async void HandleLinkFb() {
            var linkedProviders = await AuthenticationService.Instance.GetPlayerInfoAsync();
            _view.UpdateUserFeed("Facebook id :" + linkedProviders.GetFacebookId());

            Debug.Log("Not Available, Try again later");
        }

        private async void OnAuthFailure(string obj) {
            Debug.Log($"Failed to link account {obj}");
            await SetPageAsGuestOrRegistered();
            _view.UpdateUserFeed(obj);
        }
        
        private async void OnAuthSuccess(string obj) {
            Debug.Log("Account link start");

            if (_authService.IsLoggedIn() && !(await _authService.IsGuest())) {
                
                ChangePageToRegisteredUser();
                _view.UpdateUserFeed("Account linked");
                Debug.Log("Account linked");

                var playerInfo = await AuthenticationService.Instance.GetPlayerInfoAsync();
                _model.ForceRefresh();

                UpdateRegisteredUserInfo(playerInfo);

            } else {

                ChangePageToGuestUser();
                _view.UpdateUserFeed("");
                Debug.Log("not registered or offline");
            }
            
        }

        private async Task WaitUntilPlayerAccessTokenExist(int retries = 10, CancellationToken token = default) {

            //var user = AuthenticationService.Instance;
            //Debug.Log($"{user.PlayerId} - checking logged-in state, {retries} tries");
            var playerAccount = PlayerAccountService.Instance;
            Debug.Log($"{PlayerAccountService.Instance.IsSignedIn} | accessToken {PlayerAccountService.Instance.AccessToken}");

            var retryLeft = retries;
            while (!playerAccount.IsSignedIn && retryLeft > 0) {
                Debug.Log("not sign in yet");
                retryLeft--;
                await Task.Delay(TimeSpan.FromSeconds(2), token);
            }

            //_isRequestAvailable = true;

            if (retryLeft <= 0) {
                Debug.Log("Error occur: connection is too slow");
                return;
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
            _regexValidator ??= new RegexValidator();
            return _regexValidator.IsValidUsername(name);
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
