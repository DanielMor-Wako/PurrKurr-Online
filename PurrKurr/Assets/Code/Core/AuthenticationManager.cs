using System.Threading.Tasks;
using System;
using UnityEngine;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Core.Environments;
using Unity.Services.Authentication.PlayerAccounts;

namespace Code.Core.Auth
{
    public class AuthenticationManager : IAuthenticationManager, IDisposable
    {
        private const string ProdEnvironmentName = "production";
        private const string DevEnvironmentName = "development";

        private IAuthStrategy _authStrategy;
        
        private bool _initialized;

        private PlayerInfo _playerInfo;

        public PlayerInfo PlayerInfo => _playerInfo;

        public AuthenticationManager()
        {
            Debug.Log("todo: consider using static on _initialized  for reseting the service correctly on re-login");
            _initialized = false;
        }

        public void Dispose() {
            DeregisterEvents();
        }

        // todo: use the initController to init UnityServices
        public async Task InitAsync() {

            try {

                var options = new InitializationOptions().SetEnvironmentName(GetEnvironmentName());
                await UnityServices.InitializeAsync(options);

                RegisterEvents();
            }
            catch (Exception exception) {
                Debug.LogError($"An error occurred during UGS initialization {exception.Message}");
            }
        }

        public void SetAuthStrategy(IAuthStrategy authStrategy) {
            _authStrategy = authStrategy;
        }

        public void Authenticate(Action<string> onSuccess, Action<string> onFailure) {
            _authStrategy?.Authenticate(onSuccess, onFailure);
        }

        public void Register(Action<string> onSuccess, Action<string> onFailure) {
            _authStrategy?.Register(onSuccess, onFailure);
        }

        public void LinkProvider(
            IIdentityProviderStrategy auth,
            Action<string> onSuccess,
            Action<string> onFailure
        ) {
            auth.LinkFB(onSuccess, onFailure);
        }

        public void UnlinkProvider(
            IIdentityProviderStrategy auth,
            Action<string> onSuccess,
            Action<string> onFailure
        ) {
            auth.UnlinkFB(onSuccess, onFailure);
        }

        public bool IsLoggedIn() {
            return AuthenticationService.Instance.SessionTokenExists;
        }

        public async Task<bool> IsGuest() {
            
            var playerInfo = await AuthenticationService.Instance.GetPlayerInfoAsync();
            var username = playerInfo.Username;
            Debug.Log($"Username: {playerInfo.Username} | Identities : {playerInfo.Identities.Count}");
            var isAccountLinkedOrSignedUp = playerInfo.Identities.Count > 0 || !string.IsNullOrEmpty(username);

            return !isAccountLinkedOrSignedUp;
        }

        public void LogOut(bool clearCredentials = true) {

            AuthenticationService.Instance.SignOut(clearCredentials);
            PlayerAccountService.Instance.SignOut();

            if (clearCredentials) {
                AuthenticationService.Instance.ClearSessionToken();
                PlayerPrefs.DeleteAll();
            }
        }
        
        public async Task DeleteAccount() {
            await AuthenticationService.Instance.DeleteAccountAsync();
        }

