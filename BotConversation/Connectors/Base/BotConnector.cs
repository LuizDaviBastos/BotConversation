using BotConversation.Dialogs.Base;
using static BotConversation.DialogManager;

namespace BotConversation.Connectors.Base
{
    public class BotConnector
    {
        private readonly DialogManager dialogManager;

        public BotConnector(Dialog[] dialogs, Dialog[] executionOrder, StatelessDialog[] statelessDialogs)
        {
            dialogManager = new();
            dialogManager.AllDialogs = dialogs;
            dialogManager.DialogsExecutionOrder = executionOrder;
            dialogManager.StatelessDialogs = statelessDialogs;
        }

        public virtual async Task HandleUpdateAsync(string chatId, object botClient, object message, CancellationToken cancellationToken)
        {
            await dialogManager.RunDialog(chatId, null, botClient, message, cancellationToken);
        }

        public virtual void HandleException(ExceptionHandle exceptionHandler)
        {
            dialogManager.OnException += exceptionHandler;
        }
    }
}
