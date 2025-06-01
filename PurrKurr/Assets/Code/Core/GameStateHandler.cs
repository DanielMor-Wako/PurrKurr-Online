using Code.Wakoz.PurrKurr.Screens.Gameplay_Controller.Handlers;
using Code.Wakoz.PurrKurr.Screens.Gameplay_Controller;
using Code.Wakoz.PurrKurr.DataClasses.ServerData;
using System.Collections.Generic;
using Code.Wakoz.PurrKurr.DataClasses.Objectives;
using Code.Wakoz.PurrKurr;
using Code.Wakoz.PurrKurr.Screens.Levels;
using Code.Wakoz.PurrKurr.DataClasses.Enums;

namespace Code.Core
{
    public class GameStateHandler : IBindableHandler
    {
        private GameplayController _gameEvents;
        private GameManager _controller;

        private ObjectivesHandler _objectivesHandler;

        public GameStateHandler(GameplayController gameEvents, GameManager controller) {
            _gameEvents = gameEvents;
            _controller = controller;
        }

        public void Bind() {

            if (_gameEvents == null)
                return;

            _gameEvents.OnSavePlayerProgress += SaveProgress;
            _gameEvents.OnPlayerProgressLoaded += HandleNewProgressData;
        }

        public void Unbind() {

            if (_gameEvents == null)
                return;

            _gameEvents.OnSavePlayerProgress -= SaveProgress;
            _gameEvents.OnPlayerProgressLoaded -= HandleNewProgressData;
        }

        public void Dispose() {

            _gameEvents = null;
            _controller = null;
        }

        private void SaveProgress() {
            
            SaveObjectivesData();
        }

        private void SaveObjectivesData() {

            _objectivesHandler ??= _gameEvents.Handlers.GetHandler<ObjectivesHandler>();
            var objectivesData = _objectivesHandler.GetAllData();

            var completedData = new CompletedObjectives_SerializeableData() {
                objectives = objectivesData.completedObjectivesId
            };
            var ongoingData = new OngoingObjectives_SerializeableData() {
                objectives = objectivesData.objectivesOngoing
            };
            var levelIndex = SingleController.GetController<LevelsController>().CurrentLevelIndex;
            var gameStateData = new GameState_SerializeableData() {
                roomId = levelIndex,
                objects = new List<ObjectRecord>() {
                    new ObjectRecord() {key = "player", value = UnityEngine.Vector2.zero.ToString() } }
            };
            UnityEngine.Debug.Log($"Saving Progress- roomId:{gameStateData.roomId}\nongoing:{ongoingData.objectives.Count}\ndone:{completedData.objectives.Count}\nstate:{gameStateData.objects}");
            _controller.SaveProgress(completedData, ongoingData, gameStateData);
        }

        private void HandleNewProgressData() {

            var completed = _controller.CompletedObjectivesData;
            var ongoing = _controller.OngoingObjectivesData;
            
            _objectivesHandler ??= _gameEvents.Handlers.GetHandler<ObjectivesHandler>();

            _objectivesHandler?.SetCompletedObjectivesData(completed.objectives);
            _objectivesHandler?.SetOngoingObjectivesData(ongoing.objectives);
        }

        public List<Definitions.ActionType> GetCharacterAbilitiesFromCollectedObjectives() {
            
            if (_objectivesHandler == null) return null;

            var abilitiesToGainFromCompletedObjectiveIds = new Dictionary<string, Definitions.ActionType>() {
                ["CollectAbility_Jump"] = Definitions.ActionType.Jump, 
                ["CollectAbility_Block"] = Definitions.ActionType.Block,
                ["CollectAbility_Attack"] = Definitions.ActionType.Attack,
                ["CollectAbility_Grab"] = Definitions.ActionType.Grab,
                ["CollectAbility_Projectile"] = Definitions.ActionType.Projectile,
                ["CollectAbility_Special"] = Definitions.ActionType.Special,
                ["CollectAbility_Rope"] = Definitions.ActionType.Rope,
            };

            var result = new List<Definitions.ActionType>();
            foreach( var ability in abilitiesToGainFromCompletedObjectiveIds ) {
                if (_objectivesHandler.IsObjectiveCompleted(ability.Key)) {
                    result.Add(ability.Value);
                }
            }

            return result;
        }

        public bool HasCompletedTutorial() {

            if (_objectivesHandler == null)
                return false;

            // todo: make the ids not hardcoded
            var mainObjectiveIds = new List<string>() {
                "CollectAbility_Jump",
                "CollectAbility_Block",
                "CollectAbility_Attack",
                "CollectAbility_Grab",
                "CollectAbility_Special",
                "Reached_FinalSync"
            };

            foreach (var ability in mainObjectiveIds) {
                if (!_objectivesHandler.IsObjectiveCompleted(ability)) {
                    return false;
                }
            }

            return true;
        }
    }
}
