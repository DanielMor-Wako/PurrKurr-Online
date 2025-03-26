using Code.Wakoz.PurrKurr.DataClasses.Characters;
using Code.Wakoz.PurrKurr.DataClasses.Enums;
using Code.Wakoz.PurrKurr.Screens.Gameplay_Controller;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Code.Wakoz.PurrKurr.Screens.Ui_Controller.Bars
{

    [DefaultExecutionOrder(11)]
    public class UIBarsController : SingleController {

        [SerializeField] private UIBarsView _view;
        [SerializeField][Min(0)] private float LastSecondsToNotifyBeforeTimeRunsOut = 10;
        
        private UIBarsModel _model;

        private GameplayController _hero;

        protected override void Clean() {

            DeregisterEvents();
        }

        protected override Task Initialize() {

            return Task.CompletedTask;
        }
        
        public void Init(Character2DController hero = null) {

            _model = new UIBarsModel(hero.Stats.GetHealthPercentage(), hero.Stats.Health.ToString(), 0);
            _view.SetModel(_model);
        }

        public bool TryBindToCharacter(GameplayController character) {
            return RegisterEvents(character);
        }

        private bool RegisterEvents(GameplayController character = null) {

            if (character == null) {
                Debug.LogWarning("ui display has no available character events for input");
                return false;
            }

            if (_hero != null) {
                _hero.OnStatsChanged -= OnStatsChanged;
                _hero.OnTimerChanged -= OnTimerChanged;
            }

            _hero = character;
            _hero.OnStatsChanged += OnStatsChanged;
            _hero.OnTimerChanged += OnTimerChanged;
            
            return true;
        }

        private void DeregisterEvents() {

            if (_hero == null) {
                return;
            }
            
            _hero.OnStatsChanged -= OnStatsChanged;
            _hero.OnTimerChanged -= OnTimerChanged;

            _hero = null;
        }
        
        private void OnStatsChanged(List<DisplayedableStatData> list) {

            if (list == null) {
                return;
            }

            // todo: go thru each of the stats and update their ui, or move this to ui display?

            float modifiedHP = -1;
            string displayText = "";
            foreach (var stat in list) {
                switch (stat.Type) {

                    case Definitions.CharacterDisplayableStat.Health:
                        modifiedHP = stat.Percentage;
                        displayText = stat.DisplayText;
                        break;
                }
            }

            if (modifiedHP > -1) {
                Debug.LogWarning("modifiedHP = "+ modifiedHP+" <--- "+_model.HealthPercent);
                _model.UpdateHpStat(modifiedHP, displayText);
            }
        }

        private void OnTimerChanged(float secondsLeft, float totalSeconds) {

            _model.UpdateTimerStat(
                Mathf.Clamp01(secondsLeft / totalSeconds), 
                Mathf.Floor(secondsLeft) <= LastSecondsToNotifyBeforeTimeRunsOut || secondsLeft == totalSeconds,
                $"{TimeSpan.FromSeconds(secondsLeft).ToString(@"mm\:ss")}"
                );
        }

    }
}
