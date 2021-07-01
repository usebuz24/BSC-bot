using System;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

using MySqlConnector;

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

        static async void OnMessageHandler(object sender, Telegram.Bot.Args.MessageEventArgs e)
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
                      replyToMessageId: e.Message.MessageId,
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
                      replyToMessageId: e.Message.MessageId,
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
                      text: "Окей!"
                    );
                    SetState(States.MAIN_MENU, e);
                    return;
                }
                if (e.Message.Text == "Добавить токен")
                {
                    await CryptoBot.SendTextMessageAsync(
                      chatId: e.Message.Chat,
                      text: "Введите контракт токена который хотите добавить:"
                    );
                    SetState(States.FAVORITE_ADDING, e);
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
        }
        static async void GetPrice(string contract, Telegram.Bot.Args.MessageEventArgs e)
        {
            try
            {
                WebRequest request = WebRequest.Create("https://api.pancakeswap.info/api/v2/tokens/" + contract);
                WebResponse response = request.GetResponse();
                using (Stream rawData = response.GetResponseStream())
                {
                    StreamReader reader = new StreamReader(rawData);
                    string responseFromServer = reader.ReadLine();
                    
                    await CryptoBot.SendTextMessageAsync(
                        chatId: e.Message.Chat,
                        replyMarkup: Keyboards.MainMenu,
                        replyToMessageId: e.Message.MessageId,
                        text: responseFromServer
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
        static void SetState(string state, Telegram.Bot.Args.MessageEventArgs e)
        {
            var command = $"UPDATE users SET currentState = '{state}' WHERE userID = '{e.Message.From.Id}';";
            MySqlCommand changeState = new MySqlCommand(command, SqlConn);
            changeState.ExecuteNonQuery();
        }
        static string GetState(Telegram.Bot.Args.MessageEventArgs e)
        {
            var sql = $"SELECT currentState FROM users WHERE userID = '{e.Message.Chat.Id}'";
            MySqlCommand command = new MySqlCommand(sql, SqlConn);
            string state = command.ExecuteScalar().ToString();
            return state;
        }
    }
}
                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                             