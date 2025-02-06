using Code.Wakoz.PurrKurr.DataClasses.Enums;
using UnityEngine;

namespace Code.Wakoz.PurrKurr.DataClasses.GameCore {

    public interface IInteractableBody {

        public Collider2D GetCollider();
        public Transform GetTransform();
        public Transform GetCharacterRigTransform();
        public Definitions.ObjectState GetCurrentState();
        public Vector3 GetCenterPosition();
        public Vector2 GetVelocity();
        public float GetHpPercent();
        public void DealDamage(int damage);
        public void ApplyForce(Vector2 forceDir);
        public void SetTargetPosition(Vector2 position, float percentToPerform = 1);

        public void SetAsGrabbing(IInteractableBody grabbedBody);
        public void SetAsGrabbed(IInteractableBody grabberBody, Vector2 grabPosition);
        public bool IsGrabbing();
        public bool IsGrabbed();
        public IInteractableBody GetGrabbedTarget();
    }

}