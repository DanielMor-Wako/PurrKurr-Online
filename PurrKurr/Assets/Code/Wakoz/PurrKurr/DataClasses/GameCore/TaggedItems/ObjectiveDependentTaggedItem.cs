using Code.Wakoz.PurrKurr.DataClasses.Objectives;
using Code.Wakoz.PurrKurr.Screens.Gameplay_Controller;
using Code.Wakoz.PurrKurr.Screens.PersistentGameObjects;
using Code.Wakoz.PurrKurr.Views;
using System;
using System.Threading.Tasks;
using UnityEngine;

namespace Code.Wakoz.PurrKurr.DataClasses.GameCore.TaggedItems
{

    public class ObjectiveDependentTaggedItem : Controller, ITaggable
    {
        [SerializeField] private string _objectiveId = "";

        [Tooltip("Sets the View state to active or deactive")]
        [SerializeField] private MultiStateView _state;

        private PersistentGameObject _taggedObject;

        private void OnEnable() {

            _taggedObject = new PersistentGameObject(typeof(DependentTaggedItem), _objectiveId, transform);
            _taggedObject.OnStateChanged += NotifyStateChanged;
        }

        private void OnDisable() {

            if (_taggedObject == null)
                return;

            _taggedObject.OnStateChanged -= NotifyStateChanged;

            _taggedObject.Dispose();
        }

        private void NotifyStateChanged() {

            var IsComplete = SingleController.GetController<GameplayController>().Handlers.GetHandler<ObjectivesHandler>().IsObjectiveCompleted(_objectiveId);

            Debug.Log($"Notified tagged dependent object {transform.name} {IsComplete} by objectiveId {_objectiveId}");

            UpdateStateView(IsComplete);
        }

        private void UpdateStateView(bool activeState) {

            if (_state == null) {
                return;
            }

            _state.ChangeState(Convert.ToInt32(activeState));
        }

        protected override Task Initialize() {

            return Task.CompletedTask;
        }

        protected override void Clean() { }

    }
}