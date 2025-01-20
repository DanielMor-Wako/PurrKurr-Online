using Code.Wakoz.PurrKurr.DataClasses.GameCore.Detection;
using Code.Wakoz.PurrKurr.DataClasses.ScriptableObjectData;
using Code.Wakoz.PurrKurr.Popups.OverlayWindow;
using Code.Wakoz.PurrKurr.Screens.Gameplay_Controller;
using Code.Wakoz.PurrKurr.UI.Instructions;
using Code.Wakoz.PurrKurr.Views;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Code.Wakoz.PurrKurr.DataClasses.GameCore.OverlayWindowTrigger
{

    [DefaultExecutionOrder(15)]
    public class OverlayWindowTrigger : DetectionZoneTrigger {

        [Tooltip("When ref exist to scriptable object, it overrides the windowData from the scriptable data")]
        [SerializeField] private OverlayWindowDataSO _scriptableObjectData;
        [SerializeField] private OverlayWindowConditions _conditions;
        [Tooltip("Sets the max activation for the window. 0 counts as infinite activations")]
        [SerializeField][Min(0)] private int _activationLimit = 0;
        [Tooltip("Sets the View state to active and deactive")]
        [SerializeField] private MultiStateView _state;
        
        private IConditionChecker _condition;

        private GameplayController _gameplayController;
        private int _activationsCount = 0;

        public void TurnOffWindow() {
            UpdateStateView(false);
        }

        public IConditionChecker GetCondition(int pageIndex = 0) {

            if (_conditions == null) {
                return null;
            }

            _condition = _conditions.GetEventCondition(pageIndex);
            return _condition;
        }

        public List<OverlayWindowAnimationData> GetAnimationData(int pageIndex = 0) {

            if (_conditions == null) {
                return null;
            }

            return _conditions.GetAnimationData(pageIndex);
        }

        public bool IsAffectingTimeScale(int pageIndex = 0)
        {

            if (_conditions == null)
            {
                return false;
            }

            return _conditions.IsApplyingSlowMotion();
        }

        protected override void Clean() {
            base.Clean();

            OnColliderEntered -= handleColliderEntered;
            OnColliderExited -= handleColliderExited;
        }

        protected override Task Initialize() {
            base.Initialize();

            _gameplayController ??= SingleController.GetController<GameplayController>();

            OnColliderEntered += handleColliderEntered;
            OnColliderExited += handleColliderExited;

            LoadDataFromScriptableObject();

            AddPageCount();

            UpdateStateView(false);

            return Task.CompletedTask;
        }

        private void LoadDataFromScriptableObject() {
            
            if (_scriptableObjectData == null) {
                return;
            }

            var defaultData = _scriptableObjectData.GetData();
            UpdateWindowData(defaultData);
        }

        private void handleColliderExited(Collider2D triggeredCollider) {

            if (_gameplayController == null) {
                Debug.LogError("_gameplayController is missing");
                return;
            }

            if (HasReachedMaxCount()) {
                return;
            }

            if (_gameplayController.OnExitDetectionZone(this, triggeredCollider)) {

                _activationsCount++;
                _condition?.EndCheck();
            }

            UpdateStateView(false);

        }

        private void handleColliderEntered(Collider2D triggeredCollider) {

            if (_gameplayController == null) {
                Debug.LogError("_gameplayController is missing");
                return;
            }

            if (HasReachedMaxCount()) {
                return;
            }

            _gameplayController.OnEnterDetectionZone(this, triggeredCollider);

            UpdateStateView(true);
        }

        private bool HasReachedMaxCount() => _activationLimit > 0 && _activationsCount >= _activationLimit;

        private void AddPageCount() {

            List<OverlayWindowData> pagesData = GetWindowData();
            var pageCount = pagesData.Count;

            if (pagesData == null) {
                return;
            }

            if (pageCount == 1) {
                var page = pagesData[0];
                if (page != null) {
                    page.PageCount = null;
                }
                return;
            }

            for (int i = 0; i < pageCount; i++) {

                var page = pagesData[i];
                page.PageCount = $"{i + 1}/{pageCount}";

            }

        }

        private void UpdateStateView(bool activeState) {

            if (_state == null) {
                return;
            }

            _state.ChangeState(activeState ? 0 : !HasReachedMaxCount() ? 1 : 2);
        }
        
    }
}