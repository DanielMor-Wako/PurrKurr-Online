namespace Code.Wakoz.PurrKurr.Screens.MainMenu
{
    public class MainMenuModel : Model
    {
        public bool IsOpen { get; private set; }
        
        public MainMenuModel(bool isActive = true) {
            IsOpen = isActive;
        }
        
        public void ChangeState(bool isOpen) {
            IsOpen = isOpen;
            Changed();
        }

    }
}
