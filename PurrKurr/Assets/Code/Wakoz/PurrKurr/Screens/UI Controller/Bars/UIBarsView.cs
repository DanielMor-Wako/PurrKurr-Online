using UnityEngine;
using UnityEngine.UI;

namespace Code.Wakoz.PurrKurr.Screens.Ui_Controller.Bars {
    public class UIBarsView : View<UIBarsModel> {

        [SerializeField] private Image healthPercent;
        [SerializeField] private Image timerPercent;

        protected override void ModelChanged() {

            UpdateUIStats();
        }

        private void UpdateUIStats() {
            
            if (healthPercent != null) {
                healthPercent.fillAmount = 1 - Model.HealthPercent;
            }

            if (timerPercent != null) {
                timerPercent.fillAmount = 1 - Model.TimerPercent;
            }
        }
    }
}
