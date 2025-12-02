namespace TelegramShopBot
{
    internal class ProductsType
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public List<Product>? Products { get; set; }

        public int CategoryId { get; set; }


        public bool IsAnyOfProductsAvailableToBuy()
        {
            if (Products != null)
            {
                return Products.Where(p => p.IsEmployed == false).Count() != 0;
            }

            return false;
        }        
    }
}
