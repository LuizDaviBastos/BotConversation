using BotConversation.Dialogs.Base;

namespace BotConversation.Connectors.Base
{
    public class BotConnector
    {
        private readonly DialogManager dialogManager;
        public BotConnector(Dialog[] dialogs, Dialog[] executionOrder)
        {
            dialogManager = new();
            dialogManager.AllDialogs = dialogs;
            dialogManager.DialogsExecutionOrder = executionOrder;
        }

        public virtual async Task HandleUpdateAsync(string chatId, object botClient, object message, CancellationToken cancellationToken)
        {
            await dialogManager.RunDialog(chatId, null, botClient, message, cancellationToken);
        }
    }
}
