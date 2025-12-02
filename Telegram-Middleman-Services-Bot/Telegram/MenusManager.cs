using Microsoft.EntityFrameworkCore;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace TelegramShopBot
{
    internal class MenusManager
    {
        private CategoriesMenu categoriesMenu;
        //private AccountMenu accountMenu = new("👤Account👤");
        private MainMenu mainMenu;

        private List<Menu> allMenus;

        private Dictionary<ShopClientState, Func<ITelegramBotClient, Update, long, Task>> updateHandlers;

        private readonly string startCommand = "/start";



        public MenusManager(int maxKeyboardLineLenght)
        {
            categoriesMenu = new CategoriesMenu("📚MiddleMan Serive📚", maxKeyboardLineLenght);

            allMenus = new()
            {
                categoriesMenu,
                //accountMenu,
                //new SimpleMenu("❔Help❔", Tools.GetMessageTextFromTxt("HelpMessage.txt")),
                new SimpleMenu("🛑Term🛑", Tools.GetMessageTextFromTxt("RulesMessage.txt")),
                new SimpleMenu("💻Contact Us💻", Tools.GetMessageTextFromTxt("DeveloperMessage.txt"))
            };

            mainMenu = new MainMenu("main",maxKeyboardLineLenght, allMenus);

            categoriesMenu.ReturnToMainMenuAction = mainMenu.ReturnToMainMenuAsync;
            //accountMenu.ReturnToMainMenuAction = mainMenu.ReturnToMainMenuAsync;

            updateHandlers = new()
            {
                {ShopClientState.BeforeMainMenuCreating, mainMenu.CreateMenuGovernMessageAsync },
                {ShopClientState.MenuChoising, mainMenu.SendCreateCommandToMenuAsync},
                {ShopClientState.SimpleMessageReading, mainMenu.ReturnToMainMenuAsync },
                {ShopClientState.CategoryChoising, categoriesMenu.CategoriesMenuCallBackHanlerAsync },
                //{ShopClientState.ProductsTypeChoising, categoriesMenu.CategoryMenuCallBackHandlerAsync },
                //{ShopClientState.ProductChoising, categoriesMenu.ProductTypeMenuCallBackHandlerAsync },
                //{ShopClientState.ProductInfoReading, categoriesMenu.ProductMenuCallBackHandlerAsync },
                //{ShopClientState.AfterPayingForTheProduct, categoriesMenu.AfterPaymentMenuCallBackHandlerAsync },
                //{ShopClientState.AccountMenuChoising, accountMenu.AccountMenuCallBackHandlerAsync },
                //{ShopClientState.AboutAccountMessageReading, accountMenu.AboutAccountMessageCallBackHandler },
                //{ShopClientState.DeleteAccountWarningMessageReading, accountMenu.DeleteAccountWarningMessageCallBackHandlerAsync },
                //{ShopClientState.EnteringTheDepositAmount, accountMenu.TopUpBalanceMenuCallBackHanlerAsync },
                //{ShopClientState.AfterTopUpBalance, accountMenu.AfterTopUpBalanceMessageCallBackHandlerAsync },
                //{ShopClientState.ProductOrderChoising, accountMenu.ProductOrdersMenuCallBackHandlerAsync },
                //{ShopClientState.ProductOrderInfoReading, accountMenu.ProductOrderInfoMessageCallBackHandlerAsync }
            };
        }



        public async Task InvokeUpdateHandlerAsync(ITelegramBotClient botClient, Update update, long chatId)
        {
            await CheckShopClientToExistInDb(chatId);

            await CheckToRestartCommandAsync(botClient, update, chatId);

            ShopClientState shopClientState = GetShopClientState(chatId);
            await updateHandlers[shopClientState].Invoke(botClient, update, chatId);
        }

        private async Task CheckShopClientToExistInDb(long chatId)
        {
            using (DatabaseContext db = new DatabaseContext())
            {
                ShopClient shopClient = db.ShopClients.FirstOrDefault(sc => sc.ChatId == chatId);
                if (shopClient == null)
                {
                    shopClient = new ShopClient { ChatId = chatId, State = ShopClientState.BeforeMainMenuCreating, };

                    db.ShopClients.Add(shopClient);

                    await db.SaveChangesAsync();
                }
            }
        }

        private async Task CheckToRestartCommandAsync(ITelegramBotClient botClient, Update update, long chatId)
        {
            if (update.Type == UpdateType.Message && update.Message.Text == startCommand)
            {
                using (DatabaseContext db = new DatabaseContext())
                {
                    ShopClient shopClient = db.ShopClients.FirstOrDefault(sc => sc.ChatId == chatId);

                    db.ShopClients.Where(sc => sc.ChatId == chatId)
                        .Include(cs1 => cs1.BeingCreatedProductOrder)
                        .ThenInclude(o => o.Product)
                        .ToList();

                    if (shopClient != null && shopClient.ControlMessageId != null)
                    {
                        await ReturnShopClientToStartPositionAsync(botClient, shopClient, chatId, db);
                    }
                    shopClient.State = ShopClientState.BeforeMainMenuCreating;
                    shopClient.ControlMessageId = null;

                    await db.SaveChangesAsync();
                };
            }
        }
        private async Task ReturnShopClientToStartPositionAsync(ITelegramBotClient botClient, ShopClient shopClient, long chatId, DatabaseContext db)
        {
            if (shopClient.ControlMessageId != null)
            {
                try
                {
                    await botClient.DeleteMessageAsync(
                    chatId: chatId,
                    messageId: (int)shopClient.ControlMessageId);
                }
                catch (Exception)
                {
                    try
                    {
                        await botClient.EditMessageTextAsync(
                            chatId: chatId,
                            messageId: (int)shopClient.ControlMessageId,
                            text: "DELETED",
                            replyMarkup: InlineKeyboardMarkup.Empty());
                    }
                    catch (Exception) { }                           
                }
                
            }
            if (shopClient.BeingCreatedProductOrder != null)
            {
                if (shopClient.BeingCreatedProductOrder.Product != null)
                {
                    shopClient.BeingCreatedProductOrder.Product.IsEmployed = false;
                }

                db.ProductsOrders.Remove(shopClient.BeingCreatedProductOrder);
            }
        }

        private ShopClientState GetShopClientState(long chatId)
        {
            ShopClient shopClient;

            using (DatabaseContext db = new DatabaseContext())
            {
                shopClient = db.ShopClients.FirstOrDefault(sc => sc.ChatId == chatId);
            }

            return shopClient.State;
        }
       
        
    }
}
