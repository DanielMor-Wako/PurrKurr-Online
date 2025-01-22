using Code.Wakoz.PurrKurr.Popups.OverlayWindow;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Code.Wakoz.PurrKurr.DataClasses.GameCore.Detection
{

    [DefaultExecutionOrder(15)]
    public class DetectionZoneTrigger : Controller {

        public event Action<Collider2D> OnColliderEntered;
        public event Action<Collider2D> OnColliderExited;

        [SerializeField] private List<OverlayWindowData> overlayWindowData;

        [SerializeField] private DetectionZone zone;

        public List<OverlayWindowData> GetWindowData() => overlayWindowData;

        public void UpdateWindowData(List<OverlayWindowData> data) => overlayWindowData = data;

        protected override void Clean() {

            if (zone == null) {
                return;
            }
            
            zone.OnColliderEntered -= handleColliderEntered;
            zone.OnColliderExited -= handleColliderExited;
        }

        protected override Task Initialize() {

            Init();

            return Task.CompletedTask;
        }

        private void Init() {

            if (zone == null) {
                return;
            }

            zone.OnColliderEntered += handleColliderEntered;
            zone.OnColliderExited += handleColliderExited;
        }

        private void handleColliderExited(Collider2D triggeredCollider) => OnColliderExited?.Invoke(triggeredCollider);

        private void handleColliderEntered(Collider2D triggeredCollider) => OnColliderEntered?.Invoke(triggeredCollider);

    }
}