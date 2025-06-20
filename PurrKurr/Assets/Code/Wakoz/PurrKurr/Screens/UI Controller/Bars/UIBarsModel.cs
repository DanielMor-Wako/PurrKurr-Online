namespace Code.Wakoz.PurrKurr.Screens.Ui_Controller.Bars {
    public class UIBarsModel : Model {

        public float HealthPercent { get; private set; }
        public string HealthText { get; private set; } = "";
        public float TimerPercent { get; private set; }
        public string TimerText { get; private set; } = "";
        public bool IsAnimating { get; private set; }

        public UIBarsModel(float healthPercent, string healthText, float timerPercent) {

            UpdateHpInternal(healthPercent, healthText);
            UpdateTimerInternal(timerPercent);
        }

        public void UpdateHpStat(float healthPercent, string healthText) {

            UpdateHpInternal(healthPercent, healthText);
            Changed();
        }

        public void UpdateTimerStat(float timerPercent, bool isPulseAnimation, string timerText) {

            UpdateTimerInternal(timerPercent, isPulseAnimation, timerText);
            Changed();
        }

        private void UpdateHpInternal(float healthPercent, string healthText = "") {
            
            HealthPercent = healthPercent;
            HealthText = healthText;
        }

        private void UpdateTimerInternal(float timerPercent, bool isAlmostTimeOut = false, string timerText = "") {

            TimerPercent = timerPercent;
            TimerText = timerText;
            IsAnimating = isAlmostTimeOut;
        }
    }
}
