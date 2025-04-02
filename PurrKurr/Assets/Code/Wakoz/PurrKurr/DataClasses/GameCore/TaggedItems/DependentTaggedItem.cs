using Code.Wakoz.PurrKurr.DataClasses.Objectives;
using Code.Wakoz.PurrKurr.Screens.Gameplay_Controller;
using Code.Wakoz.PurrKurr.Screens.PersistentGameObjects;
using Code.Wakoz.PurrKurr.Views;
using System;
using System.Threading.Tasks;
using UnityEngine;

namespace Code.Wakoz.PurrKurr.DataClasses.GameCore.TaggedItems
{
    /// <summary>
    /// Wrapper class for items that change state and depends on a uniqueId of an objective or an itemId from an objective
    /// </summary>
    public class DependentTaggedItem : Controller, ITaggable
    {
        [SerializeField] private string _itemId = "";

        [Tooltip("Sets the View state to active or deactive")]
        [SerializeField] private MultiStateView _state;

        private PersistentGameObject _taggedObject;

        private void OnEnable() {

            _taggedObject = new PersistentGameObject(typeof(DependentTaggedItem), _itemId, transform);
            _taggedObject.OnStateChanged += NotifyStateChanged;
        }

        private void OnDisable() {

            if (_taggedObject == null)
                return;

            _taggedObject.OnStateChanged -= NotifyStateChanged;

            _taggedObject.Dispose();
        }

        private void NotifyStateChanged() {

            var _isCollectable = SingleController.GetController<GameplayController>().Handlers.GetHandler<ObjectivesHandler>().IsCollected(_itemId);

            Debug.Log($"Notified tagged dependent object {transform.name} {_isCollectable} by itemId {_itemId}");

            UpdateStateView(_isCollectable);
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