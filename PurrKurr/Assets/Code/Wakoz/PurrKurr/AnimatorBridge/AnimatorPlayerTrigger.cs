using UnityEngine;

namespace Code.Wakoz.PurrKurr.AnimatorBridge
{
    [RequireComponent(typeof(AnimatorPlayer))]
    public class AnimatorPlayerTrigger : MonoBehaviour
    {
        [Tooltip("The Animator ref that is used to invoke PlayAnimation")]
        [SerializeField] private AnimatorPlayer _animController;

        public void PlayAnimation(AnimClipType clipType)
        {
            _animController.PlayAnimation(clipType);
        }

        public void PlayInit() => PlayAnimation(AnimClipType.Init);
        public void PlayIdle() => PlayAnimation(AnimClipType.Idle);
        public void PlayStandUp() => PlayAnimation(AnimClipType.StandUp);
        public void PlayCrouch() => PlayAnimation(AnimClipType.Crouch);
        public void PlayCrawl() => PlayAnimation(AnimClipType.Crawl);
        public void PlayMoving() => PlayAnimation(AnimClipType.Moving);
        public void PlaySprint() => PlayAnimation(AnimClipType.Sprint);
        public void PlayRoll() => PlayAnimation(AnimClipType.Roll);
        public void PlayJump() => PlayAnimation(AnimClipType.Jump);
        public void PlayAerialJump() => PlayAnimation(AnimClipType.AerialJump);
        public void PlayFall() => PlayAnimation(AnimClipType.Fall);
        public void PlayLanded() => PlayAnimation(AnimClipType.Landed);
        public void PlayBlock() => PlayAnimation(AnimClipType.Block);
        public void PlayDodge() => PlayAnimation(AnimClipType.Dodge);
        public void PlayAirGlide() => PlayAnimation(AnimClipType.AirGlide);
        public void PlayWallCling() => PlayAnimation(AnimClipType.WallCling);
        public void PlayWallClimb() => PlayAnimation(AnimClipType.WallClimb);
        public void PlayTraversalRun() => PlayAnimation(AnimClipType.TraversalRun);
        public void PlayTraversalCling() => PlayAnimation(AnimClipType.TraversalCling);
        public void PlayRopeCling() => PlayAnimation(AnimClipType.RopeCling);
        public void PlayRopeClimb() => PlayAnimation(AnimClipType.RopeClimb);
        public void PlayAimJump() => PlayAnimation(AnimClipType.AimJump);
        public void PlayAimRope() => PlayAnimation(AnimClipType.AimRope);
        public void PlayAimProjectile() => PlayAnimation(AnimClipType.AimProjectile);
        public void PlayTaunt() => PlayAnimation(AnimClipType.Taunt);
        public void PlaySpecialAttack() => PlayAnimation(AnimClipType.SpecialAbility);
        public void PlayLightAttack() => PlayAnimation(AnimClipType.LightAttack);
        public void PlayMediumAttack() => PlayAnimation(AnimClipType.MediumAttack);
        public void PlayHeavyAttack() => PlayAnimation(AnimClipType.HeavyAttack);
        public void PlayRollAttack() => PlayAnimation(AnimClipType.DashAttack);
        public void PlayAerialAttack() => PlayAnimation(AnimClipType.AerialAttack);
        public void PlayLightGrab() => PlayAnimation(AnimClipType.LightGrab);
        public void PlayMediumGrab() => PlayAnimation(AnimClipType.MediumGrab);
        public void PlayMediumGrabThrow() => PlayAnimation(AnimClipType.MediumGrabThrow);
        public void PlayHeavyGrab() => PlayAnimation(AnimClipType.HeavyGrab);
        public void PlayAerialGrab() => PlayAnimation(AnimClipType.AerialGrab);
        public void PlayGrabbed() => PlayAnimation(AnimClipType.Grabbed);
        public void PlayStunned() => PlayAnimation(AnimClipType.Stunned);
        public void PlayTakeDamage() => PlayAnimation(AnimClipType.TakeDamage);
        public void PlayDropDead() => PlayAnimation(AnimClipType.DropDead);
        public void PlayRiseUp() => PlayAnimation(AnimClipType.RiseUp);
    }
}