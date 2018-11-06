namespace HisenseTest
{
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

    public class RemoteCommand
    {
        public string Name { get; set; }
        public string Command { get; set; }

        public RemoteCommand(string name, string command)
        {
            Name = name;
            Command = command;
        }
    }
}
