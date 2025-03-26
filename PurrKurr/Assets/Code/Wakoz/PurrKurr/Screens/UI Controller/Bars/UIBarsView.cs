using Code.Wakoz.PurrKurr.Views;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Code.Wakoz.PurrKurr.Screens.Ui_Controller.Bars
{

    public class UIBarsView : View<UIBarsModel> {

        [Header("Health")]
        [SerializeField] private ImageFillerView _healthFiller;
        [SerializeField] private ImageColorChangerView _healthColor;
        [SerializeField] private TextMeshProUGUI _healthText;

        [Header("Timer")]
        [SerializeField] private Image _timerFiller;
        [SerializeField] private RectTransformScalerView _timerScaler;
        [SerializeField] private CanvasGroupFaderView _timerFader;
        [SerializeField] private TextMeshProUGUI _timerText;

        protected override void ModelChanged() {

            UpdateHealth();
            UpdateTimer();
        }

        private void UpdateHealth() {

            if (_healthText != null) {
                _healthText.SetText(Model.HealthText);
            }

            if (_healthFiller == null) {
                return;
            }

            var newHpPercent = 1 - Model.HealthPercent;
            var difference = newHpPercent - _healthFiller.ImageTarget.fillAmount;
            if (difference == 0) {
                return;
            }

            _healthFiller.StartTransition(newHpPercent);

            var newColor = Color.Lerp(_healthColor.MinValue, _healthColor.MaxValue, newHpPercent);
            if (_healthColor == null || _healthColor.TargetValue == newColor) {
                return;
            }
            _healthColor.TargetImage.color = difference >= 0 ? Color.red : Color.green;
            _healthColor.ChangeColor(newColor);
        }

        private void UpdateTimer() {

            if (_timerFiller == null) {
                Debug.LogError("Missing image filler component on Timer (UI Top Bar)");
                return;
            }

            var newAmount = 1 - Model.TimerPercent;

            if (newAmount == _timerFiller.fillAmount) {
                return;
            }

            var targetBinaryState = (int)_timerFader.TargetValue;
            var newBinaryState = Mathf.Ceil(Model.TimerPercent);
            if (targetBinaryState != newBinaryState) {
                Debug.Log($"Updating timer alpha from {targetBinaryState} to {newBinaryState}");
                _timerFader.StartTransition(newBinaryState);
            }
            
            _timerFiller.fillAmount = newAmount;

            if (_timerText != null) {
                _timerText.SetText(Model.TimerText);
            }

            if (_timerScaler != null && Model.IsAlmostTimeOut) {
                _timerScaler.TargetRectTransform.localScale = _timerScaler.MaxScale;
                _timerScaler.EndTransition();
            }
        }

    }
}
