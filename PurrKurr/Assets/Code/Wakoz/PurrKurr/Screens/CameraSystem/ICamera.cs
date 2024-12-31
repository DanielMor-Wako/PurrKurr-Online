using UnityEngine;

namespace Code.Wakoz.PurrKurr.Screens.CameraSystem
{
    public interface ICamera : ICameraSetup, ICameraUpdate { }

    public interface ICameraSetup
    {
        CameraData Data { get; }
        void Initialize();
        void SwitchData(CameraData data);
        bool IsMinDurationElapsed();
        void Process();
        void Clean();
    }
    
    public interface ICameraUpdate
    {
        void UpdatePosition(Transform cameraTransform, ref Vector3 offset);
        void AdjustZoom(Camera cam);
        void SetTargetOffset(Vector3 offset);
        void SetZoom(float zoomSpeed, float minZoom, float maxZoom);
    }
}
