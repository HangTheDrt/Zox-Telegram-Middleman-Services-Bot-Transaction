using Telegram.Bot;
using Telegram.Bot.Types;

namespace TelegramShopBot
{
    internal abstract class MenuWithAbilityToReturn : Menu
    {
        protected Func<ITelegramBotClient, Update, long, Task> returnToMainMenuAction;
        public Func<ITelegramBotClient, Update, long, Task> ReturnToMainMenuAction
        {
            set
            {
                if (returnToMainMenuAction == null)
                {
                    returnToMainMenuAction = value;
                }

            }
        }



        protected MenuWithAbilityToReturn(string name) : base(name) { }
    }
}
