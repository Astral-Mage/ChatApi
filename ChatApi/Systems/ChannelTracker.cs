using ChatApi.Objects;
using System.Collections.Generic;
using System.Linq;

namespace ChatApi.Systems
{
    internal class ChannelTracker
    {
        /// <summary>
        /// 
        /// </summary>
        List<Channel> AvailablePublicChannels;

        /// <summary>
        /// 
        /// </summary>
        List<Channel> AvailablePrivateChannels;

        /// <summary>
        /// 
        /// </summary>
        public List<Channel> WatchChannels;

        /// <summary>
        /// 
        /// </summary>
        public ChannelTracker()
        {
            AvailablePublicChannels = new List<Channel>();
            AvailablePrivateChannels = new List<Channel>();
            WatchChannels = new List<Channel>();
        }

        public Channel AddManualChannel(string channelname, ChannelStatus status, string channelcode)
        {
            if (!GetCombinedChannelList(ChannelStatus.All).Any(x => x.Name.Equals(channelname) || x.Code.Equals(channelname)))
            {
                Channel ch = new Channel(channelname, channelcode, ChannelType.Private)
                {
                    Status = status
                };
                AvailablePrivateChannels.Add(ch);
                return ch;
            }

            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="channelname"></param>
        /// <param name="status"></param>
        public void StartChannelCreation(string channelname, ChannelStatus status = ChannelStatus.Creating)
        {
            if (!AvailablePrivateChannels.Any(x => x.Status == ChannelStatus.Creating))
            {
                Channel toAdd = new Channel(channelname, string.Empty, ChannelType.Private)
                {
                    Status = status
                };
                AvailablePrivateChannels.Add(toAdd);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="channelname"></param>
        /// <param name="channelcode"></param>
        /// <param name="owner"></param>
        /// <returns></returns>
        public Channel FinalizeChannelCreation(string channelname, string channelcode, User owner)
        {
            if (AvailablePrivateChannels.Any(x => x.Status == ChannelStatus.Creating && x.Name.Equals(channelname)))
            {
                Channel myCh = AvailablePrivateChannels.First(x => x.Status == ChannelStatus.Creating && x.Name.Equals(channelname));
                myCh.Code = channelcode;
                myCh.Status = ChannelStatus.Created;
                myCh.CreatedByApi = true;
                return myCh;
            }

            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="channelname"></param>
        /// <returns></returns>
        public Channel GetChannelByNameOrCode(string channelname)
        {
            var fullList = GetCombinedChannelList(ChannelStatus.All);
            if (fullList.Any(x => x.Code.Equals(channelname, System.StringComparison.InvariantCultureIgnoreCase)))
            {
                return fullList.First(x => x.Code.Equals(channelname, System.StringComparison.InvariantCultureIgnoreCase));
            }

            if (fullList.Any(x => x.Name.Equals(channelname, System.StringComparison.InvariantCultureIgnoreCase)))
            {
                return fullList.First(x => x.Name.Equals(channelname, System.StringComparison.InvariantCultureIgnoreCase));
            }

            throw new System.Exception($"No channel found: {channelname}");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="newState"></param>
        /// <returns></returns>
        public Channel ChangeChannelState(string channelnameorcode, ChannelStatus newState)
        {
            Channel channel = GetChannelByNameOrCode(channelnameorcode);

            var combinedChannelList = GetCombinedChannelList(ChannelStatus.All);
            if (combinedChannelList.Any(x => x.Name.Equals(channel.Name) && (channel.Type == ChannelType.Public || x.Code == channel.Code)))
            {
                var tChannel = combinedChannelList.First(x => x.Name.Equals(channel.Name) && (channel.Type == ChannelType.Public || x.Code == channel.Code));
                tChannel.Status = newState;
                return tChannel;
            }

            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="status"></param>
        /// <returns></returns>
        public List<Channel> GetCombinedChannelList(ChannelStatus status = ChannelStatus.AllValid)
        {
            var combinedRawList = AvailablePrivateChannels.Concat(AvailablePublicChannels).ToList();

            if (status == ChannelStatus.AllValid)
            {
                return combinedRawList.Where(x =>   x.Status == ChannelStatus.Available || 
                                                    x.Status == ChannelStatus.Invited   || 
                                                    x.Status == ChannelStatus.Joined    || 
                                                    x.Status == ChannelStatus.Kicked    || 
                                                    x.Status == ChannelStatus.Left      || 
                                                    x.Status == ChannelStatus.Pending   ||
                                                    x.Status == ChannelStatus.Created)
                                                    .ToList();
            }
            else if (status == ChannelStatus.All)
            { 
                return combinedRawList;
            }
            else
            {
                return combinedRawList.Where(x => x.Status == status).ToList();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public List<Channel> GetChannelList(ChannelType type)
        {
            if (type == ChannelType.All)
                return AvailablePrivateChannels.Concat(AvailablePublicChannels).ToList();

            return type == ChannelType.Private ? AvailablePrivateChannels : AvailablePublicChannels;
        }

        public List<Channel> GetChannelList(ChannelStatus status = ChannelStatus.AllValid)
        {
            if (status == ChannelStatus.All)
                return AvailablePrivateChannels.Concat(AvailablePublicChannels).ToList();
            else if (status == ChannelStatus.AllValid)
            {
                return AvailablePrivateChannels.Concat(AvailablePublicChannels).ToList().Where(x => 
                x.Status.Equals(ChannelStatus.Available) ||
                x.Status.Equals(ChannelStatus.Created) ||
                x.Status.Equals(ChannelStatus.Joined) ||
                x.Status.Equals(ChannelStatus.Left) ||
                x.Status.Equals(ChannelStatus.Kicked) ||
                x.Status.Equals(ChannelStatus.Invited)
                ).ToList();
            }
            else
                return AvailablePrivateChannels.Concat(AvailablePublicChannels).ToList().Where(x => x.Status.Equals(status)).ToList();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="newList"></param>
        /// <param name="type"></param>
        public void RefreshAvailableChannels(List<Channel> newList, ChannelType type)
        {
            if (type == ChannelType.Private)
            {
                AvailablePrivateChannels = newList;
            }
            else if (type == ChannelType.Public)
            {
                AvailablePublicChannels = newList;
            }
        }
    }
}
