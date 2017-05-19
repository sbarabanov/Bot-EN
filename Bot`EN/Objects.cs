using System;
using System.IO;
using System.Linq;
using System.Net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Bot_EN
{
    enum Type
    {
        Empty = 0,
        Add = 1,
        New = 2,
        Tng = 3,
    }

    class Word
    {
        public string EN { get; set; }
        public string RU { get; set; }
        public Description Ins { get; set; }
        public DateTime Prev { get; set; }
        public DateTime Next { get; set; }

        public Word()
        {

        }

        public Word(string en)
        {
            EN = en;
            RU = GetTranslation(en);
            Ins = GetDescription(en);
            Prev = DateTime.Now;
            Next = DateTime.MinValue;
        }

        private string GetTranslation(string text)
        {
            WebResponse response = WebRequest.Create(string.Format("https://translate.yandex.net/api/v1.5/tr.json/translate?key=trnsl.1.1.20170516T081423Z.897ef08ab302ee33.26081f61eea6100b6d4083ecddf8679b004965dd&lang=en-ru&text={0}", text)).GetResponse();
            StreamReader stream = new StreamReader(response.GetResponseStream());

            string[] array = JsonConvert.DeserializeObject<string[]>(JObject.Parse(stream.ReadToEnd())["text"].ToString());
            stream.Close();

            return array.Last();
        }

        private Description GetDescription(string text)
        {
            WebResponse response = WebRequest.Create(string.Format("https://dictionary.yandex.net/api/v1/dicservice.json/lookup?key=dict.1.1.20170516T091153Z.526c208fb316a52d.71ad1af0b2c524499453f469a44f4612802af1f0&lang=en-ru&text={0}&flags=4", text)).GetResponse();
            StreamReader stream = new StreamReader(response.GetResponseStream());

            Description ins = JsonConvert.DeserializeObject<Description>(stream.ReadToEnd());
            stream.Close();

            return ins;
        }

        public override bool Equals(object obj)
        {
            var other = obj as Word;
            if (other == null)
                return false;

            return EN == other.EN && RU == other.RU;
        }

        public override int GetHashCode()
        {
            return string.Format("{0}#{1}", EN, RU).GetHashCode();
        }
    }

    // yandex object`s

    public class Txt
    {
        public string Text { get; set; }
    }

    public class Example
    {
        public string Text { get; set; }
        public Txt[] Tr { get; set; }
    }

    public class Translation
    {
        public string Text { get; set; }
        public string Pos { get; set; }
        public Txt[] Syn { get; set; }
        public Txt[] Mean { get; set; }
        public Example[] Ex { get; set; }
    }

    public class Definition
    {
        public string Text { get; set; }
        public string Pos { get; set; }
        public string Ts { get; set; }
        public Translation[] Tr { get; set; }
    }

    public class Description
    {
        public Definition[] Def { get; set; }
    }

    // telegram object`s

    public class User
    {
        public int id { get; set; }
        public string first_name { get; set; }
        public string last_name { get; set; }
    }

    public class Chat
    {
        public int id { get; set; }
        public string type { get; set; }
        public string first_name { get; set; }
        public string last_name { get; set; }
    }

    public class Message
    {
        public int message_id { get; set; }
        public User from { get; set; }
        public Chat chat { get; set; }
        public int date { get; set; }
        public string text { get; set; }
    }

    public class CallbackQuery
    {
        public string id { get; set; }
        public User from { get; set; }
        public Message message { get; set; }
        public string inline_message_id { get; set; }
        public string data { get; set; }
    }

    public class Update
    {
        public int update_id { get; set; }
        public Message message { get; set; }
        public CallbackQuery callback_query { get; set; }
    }

    public class InlineKeyboardMarkup
    {
        public InlineKeyboardButton[][] inline_keyboard { get; set; }
    }

    public class InlineKeyboardButton
    {
        public string text { get; set; }
        public string callback_data { get; set; }
    }
}