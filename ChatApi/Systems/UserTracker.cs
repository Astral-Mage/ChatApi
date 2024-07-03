using ChatApi.Objects;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ChatApi.Systems
{
    public class UserTracker
    {
        List<User> KnownUsers;

         public UserTracker()
        {
            KnownUsers = new List<User>();
        }

        public bool AddUser(User user)
        {
            if (!KnownUsers.Any(x => x.name.Equals(user.name)))
            {
                KnownUsers.Add(user);
                return true;
            }

            return false;
        }

        public int GetNumberActiveUsers()
        {
            return KnownUsers.Count;
        }

        public List<User> GetUsersByStatus(UserStatus status)
        {
            return KnownUsers.Where(x => x.userstatus == status).ToList();
        }

        public User GetUserByName(string name)
        {
            if (KnownUsers.Any(x => x.name.Equals(name)))
            {
                return KnownUsers.First(x => x.name.Equals(name));
            }

            User nu = new User();
            nu.name = name;
            nu.chatstatus = ChatStatus.Offline;
            AddUser(nu);

            return nu;
        }

        public void SetChatStatus(User user, ChatStatus status, bool logging = true)
        {
            user.chatstatus = status;
            if (GetUserByName(user.name) == null)
                AddUser(user);

            User tu = KnownUsers.First(x => x.name.Equals(user.name));
            if (null == tu)
                throw new System.Exception($"Error attempting to resolve user: {user.name}.");

            tu.chatstatus = status;
            if (logging) Console.WriteLine($"{tu.name}'s chat status changed to: {status}");
        }

        public void SetUserStatus(User user, UserStatus status, bool logging = true)
        {
            user.userstatus = status;
            if (GetUserByName(user.name) == null)
                AddUser(user);

            User tu = KnownUsers.First(x => x.name.Equals(user.name));
            if (null == tu)
                throw new System.Exception($"Error attempting to resolve user: {user.name}.");

            tu.userstatus = status;
            if (logging) Console.WriteLine($"{tu.name}'s user status changed to: {status}");
        }
    }
}
