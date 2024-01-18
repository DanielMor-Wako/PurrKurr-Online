using Code.Wakoz.PurrKurr.DataClasses.Enums;
using Code.Wakoz.PurrKurr.Logic.GameFlow;
using Code.Wakoz.PurrKurr.Views;
using UnityEngine;

namespace Code.Wakoz.PurrKurr.Screens.Ui_Controller.InputDisplay {
    public class AimPadView : View<AimPadModel> {

        [SerializeField] private GameObject _radialGlowDir;
        [SerializeField] private float _radialGlowDirOffset;
        [SerializeField] private bool _hideRadialGlowWhenNoAimDir = true;
        
        [Tooltip("Displays the direction when distance is beyond _touchValidDistanceForMultiState via sections - left, left top, top, top right, right, bottom right etc..")]
        [SerializeField] private MultiStateView _multiStateGlowDir;
        
        [Tooltip("Alternative symbol when the player is aiming and distance is beyond minimum swipe distance")]
        [SerializeField] private MultiStateView _multiStateSymbol;

        [Tooltip("Indication for swipe distance - short , medium , long")]
        [SerializeField] private MultiStateView _swipeDistanceSymbol;

        private InputLogic _inputLogic;

        protected override void ModelReplaced() {
            base.ModelReplaced();
            
            _inputLogic = SingleController.GetController<LogicController>().InputLogic;
            SetSwipeDistanceIndicator();
        }

        protected override void ModelChanged() {

            UpdateRadialGlow();
            UpdateMultiStateGlow();
            UpdateMultiStateSymbol();
        }

        private void UpdateMultiStateSymbol() {
            
            if (_multiStateSymbol == null) {
                return;
            }
            
            _multiStateSymbol.ChangeState(Model.AimDir == Vector2.zero ? 0 : 1);
        }

        private void UpdateRadialGlow() {

            if (_radialGlowDir == null) {
                return;
            }

            if (_hideRadialGlowWhenNoAimDir) {
                _radialGlowDir.gameObject.SetActive(Model.AimDir != Vector2.zero);
            }
            
            if (Model.AimDir == Vector2.zero) {
                _radialGlowDir.transform.rotation = new Quaternion(0, 0, 0, 0);
                return;
            }
            
            float angle = Mathf.Atan2(Model.AimDir.y, Model.AimDir.x) * Mathf.Rad2Deg;
            Quaternion target = Quaternion.Euler(0, 0, angle + _radialGlowDirOffset);
            _radialGlowDir.transform.rotation = target;
        }
        
        private void UpdateMultiStateGlow() {
            
            if (_multiStateGlowDir == null) {
                return;
            }

            var moveDir = _inputLogic.GetInputDirection(Model.AimDir);
            
            var newState = 
                moveDir == Definitions.NavigationType.Up ? 1 :
                moveDir == Definitions.NavigationType.UpRight ? 2 :
                moveDir == Definitions.NavigationType.Right ? 3 : 
                moveDir == Definitions.NavigationType.DownRight ? 4 :
                moveDir == Definitions.NavigationType.Down ? 5:
                moveDir == Definitions.NavigationType.DownLeft ? 6 : 
                moveDir == Definitions.NavigationType.Left ? 7 :
                moveDir == Definitions.NavigationType.UpLeft ? 8 :
                0; // empty

            _multiStateGlowDir.ChangeState(newState);
        }
        
        
        private void SetSwipeDistanceIndicator() {
            
            if (_swipeDistanceSymbol == null) {
                return;
            }
            
            _swipeDistanceSymbol.ChangeState((int)Model.SwipeDistanceTypeType);
        }
    }

}