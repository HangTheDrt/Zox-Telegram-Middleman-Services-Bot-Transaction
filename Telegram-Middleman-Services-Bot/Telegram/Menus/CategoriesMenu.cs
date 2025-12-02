using Microsoft.EntityFrameworkCore;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TelegramShopBot
{
    internal class CategoriesMenu : MenuWithAbilityToReturn
    {
        private readonly string mmserviceButtonText = "1-50";
        private readonly string mmserviceButtonText2 = "50-200";
        private readonly string mmserviceButtonText3 = "200-1000";
        private readonly string mmserviceButtonText4 = "1000-100000";
        private readonly string returnToMainMenuButtonText = "⬆️Return to main menu⬆️";
        private readonly int maxKeyboardLineLenght;

        public CategoriesMenu(string name, int maxKeyboardLineLenght) : base(name)
        {
            this.maxKeyboardLineLenght = maxKeyboardLineLenght;
        }

        public override async Task CreateMenuGovernMessageAsync(ITelegramBotClient botClient, Update update, long chatId)
        {
            int controlMessageId = 0;

            using (DatabaseContext db = new DatabaseContext())
            {
                ShopClient shopClient = db.ShopClients.FirstOrDefault(sc => sc.ChatId == chatId);

                shopClient.State = ShopClientState.CategoryChoising;
                controlMessageId = (int)shopClient.ControlMessageId;

                await db.SaveChangesAsync();
            };

            await botClient.EditMessageTextAsync(
                chatId: chatId,
                messageId: controlMessageId,
                text: "Choose which number you want to trade so we can prioritize it:",
                replyMarkup: CreateCategoriesMenuKeyboard());
        }

        private InlineKeyboardMarkup CreateCategoriesMenuKeyboard()
        {
            List<Category> allCategories = new();

            using (DatabaseContext db = new DatabaseContext())
            {
                allCategories = db.Categories
                    .Where(c => c.ProductsTypes.Any(pt => pt.Products.Any()))
                    .ToList();
            }

            List<InlineKeyboardButton[]> keyboardButtons = new();
            for (int i = 0; i < (double)allCategories.Count / maxKeyboardLineLenght; i++)
            {
                List<InlineKeyboardButton> buttonsLine = new();

                for (int j = 0; j < maxKeyboardLineLenght; j++)
                {
                    int buttonNum = i * maxKeyboardLineLenght + j;
                    if (allCategories.Count > buttonNum)
                    {
                        buttonsLine.Add(InlineKeyboardButton.WithCallbackData(allCategories[buttonNum].Name));
                    }
                }

                keyboardButtons.Add(buttonsLine.ToArray());
            }

            keyboardButtons.Add(new InlineKeyboardButton[] {
                InlineKeyboardButton.WithCallbackData(mmserviceButtonText),
                InlineKeyboardButton.WithCallbackData(mmserviceButtonText2),
                InlineKeyboardButton.WithCallbackData(mmserviceButtonText3),
                InlineKeyboardButton.WithCallbackData(mmserviceButtonText4)
            });

            return new InlineKeyboardMarkup(keyboardButtons);
        }

        public async Task CategoriesMenuCallBackHanlerAsync(ITelegramBotClient botClient, Update update, long chatId)
        {
            if (update.Type == UpdateType.CallbackQuery)
            {
                string selectedCategory = update.CallbackQuery.Data;

                switch (selectedCategory)
                {
                    case "1-50":
                        await ShowAddressForSelectedCategory(botClient, chatId);
                        break;
                    case "50-200":
                        await ShowAddressForSelectedCategory(botClient, chatId);
                        break;
                    case "200-1000":
                        await ShowAddressForSelectedCategory(botClient, chatId);
                        break;
                    case "1000-100000":
                        await ShowAddressForSelectedCategory(botClient, chatId);
                        break;
                    case "Btc":
                        await ShowBtcAddress(botClient, chatId);
                        break;
                    case "Eth":
                        await ShowEthAddress(botClient, chatId);
                        break;
                    case "Ltc":
                        await ShowLtcAddress(botClient, chatId);
                        break;
                    case "Usdt":
                        await ShowUsdtAddress(botClient, chatId);
                        break;
                    default:
                        // Handle category button click
                        break;
                }
            }
        }

        private async Task ShowAddressForSelectedCategory(ITelegramBotClient botClient, long chatId)
        {
            // Create buttons for different crypto addresses
            InlineKeyboardButton ltcButton = InlineKeyboardButton.WithCallbackData("Ltc");
            InlineKeyboardButton btcButton = InlineKeyboardButton.WithCallbackData("Btc");
            InlineKeyboardButton ethButton = InlineKeyboardButton.WithCallbackData("Eth");
            InlineKeyboardButton UsdtButton = InlineKeyboardButton.WithCallbackData("Usdt");

            // Compose the message with the crypto address options
            string messageText = "Choose Crypto Address To Send:";

            // Create the reply markup with the crypto address buttons
            InlineKeyboardMarkup replyMarkup = new InlineKeyboardMarkup(new[]
            {
                new [] { ltcButton, btcButton, ethButton, UsdtButton }
            });

            // Send the message to the user with the crypto address buttons
            await botClient.SendTextMessageAsync(chatId, messageText, replyMarkup: replyMarkup);
        }

        private async Task ShowLtcAddress(ITelegramBotClient botClient, long chatId)
        {
            // Example LTC address
            string ltcAddress = "3CDJNfdWX8m2NwuGUV3nhXHXEeLygMXoAj";

            // Compose the message with the LTC address
            string messageText = $"LTC Address: {ltcAddress}";

            // Send the message to the user
            await botClient.SendTextMessageAsync(chatId, messageText);
        }

        private async Task ShowBtcAddress(ITelegramBotClient botClient, long chatId)
        {
            // Example BTC address
            string btcAddress = "3J98t1WpEZ73CNmQviecrnyiWrnqRhWNLy";

            // Compose the message with the BTC address
            string messageText = $"BTC Address: {btcAddress}";

            // Send the message to the user
            await botClient.SendTextMessageAsync(chatId, messageText);
        }

        private async Task ShowEthAddress(ITelegramBotClient botClient, long chatId)
        {
            // Example ETH address
            string ethAddress = "0xe688b84b23f322a994A53dbF8E15FA82CDB71127";

            // Compose the message with the ETH address
            string messageText = $"ETH Address: {ethAddress}";

            // Send the message to the user
            await botClient.SendTextMessageAsync(chatId, messageText);
        }

        private async Task ShowUsdtAddress(ITelegramBotClient botClient, long chatId)
        {
            // Example USDT address
            string usdtAddress = "TLxQDLNLNLAh8D5uRh4tFZVS24nr6xZJsJ";

            // Compose the message with the USDT address
            string messageText = $"USDT Address (Trc20): {usdtAddress}";

            // Send the message to the user
            await botClient.SendTextMessageAsync(chatId, messageText);
        }
    }
}
