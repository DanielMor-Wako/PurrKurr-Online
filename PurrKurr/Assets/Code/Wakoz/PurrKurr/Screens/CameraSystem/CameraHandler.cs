using UnityEngine;
using System.Collections.Generic;
using System;

namespace Code.Wakoz.PurrKurr.Screens.CameraSystem
{
    public sealed class CameraHandler
    {
        public ICamera CurrentCamera => _currentCamera;
        public Vector3 CurrentOffset => _currentOffset;

        private Vector3 _currentOffset;
        private ICamera _currentCamera;

        private Queue<ICamera> _cameraQueue = new Queue<ICamera>();
        private int _camerasQueueCount = 0;
        
        private CameraController _camController;
        private Camera _cam;

        public CameraHandler(CameraController controller)
        {
            _camController = controller;
            _cam = _camController.GetComponent<Camera>();
            _currentOffset = Vector3.zero;
        }

        public void EnqueueCamera(CameraData data, Func<Action> processActionCallback = null)
        {
            ICamera newCamera = 
                (data.TargetsCount == 1) ? new FollowSingleTarget(data, processActionCallback) 
                : new FollowMultipleTargets(data, processActionCallback);

            EnqueueCamera(newCamera);
        }

        public void EnqueueCamera(ICamera camera)
        {
            _cameraQueue.Enqueue(camera);

            _camerasQueueCount = _cameraQueue.Count;
        }

        public int CamerasQueueCount() => _camerasQueueCount;

        public void UpdateCurrent()
        {
            if (_currentCamera == null || _currentCamera != null && _currentCamera.IsMinDurationElapsed())
            {
                SwitchToNextCamera();
            }

            if (_currentCamera != null)
            {
                _currentCamera.Process();

                _currentCamera.UpdatePosition(_camController.transform, ref _currentOffset);
                _currentCamera.AdjustZoom(_cam);
            }
        }

        private void SwitchToNextCamera()
        {
            if (_cameraQueue.Count > 0)
            {
                _currentCamera?.Clean();
                _currentCamera = _cameraQueue.Dequeue();

                _currentCamera.Initialize();
                _camerasQueueCount = _cameraQueue.Count;
            }
        }
    }
}
