namespace ChatApi
{
    public static class Hybi
    {
        /// <summary>
        /// channel message sent/received
        /// </summary>
        public const string _MSG = "MSG";

        /// <summary>
        /// connected or requesting chat connection
        /// </summary>
        public const string _IDN = "IDN";

        /// <summary>
        /// private message sent/received
        /// </summary>
        public const string _PRI = "PRI";

        /// <summary>
        /// public channel list received 
        /// </summary>
        public const string _CHA = "CHA";

        /// <summary>
        /// private channel list received
        /// </summary>
        public const string _ORS = "ORS";

        /// <summary>
        /// joined or requesting to join a channel
        /// </summary>
        public const string _JCH = "JCH";

        /// <summary>
        /// left or requesting to leave a channel
        /// </summary>
        public const string _LCH = "LCH";

        /// <summary>
        /// a ping
        /// </summary>
        public const string _PIN = "PIN";

        /// <summary>
        /// requests status set
        /// </summary>
        public const string _STA = "STA";

        /// <summary>
        /// advertisement
        /// </summary>
        public const string _LRP = "LRP";

        /// <summary>
        /// create(d) a private, invite-only channel
        /// </summary>
        public const string _CCR = "CCR";

        /// <summary>
        /// chat system variables
        /// </summary>
        public const string _VAR = "VAR";

        /// <summary>
        /// server hello command
        /// </summary>
        public const string _HLO = "HLO";

        /// <summary>
        /// returns number of connected users
        /// </summary>
        public const string _CON = "CON";

        /// <summary>
        /// friends list
        /// </summary>
        public const string _FRL = "FRL";

        /// <summary>
        /// ignore list
        /// </summary>
        public const string _IGN = "IGN";

        /// <summary>
        /// chat ops list
        /// </summary>
        public const string _ADL = "ADL";

        /// <summary>
        /// character list and online status
        /// </summary>
        public const string _LIS = "LIS";

        /// <summary>
        /// a user connected
        /// </summary>
        public const string _NLN = "NLN";

        /// <summary>
        /// a user disconnected
        /// </summary>
        public const string _FLN = "FLN";

        /// <summary>
        /// list of channel ops
        /// </summary>
        public const string _COL = "COL";

        /// <summary>
        /// initial channel data
        /// </summary>
        public const string _ICH = "ICH";

        /// <summary>
        /// channel's description has updated
        /// </summary>
        public const string _CDS = "CDS";

        /// <summary>
        /// basic error response
        /// </summary>
        public const string _ERR = "ERR";

        /// <summary>
        /// invite user to channel
        /// </summary>
        public const string _CIU = "CIU";

        /// <summary>
        /// kicks a user
        /// </summary>
        public const string _CKU = "CKU";

        /// <summary>
        /// bans a user
        /// </summary>
        public const string _CBU = "CBU";

        /// <summary>
        /// promotes to channel op
        /// </summary>
        public const string _COA = "COA";

        /// <summary>
        /// demotes from channel op to user
        /// </summary>
        public const string _COR = "COR";

        /// <summary>
        /// time a user out
        /// </summary>
        public const string _CTU = "CTU";

        /// <summary>
        /// unbans a user
        /// </summary>
        public const string _CUB = "CUB";
    }
}
