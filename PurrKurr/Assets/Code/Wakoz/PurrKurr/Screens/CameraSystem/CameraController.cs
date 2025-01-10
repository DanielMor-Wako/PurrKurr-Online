using UnityEngine;
using System;
using Code.Wakoz.PurrKurr.Screens.Gameplay_Controller;

namespace Code.Wakoz.PurrKurr.Screens.CameraSystem
{
    [RequireComponent(typeof(Camera))]
    public class CameraController : MonoBehaviour
    {
        [SerializeField] private Transform _cameraFocus;

        private CameraHandler _cameraHandler;

        public CameraCharacterHandler CharacterMediator 
            => SingleController.GetController<GameplayController>().Handlers.GetHandler<CameraCharacterHandler>();

        public Transform CameraFocusTransform 
            => _cameraFocus;

        public void AddCamera(CameraData data, Action processActionCallback = null)
        {
            //_cameraHandler ??= new CameraHandler(this);
            _cameraHandler ??= SingleController.GetController<GameplayController>().Handlers.GetHandler<CameraHandler>();
            _cameraHandler.EnqueueCamera(data, processActionCallback);
        }
/*
        private void Start()
        {
            _cameraHandler ??= new CameraHandler(this);
        }

        private void Update()
        {
            _cameraHandler.UpdateProcess(Time.deltaTime);
        }

        private void OnDestroy()
        {
            _characterMediator?.Unbind();
        }*/
    }
}
