using System;
using System.IO;
using System.Net;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

using MySqlConnector;
using Telegram.Bot.Args;

namespace TelegramCryptoChatBot
{
    class Program
    {
        static TelegramBotClient CryptoBot;
        static string token = "1830595825:AAETdTaoSqldgfrWMoqUbitWotHq_XEUbkg";
        static MySqlConnection SqlConn = new MySqlConnection("server = localhost; user = root; database = testkey; password = usebuz9090;");

        static void Main(string[] args)
        {
            Console.WriteLine("Подключаю базу данных...");
            try
            {
                SqlConn.Open();
                Console.WriteLine("Подключение успешно.");
            }
            catch (Exception e)
            {
                Console.WriteLine("Ошибка: " + e.Message);
            }

            CryptoBot = new TelegramBotClient(token);
            CryptoBot.OnMessage += OnMessageHandler;
            CryptoBot.StartReceiving();
            Console.WriteLine("Слушаю");
            Console.Read();
            CryptoBot.StopReceiving();
        }
        static async void OnMessageHandler(object sender, MessageEventArgs e)
        {
            Console.WriteLine($"Получено сообщение в чате {e.Message.From.Username}.");
            #region /start
            if (e.Message.Text == "/start")
            {
                try
                {
                    var AddUser = $"INSERT INTO users VALUES('{e.Message.Chat.Id}', '{e.Message.From.Username}', '{States.MAIN_MENU}')";
                    MySqlCommand command = new MySqlCommand(AddUser, SqlConn);
                    command.ExecuteNonQuery();
                }
                catch(Exception a)
                {
                    Console.WriteLine(a.Message + " - Такой пользователь уже есть в базе");
                    SetState(States.MAIN_MENU, e);
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
                if (e.Message.Text == "Цены токенов")
                {
                    await CryptoBot.SendTextMessageAsync(
                      chatId: e.Message.Chat,
                      replyMarkup: Keyboards.RemoveMenu,
                      text: "Запрос обрабатывается..."
                    );
                    SetState(States.LIST_PRICES, e);
                    await Task.Run(() => ListPricesForAllContracts(e));
                    //SetState(States.FAVORITE_MENU, e);
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
                return;
            }
            #endregion
        }
        
        static async void ListPricesForAllContracts(MessageEventArgs e) //Метод написан максимально колхозно, когда будет время сделать его лаконичней - сделаю.
        {
            List<string[]> arrayList = new List<string[]>();
            using (var command = new MySqlCommand($"SELECT ticker, contract FROM choosencontracts WHERE userID = {e.Message.From.Id}", SqlConn))
            {
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string ticker = reader.GetString(0);
                        string price;
                        WebRequest request = WebRequest.Create("https://api.pancakeswap.info/api/v2/tokens/" + reader.GetString(1));
                        WebResponse response = request.GetResponse();
                        using (Stream rawData = response.GetResponseStream())
                        {
                            StreamReader reader2 = new StreamReader(rawData);
                            string responseFromServer = reader2.ReadLine();
                            var split = responseFromServer.Split('"');
                            price = split[15].Substring(0, 10);
                        }
                        arrayList.Add(new string[] { ticker, price });
                    }
                }
            }
            string s = "";
            int i = 1;
            foreach (var item in arrayList)
            {
                s += $"{i}){item[0]}: {item[1]}\n";
                i++;
            }
            await CryptoBot.SendTextMessageAsync(
                        chatId: e.Message.Chat,
                        replyMarkup: Keyboards.FavoriteMenu,
                        replyToMessageId: e.Message.MessageId,
                        text: s
                    );
            SetState(States.FAVORITE_MENU, e);
        }

        static async void AddToFavorite(string contract, MessageEventArgs e)
        {
            bool flag = false; //*
            await using (var command = new MySqlCommand($"SELECT contract FROM choosencontracts WHERE userID = {e.Message.From.Id}", SqlConn))
            {
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        if (reader.GetString(0) == contract) flag = true; //*
                    }
                }
            }
            if (flag)
            { 
                await CryptoBot.SendTextMessageAsync(
                    chatId: e.Message.Chat,
                    replyMarkup: Keyboards.FavoriteMenu,
                    replyToMessageId: e.Message.MessageId,
                    text: "Этот контракт уже есть в списке"
                    );                           
                                                    //* Этот "выворот" с доп. флагом сделан намеренно, так как при вызове метода SetState() внутри using'a выше, он(метод) попытается использовать
                SetState(States.FAVORITE_MENU, e);  //  занятый поток подключения к sql, что вызовет исключение. Технически, я мог бы создать дополнительное подключение, но это лишняя  
                return;                             //  секунда-две, что, по моему ощущению, достаточно много, когда мы говорим о боте в мессенджере.
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
                    var command = $"INSERT INTO choosencontracts VALUES('{split[11].ToUpper()}', '{contract}', '{e.Message.From.Id}');";
                    MySqlCommand addContract = new MySqlCommand(command, SqlConn);
                    addContract.ExecuteNonQuery();
                }
            }
            catch
            {
                await CryptoBot.SendTextMessageAsync(
                        chatId: e.Message.Chat,
                        replyMarkup: Keyboards.FavoriteMenu,
                        replyToMessageId: e.Message.MessageId,
                        text: "Что-то пошло не так, проверьте правильность контракта"
                    );
            }
            finally
            {
                SetState(States.FAVORITE_MENU, e);
            }
            
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
            var command = $"UPDATE users SET currentState = '{state}' WHERE userID = '{e.Message.From.Id}';";
            MySqlCommand changeState = new MySqlCommand(command, SqlConn);
            changeState.ExecuteNonQuery();
        }
        static string GetState(MessageEventArgs e)
        {
            try
            {
                var sql = $"SELECT currentState FROM users WHERE userID = '{e.Message.Chat.Id}'";
                MySqlCommand command = new MySqlCommand(sql, SqlConn);
                string state = command.ExecuteScalar().ToString();
                return state;
            }
            catch
            {
                return "";
            }
        }
    }
}
                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                             