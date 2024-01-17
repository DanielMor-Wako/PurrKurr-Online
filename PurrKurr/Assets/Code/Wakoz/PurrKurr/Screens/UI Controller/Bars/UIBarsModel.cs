using Code.Wakoz.PurrKurr.DataClasses.Characters;

namespace Code.Wakoz.PurrKurr.Screens.Ui_Controller.Bars {
    public class UIBarsModel : Model {

        public float HealthPercent { get; private set; }
        public float TimerPercent { get; private set; }

        public UIBarsModel(float healthPercent, float timerPercent) {
            UpdateUiStatInternal(healthPercent, timerPercent);
        }

        public void UpdateUiStat(float healthPercent, float timerPercent) {

            UpdateUiStatInternal(healthPercent, timerPercent);
            Changed();
        }

        private void UpdateUiStatInternal(float healthPercent, float timerPercent) {

            HealthPercent = healthPercent;
            TimerPercent = timerPercent;
        }
    }
}
