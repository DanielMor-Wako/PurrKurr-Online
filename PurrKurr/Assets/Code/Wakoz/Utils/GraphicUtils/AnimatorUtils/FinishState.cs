using System;
using UnityEngine;

namespace Code.Wakoz.Utils.GraphicUtils.AnimatorUtils {
    public class FinishState : StateMachineBehaviour {

        public Action onEnter;

        [field: SerializeField] public string Id { get; private set; }

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            base.OnStateEnter(animator, stateInfo, layerIndex);

            //var clip = animator.GetCurrentAnimatorClipInfo(0);
            //Debug.Log("Entered Animator State: " + clip[0].clip.name);

            if (onEnter != null) {
                onEnter();
            }

        }

        #region Implementation of registering Multi Finish State on single Animator using ID
        /*void RegisterFinishStates() {	
            var _finishStates = _animator.GetBehaviours<FinishState>();
            foreach(var animClip in _finishStates) {
                animClip.onEnter += AnimationEnded;
            }
        }
        void DeregisterFinishStates() {
            if(_finishStates != null) {
                foreach(var animClip in _finishStates) {
                    animClip.onEnter -= AnimationEnded;
                }
            }
        }*/
        #endregion
        
    }
}