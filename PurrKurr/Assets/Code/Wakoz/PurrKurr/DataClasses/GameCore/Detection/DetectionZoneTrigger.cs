using Code.Wakoz.PurrKurr.Popups.OverlayWindow;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Code.Wakoz.PurrKurr.DataClasses.GameCore.Detection
{

    [DefaultExecutionOrder(15)]
    public class DetectionZoneTrigger : Controller
    {
        public event Action<Collider2D> OnColliderEntered;
        public event Action<Collider2D> OnColliderExited;

        [SerializeField] private List<OverlayWindowData> _overlayWindowData;

        [SerializeField] private DetectionZone _zone;

        public List<OverlayWindowData> GetWindowData() => _overlayWindowData;

        public void UpdateWindowData(List<OverlayWindowData> data) => _overlayWindowData = data;

        protected override void Clean()
        {
            if (_zone == null)
            {
                return;
            }

            _zone.OnColliderEntered -= handleColliderEntered;
            _zone.OnColliderExited -= handleColliderExited;
        }

        protected override Task Initialize()
        {
            Init();

            return Task.CompletedTask;
        }

        private void Init()
        {
            if (_zone == null)
            {
                return;
            }

            _zone.OnColliderEntered += handleColliderEntered;
            _zone.OnColliderExited += handleColliderExited;
        }

        private void handleColliderEntered(Collider2D triggeredCollider) => OnColliderEntered?.Invoke(triggeredCollider);

        private void handleColliderExited(Collider2D triggeredCollider) => OnColliderExited?.Invoke(triggeredCollider);
    }
}