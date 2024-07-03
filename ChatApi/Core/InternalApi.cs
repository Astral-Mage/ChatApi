using System;
using System.Text;
using WatsonWebsocket;
using System.Linq;
using Newtonsoft.Json.Linq;
using ChatApi.Systems;
using System.Collections.Generic;
using ChatApi.Objects;

namespace ChatApi
{
    public partial class ApiConnection
    {
        public ApiConnection()
        {
            ClientId = "Fii_Bot";
            ClientVersion = "2.1.0.0";
            ConnectionTimeout = new TimeSpan(0, 0, 10);

            TicketInformation = null;
            Client = null;
            UserName = string.Empty;
            CharacterName = string.Empty;

            channelTracker = new ChannelTracker();
            userTracker = new UserTracker();
        }

        void Client_MessageReceived(object sender, MessageReceivedEventArgs e)
        {
            string message = Encoding.UTF8.GetString(e.Data.ToArray());
            //Console.WriteLine($"Message from server: {message}");
            ParseMessage(message.Split(' ').First(), message.Split(" ".ToCharArray(), 2).Last());
        }

        void Client_ChatDisConnected(object sender, EventArgs e)
        {
            Console.WriteLine("Disconnected from F-Chat servers!");
        }

        void Client_ChatConnected(object sender, EventArgs e)
        {
            Console.WriteLine("Connected to F-Chat servers! Sending identification...");
            IdentifySelf(UserName, TicketInformation.Ticket, CharacterName, ClientId, ClientVersion);
            StartReplyThread();
        }

