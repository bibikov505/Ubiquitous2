using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using GalaSoft.MvvmLight.Ioc;
using UB.Properties;
using UB.Utils;
namespace UB.Model
{
    public class StreamPageDataService : IStreamPageDataService
    {
        private List<StreamInfoPreset> presets;
        private IChatDataService chatDataService;
        public StreamPageDataService()
        {
            Initialize();
        }
        private void Initialize()
        {
            chatDataService = SimpleIoc.Default.GetInstance<IChatDataService>();

            presets = Ubiquitous.Default.Config.StreamInfoPresets;
            if (presets == null)
                presets = new List<StreamInfoPreset>();
        }
        public StreamInfoPreset AddPreset( string presetName )
        {           
            var newPreset = new StreamInfoPreset() { PresetName = presetName };
            newPreset.StreamTopics = new List<StreamInfo>();

            GetStreamTopics((streams) => {
                streams.ForEach(stream => newPreset.StreamTopics.Add(stream.Info.GetCopy()));
            });

            presets.Add(newPreset);

            return presets.Last();
        }
        public void GetPresets(Action<List<StreamInfoPreset>> callback)
        {
            callback(presets);
        }
        public void LoadTopicsFromWeb()
        {
            GetStreamTopics((streams) => streams.ForEach(stream => stream.GetTopic()));
        }       
        public void GetStreamTopics(Action<List<IStreamTopic>> callback)
        {
            var streamTopics = chatDataService.Chats.Where(chat => chat is IStreamTopic).Select( chat => 
            {
                var topic = chat as IStreamTopic;
                topic.Info.CanBeChanged = chat.Status.IsLoggedIn;
                topic.Info.CanBeRead = chat.Status.IsConnected;
                return topic;
            }).ToList();

            if (streamTopics == null)
                callback ( new List<IStreamTopic>());
            else
                callback(streamTopics);
        }
        public void UpdateTopicsOnWeb()
        {
            object lockUpdate = new object();
            List<IStreamTopic> streamTopicList = null;
            GetStreamTopics((streams) => streamTopicList = streams.Where(stream => (stream as IChat).Enabled && (stream as IChat).Status.IsLoggedIn ).ToList());
            
            if( streamTopicList == null || streamTopicList.Count <= 0 )
                return;


            Task[] setTasks = new Task[streamTopicList.Count];
            for (int i = 0; i < streamTopicList.Count; i++ )
            {
                setTasks[i] = Task.Factory.StartNew<bool>((obj) =>
                {
                    try
                    {
                        var chat = obj as IStreamTopic;
                        if( chat == null )
                            return false;

                        var result = chat.SetTopic();
                        if( !result )
                            result = chat.SetTopic();

                        Log.WriteInfo("Set topic for {0} {1} game {2} result {3}", (chat as IChat).ChatName, chat.Info.Topic, chat.Info.CurrentGame.Name, result);
                        return result;
                    }
                    catch( Exception e )
                    {
                        Log.WriteError("Set topic error: {0}\n{1}", e.Message, e.StackTrace);
                    }
                    return false;

                }, streamTopicList[i]).ContinueWith(task =>
                {
                    UI.Dispatch(() => {
                        var chat = task.AsyncState as IChat;
                        chat.Status.IsChangingTopicSucceed = task.Result;                    
                    });
                });
                Thread.Sleep(16);
            }
            Task.WaitAll(setTasks, 10000);        
        }
        public void RemovePreset(StreamInfoPreset preset)
        {
            presets.Remove(preset);
        }
    }
}
