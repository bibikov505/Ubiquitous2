using System;
using System.Threading.Tasks;
using CLROBS;
using System.ServiceModel;
using UB.Model;
using System.Diagnostics;
using System.Drawing;
using System.Threading;

namespace Ubiquitous2Plugin
{
    [CallbackBehavior(ConcurrencyMode = ConcurrencyMode.Reentrant)]
    public class Ubiquitous2Source : AbstractImageSource, IDisposable, IOBSCallback
    {
        private ChannelFactory<IOBSPluginService> pipeFactory;
        private Texture texture;
        private Size currentSize = new Size(100, 100);
        private IOBSPluginService pipeProxy;
        private ImageData imageData;
        private OBSPluginConfig config = new OBSPluginConfig();
        private object imageLock = new object();
        private bool isConnecting = true;
        private bool isRendering = false;
        private XElement configElement;
        private ulong transparency = 0xFF;
        private Timer timerFadeOut;
        public Ubiquitous2Source(XElement configElement)        
        {
            timerFadeOut = new Timer(delegate(object sender) {
                ulong step = 3;
                var obj = sender as Ubiquitous2Source;
                if (obj.transparency >= step)
                    obj.transparency -= step;
                else
                    obj.timerFadeOut.Change(Timeout.Infinite, Timeout.Infinite);

            }, this, Timeout.Infinite, Timeout.Infinite);

            this.configElement = configElement;
            UpdateSettings();
            Task.Run(() => ConnectWCF());
        }
        public void ConnectWCF()
        {
            isConnecting = true;
            
            lock(imageLock)
                imageData = null;

            string lastConnectError = String.Empty;

            while( imageData == null && isRendering )
            {
                Debug.Print("OBS plugin is trying to reconnect...");
                try
                {
                    if( pipeFactory == null )
                    {
                        pipeFactory =
                            new DuplexChannelFactory<IOBSPluginService>(this,
                            new NetNamedPipeBinding() { MaxReceivedMessageSize = 8294400 * 2, MaxBufferSize = 8294400 * 2 },
                            new EndpointAddress(
                            "net.pipe://localhost/ImageSource"));
                    }

                    if (pipeProxy == null)
                    {
                        pipeProxy = pipeFactory.CreateChannel();
                    }  

                    if (pipeProxy != null)
                    {
                        config.HideControls = Properties.Settings.Default.HideControls;
                        
                        uint timeout = 0;
                        if (uint.TryParse(Properties.Settings.Default.FadeOutTimeout, out timeout))
                            config.FadeOutTimeout = timeout;
                        else
                        {
                            Properties.Settings.Default.FadeOutTimeout = "15";
                            config.FadeOutTimeout = 15;
                        }
                        config.IsFadeOutEnabled = Properties.Settings.Default.IsFadeOutEnabled;

                        Log.WriteInfo("Set config");
                        pipeProxy.SetConfig(config);

                        Debug.Print("OBSPlugin: trying to get first image");

                        pipeProxy.Subscribe();

                        lock(imageLock )
                            imageData = pipeProxy.GetFirstImage();

                        if (imageData == null)
                        {
                            Debug.Print("OBSPlugin: got null image data");
                            lock( imageLock )
                                texture = GS.CreateTexture(100, 100, GSColorFormat.GS_BGRA, null, false, false);
                            
                            Size.X = 100;
                            Size.Y = 100;
                        }
                        else
                        {
                            Log.WriteInfo("Got first image {0}x{1}", imageData.Size.Width, imageData.Size.Height);
                            Log.WriteInfo("Update settings");
                            UpdateSettings();
                            Log.WriteInfo("Update texture");
                            UpdateTexture();
                            break;
                        }
                        
                    }     
                    else
                        Debug.Print("OBSPlugin: pipe proxy is null");
                }
                catch(Exception e)
                {
                    if( lastConnectError != e.Message )
                    {
                        Log.WriteError("OBS plugin connect error\n{0}\n{1}", e.Message, e.StackTrace);
                        lastConnectError = e.Message;
                    }
                    pipeProxy = null;         
                }

                Thread.Sleep(1000);
            }
            isConnecting = false;

        }

