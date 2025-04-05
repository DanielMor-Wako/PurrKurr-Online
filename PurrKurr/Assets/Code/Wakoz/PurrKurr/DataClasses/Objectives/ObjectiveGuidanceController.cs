using Code.Wakoz.PurrKurr.DataClasses.Objectives;
using Code.Wakoz.PurrKurr.Screens.Gameplay_Controller;
using System.Threading.Tasks;
using UnityEngine;

namespace Code.Wakoz.PurrKurr.DataClasses.GameCore.CollectableItems
{

    [DefaultExecutionOrder(15)]
    public sealed class ObjectiveGuidanceController : Controller
    {
        [SerializeField] private ObjectiveMarker[] _objectiveMarkers;

        private bool _isInitialized = false;
        private ObjectivesHandler _objectivesHandler;
        private GameplayController _gameEvents;

        protected override void Clean() {

            if (_objectivesHandler == null)
                return;

            Unbind();
        }

        protected override Task Initialize() {

            _gameEvents = SingleController.GetController<GameplayController>();

            if (_gameEvents != null) {

                _objectivesHandler = _gameEvents.Handlers.GetHandler<ObjectivesHandler>();

                Bind();
            }

            return Task.CompletedTask;
        }

        private void OnEnable() => Bind();

        private void OnDisable() => Unbind();

        private void Bind() {

            if (_isInitialized || _objectivesHandler == null)
                return;

            _objectivesHandler.OnNewObjectives += HandleObjectiveUpdated;
            _objectivesHandler.OnObjectiveUpdated += HandleObjectiveUpdated;

            _isInitialized = true;
        }

        private void Unbind() {

            if (!_isInitialized || _objectivesHandler == null)
                return;

            _objectivesHandler.OnNewObjectives -= HandleObjectiveUpdated;
            _objectivesHandler.OnObjectiveUpdated -= HandleObjectiveUpdated;

            _isInitialized = false;
        }

        private void HandleObjectiveUpdated() {

            ObjectiveMarker nextMarker = null;

            for (var i = 0; i < _objectiveMarkers.Length; i++) {

                var objMarker = _objectiveMarkers[i];
                if (objMarker == null) {
                    Debug.LogError("missing marker data"); 
                    return;
                }

                var hasNoMarkerAndItemNotCollected = nextMarker == null && !_objectivesHandler.IsCollected(objMarker.ItemId);

                if (hasNoMarkerAndItemNotCollected) {
                    nextMarker = objMarker;
                    Debug.Log($"Guidance marker set by itemId {objMarker.ItemId}");
                }

                objMarker.MarkerRef.gameObject.SetActive(nextMarker?.MarkerRef == objMarker.MarkerRef);
            }

            _gameEvents.SetGuidamceMarker(nextMarker);
        }

    }

    [System.Serializable]
    public class ObjectiveMarker
    {
        [Tooltip("Item id (Unique identifier such as objectiveId, itemId etc)")]
        [SerializeField] private string _itemId;
        [SerializeField] private Transform _markerTransform;

        public string ItemId => _itemId;
        public Transform MarkerRef => _markerTransform;
    }
}