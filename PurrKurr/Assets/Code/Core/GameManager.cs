using Code.Core.Services.DataManagement.Serializers;
using Code.Core.Services.DataManagement.Storage;
using Code.Wakoz.PurrKurr;
using Code.Wakoz.PurrKurr.DataClasses.ServerData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.CloudSave;
using Unity.Services.CloudSave.Models.Data.Player;
using UnityEngine;

namespace Code.Core {

    [DefaultExecutionOrder(10)]
    public class GameManager : SingleController {

        //[Header("Prefab Bank")]
        //[SerializeField] private PrefabBank _prefabBank;

        public float DataProgressInPercent { get; private set; }
        private CancellationTokenSource _loadPlayerDataCTS;

        public string UserId { get; private set; }
        public string DisplayName { get; private set; }

        // todo: gather into single class for player data
        public CompletedObjectives_SerializeableData CompletedObjectivesData { get; private set; }
        public OngoingObjectives_SerializeableData OngoingObjectivesData { get; private set; }
        public GameState_SerializeableData GameStateData { get; private set; }
        public MainCharacter_SerializeableData MainCharacterData { get; private set; }
        public int LevelData { get; private set; }
        public int ExpData { get; private set; }
        public CharactersIDs_SerializeableData CharacterIdsData { get; private set; }

        //[Header("View")]
        //[SerializeField] private PlayerView _playerView;

        private GameStateManager _gameStateManager;
        
        public async Task WaitUntilLoadComplete(Action onCallback = null) {

            if (DataProgressInPercent < 1) {

                _loadPlayerDataCTS ??= new CancellationTokenSource();
                await WaitUntil(() => DataProgressInPercent == 1, onCallback, _loadPlayerDataCTS.Token);
            }
        }

        public async Task DeleteGameData() {

            DataProgressInPercent = 0f;
            await _gameStateManager.DeleteAllAsync();
        }

        [ContextMenu("Load Game State")]
        public async void LoadGameState() {

            DataProgressInPercent = 0.01f;

            await RetrieveEverything();
            //var data = await CloudSaveService.Instance.Data.Player.LoadAllAsync();
            //var data = await CloudSaveService.Instance.Data.Player.LoadAllAsync(new LoadAllOptions(new PublicWriteAccessClassOptions));


            var allkeys = await _gameStateManager.GetAllKeys();

            var keysCount = allkeys.Count();
            var keysLoaded = keysCount > 0 ? 0 : 1;
            var keySet = allkeys.ToHashSet(); //new HashSet<string>(allkeys.Select(k => k));
            Debug.Log($"{keysCount} keys are listed");
            foreach (var key in keySet) {
                await Load(key);
                keysLoaded++;
                DataProgressInPercent = (float)keysLoaded / keysCount;
            }

            InvokeInstanceResults();

            //var playerData = instances["playerData"] as PlayerData;
        }

        private async Task RetrieveEverything() {
            try {
                // If you wish to load only a subset of keys rather than everything, you
                // can call a method LoadAsync and pass a HashSet of keys into it.
                Debug.Log($"Retrieve Everything");
                var results = await CloudSaveService.Instance.Data.Player.LoadAllAsync();

                Debug.Log($"{results.Count} elements loaded!");

                foreach (var result in results) {
                    Debug.Log($"Key: {result.Key}, Value: {result.Value.Value}");
                }
                //await _gameStateManager.CacheDataInstances(results);
            }
            catch (CloudSaveValidationException e) {
                Debug.LogError(e);
            }
            catch (CloudSaveRateLimitedException e) {
                Debug.LogError(e);
            }
            catch (CloudSaveException e) {
                Debug.LogError(e);
            }
        }

        private void InvokeInstanceResults() {
            
            var instances = _gameStateManager.GetSerializedInstances();
            if (instances == null || instances.Count == 0) {
                Debug.LogWarning("Instances data could not be found");
                return;
            }

            Debug.Log($"{instances.Count} Instances data are serialized");
            foreach (var instance in instances) {
                InvokeInstance(instance);
            }

            DataProgressInPercent = 1;
        }

