using System;
using System.Collections.Generic;
using System.Linq;

namespace HisenseTest
{
    public class HisenseKeyMacro
    {
        public string Name { get; set; }
        public List<string> Commands { get; set; } = new List<string>();

        public static List<HisenseKeyMacro> AllMacros => new List<HisenseKeyMacro>
        {
            new HisenseKeyMacro("Inputs", "KEY_RETURNS,KEY_HOME,KEY_DOWN,KEY_DOWN,KEY_DOWN,KEY_UP"),
            new HisenseKeyMacro("Netflix", "KEY_RETURNS,KEY_HOME,KEY_UP,KEY_UP,KEY_UP,KEY_OK"),
            new HisenseKeyMacro("YouTube", "KEY_RETURNS,KEY_HOME,KEY_UP,KEY_UP,KEY_UP,KEY_DOWN,KEY_OK"),
        };

        public HisenseKeyMacro(string name, string commands)
        {
            Name = name;
            Commands = commands.Split(new char[] { ' ', ',', ';' }, StringSplitOptions.RemoveEmptyEntries).ToList();
        }
    }
}
