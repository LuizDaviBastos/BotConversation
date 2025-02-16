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
            var dialogs = Assembly.GetCallingAssembly().GetTypes().Where(X => X.BaseType == typeof(Dialog));

            foreach (var dialog in dialogs)
            {
                services.AddScoped(dialog);
            }

            services.AddScoped<BotConnector>(x =>
            {
                var provider = x.CreateScope().ServiceProvider;
                Dialog[] dialogsInstance = dialogs.Select(x => (Dialog)provider.GetService(x)).ToArray();
                var instance = new BotConnector(dialogsInstance, dialogsInstance.Where(x => x.MainDialog).ToArray());

                instance.HandleException((ex, chatId) =>
                {
                    DialogManager.ClearStatus(chatId);
                    if(exceptionHandle != null) exceptionHandle(ex, chatId);
                });

                return instance;
            });

            return services;
        }
    }
}
