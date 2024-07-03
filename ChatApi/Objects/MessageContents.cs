namespace ChatApi
{
    public class MessageContents
    {
        /// <summary>
        /// originating channel
        /// </summary>
        public string channel;

        /// <summary>
        /// full, scrubbed message
        /// </summary>
        public string message;

        /// <summary>
        /// who sent the message
        /// </summary>
        public string sendinguser;

        /// <summary>
        /// 
        /// </summary>
        public MessageType messageType;

        /// <summary>
        /// just setting some values
        /// </summary>
        /// <param name="ch"></param>
        /// <param name="msg"></param>
        /// <param name="user"></param>
        public MessageContents(string ch, string msg, string user, MessageType type)
        {
            channel = ch;
            message = msg;
            sendinguser = user;
            messageType = type;
        }
    }
}
