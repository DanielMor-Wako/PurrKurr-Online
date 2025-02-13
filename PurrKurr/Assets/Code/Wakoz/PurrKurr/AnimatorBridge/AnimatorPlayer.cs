using System.Collections.Generic;
using UnityEngine;

namespace Code.Wakoz.PurrKurr.AnimatorBridge
{
    [System.Serializable]
    public enum AnimClipType : byte
    {
        // Initialization
        Init = 0,

        // Basic States
        Idle = 1,
        StandUp = 2,
        Crouch = 3,
        Crawl = 4,
        Moving = 5,
        Sprint = 6,
        Roll = 7,

        // Jumping and Falling
        Jump = 8,
        AerialJump = 9,
        Fall = 10,
        Landed = 11,

        // Defensive Actions
        Block = 12,
        Dodge = 13,

        // Traversal
        AirGlide = 14,
        WallCling = 15,
        WallClimb = 16,
        TraversalRun = 17,
        TraversalCling = 18,
        RopeCling = 19,
        RopeClimb = 20,

        // Aiming
        AimJump = 21,
        AimRope = 22,
        AimProjectile = 23,

        // Taunts and Special Actions
        Taunt = 24,
        SpecialAttack = 25,

        // Attacks
        LightAttack = 26,
        MediumAttack = 27,
        HeavyAttack = 28,
        RollAttack = 29,
        AerialAttack = 30,

        // Grabs
        LightGrab = 31,
        MediumGrab = 32,
        MediumGrabThrow = 33,
        HeavyGrab = 34,
        AerialGrab = 35,

        // Status Effects
        Grabbed = 36,
        Stunned = 37,

        // Damage and Death
        TakeDamage = 38,
        DropDead = 39,
        RiseUp = 40
    }

    [RequireComponent(typeof(Animator))]
    public class AnimatorPlayer : MonoBehaviour
    {

#if UNITY_EDITOR
        [Tooltip("This property exposes the valid names for clips in the Animator")]
        [SerializeField] private AnimClipType _validClipNames;
#endif
        [Space(10)]
        [SerializeField] private Animator _animator;

        [Tooltip("Cross fade duration between animations.\nWhen set as 0, crossfade is ignored")]
        [SerializeField] [Min(0)] private float _crossfade;

        private HashSet<int> _cachedClipHashes = new();
        private AnimationClip[] _runtimeAnimationClips;

        /// <summary>
        /// Updates the animator speed for transitions that use the PLAY_SPEED property
        /// Default is 1 as the multiplier takes no effect
        /// </summary>
        /// <param name="speedMultiplier"></param>
        public void SetPlaySpeed(float speedMultiplier = 1)
        {
            _animator.SetFloat("PLAY_SPEED", speedMultiplier);
        }

        /// <summary>
        /// Plays an animation based on the enum value (int) provided
        /// </summary>
        /// <param name="animClipTypeInt">The integer value corresponding to the AnimClipType enum.</param>
        public void PlayAnimation(AnimClipType animClipType)
        {
            if (_animator == null)
            {
                Debug.LogError("Animator reference is not assigned!");
                return;
            }

            var clipName = animClipType.ToString();

            int clipHash = Animator.StringToHash(clipName);

            if (!_cachedClipHashes.Contains(clipHash))
            {
                if (IsAnimationClipExists(clipName))
                {
                    _cachedClipHashes.Add(clipHash);
                }
                else
                {
                    Debug.LogError($"Animation clip '{clipName}' not found in the Animator!");
                    return;
                }
            }

            PlayClipHash(clipHash);
        }

        private void PlayClipHash(int clipHash)
        {
            //Debug.Log($"Playing animation: {clipName} (Hash: {clipHash})");
            if (_crossfade > 0)
            {
                _animator.CrossFade(clipHash, _crossfade);
                return;
            }

            _animator.Play(clipHash);
        }

        /// <summary>
        /// Checks if an animation clip with the given name exists in the Animator.
        /// </summary>
        /// <param name="clipName">The name of the animation clip to check.</param>
        /// <returns>True if the animation clip exists, false otherwise.</returns>
        private bool IsAnimationClipExists(string clipName)
        {
            _runtimeAnimationClips ??= _animator.runtimeAnimatorController.animationClips;

            for (var i = 0; i < _runtimeAnimationClips.Length; i++)
            {
                if (_runtimeAnimationClips[i].name == clipName)
                {
                    return true;
                }
            }
            return false;
        }

    }
}