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
        private CameraCharacterHandler _characterMediator;

        public CameraCharacterHandler CharacterMediator => _characterMediator;

        public void BindCharacter(Transform character, GameplayController gameEvents)
        {
            _cameraHandler ??= new CameraHandler(this);

            _characterMediator?.Unbind();
            
            _characterMediator = new CameraCharacterHandler(_cameraHandler, gameEvents, _cameraFocus, character);
            _characterMediator.Bind();
        }

        public void AddCamera(CameraData data, Action processActionCallback = null)
        {
            _cameraHandler ??= new CameraHandler(this);
            _cameraHandler.EnqueueCamera(data, processActionCallback);
        }

        private void Start()
        {
            _cameraHandler ??= new CameraHandler(this);
        }

        private void Update()
        {
            _cameraHandler.UpdateCurrent();
        }

        private void OnDestroy()
        {
            _characterMediator?.Unbind();
        }
    }
}
