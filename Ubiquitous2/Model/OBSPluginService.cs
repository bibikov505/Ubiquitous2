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
                        var windowHeight = (int)System.Windows.Application.Current.MainWindow.Height;
                        var y = (int)app.ChatBoxY;
                        currentSize.Width = (int)app.ChatBoxWidth;
                        currentSize.Height = windowHeight - y;
                        wb = wb.Crop(
                            (int)app.ChatBoxX, 
                            (int)app.ChatBoxY, 
                            currentSize.Width,
                            currentSize.Height);
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
                    imageChanged = true;
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
            lock (lockSave)
            {
                if (RenderTarget != null)
                {
                    WriteableBitmap wb = new WriteableBitmap(RenderTarget);
                    currentSize.Width = wb.PixelWidth;
                    currentSize.Height = wb.PixelHeight;
                    imageData.Size = currentSize;
                    imageData.Pixels = null;
                    imageData.Pixels = wb.ConvertToByteArray();
                }
                return imageData;
            }
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
