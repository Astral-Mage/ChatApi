using System;
using System.Collections.Generic;
using System.Linq;

namespace ChatApi.Objects
{
    public class Channel
    {
        /// <summary>
        /// what is our status with the channel
        /// </summary>
        public ChannelStatus Status;

        /// <summary>
        /// the channel
        /// </summary>
        public string Name;

        /// <summary>
        /// 
        /// </summary>
        public User Owner;

        /// <summary>
        /// sending user
        /// </summary>
        public string Code;

        /// <summary>
        /// 
        /// </summary>
        public ChannelType Type;

        /// <summary>
        /// 
        /// </summary>
        public bool AdEnabled;

        /// <summary>
        /// 
        /// </summary>
        internal List<User> Mods;

        /// <summary>
        /// 
        /// </summary>
        internal List<User> Users;

        /// <summary>
        /// 
        /// </summary>
        public bool CreatedByApi;

        /// <summary>
        /// 
        /// </summary>
        public string Description;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="code"></param>
        /// <param name="usercount"></param>
        public Channel(string name, string code, ChannelType type, bool adenabled = false)
        {
            Name = name;
            Code = code;
            Status = ChannelStatus.Available;
            Type = type;
            AdEnabled = adenabled;
            Mods = new List<User>();
            Users = new List<User>();
            CreatedByApi = false;
            Description = string.Empty;
        }

        public void AddUser(User username)
        {
            if (!Users.Any(x => x.name.Equals(username.name)))
                Users.Add(username);
            else
                Console.WriteLine($"Skipping duplicate entry: {username.name}");
        }

        public void RemoveUser(User username)
        {
            if (Users.Any(x => x.name.Equals(username.name)))
                Users.Remove(username);
        }

        public void AddMod(User username)
        {
            if (!Mods.Any(x => x.name.Equals(username.name)))
                Mods.Add(username);
            else
                Console.WriteLine($"Skipping duplicate mod entry: {username.name}");
        }

        public void RemoveMod(User username)
        {
            if (Mods.Any(x => x.name.Equals(username.name)))
                Mods.Remove(username);
        }
        ////////////////////////////////////////////////

        /// <summary>
        /// 
        /// </summary>
        private Channel()
        {

        }
    }
}