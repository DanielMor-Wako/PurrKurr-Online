namespace Code.Wakoz.PurrKurr.Screens.Notifications
{
    public class NotificationData
    {
        public string Message { get; private set; }
        public float Duration { get; private set; }

        public NotificationData(string message, float duration) 
        {
            Message = message;
            Duration = duration;
        }
    }
}