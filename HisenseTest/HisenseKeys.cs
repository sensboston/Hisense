using System;
using System.Collections.Generic;

namespace HisenseTest
{
    /// <summary>
    /// All available Hisense TV IP remote keys
    /// </summary>
    public enum HisenseKeys
    {
        KEY_HOME,
        KEY_MENU,
        KEY_CHANNELUP,
        KEY_CHANNELDOWN,
        KEY_VOLUMEUP,
        KEY_VOLUMEDOWN,
        KEY_RETURNS,
        KEY_DOWN,
        KEY_LEFT,
        KEY_RIGHT,
        KEY_UP,
        KEY_OK,
        KEY_0,
        KEY_1,
        KEY_2,
        KEY_3,
        KEY_4,
        KEY_5,
        KEY_6,
        KEY_7,
        KEY_8,
        KEY_9,
        KEY_MUTE,
        KEY_RED,
        KEY_GREEN,
        KEY_YELLOW,
        KEY_BLUE,
        KEY_STOP,
        KEY_PLAY,
        KEY_PAUSE,
        KEY_BACKS,
        KEY_FORWARDS,
        KEY_PREVIOUS,
        KEY_NEXT,
        KEY_DOT,
        KEY_CHANNELLINE,
        KEY_RECORD,
    }

    public class HisenseKey
    {
        public string Name { get; set; }
        public string Command { get; set; }

        private static List<HisenseKey> allKeys = new List<HisenseKey>();
        public static List<HisenseKey> AllKeys
        {
            get
            {
                if (allKeys.Count == 0)
                {
                    var keys = Enum.GetNames(typeof(HisenseKeys));
                    foreach (var k in keys)
                        allKeys.Add(new HisenseKey(k.ToKeyName(), k));
                }
                return allKeys;
            }
        }

        public HisenseKey(HisenseKeys key)
        {
            Command = Enum.GetName(typeof(HisenseKeys), key);
            Name = Command.ToKeyName();
        }

        public HisenseKey(string name, string command)
        {
            Command = command;
            Name = name;
        }
    }

    internal static class StringExtensions
    {
        internal static string ToKeyName(this string name)
        {
            return name[4] + name.Substring(5).ToLower().Replace("elup", "el Up").Replace("eldown", "el Down")
                .Replace("meup", "me Up").Replace("medown", "me Down").Replace("elline", "el Line");
        }
    }

}
