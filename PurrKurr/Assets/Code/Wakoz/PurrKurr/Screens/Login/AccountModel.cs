namespace Code.Wakoz.PurrKurr.Screens.Login
{
    public class AccountModel : Model
    {
        public int PageIndex { get; private set; }

        public bool IsCredentialsAvailable { get; private set; }
        public bool IsLogOutConfirmationPopup { get; private set; }

        public bool IsGuest { get; private set; }
        public string PlayerDisplayName { get; private set; }

        public AccountModel(bool isGuest = true, bool credentialsAvailable = true, bool isActive = false) {

            IsGuest = isGuest;
            PageIndex = 0;
            IsCredentialsAvailable = credentialsAvailable;
            IsLogOutConfirmationPopup = isActive;
            PlayerDisplayName = "";
        }

        public void ChangePageIndex(int index) {
            PageIndex = index;
            Changed();
        }

        public void ChangeLogOutConfirmationState(bool isOpen = false) {
            IsLogOutConfirmationPopup = isOpen;
            Changed();
        }

        public void ChangeCredentialsAvailability(bool isOpen = false, bool internalUpdate = false) {
            IsCredentialsAvailable = isOpen;

            if (!internalUpdate) return;
            Changed();
        }

        public void SetGuestUser(bool isGuest = true) {
            IsGuest = isGuest;
        }

        public void SetDisplayName(string displayName) {
            PlayerDisplayName = displayName;
            Changed();
        }

        public void ForceRefresh() {
            Changed();
        }
    }
}