        public async Task<bool> SignInCachedUserAsync() {

            if (!IsLoggedIn()) {
                return false;
            }

            try {
                
                await AuthenticationService.Instance.SignInAnonymouslyAsync();

                //await AuthenticationService.Instance.GetPlayerInfoAsync();
                Debug.Log(
                    $"Sign in anonymously succeeded! with PlayerID: {AuthenticationService.Instance.PlayerId}"
                );
                return true;
            }
            catch (AuthenticationException ex) {
                Debug.LogError($"Authentication error: {ex.Message}");
                return false;
            }
            catch (RequestFailedException ex) {
                Debug.LogError($"Request failed: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> IsUnityIdAvailable() {

            //await SignInCachedUserAsync();


            // await user.StartSignInAsync();
/*
            Debug.Log($"Player account is {(!user.IsSignedIn ? "NOT" : "")} signed in");

            if (user.IsSignedIn) {
                return true;
            }*/

            if (!IsLoggedIn()) {
                Debug.Log("Authetication service is missing SessionTokenExists");
                return false;
            }

            var auth = AuthenticationService.Instance;
            /*var user = PlayerAccountService.Instance;

            if (user.AccessToken == null) {
                return false;
            }
*/
            await auth.GetPlayerInfoAsync();

            var info = auth.PlayerInfo;

            var unityId = info.GetUnityId() != null;
            Debug.Log("Unity ID "+ unityId);
            return (unityId);
        }

        public async void HandleAddCredentialsAsync(
            string username, string password,
            Action<string> onSuccess, Action<string> onFailure
        ) {
            var authStrategy = new UsernamePasswordAuthStrategy(username, password);

            await authStrategy.AddCredentialsAsync(onSuccess, onFailure);
        }

        private void RegisterEvents() {

            if (_initialized) return;

            //AuthenticationService.Instance.SignedIn += HandleSignIn;
            //AuthenticationService.Instance.SignInFailed += HandleSignInFailed;

            PlayerAccountService.Instance.SignedIn += HandlePlayerSignIn;
            PlayerAccountService.Instance.SignInFailed += HandlePlayerSignInFailed;
            PlayerAccountService.Instance.SignedOut += HandlePlayerSignOut;

            _initialized = true;
        }

        private void DeregisterEvents() {

            if (!_initialized) return;

            //AuthenticationService.Instance.SignedIn -= HandleSignIn;
            //AuthenticationService.Instance.SignInFailed -= HandleSignInFailed;

            PlayerAccountService.Instance.SignedIn -= HandlePlayerSignIn;
            PlayerAccountService.Instance.SignInFailed -= HandlePlayerSignInFailed;
            PlayerAccountService.Instance.SignedOut -= HandlePlayerSignOut;

            _initialized = false;
        }

        private void HandlePlayerSignOut() {
            Debug.Log($"Signed out");
        }

        private void HandleSignInFailed(RequestFailedException exception) {
            Debug.LogError($"Sign-in failed: {exception.Message}");
            // Handle sign-in failure
        }

        private async void HandleSignIn() {
            Debug.Log($"Signed in successfully {AuthenticationService.Instance.AccessToken}");
            /*_appService.AccessToken = AuthenticationService.Instance.AccessToken;

            user = await _appService.LoggedIn(
                new UserLoggedInRequest(
                    AuthenticationService.Instance.PlayerId,
                    SystemInfo.deviceUniqueIdentifier.ToString()
                )
            );
            if (user.LrdDatetime != null) {
                string value = user.LrdDatetime?.ToUnixTimeSeconds().ToString();
                PlayerPrefs.SetString(ARPGPlayerPrefs.LRD, value);
            }*/
        }

        private string GetEnvironmentName()
            => !Debug.isDebugBuild ? ProdEnvironmentName : DevEnvironmentName;
/*
        public async void TrySignInUnityPlayer(Action<string> onSuccess, Action<string> onFailure) {
*//*
            if (!AuthenticationService.Instance.SessionTokenExists) {
                Debug.Log("Player not signed in to unity account");
                onFailure("Has no session token");
                return;
            }

            Debug.Log($"Current sessionToken "+ AuthenticationService.Instance.AccessToken);

            var si = IsUnityIdAvailable();
*//*
            try {

                var accessToken = PlayerAccountService.Instance.AccessToken;

                await SignInWithUnityAsync(accessToken);
                Debug.Log($"login with access token {accessToken}");
                await LinkWithUnityAsync(accessToken);
                onSuccess("linked to unity");
            }
            catch (Exception e) {
                Debug.LogException(e);
                onFailure(e.Message);
            }

        }*/

        private async void HandlePlayerSignIn() {

            try {

                var accessToken = PlayerAccountService.Instance.AccessToken;

                await SignInWithUnityAsync(accessToken);

            } catch (Exception e) {
                Debug.LogException(e);
            }
        }

        public async Task SignInWithUnityAsync(string accessToken) {
/*
            if (PlayerAccountService.Instance.IsSignedIn) {

                Debug.Log("already signed into unity account");
                return;
            }

            if (!await IsGuest()) {

                Debug.Log("already signed into unity account - (not guest)");
                return;
            }*/

            Debug.Log($"Trying to login with accessToken\n{accessToken}");

            try {

                await AuthenticationService.Instance.SignInWithUnityAsync(accessToken);

                _playerInfo = AuthenticationService.Instance.PlayerInfo;

                var name = AuthenticationService.Instance.GetPlayerNameAsync();

                Debug.Log($"{name} - Sign in successful.<br>Id {_playerInfo.Id}");

            } catch (PlayerAccountsException ex) {
                // Compare error code to PlayerAccountsErrorCodes
                // Notify the player with the proper error message
                Debug.Log("PlayerAccountsException");
                Debug.LogException(ex);
            
            } catch (RequestFailedException ex) {
                // Compare error code to CommonErrorCodes
                // Notify the player with the proper error message
                Debug.Log($"RequestFailedException {ex.InnerException}");
                Debug.LogException(ex);

            } catch (Exception e) {
                throw new Exception(e.Message);
            }
        }

        private void HandlePlayerSignInFailed(RequestFailedException exception) {
            Debug.Log($"SignInFailed exception {exception}");
        }
    }
    public interface IAuthenticationManager
    {
        void SetAuthStrategy(IAuthStrategy authStrategy);
        void Authenticate(Action<string> onSuccess, Action<string> onFailure);
        void Register(Action<string> onSuccess, Action<string> onFailure);
        bool IsLoggedIn();
        Task<bool> IsGuest();
        void LinkProvider(
            IIdentityProviderStrategy auth,
            Action<string> onSuccess,
            Action<string> onFailure
        );
        void UnlinkProvider(
            IIdentityProviderStrategy auth,
            Action<string> onSuccess,
            Action<string> onFailure
        );
        Task DeleteAccount();
        void LogOut(bool clearCredentials);
    }
    public interface IIdentityProviderStrategy
    {
        void LinkFB(System.Action<string> onSuccess, System.Action<string> onFailure);
        void UnlinkFB(System.Action<string> onSuccess, System.Action<string> onFailure);
    }
    public interface IAuthStrategy
    {
        void Authenticate(System.Action<string> onSuccess, System.Action<string> onFailure);
        void Register(System.Action<string> onSuccess, System.Action<string> onFailure) {
            throw new NotImplementedException();
        }
    }
    public class UsernamePasswordAuthStrategy : IAuthStrategy
    {
        public string Username;
        public string Password;

        public UsernamePasswordAuthStrategy(string username, string password) {
            Username = username;
            Password = password;
        }

        public async void Authenticate(Action<string> onSuccess, Action<string> onFailure) {

            try {
                var user = AuthenticationService.Instance;
                if (user.IsSignedIn) {
                    if (IsUserAnonymous()) {
                        await user.SignInWithUsernamePasswordAsync(Username, Password);
                        Debug.Log(
                            $"added username: {user.PlayerName} password to user {user.PlayerId}"
                        );
                        onSuccess("Added username and password");
                    } else {
                        Debug.Log($"{user.PlayerId} already logged in");
                        onSuccess("Already logged in");
                    }
                } else {
                    Debug.Log($"{user.PlayerId} Signing in with username and password");
                    await user.SignInWithUsernamePasswordAsync(Username, Password);
                    Debug.Log($"{user.PlayerId} Signed in with username and password");
                    onSuccess("Signed in with username and password");
                }
            }
            catch (Exception e) {
                Debug.LogException(e);
                onFailure(e.Message);
            }
        }

        private bool IsUserAnonymous() {
            
            Debug.Log($"Profile: {AuthenticationService.Instance.Profile}");
            return AuthenticationService.Instance.Profile is "anonymous" or "default";
        }

        public async void Register(Action<string> onSuccess, Action<string> onFailure) {
            try {
                await AuthenticationService.Instance.SignUpWithUsernamePasswordAsync(
                    Username,
                    Password
                );
                Debug.Log($"{Username} Signed up with username and password");
                onSuccess("Signed up with username and password");
            }
            catch (Exception e) {
                Debug.LogException(e);
                onFailure(e.Message);
            }
        }

        public async Task AddCredentialsAsync(Action<string> onSuccess, Action<string> onFailure) {
            var user = AuthenticationService.Instance;

            try {
                if (user.IsSignedIn) {
                    if (IsUserAnonymous()) {
                        await user.AddUsernamePasswordAsync(Username, Password);
                        Debug.Log(
                            $"added username: {user.PlayerName} password to user {user.PlayerId}"
                        );
                        onSuccess("Added username and password");
                    } else {
                        Debug.Log($"{user.PlayerId} already logged in");
                        onSuccess("Already logged in");
                    }
                } else {
                    Debug.Log("User not logged in");
                    onFailure("User not logged in");
                }
            }
            catch (Exception e) {
                Debug.LogException(e);
                onFailure(e.Message);
            }
        }
    }
    public class UnityAccountAuthStrategy : IAuthStrategy
    {
        public async void Authenticate(Action<string> onSuccess, Action<string> onFailure) {

            if (PlayerAccountService.Instance.IsSignedIn) {
                Debug.Log($"Player is SignedIn accessToken {PlayerAccountService.Instance.AccessToken}");
                // If the player is already signed into Unity Player Accounts, proceed directly to the Unity Authentication sign-in.
                await SignInWithUnityAuth();
                return;
            }

            var hasExistingToken = AuthenticationService.Instance.SessionTokenExists;

            Debug.Log($"starting unity auth, session token exist: {(hasExistingToken ? AuthenticationService.Instance.AccessToken : "none")}");

            try {
                // Open the system browser and prompt the user to sign in to Unity Player Accounts
                await PlayerAccountService.Instance.StartSignInAsync();
                onSuccess($"Signed in with Unity : PlayerID: {AuthenticationService.Instance.PlayerId}");
                //await SignInWithUnityAuth();
            }
            catch (PlayerAccountsException ex) {
                // Compare error code to PlayerAccountsErrorCodes
                Debug.Log("PlayerAccountsException");
                Debug.LogException(ex);
                onFailure("PlayerAccountsException");
            }
            catch (RequestFailedException ex) {
                // Compare error code to CommonErrorCodes
                Debug.Log($"RequestFailedException {ex.ErrorCode}");
                Debug.LogException(ex);
                onFailure("RequestFailedException");
            }
        }

        private async Task SignInWithUnityAuth() {

            try {
                await AuthenticationService.Instance.SignInWithUnityAsync(PlayerAccountService.Instance.AccessToken);
                Debug.Log("Sign-in is successful");
            }
            catch (AuthenticationException ex) {
                Debug.LogException(ex);
            }
            catch (RequestFailedException ex) {
                Debug.LogException(ex);
            }
        }

        public async void Register(Action<string> onSuccess, Action<string> onFailure) {

            try {

                Debug.Log($"Trying to Link (isSignedIn?){PlayerAccountService.Instance.IsSignedIn} with accessToken {PlayerAccountService.Instance.AccessToken}");
                /*if (string.IsNullOrEmpty(PlayerAccountService.Instance.AccessToken)) {
                    // If the player has no AccessToken, proceed to the Unity Authentication sign-in.
                    await PlayerAccountService.Instance.StartSignInAsync();
                }*/
                await AuthenticationService.Instance.LinkWithUnityAsync(PlayerAccountService.Instance.AccessToken);
                Debug.Log($"Link to unity with accessToken {PlayerAccountService.Instance.AccessToken}");
                onSuccess("Link is successful");
            }
            catch (AuthenticationException ex) when (ex.ErrorCode == AuthenticationErrorCodes.AccountAlreadyLinked) {
                Debug.LogError("Account already linked");
                onFailure("Error: Already linked to another account");
            }
            catch (AuthenticationException ex) {
                Debug.LogException(ex);
                onFailure("Error: Authentication Failed");
            }
            catch (PlayerAccountsException ex) {
                // Compare error code to PlayerAccountsErrorCodes
                Debug.Log($"PlayerAccountsException {ex.ErrorCode}");
                Debug.LogException(ex);
                onFailure("Error: Player Accounts Failed");
            }
            catch (RequestFailedException ex) {
                // Handle the exception
                // Compare error code to CommonErrorCodes
                Debug.Log($"RequestFailedException {ex.ErrorCode}");
                Debug.LogException(ex);
                onFailure("Error: Request Failed");
            }
            catch (Exception e) {
                Debug.LogException(e);
                onFailure(e.Message);
            }
        }
    }
    public class AnonymousAuthStrategy : IAuthStrategy
    {
        public async void Authenticate(Action<string> onSuccess, Action<string> onFailure) {
            
            try {
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
                Debug.Log($"{AuthenticationService.Instance.PlayerId} Signed in Anonymously");
                onSuccess("Signed in Anonymously");
            
            } catch (Exception e) {
                Debug.LogException(e);
                onFailure(e.Message);
            }
        }
    }
    public class GoogleAuthStrategy : IAuthStrategy, IIdentityProviderStrategy
    {
        public void Authenticate(Action<string> onSuccess, Action<string> onFailure) {
            throw new NotImplementedException();
        }

        public void LinkFB(Action<string> onSuccess, Action<string> onFailure) {
            throw new NotImplementedException();
        }

        public void UnlinkFB(Action<string> onSuccess, Action<string> onFailure) {
            throw new NotImplementedException();
        }
    }
    public class FacebookAuthStrategy : IAuthStrategy, IIdentityProviderStrategy
    {
        string[] perms = new string[] { "openid", "email", "public_profile" };

        //string[] perms = new string[] { "gaming_profile", "gaming_user_picture", "user_friends" };

        public void Authenticate(Action<string> onSuccess, Action<string> onFailure) {
            /*if (!FB.IsInitialized) {
                // Initialize the Facebook SDK
                FB.Init(
                    () => {
                        InitCallback();
                        FBLogin(onSuccess, onFailure);
                    },
                    OnHideUnity
                );
            } else {
                // Already initialized, signal an app activation App Event
                FB.ActivateApp();
                if (!FB.IsLoggedIn)
                    FBLogin(onSuccess, onFailure);
            }*/
        }

        private void FBLogin(Action<string> onSuccess, Action<string> onFailure) {
            /*FB.LogOut();

            //FB.Mobile.LoginWithTrackingPreference(LoginTracking.LIMITED, perms, "nonce123",
            FB.LogInWithReadPermissions(
                perms,
                async result => {
                    // Debug.Log("FACEBOOK IS LOGGED IN " + FB.IsLoggedIn);
                    if (FB.IsLoggedIn) {
                        //var aToken = AccessToken.CurrentAccessToken;
                        //var a = AccessToken.CurrentAccessToken.UserId;
                        // Debug.Log(result);

                        //Debug.Log("Facebook Token: " + AccessToken.CurrentAccessToken.TokenString);
                        Dictionary<string, object> message = JsonSerializer.Deserialize<
                            Dictionary<string, object>
                        >(result.RawResult);
                        // Debug.Log(message);

                        //if (!string.IsNullOrEmpty(result.AuthenticationToken.TokenString))
                        //{
                        //Dictionary<string, string> message = JsonSerializer.Deserialize<Dictionary<string, string>>(result.RawResult);
                        //Debug.Log(message);
                        //}
                        //else
                        //{

                        //    Debug.LogError("Facebook ERRROR" + result.Error);
                        //    Debug.LogError(result.ErrorDictionary);
                        //    onFailure(result.Error);
                        //    return;
                        //}
                        try {
                            // Debug.Log("Unity " + AccessToken.CurrentAccessToken);
                            // Debug.Log("FB Response" + result.AccessToken);
                            //var t = "EAAW74GrSKyQBOZBJzBKeu3YrkOjNOYkzz6Qr1WiPqptfvsQB05ZCEZBqD5PS0bXyQldktLQzrFA3GamAc9gAl5MqX2CuZCmZANA6Nymb0rZApTYl3nVdQJPjhPLdSBwG9iwbVkuE9078tWe1NMnGm5UWqAsX1gg1SMvuNm0XSLrjylEXtLCRiaTLIZD";
                            await Login(AccessToken.CurrentAccessToken.TokenString);
                            //FB.Mobile.RefreshCurrentAccessToken(async result => {
                            //    Debug.Log(result.RawResult);
                            //    await Login(result.AccessToken.TokenString);
                            //});
                            //await Login(t);
                            onSuccess("");
                        }
                        catch (Unity.Services.Authentication.AuthenticationException ex) {
                            // Debug.LogException(ex);
                            // Debug.Log(ex.Message);
                            onFailure("User cancelled login or failed to log in.");
                        }
                        catch (RequestFailedException ex) {
                            // Debug.LogException(ex);
                            // Debug.Log(ex.Message);
                            onFailure("User cancelled login or failed to log in.");
                        }
                    } else {
                        onFailure("NOT LOGGED IN ----- User cancelled login or failed to log in.");
                    }
                }
            );*/
        }

        private void FBLogout(Action<string> onSuccess, Action<string> onFailure) {
            //FB.LogOut();
        }

        public async Task Login(string accessToken) {
            /*await Unity.Services.Authentication.AuthenticationService.Instance.SignInWithFacebookAsync(
                accessToken,
                new() { CreateAccount = true }
            );*/
            Debug.Log("SignIn is successful.");
        }

        void InitCallback() {
            /*if (FB.IsInitialized) {
                // Signal an app activation App Event
                FB.ActivateApp();
                // Continue with Facebook SDK
            } else {
                Debug.Log("Failed to Initialize the Facebook SDK");
            }*/
        }

        void OnHideUnity(bool isGameShown) {
            if (!isGameShown) {
                // Pause the game - we will need to hide
                Time.timeScale = 0;
            } else {
                // Resume the game - we're getting focus again
                Time.timeScale = 1;
            }
        }

        public async void LinkFB(Action<string> onSuccess, Action<string> onFailure) {
            var token = "Face book TOKEN";
            //await AuthenticationService.Instance.LinkWithFacebookAsync(token);
        }

        public async void UnlinkFB(Action<string> onSuccess, Action<string> onFailure) {
            //await AuthenticationService.Instance.UnlinkFacebookAsync();
        }
    }
}