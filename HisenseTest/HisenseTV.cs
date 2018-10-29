using System;
using System.Collections.Generic;

namespace HisenseTest
{
    public enum PlatformOS
    {
        Android = 0,
        Linux
    }

    public class HisenseTV
    {
        private string VersionString { get; set; }
        public string IP { get; private set; }
        public string DeviceName { get; private set; }
        public string VersionID { get; private set; }
        public Dictionary<string, int> SourceDesp { get; private set; }
        public int InputMethod { get; private set; }
        public int TVConvergence { get; private set; }
        public bool UDPChannelSurport { get; private set; }
        public PlatformOS Platform { get; private set; }

        /// <summary>
        /// This constructor parse string like:
        /// 
        /// VERSION102#192.168.1.33#SmartTV08#
        /// ip = 192.168.1.33
        /// devicename = Smart TV
        /// versionid = VERSION300
        /// sourcedesp = ATV:0; DTV: 1; VIDEO1: 2; VIDEO2: 3; YPbPr: 4; HDMI1: 5; HDMI2: 6; HDMI3: 7; VGA: 8; MC: 10
        /// inputmethod = 1
        /// tvconvergence = 0
        /// udpchannelsurport = 1
        /// platform = 1  #0-android 1-linux
        /// 
        /// and creates HisenseTV object
        /// </summary>
        /// <param name="discoveryString"></param>
        public HisenseTV(string discoveryString)
        {
            var props = discoveryString.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            if (props.Length > 0)
            {
                if (props[0].Contains("VERSION")) VersionString = props[0];

                for (int i=1; i<props.Length; i++)
                {
                    if (props[i].StartsWith("ip")) IP = props[i].Substring(props[i].IndexOf("=") + 1).Trim();

                    if (props[i].StartsWith("devicename")) DeviceName = props[i].Substring(props[i].IndexOf("=") + 1).Trim();

                    if (props[i].StartsWith("versionid")) VersionID = props[i].Substring(props[i].IndexOf("=") + 1).Trim();

                    if (props[i].StartsWith("sourcedesp"))
                    {
                        var s = props[i].Substring(props[i].IndexOf("=") + 1).Trim();
                        var subProps = s.Split(';');
                        foreach (var sp in subProps)
                        {
                            var val = sp.Split('=');
                            if (val.Length == 2 && int.TryParse(val[1], out int n))
                                SourceDesp[val[0]] = n;
                        }
                    }
                    if (props[i].StartsWith("inputmethod"))
                        if (int.TryParse(props[i].Substring(props[i].IndexOf("=") + 1).Trim(), out int inputMethod))
                            InputMethod = inputMethod;

                    if (props[i].StartsWith("tvconvergence"))
                        if (int.TryParse(props[i].Substring(props[i].IndexOf("=") + 1).Trim(), out int tvConvergence))
                            TVConvergence = tvConvergence;

                    if (props[i].StartsWith("udpchannelsurport"))
                        if (int.TryParse(props[i].Substring(props[i].IndexOf("=") + 1).Trim(), out int udpChannelSurport))
                            UDPChannelSurport = (udpChannelSurport == 1);

                    if (props[i].StartsWith("platform"))
                        if (int.TryParse(props[i].Substring(props[i].IndexOf("=") + 1, 3).Trim(), out int platform))
                            Platform = (platform == 1) ? PlatformOS.Linux : PlatformOS.Android;
                }
            }
        }
    }
}
