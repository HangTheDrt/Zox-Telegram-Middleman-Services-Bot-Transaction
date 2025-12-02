namespace TelegramShopBot
{
    internal class ShopClient
    {
        public long ChatId { get; set; }
        public int? ControlMessageId { get; set; }
        public ShopClientState State { get; set; }
        public decimal Balance { get; set; }
        public List<ProductOrder>? ProductOrders { get; set; }
        public ProductOrder? BeingCreatedProductOrder { get; set; }
    }
}
