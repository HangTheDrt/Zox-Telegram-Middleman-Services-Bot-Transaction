using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace TelegramShopBot
{
    abstract class Menu
    {
        public string Name { get; private set; }

        protected const string returnButtonText = "↩️Back↩️";



        public Menu(string name)
        {
            Name = name;
        }



        public abstract Task CreateMenuGovernMessageAsync(ITelegramBotClient botClient, Update update, long chatId);
    }
}
