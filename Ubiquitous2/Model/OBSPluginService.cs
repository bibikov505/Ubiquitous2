using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using UB.Properties;
using UB.Utils;

namespace UB.Model
{
    public class OBSPluginService : IOBSPluginService   
    {
        private ServiceHost serviceHost;
        private object lockSave = new object();
        private static ImageData imageData = new ImageData();
        private static Size currentSize = new Size();
        private static bool imageChanged = false;
        private static bool isConnected = false;
        private App app = (System.Windows.Application.Current as App);
        private static readonly List<IOBSCallback> subscribers = new List<IOBSCallback>();

        [DataMember]
        private static RenderTargetBitmap renderTarget;

        public ImageData GetImage()
        {
            isConnected = true;
            lock (lockSave)
            {
                if ( imageChanged && RenderTarget != null)
                {
                    WriteableBitmap wb = new WriteableBitmap(RenderTarget);

                    if( Config != null && app != null && currentSize != null && Config.HideControls )
                    {
                        var x = (int)(double.IsNaN(app.ChatMessageX) ? 0 : app.ChatMessageX);
                        x = x < 0 ? 0 : x;

                        var y = (int)(double.IsNaN(app.ChatBoxY) ? 0 : app.ChatBoxY);
                        //var height = (int)System.Windows.Application.Current.MainWindow.Height - y;
                        var height = (int)app.ChatBoxHeight > 0 ? (int)app.ChatBoxHeight : (int)System.Windows.Application.Current.MainWindow.Height - y;
                        var width = (int)app.ChatMessageWidth > 0 ? (int)(app.ChatMessageWidth + x - app.ChatBoxX) : (int)app.ChatBoxWidth;
                        
                        currentSize.Width = width;
                        currentSize.Height = height;
                        wb = wb.Crop(
                            (int)app.ChatBoxX, 
                            y, 
                            width,
                            height);

                    }
                    else
                    {
                        currentSize.Width = wb.PixelWidth;
                        currentSize.Height = wb.PixelHeight;
                    }
                    imageData.Size = currentSize;
                    imageData.Pixels = null;
                    imageData.Pixels = wb.ConvertToByteArray();
                    return imageData;
                }
                else
                {
                    return null;
                }

            }
        }

        public void OnImageChange()
        {
            try
            {

                subscribers.ForEach(delegate(IOBSCallback callback)
                {
                    if (((ICommunicationObject)callback).State == CommunicationState.Opened)
                    {
                        callback.OnImageChanged();
                    }
                    else
                    {
                        subscribers.Remove(callback);
                    }
                });
            }
            catch { }
        }

        public bool Subscribe()
        {
            try
            {
                IOBSCallback callback = OperationContext.Current.GetCallbackChannel<IOBSCallback>();
                if (!subscribers.Contains(callback))
                    subscribers.Add(callback);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool Unsubscribe()
        {
            try
            {
                IOBSCallback callback = OperationContext.Current.GetCallbackChannel<IOBSCallback>();
                if (subscribers.Contains(callback))
                    subscribers.Remove(callback);
                return true;
            }
            catch
            {
                return false;
            }
        }
        [DataMember]
        public RenderTargetBitmap RenderTarget {
            get {
                lock (lockSave)
                {
                    imageChanged = false;
                    return renderTarget;             
                }
            }
            set
            {
                lock (lockSave)
                {
                    renderTarget = value;
                    if( !imageChanged )
                    {
                        imageChanged = true;
                        OnImageChange();
                    }
                }
            }
        
        }



        public bool IsConnected
        {
            get { return isConnected; } 

        }
        public void Start()
        {
            serviceHost = new ServiceHost(
              typeof(OBSPluginService),
              new Uri[]{
                new Uri("net.pipe://localhost")
              });

            serviceHost.AddServiceEndpoint(typeof(IOBSPluginService),
              new NetNamedPipeBinding(),
              "ImageSource");

            serviceHost.Open();
        }

        public void Stop()
        {
            //try
            //{
            //    serviceHost.Close();
            //}
            //catch (Exception e)
            //{
                
            //}
        }
        public ImageData GetFirstImage()
        {
            isConnected = true;
            imageChanged = true;
            return GetImage();
        }

        public OBSPluginConfig GetConfig()
        {
            return Config;
        }

        public void SetConfig( OBSPluginConfig config )
        {
            Config = config;
        }

        [DataMember]
        private static OBSPluginConfig _config = new OBSPluginConfig();

        [DataMember]
        public OBSPluginConfig Config
        {
            get { return _config; }
            set { _config = value; }
        }
        

        
    }


}
