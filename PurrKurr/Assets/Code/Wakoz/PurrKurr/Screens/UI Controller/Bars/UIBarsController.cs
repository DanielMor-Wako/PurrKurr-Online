using Code.Wakoz.PurrKurr.DataClasses.Characters;
using Code.Wakoz.PurrKurr.DataClasses.Enums;
using Code.Wakoz.PurrKurr.Screens.Gameplay_Controller;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Code.Wakoz.PurrKurr.Screens.Ui_Controller.Bars {
    [DefaultExecutionOrder(11)]
    public class UIBarsController : SingleController {

        [SerializeField] private UIBarsView _view;
        
        private UIBarsModel _model;

        private GameplayController _playerInput;

        protected override void Clean() {

            DeregisterEvents();
        }

        protected override Task Initialize() {

            return Task.CompletedTask;
        }
        
        public void Init(Character2DController hero = null) {

            // todo: check the hero abilities fir displaying the correct ui update, like Health -> hp, power, supurr etc...

            _model = new UIBarsModel(hero.Stats.Health / hero.Stats.MaxHealth, 1);

            _view.SetModel(_model);
        }
        public bool TryBindToCharacterController(GameplayController character) {
            return RegisterEvents(character);
        }

        private bool RegisterEvents(GameplayController character = null) {

            if (_playerInput != null) {
                return false;
            }

            if (character == null) {
                Debug.LogWarning("ui display has no available character events for input");
                return false;
            }

            Debug.Log("ui display has available character to set");

            _playerInput = character;
            _playerInput.OnStatsChanged += OnStatsChanged;
            
            return true;
        }

        private void OnStatsChanged(List<DisplayedableStatData> list) {

            if (list == null) {
                return;
            }

            // todo: go thru each of the stats and update their ui, or move this to ui display?

            float modifiedHP = -1;
            foreach (var i in list) { 
                if (i.Type == Definitions.CharacterDisplayableStat.Health) {
                    modifiedHP = i.Percentage;
                }
            }

            if (modifiedHP > -1) {
                Debug.LogWarning("modifiedHP = "+ modifiedHP+" <--- "+_model.HealthPercent);
                _model.UpdateUiStat(modifiedHP, 1);
            }
            
        }

        private void DeregisterEvents() {

            if (_playerInput == null) {
                return;
            }
            
            //_playerInput.OnStatsChanged -= OnStatsChanged;

            _playerInput = null;
        }
        

    }
}
