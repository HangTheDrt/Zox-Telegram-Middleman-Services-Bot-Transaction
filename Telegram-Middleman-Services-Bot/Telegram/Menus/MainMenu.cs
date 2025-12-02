using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace TelegramShopBot
{
    internal class MainMenu : Menu
    {
        private readonly string mainMenuMessage = "MiddleMan Service Main Menu";
        private readonly int maxKeyboardLineLenght;

        private List<Menu> secondaryMenus;
        


        public MainMenu(string name,int maxKeyboardLineLenght, List<Menu> secondaryMenus) : base(name)
        {
            this.secondaryMenus = secondaryMenus;
            this.maxKeyboardLineLenght = maxKeyboardLineLenght;
        }



        public override async Task CreateMenuGovernMessageAsync(ITelegramBotClient botClient, Update update, long chatId)
        {
            Message controlMessage = await botClient.SendTextMessageAsync(
               chatId: chatId,
               text: mainMenuMessage,
               replyMarkup: CreateMainKeyboard());

            using (DatabaseContext db = new DatabaseContext())
            {
                ShopClient shopClient = db.ShopClients.FirstOrDefault(sc => sc.ChatId == chatId);

                shopClient.State = ShopClientState.MenuChoising;
                shopClient.ControlMessageId = controlMessage.MessageId;

                await db.SaveChangesAsync();
            };
        }

        private InlineKeyboardMarkup CreateMainKeyboard()
        {
            List<InlineKeyboardButton[]> keyboardButtons = new();
            for (int i = 0; i < (double)secondaryMenus.Count / maxKeyboardLineLenght; i++)
            {
                List<InlineKeyboardButton> buttonsLine = new();

                for (int j = 0; j < maxKeyboardLineLenght; j++)
                {
                    int buttonNum = i * maxKeyboardLineLenght + j;
                    if (secondaryMenus.Count > buttonNum)
                    {
                        buttonsLine.Add(InlineKeyboardButton.WithCallbackData(secondaryMenus[buttonNum].Name));
                    }
                }

                keyboardButtons.Add(buttonsLine.ToArray());
            }

            InlineKeyboardMarkup keyboard = new(keyboardButtons);
            return keyboard;
        }


        public async Task ReturnToMainMenuAsync(ITelegramBotClient botClient, Update update, long chatId)
        {
            int controlMessageId = 0;

            using (DatabaseContext db = new DatabaseContext())
            {
                ShopClient shopClient = db.ShopClients.FirstOrDefault(sc => sc.ChatId == chatId);

                shopClient.State = ShopClientState.MenuChoising;
                controlMessageId = (int)shopClient.ControlMessageId;

                await db.SaveChangesAsync();
            };

            await botClient.EditMessageTextAsync(
                chatId: chatId,
                messageId: controlMessageId,
                text: mainMenuMessage,
                replyMarkup: CreateMainKeyboard());
        }


        public async Task SendCreateCommandToMenuAsync(ITelegramBotClient botClient, Update update, long chatId)
        {
            if (update.Type == UpdateType.CallbackQuery)
            {
                Menu menu = secondaryMenus.FirstOrDefault(m => m.Name == update.CallbackQuery.Data);
                if (menu != null)
                {
                    await menu.CreateMenuGovernMessageAsync(botClient, update, chatId);
                }
            }
        }
    }
}