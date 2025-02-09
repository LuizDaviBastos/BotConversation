using BotConversation.Dialogs.Base;
using BotConversation.Models;
using BotConversation.Models.Attributes;
using System.Reflection;

namespace BotConversation
{
    public class DialogManager
    {
        public static Dictionary<string, DialogStatus> DialogStatus = new Dictionary<string, DialogStatus>();
        public Dialog[] AllDialogs { get; set; } = { };
        public Dialog[] DialogsExecutionOrder { get; set; } = { };

        public DialogStatus GetDialogStatus(string chatId)
        {
            if(!DialogStatus.ContainsKey(chatId))
            {
                var firsDialog = DialogsExecutionOrder.First();
                var firsConversationDialog = firsDialog.ConversationOrder.First();
                SaveStatus(chatId, new DialogStatus(firsDialog.Name, firsConversationDialog));
            }
            return DialogStatus[chatId];
        }

        public void SaveStatus(string chatId, DialogStatus status)
        {
            DialogStatus[chatId] = status;
        }

        public void RemoveStatus(string chatId, DialogStatus? dialogStatus = default)
        {
            if(dialogStatus?.Parent != null)
            {
                dialogStatus.Parent.SubStatus = null;
            } 
            else if (DialogStatus.ContainsKey(chatId))
            {
                DialogStatus.Remove(chatId);
            }
        }

        public bool Next(string chatId, DialogStatus? dialogStatus = default)
        {
            bool hasNext = false;
            DialogStatus mainStatus = GetDialogStatus(chatId);
            DialogStatus currentStatus = dialogStatus == default ? mainStatus : dialogStatus;
            
            if (currentStatus != null)
            {
                Dialog? dialog = AllDialogs.FirstOrDefault(x => x.Name == currentStatus.DialogName);
                hasNext = true;

                dialog.DialogStatus = currentStatus;

                int currentConversationIndex = Array.IndexOf(dialog.ConversationOrder, currentStatus.ConversationStatus.Name);

                //if no has more conversation
                if(currentConversationIndex >= dialog.ConversationOrder.Length - 1)
                {
                    //if has next dialogs
                    int currentDialogIndex = Array.IndexOf(DialogsExecutionOrder, dialog);
                    if(DialogsExecutionOrder.Any(x => x.Name == dialog.Name) && currentDialogIndex < DialogsExecutionOrder.Length -1) 
                    {
                        var newCurrentDialog = DialogsExecutionOrder[currentDialogIndex + 1];
                        currentStatus.Set(newCurrentDialog.Name, newCurrentDialog.ConversationOrder.First());
                        SaveStatus(chatId, currentStatus);
                    }
                    else
                    {
                        hasNext = false;
                    }
                }
                else
                {
                    currentStatus.Set(dialog.ConversationOrder[currentConversationIndex + 1]);
                    SaveStatus(chatId, mainStatus);
                }   
            }
            return hasNext;
        }

        public async Task RunDialog(string chatId, DialogStatus? dialogStatus = default, params object[] args)
        {
            DialogStatus currentStatus = dialogStatus == default ? GetDialogStatus(chatId) : dialogStatus;

            if(currentStatus.SubStatus != null)
            {
                await RunDialog(chatId, currentStatus.SubStatus, args);
                return;
            }

            Dialog? dialog = AllDialogs.FirstOrDefault(x => x.Name == currentStatus.DialogName);
            if (dialog != null)
            {
                dialog.DialogStatus = currentStatus;
                dialog.DialogManager = this;

                if (currentStatus.ConversationStatus.Sent)
                {
                    MethodInfo? methodValidator = dialog.GetType().GetMethods()
                        .FirstOrDefault(m =>
                        m.GetCustomAttributes(typeof(ConversationValidator), true).Length > 0 && ((ConversationValidator?)m.GetCustomAttribute(typeof(ConversationValidator), true))?.Name == currentStatus.ConversationStatus.Name);

                    if((methodValidator != null && ((bool)methodValidator.Invoke(dialog, args)!)) || methodValidator == null)
                    {
                        if(Next(chatId, currentStatus))
                        {
                            await RunDialog(chatId, currentStatus, args);
                        }
                        else
                        {
                            RemoveStatus(chatId, dialogStatus);
                            if(dialogStatus?.Parent != null) await RunDialog(chatId, dialogStatus.Parent, args);
                            else await RunDialog(chatId, GetDialogStatus(chatId), args);
                        }
                        return;
                    }
                }

                MethodInfo method = dialog.GetType().GetMethods().FirstOrDefault(m => m.Name == currentStatus.ConversationStatus.Name)!;
                if (method == null) return;

                Task task = (Task)method.Invoke(dialog, args)!;
                await task.ConfigureAwait(false);
                currentStatus.ConversationStatus.Sent = true;
            }
        }

        public async Task RunDialogManually(string chatId, string dialogName, params object[] args)
        {
            var dialog = AllDialogs.FirstOrDefault(x => x.Name == dialogName);
            if (dialog != null)
            {
                var conversation = dialog.ConversationOrder.First();
                SaveStatus(chatId, new DialogStatus(dialog.Name, conversation));
                await RunDialog(chatId, args: args);
                RemoveStatus(chatId);
            }
        }
    }
}