        private void InvokeInstance(KeyValuePair<string, object> instance) {
            var key = instance.Key;
            var value = instance.Value;
            switch (key) {
                case "characters":
                    CharacterIdsData = value as CharactersIDs_SerializeableData;
                    Debug.Log($"characters owned: {CharacterIdsData.characters.Count}");
                    break;
                case "completedObjectives":
                    CompletedObjectivesData = value as CompletedObjectives_SerializeableData;
                    Debug.Log($"completedObjectivesData: {CompletedObjectivesData.objectives.Count}");
                    break;
                case "ongoingObjectives":
                    OngoingObjectivesData = value as OngoingObjectives_SerializeableData;
                    Debug.Log($"ongoingObjectivesData: {OngoingObjectivesData.objectives.Count}");
                    break;
                case "gameState":
                    GameStateData = value as GameState_SerializeableData;
                    Debug.Log($"gameState: roomId {GameStateData.roomId} with {GameStateData.objects.Count} objects");
                    break;
                case "exp":
                    ExpData = (int)value;
                    Debug.Log($"exp {ExpData}");
                    break;
                case "level":
                    LevelData = (int)value;
                    Debug.Log($"level {LevelData}");
                    break;
                case "mainCharacter":
                    MainCharacterData = value as MainCharacter_SerializeableData;
                    Debug.Log($"mainCharacter {MainCharacterData.id}");
                    break;
            }
        }

        private async Task Load(string key) {
            /*
            var registered = _gameStateManager.GetRegisteredInstances();
            foreach (var item in registered.Values) {
                var itemtype = item.GetType();
                await _gameStateManager.LoadGameState<item>();
            }
            */

            //await _gameStateManager.LoadGameState(key);

            //Public Keys: exp | level | mainCharacterData |
            //Private Keys: characters | completedObjectives | ongoingObjectives | gameState | 
            // make all these into serializeable classes and gather them under one data as user template
            switch (key) {
                case "characters":
                    await _gameStateManager.LoadGameState<CharactersIDs_SerializeableData>(key);

                    break;
                case "completedObjectives":
                    await _gameStateManager.LoadGameState<CompletedObjectives_SerializeableData>(key);

                    break;
                case "ongoingObjectives":
                    await _gameStateManager.LoadGameState<OngoingObjectives_SerializeableData>(key);

                    break;
                case "gameState":
                    await _gameStateManager.LoadGameState<GameState_SerializeableData>(key);

                    break;
                case "exp":
                    await _gameStateManager.LoadGameState<int>(key);

                    break;
                case "level":
                    await _gameStateManager.LoadGameState<int>(key);

                    break;
                case "mainCharacter":
                    await _gameStateManager.LoadGameState<MainCharacter_SerializeableData>(key);
                    
                    break;
            }
        }

        /// <summary>
        /// Todo: implement documentation for new player creation: 
        /// https://docs.unity.com/ugs/en-us/manual/cloud-code/manual/triggers/tutorials/use-cases/initialize-new-players
        /// </summary>
        [ContextMenu("Save Default Game State")]
        public async void SaveDefaultGameState() {

            DataProgressInPercent = 0;
            
            var characterId = UnityEngine.Random.Range(1, 3);
            LevelData = 1;
            ExpData = 0;
            MainCharacterData = new MainCharacter_SerializeableData(characterId);
            
            CharacterIdsData = new CharactersIDs_SerializeableData(new List<string>() { characterId.ToString() });
            CompletedObjectivesData = new CompletedObjectives_SerializeableData();
            OngoingObjectivesData = new OngoingObjectives_SerializeableData();
            GameStateData = new GameState_SerializeableData() {
                roomId = 0,
                objects = new List<ObjectRecord>() {
                    new ObjectRecord() { key = "player", value = Vector2.zero.ToString() }
                }
            };

            var publicData = new (string key, object value)[]
            {
                ("level", LevelData),
                ("exp", ExpData),
                ("mainCharacter", MainCharacterData),
            };
            var privateData = new (string key, object value)[]
            {
                ("completedObjectives", CompletedObjectivesData),
                ("ongoingObjectives", OngoingObjectivesData),
                ("characters", CharacterIdsData),
                ("gameState", GameStateData),
            };

            await _gameStateManager.SavePlayerPublicData(publicData);

            await _gameStateManager.SavePlayerPrivateData(privateData);

            DataProgressInPercent = 1;

            return;
            
            /*var instances = _gameStateManager.GetSerializedInstances();

            instances["playerData"] = new PlayerData() { displayName = "test", level = 1 };

            await _gameStateManager.SaveGameState("data", "new");*/
        }

