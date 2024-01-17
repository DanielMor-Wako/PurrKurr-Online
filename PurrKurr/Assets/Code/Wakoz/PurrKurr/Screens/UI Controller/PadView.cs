using Code.Wakoz.PurrKurr.Logic.GameFlow;
using Code.Wakoz.PurrKurr.Views;
using UnityEngine;
using UnityEngine.UI;

namespace Code.Wakoz.PurrKurr.Screens.Ui_Controller {
    public class PadView : View<PadModel> {

        //[SerializeField] private bool HideFixedTouchPadWhenFlexibleIsActive = false;
        //[SerializeField] private bool AllowRepositionOnClick = false;
        private Camera _cam;
        
        [Header("Settings")]
        [SerializeField] private RectTransform _interactionEffect;
        [SerializeField] private RectTransform _fixedTouchPad;
        [SerializeField] private MultiStateView _fixedTouchPadSymbol;
        [SerializeField] private Image _cooldown;
        [Header("Aim Pad")]
        [SerializeField] private AimPadView _flexiblePad;

        private RectTransform _aimPadRectTransform;
        private AimPadModel _aimPadModel;

        private AbilitiesLogic _abilitiesLogic;
        
        protected override void ModelReplaced() {
            base.ModelReplaced();

            if (_flexiblePad != null) {
                _aimPadRectTransform ??= _flexiblePad.gameObject.GetComponent<RectTransform>();
                _abilitiesLogic = SingleController.GetController<LogicController>().AbilitiesLogic;
                if (_aimPadRectTransform != null) {
                    _cam ??= Camera.main;
                }
            }
        }

        protected override void ModelChanged() {

            if (!Model.Config.IsAvailable) {
                //Debug.Log($"no available pad display {Model.Config.actionType}");
                return;
            }
            
            var hasAimPad = Model.IsActive && Model.Config.IsAimAvailable && Model.AimStartPos != Vector2.zero;
            if (_aimPadRectTransform != null) {
                SetStateFlexiblePad(hasAimPad);

            } else {
                SetStateFixedPad();
            }
            
            SetAlternativeSymbol();
            SetInteractionEffect();
            SetCooldown();
        }

        private void SetCooldown() {

            if (_cooldown == null) {
                return;
            }

            UpdateCooldown(_cooldown.fillAmount, Model.CooldownPercent);
        }

        private void UpdateCooldown(float startAmount, float targetAmount) {
            
            // todo: animate from start amount to target amount
            _cooldown.fillAmount = targetAmount;
        }

        private void SetInteractionEffect() {
            
            if (_interactionEffect == null) {
                return;
            }
            
            _interactionEffect.gameObject.SetActive(Model.IsActive);
        }

        private void SetAlternativeSymbol() {

            if (_fixedTouchPadSymbol == null) {
                return;
            }

            _fixedTouchPadSymbol.ChangeState(Model.SymbolState);
        }

        private void SetStateFlexiblePad(bool hasAimPad) {
            
            _fixedTouchPad.gameObject.SetActive(!hasAimPad);
            _flexiblePad.gameObject.SetActive(hasAimPad);

            if (hasAimPad) {
                
                var newPos = _cam.ScreenToWorldPoint(Model.AimStartPos);
                newPos.z = 0;
                _aimPadRectTransform.position = newPos;

                if (_aimPadModel != null) {
                    _aimPadModel.UpdateDir(Model.AimDir);
                    
                } else {
                    _aimPadModel = new AimPadModel(Vector2.zero, _abilitiesLogic.GetSwipeTypeByActionType(Model.Config.actionType));
                    _flexiblePad.SetModel(_aimPadModel);
                }
                
            }
        }

        private void SetStateFixedPad() {

            _fixedTouchPad.gameObject.SetActive(true);

            if (_flexiblePad == null) {
                return;
            }

            _flexiblePad.gameObject.SetActive(false);
            _aimPadModel = null;
        }

    }

}