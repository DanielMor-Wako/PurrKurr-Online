using Code.Wakoz.Utils.Extensions;
using UnityEngine;

namespace Code.Wakoz.PurrKurr.UI.Instructions {
    public class OverlayInstructionView : MonoBehaviour {

        [SerializeField] private Animator _animator;
        [SerializeField] private Transform _pointerHolder;

        public static int ActiveHash = Animator.StringToHash("Active");
        public static int IsSwipeHash = Animator.StringToHash("IsSwipe");
        public static int HoldSwipeHash = Animator.StringToHash("HoldSwipe");

        public void UpdateAnimatorValues(OverlayInstructionModel data) {

            var animationData = data._animationData;
            UpdateActiveState(data.IsActive);
            _animator.SetBool(IsSwipeHash, animationData?.SwipeAngle != -1);
            _animator.SetBool(HoldSwipeHash, animationData.IsHoldSwipe);

            var eulr = _pointerHolder.eulerAngles;
            eulr.z = animationData.SwipeAngle > -1 ? animationData.SwipeAngle : 0;
            _pointerHolder.eulerAngles = eulr;

            if (animationData?.CursorTarget) {
                transform.position = animationData.CursorTarget.transform.position;
            }

        }

        public void UpdateActiveState(bool isActive) {

            _animator.SetBool(ActiveHash, isActive);
        }

    }
}