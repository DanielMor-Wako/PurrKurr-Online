namespace Code.Wakoz.PurrKurr.Screens.MainMenu
{
    public class MainMenuModel : Model
    {
        public bool IsOpen { get; private set; }
        public bool AreButtonsAvailable{ get; private set; }
        public string PlayerDisplayName { get; private set; }

        public MainMenuModel(bool isActive = true) {
            IsOpen = isActive;
            PlayerDisplayName = "";
        }
        
        public void ChangeState(bool isOpen) {
            IsOpen = isOpen;
            Changed();
        }

        public void SetDisplayName(string displayName) {
            PlayerDisplayName = displayName;
            Changed();
        }

        public void SetButtonsAvailability(bool areButtonsAvailable, bool isInternalUpdate = false) {
            AreButtonsAvailable = areButtonsAvailable;
            if (isInternalUpdate) return;
            Changed();
        }
    }
}
