using Code.Wakoz.PurrKurr.DataClasses.GameCore.Detection;
using Code.Wakoz.PurrKurr.DataClasses.ScriptableObjectData;
using Code.Wakoz.PurrKurr.Popups.OverlayWindow;
using Code.Wakoz.PurrKurr.Screens.Gameplay_Controller;
using Code.Wakoz.PurrKurr.Views;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Code.Wakoz.PurrKurr.DataClasses.GameCore.OverlayWindowTrigger
{
    [DefaultExecutionOrder(15)]
    public class ObjectiveZoneTrigger : DetectionZoneTrigger
    {
        [Tooltip("When ref exist to scriptable object, it overrides the windowData from the scriptable data")]
        [SerializeField] private OverlayWindowDataSO _scriptableObjectData;

        [SerializeField] private ObjectiveSequenceDataSO _sequenceData;

        [Tooltip("Sets the View state to active and deactive")]
        [SerializeField] private MultiStateView _state;

        private GameplayController _gameplayController;
        private Coroutine _activeStateCO;

        public ObjectiveSequenceDataSO SequenceData => _sequenceData;

        protected override void Clean()
        {
            base.Clean();

            OnColliderEntered -= HandleColliderEntered;
            OnColliderExited -= HandleColliderExited;
        }

        protected override Task Initialize()
        {
            base.Initialize();

            _gameplayController ??= SingleController.GetController<GameplayController>();

            OnColliderEntered += HandleColliderEntered;
            OnColliderExited += HandleColliderExited;

            LoadDataFromScriptableObject();
            AddPageCount();
            UpdateStateView(false);

            return Task.CompletedTask;
        }

        private void LoadDataFromScriptableObject()
        {
            if (_scriptableObjectData == null)
            {
                return;
            }

            var defaultData = _scriptableObjectData.GetData();
            UpdateWindowData(defaultData);
        }

        private void AddPageCount()
        {
            List<OverlayWindowData> pagesData = GetWindowData();
            var pageCount = pagesData.Count;

            if (pagesData == null)
            {
                return;
            }

            if (pageCount == 1)
            {
                var page = pagesData[0];
                if (page != null)
                {
                    page.PageCount = null;
                }

                return;
            }

            for (int i = 0; i < pageCount; i++)
            {
                var page = pagesData[i];
                page.PageCount = $"{i + 1} / {pageCount}";

            }

        }

        private void HandleColliderExited(Collider2D triggeredCollider)
        {
            UpdateStateView(false);

            if (_gameplayController == null)
            {
                Debug.LogError("_gameplayController is missing");
                return;
            }

            _gameplayController.OnExitDetectionZone(this, triggeredCollider);
        }

        private void HandleColliderEntered(Collider2D triggeredCollider)
        {
            UpdateStateView(true);

            if (_gameplayController == null)
            {
                Debug.LogError("_gameplayController is missing");
                return;
            }

            _gameplayController.OnEnterDetectionZone(this, triggeredCollider);
        }

        private void UpdateStateView(bool activeState)
        {
            if (_state == null)
            {
                return;
            }

            _state.ChangeState(Convert.ToInt32(!activeState));

            if (activeState)
            {
                ClearExistingCO();
                _activeStateCO = StartCoroutine(ChangeStateToFalseWithDelay());
            }
        }

        private void ClearExistingCO()
        {
            if (_activeStateCO != null)
            {
                StopCoroutine(_activeStateCO);
                _activeStateCO = null;
            }
        }

        private IEnumerator ChangeStateToFalseWithDelay()
        {
            yield return new WaitForSeconds(1);

            UpdateStateView(false);
        }
    }
}