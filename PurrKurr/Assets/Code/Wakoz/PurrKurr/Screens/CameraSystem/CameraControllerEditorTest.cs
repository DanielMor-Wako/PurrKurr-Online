using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;

namespace Code.Wakoz.PurrKurr.Screens.CameraSystem
{

#if UNITY_EDITOR
    
    [RequireComponent(typeof(CameraController))]
    public class CameraControllerEditorTest : MonoBehaviour
    {
        [SerializeField] private CameraController _camController;

        [Header("Refresh Rate")]
        [Tooltip("Value affects the refresh of inspecting current data. 0 = no limit")]
        [SerializeField][Min(0)] private int _refreshRateLimit = 0;

        [Header("Inspect Current Data")]
        [SerializeField] private Vector2 _currentOffset;
        [SerializeField] private int _camerasInQueue;
        [SerializeField] private CameraData _currentCamera;
        [SerializeField] private List<Transform> _targetsList;

        private int _refreshRate = 0;
        private CameraCharacterHandler _mediator;
        private CameraHandler _handler;

        private void Start()
        {
#if !UNITY_EDITOR
            Destroy(this);
            Debug.LogWarning("The script is for camera test only, and should not exist outside of the editor");
#endif
            _camController ??= GetComponent<CameraController>();
        }

        private void AddCamera(CameraData data, Action processActionCallback = null)
        {
            _camController.AddCamera(data, processActionCallback);
        }

        private void FixedUpdate()
        {
            if (!CanRefreshValues())
            {
                return;
            }

            UpdateCurrentCameraValues();
        }
        
        private bool CanRefreshValues()
        {
            if (_refreshRate >= _refreshRateLimit)
            {
                _refreshRate = 0;
                return true;
            }

            _refreshRate++;
            return false;
        }

        private void UpdateCurrentCameraValues()
        {
            SetMediator();
            SetHandler();

            if (_handler == null)
            {
                return;
            }

            _currentOffset = _handler.CurrentOffset;
            _currentCamera = (_handler.CurrentCamera as BaseCamera).Data;
            _targetsList = _handler.CurrentCamera.Data.Targets.ToList();
            _camerasInQueue = _handler.CamerasQueueCount();
        }

        private void SetHandler()
        {
            _handler ??= _mediator.CameraHandler;
        }

        private void SetMediator()
        {
            _mediator ??= _camController.CharacterMediator as CameraCharacterHandler;
        }


        #region Tests In Inspector

        [ContextMenu("Test Interface - NotifyCameraChange(BaseCamera) - Focus To Single")]
        public void SequenceCameraFocusToSingle_Interface()
        {
            var focusCamera = new FollowSingleTarget(new CameraData(new Transform[1] { _mediator.FocusTransform }, new LifeCycleData(1.5f) ));
            _mediator.NotifyCameraChange(focusCamera);

            var characterCamera = new FollowSingleTarget(new CameraData(new Transform[1] { _mediator.CharacterTransform }, new LifeCycleData(1.5f) ));
            _mediator.NotifyCameraChange(characterCamera);
        }

        [ContextMenu("Test - Observe Entire Level")]
        public void SequenceCameraObserveEntireLevel()
        {
            var focusCamera = new FollowSingleTarget(new CameraData(new Transform[1] { _mediator.CharacterTransform }, new LifeCycleData(3f),
                new CameraTransitionsData() { MaxZoom = 25 , ZoomSpeed = 5f } ));
            _mediator.NotifyCameraChange(focusCamera);
        }

        [ContextMenu("Override Settings - To single")]
        public void CameraMainSingleTarget()
        {
            SetMediator();

            var target = new List<Transform>() { _mediator.CharacterTransform };
            var data = new CameraData(target, new LifeCycleData(1f), _currentCamera.Transitions);
            AddCamera(data);
        }

        [ContextMenu("Override Settings - Single to Multi")]
        public void SequenceCameraSingleToMulti()
        {
            SetMediator();

            var tempList = new List<Transform> { _mediator.CharacterTransform };
            var data = new CameraData(tempList, new LifeCycleData(2f), _currentCamera.Transitions);
            AddCamera(data, null);
            tempList = new List<Transform>() { _mediator.CharacterTransform, _mediator.FocusTransform };

            var data2 = new CameraData(tempList, new LifeCycleData(2f), _currentCamera.Transitions);
            AddCamera(data2);
        }

        [ContextMenu("Test - Focus to Single")]
        public void SequenceCameraFocusToSingle()
        {
            SetMediator();

            var tempList = new List<Transform> { _mediator.FocusTransform };
            var tempData = new CameraTransitionsData() { SmoothSpeed = 2f };
            var data = new CameraData(tempList, new LifeCycleData(2f), tempData);
            AddCamera(data);

            tempList = new List<Transform>() { _mediator.CharacterTransform };
            var tempData2 = new CameraTransitionsData() { SmoothSpeed = 8f };
            var data2 = new CameraData(tempList, new LifeCycleData(2f), tempData2);
            AddCamera(data2);
        }

        [ContextMenu("Test - Focus to Multi to Single")]
        public void SequenceCameraFocusToMultiToSingle()
        {
            SetMediator();

            var tempList = new List<Transform> { _mediator.FocusTransform };
            var tempData = new CameraTransitionsData() { SmoothSpeed = 2f };
            var data = new CameraData(tempList, new LifeCycleData(2f), tempData);
            AddCamera(data);

            tempList = new List<Transform> { _mediator.CharacterTransform, _mediator.FocusTransform };
            tempData = new CameraTransitionsData() { SmoothSpeed = 1.5f };
            var data2 = new CameraData(tempList, new LifeCycleData(3f), tempData);
            AddCamera(data2);

            tempList = new List<Transform> { _mediator.CharacterTransform };
            tempData = new CameraTransitionsData() { SmoothSpeed = 8f };
            var data3 = new CameraData(tempList, new LifeCycleData(1f), tempData);
            AddCamera(data3);
        }

        [ContextMenu("Test - Focus to Single x3")]
        public void SequenceCameraFocusToSingleX3()
        {
            for (var i = 0; i < 3; i++)
            {
                SequenceCameraFocusToMultiToSingle();
            }
        }

#endregion

    }
#endif
}
