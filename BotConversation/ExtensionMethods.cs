using BotConversation.Connectors.Base;
using BotConversation.Dialogs.Base;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using static BotConversation.DialogManager;

namespace BotConversation
{
    public static class ExtensionMethods
    {
        public static IServiceCollection AddBotConnector(this IServiceCollection services, ExceptionHandle? exceptionHandle = null)
        {
            IEnumerable<Type> dialogTypes = Assembly.GetCallingAssembly().GetTypes().Where(X => X.BaseType == typeof(Dialog));
            IEnumerable<Type> statelessDialogTypes = Assembly.GetCallingAssembly().GetTypes().Where(X => X.BaseType == typeof(StatelessDialog));
            List<Type> allDialogTypes = [.. dialogTypes, .. statelessDialogTypes];

            foreach (var dialog in allDialogTypes)
            {
                services.AddScoped(dialog);
            }

            services.AddScoped<BotConnector>(x =>
            {
                IServiceProvider provider = x.CreateScope().ServiceProvider;
                Dialog[] dialogsInstance = dialogTypes.Select(x => (Dialog)provider.GetService(x)).ToArray();
                StatelessDialog[] statelessDialogsInstance = statelessDialogTypes.Select(x => (StatelessDialog)provider.GetService(x)).ToArray();
                BotConnector instance = new(dialogsInstance, dialogsInstance.Where(x => x.MainDialog).ToArray(), statelessDialogsInstance);

                instance.HandleException((ex, chatId) =>
                {
                    ClearStatus(chatId);
                    if (exceptionHandle != null) exceptionHandle(ex, chatId);
                });

                return instance;
            });

            return services;
        }
    }
}
