namespace Code.Wakoz.PurrKurr.Screens.Login
{
    public class LoginModel : Model {
        
        public int PageIndex { get; private set; }

        private readonly int _formPageIndex;
        private readonly int _welcomePageIndex;

        public LoginModel(int startingIndex = 0, int welcomePageIndex = 1, int signupFormPageIndex = 2) {
            PageIndex = startingIndex;
            _welcomePageIndex = welcomePageIndex;
            _formPageIndex = signupFormPageIndex;
        }

        public void SetPageIndex(int pageIndex) {
            PageIndex = pageIndex;
            Changed();
        }

        public bool IsFormAvailable() => PageIndex == _formPageIndex;
        public bool IsWelcomeAvailable() => PageIndex == _welcomePageIndex;
    }
}
