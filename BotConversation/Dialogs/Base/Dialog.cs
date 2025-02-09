using BotConversation.Models;

namespace BotConversation.Dialogs.Base
{
    public abstract class Dialog
    {
        public string Name { get; set; }
        public string[] ConversationOrder = { };
        public string ChatId { get; set; }
        public DialogStatus DialogStatus { get; set; }
        public DialogManager DialogManager { get; set; } = new();

        public bool IsValidationContext() => DialogStatus.ConversationStatus.Sent;

        public void SetUserStatus(string chatId, string key, object value)
        {
            if(DialogManager.GetDialogStatus(chatId).UserStatus.ContainsKey(key))
            {

            }
            DialogManager.GetDialogStatus(chatId).UserStatus[key] = value;
        }

        public T GetUserStatus<T>(string chatId, string key)
        {
            if (!this.DialogManager.GetDialogStatus(chatId).UserStatus.ContainsKey(key))
            {
                return (T)(object)null;
            }
            return (T)this.DialogManager.GetDialogStatus(chatId).UserStatus[key];
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
