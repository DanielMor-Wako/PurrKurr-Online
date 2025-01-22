using Code.Wakoz.PurrKurr.DataClasses.Characters;
using Code.Wakoz.PurrKurr.DataClasses.GameCore.Detection;
using Code.Wakoz.PurrKurr.Screens.Gameplay_Controller;
using Code.Wakoz.PurrKurr.Screens.Shaker;
using System.Threading.Tasks;
using UnityEngine;

namespace Code.Wakoz.PurrKurr.DataClasses.GameCore.Damagers
{
    [DefaultExecutionOrder(15)]
    public class DamagerBody2DController : Controller
    {
        // public event System.Action<Collider2D> OnColliderEntered;
        // public event System.Action<Collider2D> OnColliderExited;

        [SerializeField] private DetectionZone zone;

        [Header("Force Settings - Durring Collision")]
        [Range(-1, 1)] [SerializeField] private int _forceXdir = 0;
        [Range(-1, 1)] [SerializeField] private int _forceYdir = 0;
        [Min(0)] [SerializeField] private int _forceAmount = 0;

        [SerializeField] private Transform _bodyTransform;

        protected override void Clean()
        {
            if (zone == null)
            {
                return;
            }

            zone.OnColliderEntered -= handleColliderEntered;
            //zone.OnColliderExited -= handleColliderExited;
        }

        protected override Task Initialize()
        {
            Init();

            return Task.CompletedTask;
        }

        private void Init()
        {
            if (zone == null)
            {
                return;
            }

            zone.OnColliderEntered += handleColliderEntered;
            //zone.OnColliderExited += handleColliderExited;
        }

        private void handleColliderExited(Collider2D triggeredCollider)
        {
            // OnColliderExited?.Invoke(triggeredCollider);
        }

        private void handleColliderEntered(Collider2D triggeredCollider)
        {
            //Debug.Log(triggeredCollider.name);
            var interactable = triggeredCollider.GetComponent<IInteractable>();
            if (interactable == null)
            {
                return;
            }

            var isKinematicCollider = triggeredCollider.attachedRigidbody.isKinematic;
            //Debug.Log($"is Kinematic {isKinematicCollider}");
            if (isKinematicCollider)
            {
                return;
            }

            var interactableBody = interactable.GetInteractable();
            if (interactableBody == null)
            {
                return;
            }
            Debug.Log($"interactable coll state {interactableBody.GetCurrentState()}");
            // Apply force
            if (interactableBody.GetCurrentState() is not Enums.Definitions.ObjectState.Blocking)
            {
                interactableBody.ApplyForce(new Vector2(_forceXdir, _forceYdir) * _forceAmount);
            }

            // Shake Damager
            if (_bodyTransform != null)
            {
                SingleController.GetController<GameplayController>().Handlers.GetHandler<ShakeHandler>().TriggerShake(_bodyTransform,
                new ShakeData(ShakeStyle.Circular, 0.5f));
            }

            // OnColliderEntered?.Invoke(triggeredCollider);
        }
    }
}