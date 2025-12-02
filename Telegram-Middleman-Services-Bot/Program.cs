using System;

namespace TelegramShopBot
{
    //CODENAME : OYA_Project

    internal class Program
    {
        private const string token = "6455238621:AAEWpyzS7hB1LOz4yNGddn7tARmsEmU8xWg";



        static void Main(string[] args)
        {
            using(DatabaseContext db = new DatabaseContext())
            {
                db.Database.EnsureCreated();
            }
            Console.WriteLine("DataBase Started");


            TelegramBotManager telegramBotManager = new(token);
            telegramBotManager.StartTelegramBot();
            Console.WriteLine("Bot Started");


            Console.ReadKey();
            Console.WriteLine("End");
        }
    }
}
