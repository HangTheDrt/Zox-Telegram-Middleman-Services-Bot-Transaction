using Microsoft.EntityFrameworkCore;
using System.Net.Mail;
using System.Text;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace TelegramShopBot
{
    internal class AccountMenu : MenuWithAbilityToReturn
    {
        private const string productOrdersButtonText = "🛒Orders🛒";
        private const string aboutAccountButtonText = "🔐About the account🔐";
        private const string topUpBalanceButtonText = "💵Top up balance💵";
        private const string deleteAccountButtonText = "🚫Delete account\n🚫";
        private const string returnToMainAccountMenuButtonText = "⬆️Return to account menu⬆️";


        public AccountMenu(string name) : base(name) { }
        


        public override async Task CreateMenuGovernMessageAsync(ITelegramBotClient botClient, Update update, long chatId)
        {
            int controlMessageId = 0;

            using(DatabaseContext db = new DatabaseContext())
            {
                ShopClient shopClient = db.ShopClients.FirstOrDefault(sc => sc.ChatId == chatId);

                shopClient.State = ShopClientState.AccountMenuChoising;
                controlMessageId = (int)shopClient.ControlMessageId;

                await db.SaveChangesAsync();
            }

            await botClient.EditMessageTextAsync(
                chatId: chatId,
                messageId: controlMessageId,
                text: "In this menu you can view your purchases and also top up your balance",
                replyMarkup: CreateAccountMenuKeyboard());
        }
        private InlineKeyboardMarkup CreateAccountMenuKeyboard()
        {
            InlineKeyboardMarkup keyboard = new(
                new[]
                {
                    new[]
                    {
                        InlineKeyboardButton.WithCallbackData(productOrdersButtonText),
                        InlineKeyboardButton.WithCallbackData(aboutAccountButtonText)
                    },

                    new[]
                    {
                        InlineKeyboardButton.WithCallbackData(topUpBalanceButtonText),
                        InlineKeyboardButton.WithCallbackData(deleteAccountButtonText)
                    },

                    new[]
                    {
                        InlineKeyboardButton.WithCallbackData(returnButtonText)
                    }
                });

            return keyboard;
        }

        public async Task AccountMenuCallBackHandlerAsync(ITelegramBotClient botClient, Update update, long chatId)
        {
            if (update.Type == UpdateType.CallbackQuery)
            {
                switch (update.CallbackQuery.Data)
                {
                    case productOrdersButtonText:
                        await CreateProductOrdersMenuGovernMessageAsync(botClient, update, chatId);
                        break;

                    case topUpBalanceButtonText:
                        await CreateTopUpBalanceMenuGovernMessageAsync(botClient, update, chatId);
                        break;

                    case deleteAccountButtonText:
                        await CreateDeleteAccountWarningMessageAsync(botClient, update, chatId);
                        break;

                    case aboutAccountButtonText:
                        await CreateAboutAccountMessageAsync(botClient, update, chatId);
                        break;

                    case returnButtonText:
                        await returnToMainMenuAction.Invoke(botClient, update, chatId);
                        break;
                }
            }
        }

        

        private async Task CreateTopUpBalanceMenuGovernMessageAsync(ITelegramBotClient botClient, Update update, long chatId)
        {
            int controlMessageId = 0;

            using (DatabaseContext db = new DatabaseContext())
            {
                ShopClient shopClient = db.ShopClients.FirstOrDefault(sc => sc.ChatId == chatId);

                shopClient.State = ShopClientState.EnteringTheDepositAmount;
                controlMessageId = (int)shopClient.ControlMessageId;

                await db.SaveChangesAsync();
            }

            await botClient.EditMessageTextAsync(
                chatId: chatId,
                messageId: controlMessageId,
                text: "Enter the top-up amount:",
                replyMarkup: new InlineKeyboardMarkup(InlineKeyboardButton.WithCallbackData(returnButtonText)));
        }

        public async Task TopUpBalanceMenuCallBackHanlerAsync(ITelegramBotClient botClient, Update update, long chatId)
        {
            if (update.Type == UpdateType.CallbackQuery && update.CallbackQuery.Data == returnButtonText)
            {        
                 await CreateMenuGovernMessageAsync(botClient, update, chatId);
            }
            else if (update.Type == UpdateType.Message)
            {
                if (int.TryParse(update.Message.Text, out int depositAmount))
                {
                    using (DatabaseContext db = new DatabaseContext())
                    {
                        ShopClient shopClient = db.ShopClients.FirstOrDefault(sc => sc.ChatId == chatId); 
                        
                        shopClient.Balance += depositAmount;

                        await db.SaveChangesAsync();
                    }

                    await CreateAfterTopUpBalanceMessageAsync(botClient, update, chatId, depositAmount);
                }
            }
        }


        private async Task CreateAfterTopUpBalanceMessageAsync(ITelegramBotClient botClient, Update update, long chatId, int depositAmount)
        {
            using (DatabaseContext db = new DatabaseContext())
            {
                ShopClient shopClient = db.ShopClients.FirstOrDefault(sc => sc.ChatId == chatId);

                int controlMessageId = (int)shopClient.ControlMessageId;

                try
                {
                    await botClient.DeleteMessageAsync(
                    chatId: chatId,
                    messageId: controlMessageId);
                }
                catch (Exception)
                {
                    await botClient.EditMessageTextAsync(
                        chatId: chatId,
                        messageId: controlMessageId,
                        text: "DELETED",
                        replyMarkup: InlineKeyboardMarkup.Empty());
                }

                Message newMessage = await botClient.SendTextMessageAsync(
                    chatId: chatId,
                    text: CreateAfterTopUpBalanceMessageText(shopClient, depositAmount),
                    replyMarkup: new InlineKeyboardMarkup(InlineKeyboardButton.WithCallbackData(returnButtonText)));

                shopClient.State = ShopClientState.AfterTopUpBalance;
                shopClient.ControlMessageId = newMessage.MessageId;

                await db.SaveChangesAsync();
            }
        }
        private string CreateAfterTopUpBalanceMessageText(ShopClient shopClient, int depositAmount)
        {
            string text = $"Balance completed on: {depositAmount}₽ \n" +
                $"Current balance: {shopClient.Balance}₽";

            return text;
        }

        public async Task AfterTopUpBalanceMessageCallBackHandlerAsync(ITelegramBotClient botClient, Update update, long chatId)
        {
            if (update.Type == UpdateType.CallbackQuery && update.CallbackQuery.Data == returnButtonText)
            {
                await CreateMenuGovernMessageAsync(botClient, update, chatId);
            }
        }



        private async Task CreateProductOrdersMenuGovernMessageAsync(ITelegramBotClient botClient, Update update, long chatId)
        {
            ShopClient shopClient = new();
            int controlMessageId = 0;

            using (DatabaseContext db = new DatabaseContext())
            {
                shopClient = db.ShopClients.FirstOrDefault(sc => sc.ChatId == chatId);

                db.ShopClients.Where(sc => sc.ChatId == chatId)
                    .Include(sc1 => sc1.ProductOrders)
                    .ThenInclude(o1 => o1.Product)
                    .Include(sc2 => sc2.ProductOrders)
                    .ThenInclude(o2 => o2.ProductType)
                    .Include(sc3 => sc3.ProductOrders)
                    .ThenInclude(o3 => o3.Category)
                    .ToList();

                shopClient.State = ShopClientState.ProductOrderChoising;
                controlMessageId = (int)shopClient.ControlMessageId;

                await db.SaveChangesAsync();
            }

            await botClient.EditMessageTextAsync(
                chatId: chatId,
                messageId: controlMessageId,
                text: CreateProductOrdersMenuMessageText(shopClient),
                replyMarkup: new InlineKeyboardMarkup(InlineKeyboardButton.WithCallbackData(returnButtonText)));
        }
        private string CreateProductOrdersMenuMessageText(ShopClient shopClient)
        {
            StringBuilder text = new();

            text.Append("🗂Your order history🗂\n\n\n");

            for (int i = 0; i < shopClient.ProductOrders.Count; i++)
            {
                ProductOrder order = shopClient.ProductOrders[i];
                text.Append($"{i+1}. 🕐order date: {order.TimeOfCreating.Value.Date.Day}/{order.TimeOfCreating.Value.Date.Month}/{order.TimeOfCreating.Value.Date.Year} \n " +
                    $"📕Category: {order.Category.Name} \n" +
                    $"💬Product Name: {order.ProductType.Name} \n" +
                    $"⏳\nSubscription time: {order.Product.SubscribeTime} \n\n");
            }

            text.Append("\nTo view product details, write its number");

            return text.ToString();
        }

        public async Task ProductOrdersMenuCallBackHandlerAsync(ITelegramBotClient botClient, Update update, long chatId)
        {
            if (update.Type == UpdateType.CallbackQuery && update.CallbackQuery.Data == returnButtonText)
            {
                await CreateMenuGovernMessageAsync(botClient, update, chatId);
            }
            else if (update.Type == UpdateType.Message)
            {
                if (int.TryParse(update.Message.Text, out int numOfProductOrder))
                {
                    ShopClient shopClient = new();
                         
                    using (DatabaseContext db = new DatabaseContext())
                    {
                        shopClient = db.ShopClients.FirstOrDefault(sc => sc.ChatId == chatId);

                        db.ShopClients.Where(sc => sc.ChatId == chatId)
                            .Include(sc1 => sc1.ProductOrders)
                            .ThenInclude(o1 => o1.Product)
                            .Include(sc2 => sc2.ProductOrders)
                            .ThenInclude(o2 => o2.ProductType)
                            .Include(sc3 => sc3.ProductOrders)
                            .ThenInclude(o3 => o3.Category)
                            .ToList();

                        if (shopClient.ProductOrders.Count <= numOfProductOrder)
                        {
                            shopClient.State = ShopClientState.ProductOrderInfoReading;
                        }

                        await db.SaveChangesAsync();
                    }

                    if (shopClient.ProductOrders.Count <= numOfProductOrder)
                    {
                        await CreateProductOrderInfoMessageAsync(botClient, chatId, (int)shopClient.ControlMessageId, shopClient.ProductOrders[numOfProductOrder - 1]);
                    }
                }
            }
        }



        private async Task CreateProductOrderInfoMessageAsync(ITelegramBotClient botClient, long chatId, int controlMessageId, ProductOrder productOrder)
        {
            await botClient.EditMessageTextAsync(
               chatId: chatId,
               messageId: controlMessageId,
               text: CreateProductOrderInfoText(productOrder),
               replyMarkup: CreateProductOrderInfoMessageKeyboard());
        }
        private string CreateProductOrderInfoText(ProductOrder productOrder)
        {
            string text = $"📕Category: {productOrder.Category.Name} \n" +
                $"💬Product name: {productOrder.ProductType.Name} \n" +
                $"⏳Subscription time: {productOrder.Product.SubscribeTime} \n" +
                $"💵Price: {productOrder.Product.Price} \n\n" +
                $"💬Goods:\n" +
                $"✉️Post office: {productOrder.Product.Mail} \n" +
                $"🔐Email password: {productOrder.Product.MailPassword} \n " +
                $"🔐Account password: {productOrder.Product.AccountPassword}";

            return text;
        }
        private InlineKeyboardMarkup CreateProductOrderInfoMessageKeyboard()
        {
            InlineKeyboardMarkup keyboard = new(
                new[]
                {
                    new[]
                    {
                        InlineKeyboardButton.WithCallbackData(returnButtonText),
                        InlineKeyboardButton.WithCallbackData(returnToMainAccountMenuButtonText)
                    }
                });

            return keyboard;
        }

        public async Task ProductOrderInfoMessageCallBackHandlerAsync(ITelegramBotClient botClient, Update update, long chatId)
        {
            if (update.Type == UpdateType.CallbackQuery)
            {
                if (update.CallbackQuery.Data == returnToMainAccountMenuButtonText)
                {
                    await CreateMenuGovernMessageAsync(botClient, update, chatId);
                }
                else if (update.CallbackQuery.Data 
                    == returnButtonText)
                {
                    await CreateProductOrdersMenuGovernMessageAsync(botClient, update, chatId);
                }
            }
        }
        


        private async Task CreateDeleteAccountWarningMessageAsync(ITelegramBotClient botClient, Update update, long chatId)
        {
            int controlMessageId = 0;

            using (DatabaseContext db = new DatabaseContext())
            {
                ShopClient shopClient = db.ShopClients.FirstOrDefault(sc => sc.ChatId == chatId);

                shopClient.State = ShopClientState.DeleteAccountWarningMessageReading;
                controlMessageId = (int)shopClient.ControlMessageId;

                await db.SaveChangesAsync();
            }

            await botClient.EditMessageTextAsync(
                chatId: chatId,
                messageId: controlMessageId,
                text: CreateWarningMessageText(),
                replyMarkup: CreateWarningMessageKeyboard());
        }
        private string CreateWarningMessageText()
        {
            string text = "Are you sure you want to delete your account? \n" +
                "Deleting your account will: \n" +
                "♦️ к deleting all purchases \n" +
                "♦️ к balance reset \n" +
                "♦️ к deleting information about you in the database \n" +
                "♦️ к voluntary transfer of the soul to the developer of this bot (and he is also the devil)";

            return text;
        }
        private InlineKeyboardMarkup CreateWarningMessageKeyboard()
        {
            InlineKeyboardMarkup keyboard = new(
                new[]
                {
                    new[]
                    {
                        InlineKeyboardButton.WithCallbackData(deleteAccountButtonText),
                        InlineKeyboardButton.WithCallbackData(returnButtonText)
                    }
                });

            return keyboard;
        }

        public async Task DeleteAccountWarningMessageCallBackHandlerAsync(ITelegramBotClient botClient, Update update, long chatId)
        {
            if (update.Type == UpdateType.CallbackQuery)
            {
                if (update.CallbackQuery.Data == returnButtonText)
                {
                    await CreateMenuGovernMessageAsync(botClient, update, chatId);
                }
                else if(update.CallbackQuery.Data == deleteAccountButtonText)
                {
                    int controlMessageId = 0;

                    using (DatabaseContext db = new DatabaseContext())
                    {
                        ShopClient shopClient = db.ShopClients.FirstOrDefault(sc => sc.ChatId == chatId);

                        db.ShopClients.Where(sc => sc.ChatId == chatId)
                            .Include(sc1 => sc1.ProductOrders)
                            .ThenInclude(o1 => o1.Product).ToList();

                        controlMessageId = (int)shopClient.ControlMessageId;

                        db.Products.RemoveRange(shopClient.ProductOrders.Select(o => o.Product));
                        db.ShopClients.Remove(shopClient);
                      
                        await db.SaveChangesAsync();
                    }

                    await botClient.EditMessageTextAsync(
                        chatId: chatId,
                        messageId: controlMessageId,
                        text: "Для того чтобы запустить бота напишите /start",
                        replyMarkup: InlineKeyboardMarkup.Empty());
                }
            }
        }



        private async Task CreateAboutAccountMessageAsync(ITelegramBotClient botClient, Update update, long chatId)
        {
            ShopClient shopClient = new();
            int controlMessageId = 0;
                
            using (DatabaseContext db = new DatabaseContext())
            {
                shopClient = db.ShopClients.FirstOrDefault(sc => sc.ChatId == chatId);

                shopClient.State = ShopClientState.AboutAccountMessageReading;
                controlMessageId = (int)shopClient.ControlMessageId;

                await db.SaveChangesAsync();
            }

            await botClient.EditMessageTextAsync(
                chatId: chatId,
                messageId: controlMessageId,
                text: CreateAboutAccountText(shopClient),
                replyMarkup: new InlineKeyboardMarkup(InlineKeyboardButton.WithCallbackData(returnButtonText)));
        }
        private string CreateAboutAccountText(ShopClient shopClient)
        {
            string text = $"ChatId: {shopClient.ChatId} \n" +
                $"Balance: {shopClient.Balance}₽ \n";

            return text;            
        }

        public async Task AboutAccountMessageCallBackHandler(ITelegramBotClient botClient, Update update, long chatId)
        {
            if (update.Type == UpdateType.CallbackQuery && update.CallbackQuery.Data == returnButtonText)
            {
                await CreateMenuGovernMessageAsync(botClient, update, chatId);
            }
        }
    }
}
