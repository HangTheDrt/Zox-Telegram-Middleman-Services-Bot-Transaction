namespace TelegramShopBot
{
    public enum ShopClientState
    {
        BeforeMainMenuCreating,
        MenuChoising,
        SimpleMessageReading,
        CategoryChoising,
        ProductsTypeChoising,
        ProductChoising,
        ProductInfoReading,
        AfterPayingForTheProduct,
        AccountMenuChoising,
        AboutAccountMessageReading,
        DeleteAccountWarningMessageReading,
        EnteringTheDepositAmount,
        AfterTopUpBalance,
        ProductOrderChoising,
        ProductOrderInfoReading
    }
}