using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Bot_EN
{
    class Program
    {
        static string token = "330426260:AAGOVMlLotr2_bn_PwfWWacyZtgObEULFvA";

        static void Main()
        {
            List<Word> words = JsonConvert.DeserializeObject<Word[]>(File.ReadAllText("word`s")).ToList();

            new Thread(t => {
                while (true)
                {
                    DateTime now = DateTime.Now;

                    File.WriteAllText("word`s", JsonConvert.SerializeObject(words.OrderBy(w => w.EN)));
                    Console.WriteLine("write {0} word`s in file", words.Count);

                    while ((DateTime.Now - now).TotalMinutes < 1)
                        Thread.Sleep(5000);
                }
            }).Start();

            InlineKeyboardMarkup keyboardEmpty = new InlineKeyboardMarkup {
                inline_keyboard = new InlineKeyboardButton[][] {
                    new InlineKeyboardButton[] {
                        new InlineKeyboardButton { text = "add word`s", callback_data = "btn`add" },
                        new InlineKeyboardButton { text = "new word`s", callback_data = "btn`new" },
                        new InlineKeyboardButton { text = "tng word`s", callback_data = "btn`tng" },
                    },
                }
            };

            InlineKeyboardMarkup keyboardNew = new InlineKeyboardMarkup {
                inline_keyboard = new InlineKeyboardButton[][] {
                    new InlineKeyboardButton[] {
                        new InlineKeyboardButton { text = "save", callback_data = "btn`save" },
                        new InlineKeyboardButton { text = "next", callback_data = "btn`next" },
                    },
                }
            };

            Update update = GetUpdate();

            while (update == null)
            {
                Console.WriteLine("not active updates");
                Thread.Sleep(3000);
                update = GetUpdate();
            }

            int idChat = update.message != null ? update.message.chat.id : update.callback_query.message.chat.id;
            int idLastUpdate = update.update_id;

            SendMessage(idChat, " ` ", keyboardEmpty);

            Type type = Type.Empty;
            Word lesson = null;

            while (true)
            {
                DateTime now = DateTime.Now;
                update = GetUpdate();

                if (update.update_id > idLastUpdate)
                {
                    // текст
                    if (update.message != null)
                    {
                        switch (update.message.text)
                        {
                            case "#":
                                {
                                    type = Type.Empty;
                                    SendMessage(idChat, " ` ", keyboardEmpty);
                                    
                                    break;
                                }
                            case "#del":
                                {
                                    if (type == Type.New)
                                    {
                                        words.RemoveAll(w => w.EN == lesson.EN);

                                        lesson = words.OrderBy(w => w.Prev).First();
                                        SendMessage(idChat, GetWTr(lesson), keyboardNew);
                                    }
                                    else
                                    {
                                        SendMessage(idChat, " ` the command is not correct ");
                                    }

                                    break;
                                }
                            case "#exit":
                                {
                                    File.WriteAllText("word`s", JsonConvert.SerializeObject(words.OrderBy(w => w.EN)));
                                    SendMessage(idChat, string.Format(" ` write {0} word`s in file ", words.Count));

                                    break;
                                }
                            default:
                                {
                                    if (type == Type.Add)
                                    {
                                        string str = update.message.text.Trim();
                                        Word equal = words.Find(w => w.EN == str);

                                        if (equal != null)
                                        {
                                            SendMessage(idChat, " ` this word already exists ");
                                            SendMessage(idChat, GetWTr(equal));
                                        }
                                        else
                                        {
                                            Word word = new Word(str);
                                            SendMessage(idChat, GetWTr(word));
                                            words.Add(word);
                                        }
                                    }

                                    break;
                                }
                        }
                    }

                    // кнопка
                    if (update.callback_query != null)
                    {
                        switch (update.callback_query.data)
                        {
                            case "btn`add":
                                {
                                    type = Type.Add;
                                    SendMessage(idChat, " ` add world`s ");
                                    
                                    break;
                                }
                            case "btn`new":
                                {
                                    if (words.Count(w => w.Next == DateTime.MinValue) > 0)
                                    {
                                        type = Type.New;
                                        SendMessage(idChat, " ` new world`s ");

                                        lesson = words.OrderBy(w => w.Prev).First();
                                        SendMessage(idChat, GetWTr(lesson), keyboardNew);
                                    }
                                    else
                                    {
                                        SendMessage(idChat, " ` new world`s not found ", keyboardEmpty);
                                    }

                                    break;
                                }
                            case "btn`tng":
                                {
                                    if (words.Count(w => w.Prev != DateTime.MinValue) > 0)
                                    {
                                        type = Type.Tng;
                                        SendMessage(idChat, " ` repetition world`s ");

                                        lesson = words.Where(w => w.Prev != DateTime.MinValue).OrderBy(w => w.Prev).First();
                                        SendMessage(idChat, GetWEn(lesson), GetKeyboard(lesson, words));
                                    }
                                    else
                                    {
                                        SendMessage(idChat, " ` no word`s found for repetition ", keyboardEmpty);
                                    }

                                    break;
                                }
                            case "btn`save":
                            case "btn`next":
                                {
                                    words.Find(w => w.EN == lesson.EN).Prev = now;

                                    if (update.callback_query.data == "save")
                                        words.Find(w => w.EN == lesson.EN).Next = now;

                                    lesson = words.OrderBy(w => w.Prev).First();
                                    SendMessage(idChat, GetWTr(lesson), keyboardNew);

                                    break;
                                }
                            default:
                                {
                                    if (type == Type.Tng)
                                    {
                                        if (lesson.EN == update.callback_query.data)
                                        {
                                            SendMessage(idChat, " ` yes ");

                                            words.Find(w => w.EN == lesson.EN).Prev = now;
                                            words.Find(w => w.EN == lesson.EN).Next = now;
                                        }
                                        else
                                        {
                                            SendMessage(idChat, " ` no ");
                                        }

                                        lesson = words.Where(w => w.Prev != DateTime.MinValue).OrderBy(w => w.Prev).First();
                                        SendMessage(idChat, GetWEn(lesson), GetKeyboard(lesson, words));
                                    }

                                    break;
                                }
                        }
                    }

                    idLastUpdate = update.update_id;
                }

                while ((DateTime.Now - now).TotalSeconds < 1)
                    Thread.Sleep(250);
            }
        }

        static void SendMessage(int idChat, string message)
        {
            WebRequest request = WebRequest.Create(string.Format("https://api.telegram.org/bot{0}/sendMessage?chat_id={1}&text={2}", token, idChat, message));

            request.GetResponse();
            request.Abort();
        }

        static void SendMessage(int idChat, string message, InlineKeyboardMarkup keyboard)
        {
            WebRequest request = WebRequest.Create(string.Format("https://api.telegram.org/bot{0}/sendMessage?chat_id={1}&text={2}&reply_markup={3}", token, idChat, message, JsonConvert.SerializeObject(keyboard)));

            request.GetResponse();
            request.Abort();
        }

        static Update GetUpdate()
        {
            WebResponse response = WebRequest.Create(string.Format("https://api.telegram.org/bot{0}/getUpdates", token)).GetResponse();
            StreamReader stream = new StreamReader(response.GetResponseStream());

            Update[] updates = JsonConvert.DeserializeObject<Update[]>(Convert.ToString(JObject.Parse(stream.ReadToEnd())["result"]));
            stream.Close();

            return updates.Count() > 0 ? updates.Last() : null;
        }

        static string GetWTr(Word word)
        {
            if (word.Ins.Def.Count() > 0)
                return " ` " + word.EN + " [" + word.Ins.Def.First().Ts + "] - " + word.RU;

            return " ` " + word.EN + " - " + word.RU;
        }

        static string GetWEn(Word word)
        {
            if (word.Ins.Def.Count() > 0)
                return " ` " + word.EN + " [" + word.Ins.Def.First().Ts + "]";

            return " ` " + word.EN;
        }

        static InlineKeyboardMarkup GetKeyboard(Word word, List<Word> words)
        {
            Random rnd = new Random();
            Word[] others = words.Where(w => w.EN != word.EN).OrderBy(x => rnd.Next()).Take(3).ToArray();
            Word[] union = others.Union(new Word[] { word }).OrderBy(x => rnd.Next()).ToArray();

            InlineKeyboardMarkup keyboard = new InlineKeyboardMarkup {
                inline_keyboard = new InlineKeyboardButton[][] {
                    (new int[] { 0, 1 }).Select(i => new InlineKeyboardButton { text = union[i].RU, callback_data = union[i].EN }).ToArray(),
                    (new int[] { 2, 3 }).Select(i => new InlineKeyboardButton { text = union[i].RU, callback_data = union[i].EN }).ToArray(),
                }
            };

            return keyboard;
        }
    }
}