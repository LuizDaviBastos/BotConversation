using BotConversation.Models;
using BotConversation.Models.Attributes;
using System.Reflection;

namespace BotConversation.Dialogs.Base
{
    public abstract class Dialog
    {
        public Dialog(string name, string[] conversationOrder)
        {
            Name = name;
            ConversationOrder = conversationOrder;
        }

        public Dialog(string[] conversationOrder)
        {
            Name = this.GetType().Name;
            ConversationOrder = conversationOrder;
        }

        public Dialog()
        {
            Name = this.GetType().Name;
            ConversationOrder = GetType().GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                .Where(m => !m.GetCustomAttributes(typeof(ConversationValidator), false).Any()).Select(x => x.Name).ToArray();
        }

        public bool MainDialog { get; set; }
        public string Name { get; set; }
        public string[] ConversationOrder = { };
        public string ChatId { get; set; }
        public DialogStatus DialogStatus { get; set; }
        public DialogManager DialogManager { get; set; }

        public bool IsValidationContext() => DialogStatus.ConversationStatus.Sent;

        public void SetData<T>(string key, T value)
        {
            var userStatus = DialogManager.GetDialogStatus(ChatId).UserStatus;
            userStatus[key] = value;
        }

        public T? GetData<T>(string key)
        {
            var userStatus = DialogManager.GetDialogStatus(ChatId).UserStatus;
            if (!userStatus.ContainsKey(key)) return default(T);

            var value = userStatus[key];
            if(value != null)
            {
                return (T)userStatus[key];
            }
            else
            {
                return default(T);
            }
        }

        public void FinishDialog()
        {
            if (DialogStatus.Parent != null)
            {
                DialogStatus.Parent.SubStatus = null;
            }
            else
            {
                this.DialogManager.RemoveStatus(ChatId);
            }
        }

        public async Task RunDialog(string dialogName, params object[] args)
        {
            Dialog? dialog = DialogManager.AllDialogs.FirstOrDefault(x => x.Name == dialogName);
            if (dialog == null) return;
            
            DialogStatus.SubStatus = new DialogStatus(dialogName, dialog.ConversationOrder.First(), DialogStatus);
            await this.DialogManager.RunDialog(ChatId, DialogStatus.SubStatus, args);
        }

        public async Task RunDialog(string dialogName, string conversation, params object[] args)
        {
            Dialog? dialog = DialogManager.AllDialogs.FirstOrDefault(x => x.Name == dialogName);
            if (dialog == null) return;

            DialogStatus.SubStatus = new DialogStatus(dialogName, conversation, DialogStatus);
            await this.DialogManager.RunDialog(ChatId, DialogStatus.SubStatus, args);
        }
    }
}
