using Code.Wakoz.PurrKurr.DataClasses.Characters;
using Code.Wakoz.PurrKurr.DataClasses.Enums;
using UnityEngine;

namespace Code.Wakoz.PurrKurr.DataClasses.GameCore {

    public interface IInteractableBody {

        public abstract Collider2D GetCollider();
        public abstract Transform GetTransform();
        public abstract Transform GetCharacterRigTransform();
        public abstract Definitions.ObjectState GetCurrentState();
        public abstract Vector3 GetCenterPosition();
        public abstract Vector2 GetVelocity();
        public abstract float GetHpPercent();
        public abstract int DealDamage(int damage);
        public abstract void ApplyForce(Vector2 forceDir);
        public abstract void SetTargetPosition(Vector2 position, float percentToPerform = 1);

        public abstract void SetAsGrabbing(IInteractableBody grabbedBody);
        public abstract void SetAsGrabbed(IInteractableBody grabberBody, Vector2 grabPosition);
        public abstract bool IsGrabbing();
        public abstract bool IsGrabbed();
        public abstract IInteractableBody GetGrabbedTarget();
    }

}