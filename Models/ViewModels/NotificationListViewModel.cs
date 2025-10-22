namespace FutMatchApp.Models.ViewModels
{
    public class NotificationListViewModel
    {
        public List<Notification> Notifications { get; set; } = new List<Notification>();
        public int UnreadCount { get; set; }
        public int TotalCount { get; set; }
        public bool HasChallengeNotifications { get; set; }
        public bool HasMatchReminders { get; set; }
    }
}
