namespace BotConversation.Dialogs.Base
{
    public abstract class StatelessDialog : Dialog
    {
        public StatelessDialog(string name, string[] conversationOrder) : base(name, conversationOrder) { }

        public StatelessDialog(string[] conversationOrder) : base(conversationOrder) { }

        public StatelessDialog() : base() { }

        public virtual async Task<bool> Main(object botClient, object message, CancellationToken cancellationToken)
        {
            return await Task.FromResult(true);
        }
    }
}
