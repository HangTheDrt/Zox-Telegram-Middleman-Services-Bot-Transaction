using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace TelegramShopBot
{
    public class TelegramBotManager
    {
        private MenusManager menusManager = new(2);

        private CancellationTokenSource cts = new();
        private string token;



        public TelegramBotManager(string token)
        {
            this.token = token;

        }



        public void StartTelegramBot()
        {
            TelegramBotClient botClient = new(token);
            botClient.StartReceiving(
                updateHandler: UpdateHandlerAsync,
                pollingErrorHandler: ErrorHandlerAsync,
                cancellationToken: cts.Token);
        }

        private async Task UpdateHandlerAsync(ITelegramBotClient botClient, Update update, CancellationToken ct)
        {
            long chatId = GetChatId(update);
            if (chatId != 0)
            {
                await menusManager.InvokeUpdateHandlerAsync(botClient, update, chatId);
            }
        }
        private long GetChatId(Update update)
        {
            switch (update.Type)
            {
                case UpdateType.Message:
                    return update.Message.Chat.Id;

                case UpdateType.CallbackQuery:
                    return update.CallbackQuery.Message.Chat.Id;

                default:
                    Console.WriteLine($"update.Type didnt Process : {update.Type}");
                    return 0;
            }
        }

        private async Task ErrorHandlerAsync(ITelegramBotClient botClient, Exception exception, CancellationToken ct)
        {
            Console.WriteLine(exception.Message);
        }
    }
}
