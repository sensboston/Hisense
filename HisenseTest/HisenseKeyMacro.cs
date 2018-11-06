using System;
using System.Collections.Generic;
using System.Linq;

namespace HisenseTest
{
    public class HisenseKeyMacro
    {
        const int DEFAULT_DELAY = 600;

        public class MacroCommand
        {
            public string Key { get; set; }
            public int Delay { get; set; }
            public MacroCommand(string key, int delay = DEFAULT_DELAY)
            {
                Key = key;
                Delay = delay;
            }
        }

        public string Name { get; set; }
        public List<MacroCommand> Commands { get; set; } = new List<MacroCommand>();

        public static List<HisenseKeyMacro> AllMacros => new List<HisenseKeyMacro>
        {
            new HisenseKeyMacro("Inputs",     "KEY_RETURNS:1000,KEY_HOME:1000,KEY_DOWN,KEY_DOWN,KEY_DOWN,KEY_UP"),
            new HisenseKeyMacro("Prev input", "KEY_RETURNS:1000,KEY_HOME:1000,KEY_DOWN,KEY_DOWN,KEY_DOWN,KEY_UP,KEY_RIGHT,KEY_OK"),
            new HisenseKeyMacro("Netflix",    "KEY_RETURNS:1000,KEY_HOME:1000,KEY_UP,KEY_UP,KEY_UP,KEY_OK"),
            new HisenseKeyMacro("YouTube",    "KEY_RETURNS:1000,KEY_HOME:1000,KEY_UP,KEY_UP,KEY_UP,KEY_DOWN,KEY_OK"),
        };

        public HisenseKeyMacro(string name, string commands)
        {
            Name = name;
            var cmds = commands.Split(new char[] { ' ', ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var cmd in cmds)
            {
                string newCmd = cmd;
                int delay = DEFAULT_DELAY;
                if (cmd.Contains(":"))
                {
                    var s = cmd.Split(':');
                    newCmd = s[0];
                    int.TryParse(s[1], out delay);
                }
                Commands.Add(new MacroCommand(newCmd, delay));
            }
        }
    }
}
