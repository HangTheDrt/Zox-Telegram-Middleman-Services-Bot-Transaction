using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace TelegramShopBot
{
    internal class SimpleMenu : Menu
    {
        private readonly string messageText;



        public SimpleMenu(string name, string messageText) : base(name)
        {
            this.messageText = messageText;
        }



        public override async Task CreateMenuGovernMessageAsync(ITelegramBotClient botClient, Update update, long chatId)
        {
            int controlMessageId;

            using (DatabaseContext db = new DatabaseContext())
            {
                ShopClient shopClient = db.ShopClients.FirstOrDefault(sc => sc.ChatId == chatId);

                shopClient.State = ShopClientState.SimpleMessageReading;
                controlMessageId = (int)shopClient.ControlMessageId;

                await db.SaveChangesAsync();
            };

            await botClient.EditMessageTextAsync(
                chatId: chatId,
                messageId: controlMessageId,
                text: messageText,
                replyMarkup: CreateGovernMessageKeyboard());
        }

        private InlineKeyboardMarkup CreateGovernMessageKeyboard()
        {
            InlineKeyboardMarkup keyboard = new InlineKeyboardMarkup(
                new[]
                {
                    new[]
                    {
                        InlineKeyboardButton.WithCallbackData(returnButtonText)
                    }
                });

            return keyboard;
        }

    }
}
