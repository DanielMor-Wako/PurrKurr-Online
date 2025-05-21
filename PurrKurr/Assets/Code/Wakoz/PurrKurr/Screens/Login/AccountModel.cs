using Unity.Services.Authentication;

namespace Code.Wakoz.PurrKurr.Screens.Login
{
    public class AccountModel : Model
    {
        public PlayerInfo Player { get; set; }

        public int PageIndex { get; private set; }

        public bool IsCredentialsAvailable { get; private set; }
        public bool IsLogOutConfirmationPopup { get; private set; }

        public AccountModel(bool credentialsAvailable = true, bool isActive = false) {

            PageIndex = 0;
            IsCredentialsAvailable = credentialsAvailable;
            IsLogOutConfirmationPopup = isActive;
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

        public void SetPlayerinfo(PlayerInfo playerInfo) {
            Player = playerInfo;
            Changed();
        }
    }
}
