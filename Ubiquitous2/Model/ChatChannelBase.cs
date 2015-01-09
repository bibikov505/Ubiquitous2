using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace UB.Model
{
    public class ChatChannelBase : IChatChannel, IDisposable
    {
        private Timer joinTimeout;
        public ChatChannelBase()
        {
            ChannelStats = new ChannelStats();
            joinTimeout = new Timer((sender) =>
            {
                if( !Chat.Status.IsConnected )
                    Leave();
            }, this, 60000, Timeout.Infinite);
        }



        public ChatConfig ChatConfig
        {
            get;
            set;
        }

        public Action<IChatChannel> JoinCallback
        {
            get;
            set;
        }

        public Action<IChatChannel> LeaveCallback
        {
            get;
            set;
        }

        public Action<ChatMessage> ReadMessage
        {
            get;
            set;
        }

        public string ChannelName
        {
            get;
            set;
        }

        public virtual void Join(Action<IChatChannel> callback, string channel)
        {

        }

        public virtual void Leave()
        {
           
        }

        public virtual void SendMessage(ChatMessage message)
        {
         
        }
        
        public IChat Chat
        {
            get;
            set;
        }

        public virtual void SetupStatsWatcher()
        {
        }



        public ChannelStats ChannelStats
        {
            get;
            set;
        }


        public void Dispose()
        {
            Dispose(true);
        }
        protected virtual void Dispose(bool nativeOnly)
        {
            if( joinTimeout != null )
            {
                joinTimeout.Dispose();
                joinTimeout = null;
            }
        }
    }
}
