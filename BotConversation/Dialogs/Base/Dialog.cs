using BotConversation.Models;

namespace BotConversation.Dialogs.Base
{
    public abstract class Dialog
    {
        public Dialog(string name, string[] conversationOrder)
        {
            Name = name;
            ConversationOrder = conversationOrder;
        }

        public string Name { get; set; }
        public string[] ConversationOrder = { };
        public string ChatId { get; set; }
        public DialogStatus DialogStatus { get; set; }
        public DialogManager DialogManager { get; set; } = new();

        public bool IsValidationContext() => DialogStatus.ConversationStatus.Sent;

        public void SetData<T>(string key, T value)
        {
            var userStatus = DialogManager.GetDialogStatus(ChatId).UserStatus;
            userStatus[key] = value;
        }

        public T GetData<T>(string key)
        {
            var userStatus = DialogManager.GetDialogStatus(ChatId).UserStatus;
            return (T)userStatus[key];
        }

        public void FinishDialog(string chatId)
        {
            if (DialogStatus.Parent != null)
            {
                DialogStatus.Parent.SubStatus = null;
            }
            else
            {
                this.DialogManager.RemoveStatus(chatId);
            }
        }

        public async Task RunDialog(string dialogName, params object[] args)
        {
            Dialog? dialog = DialogManager.AllDialogs.FirstOrDefault(x => x.Name == dialogName);
            if (dialog == null) return;
            
            DialogStatus.SubStatus = new DialogStatus(dialogName, dialog.ConversationOrder.First(), DialogStatus);
            await this.DialogManager.RunDialog(ChatId, DialogStatus, args);
        }
    }
}