        void IdentifySelf(string accountName, string ticket, string botName, string botClientID, string botClientVersion)
        {
            try
            {
                string toSend = $"{Hybi._IDN}  {{ \"method\": \"ticket\", \"account\": \"{accountName}\", \"ticket\": \"{ticket}\", \"character\": \"{botName}\", \"cname\": \"{botClientID}\", \"cversion\": \"{botClientVersion}\" }}";
                Client.SendAsync(toSend);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        JObject ParseToJObject(string message, string hybi)
        {
            JObject toReturn;

            if (message.Split(' ').Length <= 1)
            {
                try
                {
                    if (string.Equals(hybi, message))
                    {
                        return null;
                    }

                    toReturn = JObject.Parse(message);
                    return toReturn;
                }
                catch
                {
                    //throw new Exception($"Failure to parse message: {message}");
                }
            }

            try
            {
                toReturn = JObject.Parse(message.Replace(hybi, "").TrimStart());
            }
            catch
            {
                throw new Exception($"Failure to parse message: {message}");
            }

            return toReturn;
        }

        ///////////////////////////////////////////////////
        ///////////////////////////////////////////////////
        ///////////////////////////////////////////////////

        void ParseMessage(string hybi, string message)
        {
            JObject json;
            try
            {
                json = ParseToJObject(message, hybi);
            }
            catch(Exception)
            {
                json = null;
            }

            switch (hybi)
            {
                case Hybi._STA:
                    {
                        if (json["character"].ToString().Equals(CharacterName, StringComparison.InvariantCultureIgnoreCase))
                        {
                            Console.WriteLine("Status Changed");
                        }
                    }
                    break;
                case Hybi._IDN:
                    {
                        ConnectedToChat?.Invoke(this, null);

                        RequestInternalChannelList(ChannelType.Private);
                        RequestInternalChannelList(ChannelType.Public);

                        Console.WriteLine("Connected to Chat");
                    }
                    break;
                case Hybi._ORS:
                    {
                        List<Channel> privateChannelList = new List<Channel>();
                        
                        foreach (var v in json["channels"])
                        {
                            privateChannelList.Add(new Channel(v["title"].ToString(), v["name"].ToString(), ChannelType.Private));
                        }

                        channelTracker.RefreshAvailableChannels(privateChannelList, ChannelType.Private);
                        PrivateChannelsReceivedHandler?.Invoke(this, new ChannelEventArgs() { });
                        Console.WriteLine($"Private Channels Recieved... {privateChannelList.Count} total Private Channels.");
                    }
                    break;
                case Hybi._CHA:
                    {
                        List<Channel> publicChannelList = new List<Channel>();

                        foreach (var v in json["channels"])
                        {
                            publicChannelList.Add(new Channel(string.Empty, v["name"].ToString(), ChannelType.Private));
                        }

                        channelTracker.RefreshAvailableChannels(publicChannelList, ChannelType.Public);
                        PublicChannelsReceivedHandler?.Invoke(this, new ChannelEventArgs() { });
                        Console.WriteLine($"Public Channels Recieved... {publicChannelList.Count} total Public Channels.");
                    }
                    break;
                case Hybi._MSG:
                    {
                        MessageHandler?.Invoke(Hybi._MSG, new MessageEventArgs() { channel = json["channel"].ToString(), message = json["message"].ToString(), user = json["character"].ToString() });
                        Console.WriteLine(message);
                    }
                    break;
                case Hybi._PRI:
                    {
                        MessageHandler?.Invoke(Hybi._PRI, new MessageEventArgs() { user = json["character"].ToString(), message = json["message"].ToString() });
                        Console.WriteLine(message);
                    }
                    break;
                case Hybi._JCH:
                    {
                        User user = userTracker.GetUserByName(json["character"]["identity"].ToString());
                        Channel myCh;
                        try
                        {
                            myCh = channelTracker.GetChannelByNameOrCode(json["title"].ToString());
                        }
                        catch
                        {
                            myCh = channelTracker.AddManualChannel(json["title"].ToString(), ChannelStatus.Available, json["channel"].ToString());
                        }

                        if (user.name.Equals(CharacterName))
                        {
                            channelTracker.WatchChannels.Add(myCh);
                        }

                        if (myCh == null)
                        {
                            return;
                        }

                        // creating channel
                        bool creating = myCh.Status == ChannelStatus.Creating && user.name.Equals(CharacterName);
                        if (creating)
                        {
                            myCh = channelTracker.FinalizeChannelCreation(json["title"].ToString(), json["channel"].ToString(), user);
                            Console.WriteLine($"Created Channel: {json["channel"].ToString()}");
                            CreatedChannelHandler?.Invoke(this, new ChannelEventArgs() { name = myCh.Name, status = ChannelStatus.Joined, code = myCh.Code, type = myCh.Type });
                        }

                        // join channel
                        Channel channel = channelTracker.GetChannelByNameOrCode(json["channel"].ToString());
                        if (user != null && channel != null)
                        {
                            if (!creating)
                            {
                                JoinedChannelHandler?.Invoke(this, new ChannelEventArgs() { name = json["title"].ToString(), status = ChannelStatus.Joined, code = myCh.Code, type = myCh.Type, userJoining = user.name });
                            }

                            channel.AddUser(user);
                            Console.WriteLine($"{user.name} joined Channel: {channel.Name}. {channel.Users.Count} total users in channel.");
                        }
                    }
                    break;
                case Hybi._LCH:
                    {
                        if (json["character"].ToString().Equals(CharacterName, StringComparison.InvariantCultureIgnoreCase))
                        {
                            LeftChannelHandler?.Invoke(this, new ChannelEventArgs() { name = json["channel"].ToString(), status = ChannelStatus.Left });
                            channelTracker.ChangeChannelState(json["channel"].ToString(), ChannelStatus.Available);
                        }

                        User user = userTracker.GetUserByName(json["character"].ToString());
                        Channel channel = channelTracker.GetChannelByNameOrCode(json["channel"].ToString());

                        if (user.name.Equals(CharacterName))
                        {
                            channelTracker.WatchChannels.Remove(channel);
                        }

                        if (user != null && channel != null)
                        {
                            channel.RemoveUser(user);
                            Console.WriteLine($"{user.name} left Channel: {json["channel"].ToString()}. {channel.Users.Count} total users in channel.");
                        }

                    }
                    break;
                case Hybi._PIN:
                    {
                        Client.SendAsync(Hybi._PIN);
                    }
                    break;
                case Hybi._VAR:
                    {

                    }
                    break;
                case Hybi._HLO:
                    {

                    }
                    break;
                case Hybi._CON:
                    {
                        Console.WriteLine($"{json["count"]} connected users sent.");
                    }
                    break;
                case Hybi._FRL:
                    {
                        // friends list
                    }
                    break;
                case Hybi._IGN:
                    {
                        // ignore list
                    }
                    break;
                case Hybi._ADL:
                    {

                    }
                    break;
                case Hybi._LIS:
                    {
                        foreach(var v in json["characters"])
                        {
                            User tu = new User()
                            {
                                name = v[0].ToString(),
                                gender = v[1].ToString(),
                                chatstatus = (ChatStatus)Enum.Parse(typeof(ChatStatus), v[2].ToString().ToLowerInvariant(), true)
                            };
                            userTracker.SetChatStatus(tu, tu.chatstatus, false);
                        }

                        Console.WriteLine($"Added {json["characters"].Count()} users. Total users: {userTracker.GetNumberActiveUsers()}");
                    }
                    break;
                case Hybi._NLN:
                    {
                        User tu = new User()
                        {
                            name = json["identity"].ToString(),
                            userstatus = (UserStatus)Enum.Parse(typeof(UserStatus), json["status"].ToString().ToLowerInvariant(), true),
                            gender = json["gender"].ToString()
                        };
                        userTracker.SetChatStatus(tu, ChatStatus.Online, false);
                    }
                    break;
                case Hybi._COL:
                    {
                        Channel myCh = channelTracker.GetChannelByNameOrCode(json["channel"].ToString());

                        int counter = 0;
                        foreach (string v in json["oplist"])
                        {
                            if (string.IsNullOrWhiteSpace(v.ToString()))
                            {
                                continue;
                            }

                            User tu = userTracker.GetUserByName(v);

                            if (counter == 0)
                            {
                                myCh.Owner = tu;
                            }

                            myCh.AddMod(tu);
                        }

                        Console.WriteLine($"Found {myCh.Mods.Count} mods for channel: {myCh.Name}");
                    }
                    break;
                case Hybi._FLN:
                    {
                        User tu = new User()
                        {
                            name = json["character"].ToString()
                        };

                        userTracker.SetChatStatus(tu, ChatStatus.Offline, false);
                        foreach (var v in channelTracker.WatchChannels)
                        {
                            bool needsRemoved = false;

                            if (v.Users.Any(x => x.name.Equals(tu.name)))
                            {
                                needsRemoved = true;
                            }

                            if (needsRemoved)
                            {
                                v.RemoveUser(tu);
                            }
                        }
                    }
                    break;
                case Hybi._ICH:
                    {
                        // joining channel
                        Channel myCh = channelTracker.ChangeChannelState(json["channel"].ToString(), ChannelStatus.Joined);
                        foreach (var v in json["users"])
                        {
                            User tu = userTracker.GetUserByName(v["identity"].ToString());
                            if (null == tu)
                            {
                                Console.WriteLine($"Error attempting to add user {v["identity"].ToString()} to {json["channel"].ToString()} channel's userlist.");
                            }

                            myCh.AddUser(tu);
                            myCh.AdEnabled = !json["mode"].ToString().Equals("chat");
                        }

                        Console.WriteLine($"Adding {json["users"].Count()} users to {myCh.Name} channel's userlist successful.");
                    }
                    break;
                case Hybi._CDS:
                    {
                        Channel myCh = channelTracker.GetChannelByNameOrCode(json["channel"].ToString());
                        myCh.Description = json["description"].ToString();
                    }
                    break;
                default:
                    {

                    }
                    break;
            }
        }
    }
}