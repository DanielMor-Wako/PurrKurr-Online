using Code.Core.Auth;
using Code.Wakoz.PurrKurr.Screens.SceneTransition;
using System;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Code.Wakoz.PurrKurr.Screens.Login
{
    [DefaultExecutionOrder(11)]
    public class LoginController : SingleController
    {

        [SerializeField] private LoginView _view;

        [Tooltip("Reference for Offline playing button, used for testing in editor")]
        [SerializeField] private Transform _offlineTestingButton;

        private LoginModel _model;

        private AuthenticationManager _authService;

        protected override void Clean() {

            _view.OnOfflinePlay -= HandleOfflinePlay;
            _view.OnAnonymousAuth -= HandleAnonymousAuth;
            _view.OnMoveToUsernamePasswordSignUp -= HandleSignClicked;
            _view.OnGoogleAppleClicked -= HandleGoogleAppleClicked;
            _view.OnFacebookAuth -= HandleFacebookClicked;
            _view.OnPreviousClicked -= HandlePreviousClicked;
            _view.OnUsernamePasswordSignUp -= HandleSignUpCredentials;
            _view.OnUsernamePasswordSignIn -= HandleSignInCredentials;

            _authService.Dispose();
        }

        protected override async Task Initialize() {

            _view.OnOfflinePlay += HandleOfflinePlay;
            _view.OnAnonymousAuth += HandleAnonymousAuth;
            _view.OnMoveToUsernamePasswordSignUp += HandleSignClicked;
            _view.OnGoogleAppleClicked += HandleGoogleAppleClicked;
            _view.OnFacebookAuth += HandleFacebookClicked;
            _view.OnPreviousClicked += HandlePreviousClicked;
            _view.OnUsernamePasswordSignUp += HandleSignUpCredentials;
            _view.OnUsernamePasswordSignIn += HandleSignInCredentials;

            _model = new LoginModel();

            _view.SetModel(_model);

            _authService = new AuthenticationManager();
            await _authService.InitAsync();

            // Show Login Or WelcomeWindow
            if (await _authService.SignInCachedUserAsync()) {
                MoveToNextScene();
            } else {
                ShowWelcomeWindow();
            }
        }

        /*
       private async void Start() {

           _authService = new AuthenticationManager();
           await _authService.InitAsync();

           _authService.SetAuthStrategy(new AnonymousAuthStrategy());
           SignIn(OnSessionTokenExist, OnAuthFailure);



           //if (await _authService.SignInCachedUnityAccountAsync()) {
           //if (await _authService.IsUnityIdAvailable()) {
           //var accessToken = PlayerAccountService.Instance.AccessToken;
           *//*if (accessToken != null) {

               ChangeSceneToMainMenu();
           } else {
               ShowWelcomeWindow();
           }*//*
       }
*/
/*
        private void OnSessionTokenExist(string msgOnSuccess) {
            _authService.TrySignInUnityPlayer(
                (string msgOnSuccess) => MoveToNextScene(),
                (string x) => ShowWelcomeWindow());
        }
*/
        private void MoveToPage(int index)
            => _model.SetPageIndex(index);

        private void HandlePreviousClicked() 
            => ShowWelcomeWindow();

        private void ShowWelcomeWindow() 
            => MoveToPage(1);

        private void MoveToNextScene() {

            MoveToPage(3);
            int newIndex = SceneManager.GetActiveScene().buildIndex + 1;
            
            GetController<SceneTransitionController>().LoadSceneByIndex(newIndex, "Loading"
                /*,() => {
                     actionOnEnd.Invoke();
                 }*/);

        }

        public void SetAuthStrategy(IAuthStrategy authStrategy) {
            _authService.SetAuthStrategy(authStrategy);
        }

        public void SignIn(Action<string> onSuccess, Action<string> onFailure) {
            _authService.Authenticate(onSuccess, onFailure);
        }

        public void SignUp(Action<string> onSuccess, Action<string> onFailure) {
            _authService.Register(onSuccess, onFailure);
        }

        private void OnAuthSuccess(string accessToken) {

            var user = AuthenticationService.Instance;
            Debug.Log(user.PlayerId + $" : Authentication successful with token {accessToken} - logged in? {_authService.IsLoggedIn()}");

            if (!_authService.IsLoggedIn()) {
                return;
            }

            MoveToNextScene();
        }

        private void OnAuthFailure(string error) {
            _view.UpdateUserFeed(error);
            Debug.LogError("Authentication failed: " + error);
        }

        private void OnUnityWebAuthFailure(string error) {
            OnAuthFailure(error);
            ShowWelcomeWindow();
        }

        private void HandleOfflinePlay() {
            MoveToNextScene();
        }

        private void HandleAnonymousAuth() {
            SetAuthStrategy(new AnonymousAuthStrategy());
            SignIn(OnAuthSuccess, OnAuthFailure);
        }

        private void HandleSignClicked() {

            MoveToPage(2);

            SetAuthStrategy(new UnityAccountAuthStrategy());
            SignIn(OnAuthSuccess, OnUnityWebAuthFailure);

/*
            if (PlayerAccountService.Instance.IsSignedIn) {
                // If the player is already signed into Unity Player Accounts, proceed directly to the Unity Authentication sign-in.
                Debug.Log("already signed!!!");
                await _authService.SignInWithUnityAsync(PlayerAccountService.Instance.AccessToken);
                return;
            }*/
            // clicked login --> starting web request
            //await _authService.SignInCachedUnityAccountAsync();
/*
            if (await _authService.IsUnityIdAvailable()) {

                ChangeSceneToMainMenu();
                return;
            }

            ShowWelcomeWindow();*/
        }

        private void HandleGoogleAppleClicked() {
            SetAuthStrategy(new GoogleAuthStrategy());
            SignIn(OnAuthSuccess, OnAuthFailure);
        }

        private void HandleFacebookClicked() {
            SetAuthStrategy(new FacebookAuthStrategy());
            SignIn(OnAuthSuccess, OnAuthFailure);
        }

        private void HandleSignInCredentials() {

            var username = _view.UsernameInput.text;
            var password = _view.PasswordInput.text;

            if (!IsValidUsername(username)) {
                _view.UpdateUserFeed("Name is not valid");

            } else {
                SetAuthStrategy(new EmailPasswordAuthStrategy(username, password));
                SignIn(OnAuthSuccess, OnAuthFailure);
            }
        }

        private void HandleSignUpCredentials() {

            var username = _view.UsernameInput.text;
            var password = _view.PasswordInput.text;

            if (!IsValidUsername(username)) {
                _view.UpdateUserFeed("Name is not valid");

            } else {
                SetAuthStrategy(new EmailPasswordAuthStrategy(username, password));
                SignUp(OnAuthSuccess, OnAuthFailure);
            }
        }

        private bool IsValidUsername(string name) {
            return !string.IsNullOrEmpty(name) && name.Length >= 3 && name.Length <= 20;
        }

        // todo: move the methods to LinkAccountController
        private async void HandleLinkFb() {

            var linkedProviders = await AuthenticationService.Instance.GetPlayerInfoAsync();
            Debug.Log("facebook id :" + linkedProviders.GetFacebookId());
            _view.UpdateUserFeed($"Service not available, Try again later.");

        }

        private async void HandleLinkEmail() {

            var isGuest = await _authService.IsGuest();

            if (!isGuest) {

                var playerInfo = await AuthenticationService.Instance.GetPlayerInfoAsync();
                _view.UpdateUserFeed($"Account already signed as<br>{playerInfo.Username}<br>on<br>{playerInfo.LastPasswordUpdate}");

                return;
            }

            // Show window to confirm
        }

        private void OnConfirmUpdateCredentials() {

            var username = _view.UsernameInput.text;
            var password = _view.PasswordInput.text;

            var regexValidator = new RegexValidator();

            if (!IsValidUsername(username)) {
                _view.UpdateUserFeed("Name is not valid");

            } else {
                AddCredentialsAsync(
                    username,
                    password,
                    OnSuccessAddCredentials,
                    onFailureAddCredentials
                );
            }
        }

        private void AddCredentialsAsync(
            string username,
            string password,
            Action<string> onSuccess,
            Action<string> onFailure
        ) {
            _authService.HandleAddCredentialsAsync(username, password, onSuccess, onFailure);
        }

        private void onFailureAddCredentials(string obj) {

            Debug.Log("Credentials Error");

            _view.UpdateUserFeed($"Failed: {obj}");
        }

        private async void OnSuccessAddCredentials(string obj) {
            
            var playerInfo = await AuthenticationService.Instance.GetPlayerInfoAsync();

            _view.UpdateUserFeed($"Account was linked to {playerInfo.Username} <br>Password updated on<br>{playerInfo.LastPasswordUpdate}");
        }

    }
}
