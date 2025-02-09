namespace BotConversation.Models
{
    public class ConversationStatus
    {
        public ConversationStatus() { }

        public ConversationStatus(string conversationName) => (Name) = (conversationName);
        public string Name { get; set; }
        public bool Sent { get; set; } = false;
    }
}