        public override void Render(float x, float y, float width, float height)
        {
            if (isConnecting)
                return;

            GetImage(x, y, width, height);

            if (imageData != null && texture != null)
            {
                UpdateSettings();
                lock (imageLock)
                    texture.SetImage(imageData.Pixels, GSImageFormat.GS_IMAGEFORMAT_BGRA, (UInt32)(imageData.Size.Width * 4));
            }
            if (texture != null)
                lock (imageLock)
                    GS.DrawSprite(texture, 0x00FFFFFF + (uint)(transparency << 24), x, y, x + width, y + height);
        }

        public override void UpdateSettings()
        {
            if( imageData == null )
                return;

            Size.X = currentSize.Width;
            Size.Y = currentSize.Height;

            Properties.Settings.Default.Width = Size.X;
            Properties.Settings.Default.Height = Size.Y;

            if (config.HideControls != Properties.Settings.Default.HideControls)
            {
                config.HideControls = Properties.Settings.Default.HideControls;
                pipeProxy.SetConfig(config);
                lock (imageLock)
                    imageData = pipeProxy.GetFirstImage();
            }

            if (currentSize.Width != imageData.Size.Width ||
            currentSize.Height != imageData.Size.Height)
            {
                UpdateTexture();
            }
        }
        private void UpdateTexture()
        {
            if (imageData == null)
                return;

            currentSize.Height = imageData.Size.Height;
            currentSize.Width = imageData.Size.Width;
            lock (imageLock)
            {
                texture = GS.CreateTexture((uint)imageData.Size.Width, (uint)imageData.Size.Height, GSColorFormat.GS_BGRA, null, false, false);
                texture.SetImage(imageData.Pixels, GSImageFormat.GS_IMAGEFORMAT_BGRA, (UInt32)(imageData.Size.Width * 4));
            }
        }

        private void GetImage(float x, float y, float width, float height)
        {          
            try
            {
                var image = pipeProxy.GetImage();

                if (image != null)
                    lock (imageLock)
                        imageData = image;
            }
            catch
            {
                isConnecting = true;
                lock (imageLock)
                {
                    texture = GS.CreateTexture((uint)Size.X, (uint)Size.Y, GSColorFormat.GS_BGRA, null, false, false);
                    GS.DrawSprite(texture, 0xFFFFFFFF, x, y, x + width, y + height);
                }

                Task.Run(() => ConnectWCF());
            }
        }

        public override void BeginScene()
        {
            isRendering = true;
            Log.WriteInfo("OBS scene begin");
            base.BeginScene();

            config.HideControls = Properties.Settings.Default.HideControls;

            pipeProxy.SetConfig(config);

            var image = pipeProxy.GetFirstImage();
            if( image != null )
                lock(imageLock )
                    imageData = image;

            if (imageData == null)
                Log.WriteError("OBS initial image is null");
            else
                UpdateTexture();
        }

        public override void EndScene()
        {
            isRendering = false;
            Log.WriteInfo("OBS scene end");
            base.EndScene();
            try
            {
                pipeFactory.Close();
            }
            catch { }
        }

        public void Dispose()
        {
            try
            {
                pipeProxy.Unsubscribe();
                timerFadeOut.Change(Timeout.Infinite, Timeout.Infinite);
                timerFadeOut.Dispose();
            }
            catch { }
        }

        public void OnImageChanged()
        {
            Log.WriteInfo("OBS image changed");
            transparency = 0x0FF;
            if( Properties.Settings.Default.IsFadeOutEnabled)
            {
                uint delay;
                if (!uint.TryParse(Properties.Settings.Default.FadeOutTimeout, out delay))
                    delay = 15;

                timerFadeOut.Change(delay * 1000, 300 / 255);
            }
        }


        public void OnConfigChanged()
        {
            Log.WriteInfo("OBS config changed");
        }

        public override void Preprocess()
        {
            base.Preprocess();
        }
    }
}
