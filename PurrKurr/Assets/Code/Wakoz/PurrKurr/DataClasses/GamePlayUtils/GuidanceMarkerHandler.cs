using Code.Wakoz.PurrKurr.DataClasses.GameCore.CollectableItems;
using Code.Wakoz.PurrKurr.DataClasses.Objectives;
using Code.Wakoz.PurrKurr.DataClasses.ScriptableObjectData;
using Code.Wakoz.PurrKurr.Screens.Gameplay_Controller;
using Code.Wakoz.PurrKurr.Screens.Gameplay_Controller.Handlers;
using UnityEngine;

namespace Code.Wakoz.PurrKurr.DataClasses.GamePlayUtils
{
    public class GuidanceMarkerHandler : IBindableHandler
    {
        private GameplayController _gameEvents;
        private UiGuidanceController _controller;

        private bool _forceHideMarker;
        private ObjectiveMarker _currentMarker;

        public GuidanceMarkerHandler(GameplayController gameEvents, UiGuidanceController controller) {
            _gameEvents = gameEvents;
            _controller = controller;
        }

        public void Bind() {

            if (_gameEvents == null)
                return;

            _gameEvents.OnHideGuidanceMarker += HideMarker;
            _gameEvents.OnShowGuidanceMarker += ShowMarker;

            _gameEvents.OnObjectiveMissionStarted += StartForceHiding;
            _gameEvents.OnHeroReposition += StopForceHiding;
            _gameEvents.OnHeroDeath += TagNoneActiveMission;
        }

        public void Unbind() {

            if (_gameEvents == null)
                return;

            _gameEvents.OnHideGuidanceMarker -= HideMarker;
            _gameEvents.OnShowGuidanceMarker -= ShowMarker;

            _gameEvents.OnObjectiveMissionStarted -= StartForceHiding;
            _gameEvents.OnHeroReposition -= StopForceHiding;
            _gameEvents.OnHeroDeath -= TagNoneActiveMission;
        }

        public void Dispose() {

            _gameEvents = null;
        }

        private void HideMarker() {
            
            _currentMarker = null;
            _controller.DeactivateMarker();
        }

        private void ShowMarker(ObjectiveMarker marker) {
            
            _currentMarker = marker;

            if (_forceHideMarker) {
                _controller.DeactivateMarker();
                return;
            }
            
            _controller.ActivateMarker(marker);
        }

        private void ForceHideMarker(bool isHiding) {

            _forceHideMarker = isHiding;

            if (_forceHideMarker || _currentMarker == null) {
            
                _controller.DeactivateMarker();
                return;
            }
            
            _controller.ActivateMarker(_currentMarker);
        }

        private void StartForceHiding(IObjective objective, ObjectiveSequenceDataSO sO)
            => ForceHideMarker(true);

        private void StopForceHiding(Vector3 vector) 
            => ForceHideMarker(false);

        private void TagNoneActiveMission() 
            => ForceHideMarker(false);

    }
}