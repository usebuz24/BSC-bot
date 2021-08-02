using System;
using System.IO;
using System.Net;
using System.Linq;
using System.Threading.Tasks;

using Telegram.Bot;

using Telegram.Bot.Args;
using TelegramCryptoChatBot.Models;

namespace TelegramCryptoChatBot
{
    class Program
    {
        static TelegramBotClient CryptoBot;
        static void Main(string[] args)
        {
            string token;
            using (var sr = new StreamReader("token.txt"))
            {
                token = sr.ReadToEnd();
            }

            CryptoBot = new TelegramBotClient(token);
            CryptoBot.OnMessage += OnMessageHandler;
            CryptoBot.StartReceiving();
            Console.WriteLine("Бот запущен.");
            Console.Read();
            CryptoBot.StopReceiving();
        }
        static async void OnMessageHandler(object sender, MessageEventArgs e)
        {
            Console.WriteLine($"Получено сообщение в чате {e.Message.From.Username}.");
            #region /start
            if (e.Message.Text == "/start")
            {
                using (var DbContext = new bscBotContext())
                {
                    if (!DbContext.Users.Contains(new Models.User($"{e.Message.Chat.Id}")))
                    {
                        DbContext.Users.Add(new Models.User(e.Message.Chat.Id.ToString(), e.Message.Chat.Username, States.MAIN_MENU));
                        DbContext.SaveChanges();
                        Console.WriteLine("Новый пользователь добавлен.");
                    }
                    else
                    {
                        SetState(States.MAIN_MENU, e);
                    }
                }
                await CryptoBot.SendTextMessageAsync(
                  chatId: e.Message.Chat,
                  replyMarkup: Keyboards.MainMenu,
                  text: "\U0001F916 Привет, я помогу тебе узнать цену на интересующую тебя криптовалюту в сети BSC"
                );
            }
            #endregion
            var currentState = GetState(e);
            #region mainMenu
            if (currentState == States.MAIN_MENU)
            {
                if (e.Message.Text == "Цена токена")
                {
                    await CryptoBot.SendTextMessageAsync(
                      chatId: e.Message.Chat,
                      replyMarkup: Keyboards.RemoveMenu,
                      text: "Введите контракт BSC токена:"

                    );
                    SetState(States.SENDING_CONTRACT, e);
                    return;
                }
                if (e.Message.Text == "Избранное")
                {
                    await CryptoBot.SendTextMessageAsync(
                      chatId: e.Message.Chat,
                      replyMarkup: Keyboards.FavoriteMenu,
                      text: "Что сделать?"
                    );
                    SetState(States.FAVORITE_MENU, e);
                    return;
                }
            }
            #endregion
            #region favoriteMenu
            if (currentState == States.FAVORITE_MENU)
            {
                if (e.Message.Text == "Назад")
                {
                    await CryptoBot.SendTextMessageAsync(
                      chatId: e.Message.Chat,
                      replyMarkup: Keyboards.MainMenu,                    
                      text: "..."
                    );
                    SetState(States.MAIN_MENU, e);
                    return;
                }
                if (e.Message.Text == "Добавить токен")
                {
                    await CryptoBot.SendTextMessageAsync(
                      chatId: e.Message.Chat,
                      replyMarkup: Keyboards.RemoveMenu,
                      text: "Введите контракт токена который хотите добавить:"
                    );
                    SetState(States.FAVORITE_ADDING, e);
                    return;
                }
                if (e.Message.Text == "Удалить токен")
                {
                    await CryptoBot.SendTextMessageAsync(
                      chatId: e.Message.Chat,
                      replyMarkup: Keyboards.RemoveMenu,
                      text: "Введите номер токена который хотите удалить:"
                    );
                    SetState(States.DELETE_CONTRACT, e);
                    return;
                }
                if (e.Message.Text == "Цены токенов")
                {                  
                    await Task.Run(() => ListPricesForAllContracts(e));
                    SetState(States.FAVORITE_MENU, e);
                    return;
                }

            }
            #endregion
            #region sendContract
            if (currentState == States.SENDING_CONTRACT)
            {
                string contract = e.Message.Text;
                await Task.Run(() => GetPrice(contract, e));
                SetState(States.MAIN_MENU, e);
                return;
            }
            #endregion
            #region favoriteAdding
            if (currentState == States.FAVORITE_ADDING)
            {
                string contract = e.Message.Text;
                await Task.Run(() => AddToFavorite(contract, e));
                SetState(States.FAVORITE_MENU, e);
                return;
            }
            #endregion
            #region deleteContract
            if (currentState == States.DELETE_CONTRACT)
            {
                string contractNum = e.Message.Text;
                await Task.Run(() => DeleteContract(contractNum, e));
                SetState(States.FAVORITE_MENU, e);
                return;
            }
            #endregion
        }
        static async void ListPricesForAllContracts(MessageEventArgs e)
        {
            await CryptoBot.SendTextMessageAsync(
                      chatId: e.Message.Chat,
                      replyMarkup: Keyboards.RemoveMenu,
                      text: "Запрос обрабатывается..."
                    );
            string s = "";
            using (var DbContext = new bscBotContext())
            {
                int i = 1;
                var ChoosenContracts = DbContext.Choosencontracts.Where(item => item.UserId == e.Message.Chat.Id.ToString());
                foreach(var item in ChoosenContracts)
                {
                    string ticker = item.Ticker;
                    string price;
                    
                    WebRequest request = WebRequest.Create("https://api.pancakeswap.info/api/v2/tokens/" + item.Contract);
                    WebResponse response = request.GetResponse();
                    using (Stream rawData = response.GetResponseStream())
                    {
                        StreamReader reader = new StreamReader(rawData);
                        string responseFromServer = reader.ReadLine();
                        var split = responseFromServer.Split('"');
                        price = split[15].Substring(0, 10);
                    }
                    s += $"{i}){ticker}: {price}\n";
                    i++;
                }
            }
            await CryptoBot.SendTextMessageAsync(
                        chatId: e.Message.Chat,
                        replyMarkup: Keyboards.FavoriteMenu,
                        replyToMessageId: e.Message.MessageId,
                        text: s
                    );
        }
        static async void AddToFavorite(string contract, MessageEventArgs e)
        {
            using (var DbContext = new bscBotContext())
            {
                var query = DbContext.Choosencontracts.Where(item => item.Contract == contract && item.UserId == e.Message.Chat.Id.ToString());
                foreach (var item in query)
                {
                    await CryptoBot.SendTextMessageAsync(
                        chatId: e.Message.Chat,
                        replyMarkup: Keyboards.FavoriteMenu,
                        replyToMessageId: e.Message.MessageId,
                        text: "Этот контракт уже есть в списке"
                        );
                    return;
                }
            }
            try
            {
                WebRequest request = WebRequest.Create("https://api.pancakeswap.info/api/v2/tokens/" + contract);
                WebResponse response = request.GetResponse();
                using (Stream rawData = response.GetResponseStream())
                {
                    StreamReader reader = new StreamReader(rawData);
                    string responseFromServer = reader.ReadLine();
                    var split = responseFromServer.Split('"');
                    using (var DbContext = new bscBotContext())
                    {
                        DbContext.Choosencontracts.Add(new Choosencontract(split[11].ToUpper(), contract, e.Message.Chat.Id.ToString()));
                        DbContext.SaveChanges();
                    }
                }   
                await CryptoBot.SendTextMessageAsync(
                        chatId: e.Message.Chat,
                        replyMarkup: Keyboards.FavoriteMenu,
                        replyToMessageId: e.Message.MessageId,
                        text: "Готово."
                    );
            }
            catch
            {
                await CryptoBot.SendTextMessageAsync(
                        chatId: e.Message.Chat,
                        replyMarkup: Keyboards.FavoriteMenu,
                        replyToMessageId: e.Message.MessageId,
                        text: "Что-то пошло не так, проверьте правильность контракта."
                    );
            }

        }
        static async void DeleteContract(string contractNum, MessageEventArgs e)
        {
            try
            {
                using (var DbContext = new bscBotContext())
                {
                    var contract = DbContext.Choosencontracts.Where(item => item.UserId == e.Message.Chat.Id.ToString())
                                                             .Skip(Int32.Parse(contractNum) - 1)
                                                             .First();

                    DbContext.Remove(contract);
                    DbContext.SaveChanges();
                }
            }
            catch
            {
                await CryptoBot.SendTextMessageAsync(
                       chatId: e.Message.Chat,
                       replyMarkup: Keyboards.FavoriteMenu,
                       replyToMessageId: e.Message.MessageId,
                       text: "Некорректный ввод."
                   );
                return;
            }
            await CryptoBot.SendTextMessageAsync(
                       chatId: e.Message.Chat,
                       replyMarkup: Keyboards.FavoriteMenu,
                       replyToMessageId: e.Message.MessageId,
                       text: "Готово"
                   );
        }
        static async void GetPrice(string contract, MessageEventArgs e)
        {
            try
            {
                WebRequest request = WebRequest.Create("https://api.pancakeswap.info/api/v2/tokens/" + contract);
                WebResponse response = request.GetResponse();
                using (Stream rawData = response.GetResponseStream())
                {
                    StreamReader reader = new StreamReader(rawData);
                    string responseFromServer = reader.ReadLine();
                    var split = responseFromServer.Split('"');
                    await CryptoBot.SendTextMessageAsync(
                        chatId: e.Message.Chat,
                        replyMarkup: Keyboards.MainMenu,
                        replyToMessageId: e.Message.MessageId,
                        text: $"Имя: {split[7]}\n" +
                              $"Тикер: {split[11].ToUpper()}\n" +
                              $"Цена в USD: {split[15].Substring(0, 10)}\n"
                    );
                }
            }
            catch
            {
                await CryptoBot.SendTextMessageAsync(
                        chatId: e.Message.Chat,
                        replyMarkup: Keyboards.MainMenu,
                        replyToMessageId: e.Message.MessageId,
                        text: "Что-то пошло не так, проверьте правильность контракта"
                    );
            }

        }
        static void SetState(string state, MessageEventArgs e)
        {
            using (var DbContext = new bscBotContext())
            {
                DbContext.Users.Find(e.Message.Chat.Id.ToString())
                           .CurrentState = state;
                DbContext.SaveChanges();
            }
        }
        static string GetState(MessageEventArgs e)
        {
            using (var DbContext = new bscBotContext())
            {
                var user = DbContext.Users.Find(e.Message.Chat.Id.ToString());
                return user.CurrentState;
            }
        }
    }
}