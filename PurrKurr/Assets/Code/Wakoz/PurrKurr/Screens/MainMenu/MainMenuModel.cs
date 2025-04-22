namespace Code.Wakoz.PurrKurr.Screens.MainMenu
{
    public class MainMenuModel : Model {
        public bool IsOpen { get; private set; }
        public const string PlayBtnCode = "Play";
        public const string DailyEventsBtnCode = "DailyChallenges";
        public MainMenuModel(bool isActive = true)
        {
            IsOpen = isActive;
        }
        public void SetOpenState(bool isActive) {
            IsOpen = isActive;
            Changed();
        }
    }
}
