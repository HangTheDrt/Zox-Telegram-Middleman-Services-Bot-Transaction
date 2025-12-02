namespace TelegramShopBot
{
    internal class Product
    {
        public int Id { get; set; }


        public string SubscribeTime { get; set; }
        public decimal Price { get; set; }


        public string Mail { get; set; }
        public string MailPassword { get; set; }
        public string AccountPassword { get; set; }


        public bool IsEmployed { get; set; }
    }
}