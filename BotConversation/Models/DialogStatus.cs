namespace BotConversation.Models
{
    public class DialogStatus
    {
        public DialogStatus() { }

        public DateTime UpdatedAt { get; set; } = DateTime.Now;
        public DialogStatus(string dialogName, string conversationName) => (DialogName, ConversationStatus) = (dialogName, new ConversationStatus(conversationName));
        public DialogStatus(string dialogName, string conversationName, DialogStatus parent) => (DialogName, ConversationStatus, Parent) = (dialogName, new ConversationStatus(conversationName), parent);

        public void Set(string dialogName, string conversationName)
        {
            this.DialogName = dialogName;
            this.ConversationStatus = new ConversationStatus(conversationName);
        }

        public void Set(string conversationName) => this.ConversationStatus = new ConversationStatus(conversationName);

        public string DialogName { get; set; }
        public ConversationStatus ConversationStatus { get; set; }
        public Dictionary<string, object> UserStatus { get; set; } = new Dictionary<string, object>();

        public DialogStatus? Parent { get; set; }
        public  DialogStatus? SubStatus { get;set; }
        public DialogStatus? Last()
        {
            DialogStatus? current = this;
            while(current.SubStatus != null)
            {
                current = current.SubStatus;
            }
            return current;
        }
    }
}
