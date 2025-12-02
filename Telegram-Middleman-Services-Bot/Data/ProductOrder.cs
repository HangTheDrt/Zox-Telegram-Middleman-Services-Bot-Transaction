namespace TelegramShopBot
{
    internal class ProductOrder
    {
        public int Id { get; set; }
        public DateTime? TimeOfCreating { get; set; }
        public Category? Category { get; set; }
        public ProductsType? ProductType { get; set; }
        public Product? Product { get; set; }

        public ShopClient? ShopClientOnCreating { get; set; }
        public long? ShopClientIdOnCreating { get; set; }

        public ShopClient? ShopClientOnStoring { get; set; }
        public long? ShopClientIdOnStoring { get; set; }
    }
}
