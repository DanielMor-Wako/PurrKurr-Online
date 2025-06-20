using UnityEngine;
using Code.Wakoz.Utils.CameraUtils;
using System;

namespace Code.Wakoz.PurrKurr.Screens.CameraSystem
{
    // todo: implement the derived BaseCamera classes
    public sealed class FollowSingleTarget : BaseCamera
    {
        public FollowSingleTarget(CameraData data, Action processActionCallback = null) : base(data, processActionCallback)
        {

        }
    }
    public sealed class FollowMultipleTargets : BaseCamera
    {
        public FollowMultipleTargets(CameraData data, Action processActionCallback = null) : base(data, processActionCallback)
        {

        }
    }
    public abstract class BaseCamera : ICamera
    {
        protected CameraData _data;
        protected readonly Action _processActionCallback;

        protected float _startTime;
        protected float _currentZoom;
        protected Vector3 _newPosition;

        private float _greatestDistance;

        public BaseCamera(CameraData data, Action processActionCallback = null)
        {
            _data = data;
            _processActionCallback = processActionCallback;
            _startTime = Time.time;
        }

        public CameraData Data => _data;

        public virtual void Initialize()
        {
            _startTime = Time.time;
        }

        public void SwitchData(CameraData data)
        {
            _data = data;
        }

        public void Process()
        {
            _processActionCallback?.Invoke();
        }

        public virtual void Clean()
        {

        }

        public bool IsMinDurationElapsed() 
            => (Time.time - _startTime) >= _data.Duration.MinimumDuration;
        
        public virtual void UpdatePosition(Transform cameraTransform, ref Vector3 offset)
        {
            if (_data.TargetsCount > 0)
            {
                offset = Vector2.Lerp(offset, _data.OffsetData.TargetOffset, _data.OffsetData.SmoothSpeed * Time.unscaledDeltaTime);

                _newPosition = CameraUtils.GetCenterPoint(_data.Targets) + offset;
                _newPosition.z = cameraTransform.position.z; // Maintain the camera's Z position

                cameraTransform.position = Vector3.Lerp(cameraTransform.position, _newPosition, _data.Transitions.SmoothSpeed * Time.unscaledDeltaTime);
            }
        }

        public virtual void AdjustZoom(Camera cam)
        {
            CameraUtils.GetGreatestDistance(_data.Targets, ref _greatestDistance);
            _currentZoom = Mathf.Lerp(_data.Transitions.MaxZoom, _data.Transitions.MinZoom, _greatestDistance / _data.Transitions.ZoomLimiter);
            cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, _currentZoom, Time.unscaledDeltaTime * _data.Transitions.ZoomSpeed);
        }

        public void SetTargetOffset(Vector3 targetOffset)
        {
            _data.OffsetData.TargetOffset = targetOffset;
        }

        public void SetZoom(float zoomSpeed, float minZoom, float maxZoom)
        {
            _data.Transitions.ZoomSpeed = zoomSpeed;
            _data.Transitions.MinZoom = minZoom;
            _data.Transitions.MaxZoom = maxZoom;
        }

    }
}