        public async void SaveProgress(
            CompletedObjectives_SerializeableData completed,
            OngoingObjectives_SerializeableData ongoing,
            GameState_SerializeableData gameState) 
            {

            var privateData = new (string key, object value)[]
            {
                ("completedObjectives", completed),
                ("ongoingObjectives", ongoing),
                ("gameState", gameState),
            };
            
            await _gameStateManager.SavePlayerPrivateData(privateData);
        }
        /*
                private void Start() {

                    var serializer = new CloudSaveSerializer();
                    var storage = new CloudSaveStorage();

                    GetPrefabBank();

                    _gameStateManager = new GameStateManager(storage, serializer);

                    UpdatePlayerInfo();
                }
        */

        private void SetNoPlayer() => SetPlayerIdAndDisplayName("", "");

        private void SetPlayerIdAndDisplayName(string userId, string playerName) {
            UserId = userId;
            DisplayName = playerName;
        }

        private async void UpdatePlayerInfo() {

            var auth = AuthenticationService.Instance;

            if (auth == null) {
                SetNoPlayer();
                Debug.LogError("AuthenticationService.Instance not initialized");
                return;
            }

            var player = auth.PlayerInfo;

            if (player == null)
                await auth.GetPlayerInfoAsync();

            if (player == null) {
                SetNoPlayer();
                Debug.LogError("no player info");
                return;
            }

            var id = AuthenticationService.Instance.PlayerInfo.Id;
            var playerName = AuthenticationService.Instance.PlayerName;
            var displayName = GetDisplayName(playerName);

            SetPlayerIdAndDisplayName(id, displayName);

            var userLevel = await _gameStateManager.GetUserLevel();
            var isFirstTime = userLevel <= 0;

            if (isFirstTime) {
                Debug.Log($"New account, override to default data\nId {id}");
                SaveDefaultGameState();
            } else {
                Debug.Log($"Loading account data\nId {id}");
                Debug.Log($"{playerName} mainCharacter Id {userLevel}");
                // set the player with its own data
                LoadGameState();
            }
        }

        /// <summary>
        /// Strips the input player name from the suffix, if none exist it returns the input value
        /// </summary>
        /// <param name="playerName"></param>
        /// <returns></returns>
        private string GetDisplayName(string playerName) {

            if (string.IsNullOrEmpty(playerName))
                return string.Empty;
            return playerName.Contains("#") ? playerName.Split('#')[0] : playerName;
        }

        private void GetPrefabBank() {

            //_prefabBank ??= Resources.Load<PrefabBank>("PrefabBank/PrefabBank");
/*
            if (_prefabBank == null) {
                Debug.LogError("PrefabBank not found in Resources folder. Please check that the PrefabBank is located at Assets/Resources/PrefabBank/PrefabBank.asset");
            }*/
        }

        protected override Task Initialize() {

            // keep between scenes and save data, possible to be saved in a scriptable object
            transform.SetParent(null);
            DontDestroyOnLoad(this);

            // Set up
            var serializer = new CloudSaveSerializer();
            var storage = new CloudSaveStorage();

            GetPrefabBank();

            _gameStateManager = new GameStateManager(storage, serializer);

            UpdatePlayerInfo();

            return Task.CompletedTask;
        }

        protected override void Clean() {
            CancelLoadingPlayerData();
        }

        private async Task WaitUntil(Func<bool> condition, Action onCallback = null, CancellationToken cancellationToken = default) {
            while (!condition()) {
                await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
                Debug.Log($"LoadProgress % {Mathf.CeilToInt(DataProgressInPercent * 100)}");
                onCallback?.Invoke();
            }
        }

        private void CancelLoadingPlayerData() {
            _loadPlayerDataCTS?.Cancel();
            _loadPlayerDataCTS = null;
        }
    }
}
