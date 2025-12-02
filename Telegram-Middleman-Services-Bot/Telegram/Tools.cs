using System.Text;

namespace TelegramShopBot
{
    internal static class Tools
    {
        public static string GetMessageTextFromTxt(string path)
        {
            using (StreamReader sr = new StreamReader(Environment.CurrentDirectory + @"\Telegram\Messages\" + path, Encoding.Default))
            {
                string readText = sr.ReadToEnd();
                return readText;
            }
        }
    }
}
