﻿using ChatApi.Systems;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web.Script.Serialization;
using WatsonWebsocket;

namespace ChatApi
{
    public partial class ApiConnection
    {
        WatsonWsClient Client;
        TicketInformation TicketInformation;
        private string UserName;
        private string CharacterName;
        private string ClientId;
        private string ClientVersion;
        private TimeSpan ConnectionTimeout;
        ChannelTracker channelTracker;
        UserTracker userTracker;

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public List<string> GetCharacterList()
        {
            if (TicketInformation != null)
            {
                return TicketInformation.Characters.ToList();
            }

            throw new Exception("You must acquire a ticket before attempting to retrieve a character list.");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public TicketInformation GetTicketInformation(string username, string password)
        {
            try
            {
                // fchat login info and url
                string fchatURI = "https://www.f-list.net/json/getApiTicket.php";
                string completeString = $"{fchatURI}?account={username}&password={password}&no_friends=true&no_bookmarks=true";

                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(completeString);
                using (Stream responseStream = request.GetResponse().GetResponseStream())
                {
                    using (StreamReader sr = new StreamReader(responseStream, Encoding.UTF8))
                    {
                        string rawReply = sr.ReadToEnd();
                        TicketInformation = new JavaScriptSerializer().Deserialize<TicketInformation>(rawReply);
                    }
                    UserName = username;
                    return TicketInformation;
                }
            }
            catch(Exception e)
            {
                throw new Exception($"Failure obtaining ticket: {e}");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <param name="charactername"></param>
        /// <returns></returns>
        public bool ConnectToChat(string username, string password, string charactername)
        {
            TicketInformation = GetTicketInformation(username, password);

            if (TicketInformation == null)
            {
                throw new Exception("Error obtaining ticket information");
            }

            try
            {
                if (Client != null)
                {
                    Client.Dispose();
                    Client = null;
                }
            }
            catch (Exception e)
            {
                throw new Exception($"Error disposing of previous client: {e}");
            }
            try
            {
                UserName = username;
                CharacterName = charactername;
                Client = new WatsonWsClient(new Uri("wss://chat.f-list.net/chat2"));
                Client.ServerConnected += Client_ChatConnected;
                Client.ServerDisconnected += Client_ChatDisConnected;
                Client.MessageReceived += Client_MessageReceived;
                Client.Start();
            }
            catch (Exception e)
            {
                throw new Exception($"Error connecting to chat: {e}");
            }
            DateTime whenToTimeout = DateTime.Now + ConnectionTimeout;
            while (!Client.Connected && (DateTime.Now) < whenToTimeout);
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public bool IsConnected()
        {
            if (Client != null)
            {
                return Client.Connected;
            }

            return false;
        }
    }
}