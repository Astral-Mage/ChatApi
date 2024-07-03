using System;
using System.Collections.Generic;
using System.Timers;

namespace ChatApi
{
    public partial class ApiConnection
    {
        private object SocketLocker = new object();
        static Queue<MessageContents> MessageQueue = new Queue<MessageContents>();

        private void SendMessage(string channel, string message)
        {
            try
            {
                string toSend = $"{Hybi._MSG} {{ \"channel\": \"{channel}\", \"message\": \"{message}\" }}";
                Console.WriteLine($"{DateTime.Now.ToShortTimeString()} | {channel}: {message}");
                Client.SendAsync(toSend);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        private void SendAd(string channel, string message)
        {
            try
            {
                string toSend = $"{Hybi._LRP} {{ \"channel\": \"{channel}\", \"message\": \"{message}\" }}";
                Console.WriteLine($"{DateTime.Now.ToShortTimeString()} | {channel}: {message}");
                Client.SendAsync(toSend);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public void SetStatus(string statusMessage, ChatStatus status, string sendingUser)
        {
            try
            {
                string toSend = $"{Hybi._STA} {{ \"status\": \"{status.ToString()}\", \"statusmsg\": \"{statusMessage}\", \"character\": \"{sendingUser}\" }}";
                Console.WriteLine($"{DateTime.Now.ToShortTimeString()} | {status.ToString()}: {statusMessage}");
                Client.SendAsync(toSend);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        private void SendWhisper(string targetUser, string message)
        {
            try
            {
                string toSend = $"{Hybi._PRI} {{ \"recipient\": \"{targetUser}\", \"message\": \"{message}\" }}";
                Console.WriteLine($"{DateTime.Now.ToShortTimeString()} | {targetUser}: {message}");
                Client.SendAsync(toSend);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        private void StartReplyThread()
        {
            Timer aTimer = new Timer();
            aTimer.Elapsed += ReplyTicker;
            aTimer.Interval = 1010;
            aTimer.Enabled = true;
            Console.WriteLine("Starting Reply-ticket");
        }

        private void ReplyTicker(object source, ElapsedEventArgs e)
        {
            DateTime currentTime = DateTime.Now;
            MessageContents message;
            try
            {
                lock (SocketLocker)
                {
                    if (MessageQueue.Count == 0) return;
                    message = MessageQueue.Dequeue();
                }
            }
            catch
            {
                Console.WriteLine("Error dequeueing message when queue not empty.");
                return;
            }


            if (message.messageType == MessageType.Whisper)
            {
                SendWhisper(message.sendinguser, message.message);
            }
            else if (message.messageType == MessageType.Advertisement)
            {
                SendAd(message.channel, message.message);
            }
            else if (message.messageType == MessageType.Basic)
            {
                SendMessage(message.channel, message.message);
            }
            else
            {
                Console.WriteLine("Bad reply: " + message.channel + " / " + message.sendinguser + " / " + message);
            }
        }
    }
}
