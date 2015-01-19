using System;
using System.Configuration;
using System.Diagnostics;
using System.Net;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using GalaSoft.MvvmLight.Threading;
using UB.Model;
using UB.Properties;
using UB.Utils;
using System.Deployment.Application;
using System.Text.RegularExpressions;
using System.IO;

namespace UB
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public double ChatBoxWidth { get; set; }
        public double ChatBoxHeight { get; set; }
        public AppConfig AppConfig { get; set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            NativeMethods.SetProcessDPIAware();

            Utils.Net.DemandTCPPermission();

            if (RenderCapability.Tier == 0)
                Timeline.DesiredFrameRateProperty.OverrideMetadata(
                    typeof(Timeline),
                    new FrameworkPropertyMetadata { DefaultValue = 20 });

            Regex.CacheSize = 0;

            var rootDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\Ubiquitous2";
            CreateDataFolders(rootDataFolder);
            CopyDataFolders(rootDataFolder);
            AppDomain.CurrentDomain.SetData("DataDirectory", rootDataFolder );

            WebRequest.DefaultWebProxy = null;

            var scriptManager = new ScriptManager();
            scriptManager.LoadScripts();

            //RenderOptions.ProcessRenderMode = RenderMode.SoftwareOnly; 
        }
        private void CopyDataFolders(string destinationDir)
        {

            var copyFolders = new Tuple<string,string>[] {
                new Tuple<string,string>(@".\Skins\UserTheme", @"\Themes\UserTheme\"),
            };

            try
            {
                foreach( var copyPair in copyFolders )
                {
                    if( !Directory.Exists( destinationDir + copyPair.Item2 ))
                        Directory.CreateDirectory( destinationDir + copyPair.Item2 );

                    var sourceFiles = Directory.GetFiles(copyPair.Item1);
                    foreach( var file in sourceFiles )
                    {
                        File.Copy(file, destinationDir + copyPair.Item2 + Path.GetFileName(file),true);
                    }
                }
            }
            catch (Exception e)
            {
                Log.WriteError("Data file copy error: {0}", e.Message);

            }


        }
        private void CreateDataFolders(string rootDataFolder)
        {
            var appDataFolders = new string[] {
                @"\Scripts", 
                @"\Scripts\Example", 
                @"\Themes",
                @"\Web",
                @"\Web\Themes",
            };

            foreach( var folder in appDataFolders )
            {
                if( !Directory.Exists( rootDataFolder + folder ) )
                {
                    try
                    {
                        Directory.CreateDirectory(rootDataFolder + folder);
                    }
                    catch (Exception e) {
                        Log.WriteError(@"Can't create data directory {0}: {1}", folder, e.Message);
                    }
                }

            }
        }

    }
}
