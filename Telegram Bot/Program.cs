using System;
using System.Threading.Tasks;
using System.Threading;
using Telegram.Bot;
using Telegram.Bot.Types;
using Newtonsoft.Json.Linq;
using Telegram.Bot.Polling;
using xNet;
using Telegram.Bot.Types.ReplyMarkups;
using System.Collections.Generic;

namespace Telegram_Bot
{
    public enum Valutecodes
    {
        USD = 840,
        EUR = 978,
        GBP = 826,
        JPY = 932,
        None
    }
    class Program
    {

        public static string token = "5511397085:AAGILbUwNRsEt7_36cqPVYgDOABGvXoSrCk";
        public static TelegramBotClient client = new TelegramBotClient(token);
        public static async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken) //CallbackQuery back)
        {
            Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(update));
            if (update.Type == Telegram.Bot.Types.Enums.UpdateType.Message)
            {
                var message = update.Message;
                var a = Valutecodes.None;
                if (message.Text == "EUR")
                    a = Valutecodes.EUR;
                else if (message.Text == "USD")
                    a = Valutecodes.USD;
                if (message.Text.ToLower() == "/start")
                {
                    await botClient.SendTextMessageAsync(message.Chat, "Привет! На сегодня доступны только 2 валюты.\nНо скоро я добавлю больше", replyMarkup: Buttons());
                    return;
                }
                if (message.Text == a.ToString())
                {
                    await botClient.SendTextMessageAsync(message.Chat, Valut((int)a), replyMarkup: Buttons());
                    return;
                }
                await botClient.SendTextMessageAsync(message.Chat, "?", replyMarkup: Buttons());
                return;
            }

            if (update.Type == Telegram.Bot.Types.Enums.UpdateType.CallbackQuery)
            {
                var message = update.CallbackQuery;
                var a = Valutecodes.None;
                if (message.Data == "EUR")
                    a = Valutecodes.EUR;
                else if (message.Data == "USD")
                    a = Valutecodes.USD;
                if (message.Data == a.ToString())
                {
                    await botClient.SendTextMessageAsync(update.CallbackQuery.Message.Chat.Id, Valut((int)a), replyMarkup: Buttons());
                    return;
                }
            }

        }

        public static InlineKeyboardMarkup Buttons()
        {
            var ikm = new InlineKeyboardMarkup(new[]
{
    new[]
    {
        InlineKeyboardButton.WithCallbackData("USD", "USD"),
    },
    new[]
    {
        InlineKeyboardButton.WithCallbackData("EUR", "EUR"),
    },
});

            return ikm;
        }

        public static async Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(exception));
        }


        static void Main(string[] args)
        {
            Console.WriteLine("Запущен бот " + client.GetMeAsync().Result.FirstName);


            var cts = new CancellationTokenSource();
            var cancellationToken = cts.Token;
            var receiverOptions = new ReceiverOptions
            {
                AllowedUpdates = { }, // receive all update types
            };
            client.StartReceiving(
                HandleUpdateAsync,
                HandleErrorAsync,
                receiverOptions,
                cancellationToken
                );

            
            Console.ReadLine();
        }

      

        public static string Valut(int valut)
        {
            string content = "";
            string result = "";
            try
            {
                using (HttpRequest request = new HttpRequest())
                {
                    request.AddHeader("content type", "application/json; charset=utf-8");
                    request["upgrade-insecure-requests"] = "1";
                    request["accept"] = "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.9";
                    request["accept-language"] = "ru-RU,ru;q=0.9,en-US;q=0.8,en;q=0.7";
                    content = request.Get("https://api.monobank.ua/bank/currency").ToString();
                }
                string[] data = content.Split('}');
                foreach (string el in data)
                {
                    if (el.Contains(valut.ToString()))
                    {
                        result = $"Buy price: {el.Substring("rateBuy\":", ",")} UAH\nSell Price: {el.Substring("rateSell\":")} UAH";
                        break;
                    }
                }
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return "ошибка в компьютере Алексея Васильевича";
            }
            
        } 
    }
}
