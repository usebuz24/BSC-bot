using System;
using System.Collections.Generic;
using System.Text;
using Telegram.Bot.Types.ReplyMarkups;

namespace TelegramCryptoChatBot
{
    class Keyboards
    {
        public static ReplyKeyboardMarkup MainMenu = new ReplyKeyboardMarkup(
            new KeyboardButton[][]
            {
                new KeyboardButton[]
                {
                    new KeyboardButton("Цена токена")
                },
                new KeyboardButton[]
                {
                    new KeyboardButton("Избранное")
                }
            }, true);

        public static ReplyKeyboardMarkup FavoriteMenu = new ReplyKeyboardMarkup(
            new KeyboardButton[][]
            {
                new KeyboardButton[]
                {
                    new KeyboardButton("Цены токенов"),
                    new KeyboardButton("Удалить токен"),
                },
                new KeyboardButton[]
                {
                    new KeyboardButton("Добавить токен"),
                    new KeyboardButton("Назад"),
                }
            }, true);

        public static ReplyKeyboardRemove RemoveMenu = new ReplyKeyboardRemove();
    }
}
