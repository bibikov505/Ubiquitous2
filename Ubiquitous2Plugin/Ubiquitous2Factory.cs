using CLROBS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Windows.Forms;
using UB.Model;

namespace Ubiquitous2Plugin
{
    internal class Ubiquitous2Factory : AbstractImageSourceFactory
    {
        public Ubiquitous2Factory()
        {
            ClassName = "Ubiquitous2Source";
            DisplayName = "Ubiquitous2 chat";
        }
        public override bool ShowConfiguration(XElement data)
        {
            float currentWidth = data.Parent.GetFloat("cx");
            float currentHeight = data.Parent.GetFloat("cy");
            if( currentWidth == 0 || currentWidth == float.NaN)
            {
                currentWidth = Properties.Settings.Default.Width;
                currentHeight = Properties.Settings.Default.Height;
            }
            
            if( currentWidth == 0 || currentWidth == double.NaN)
            {
                currentWidth = 100;
                currentHeight = 100;
            }

            data.Parent.SetFloat("cx", currentWidth);
            data.Parent.SetFloat("cy", currentHeight);

            var dialog = new SettingsForm();
            
            if( dialog.ShowDialog() == DialogResult.OK )
            {
                Log.WriteInfo("OBS plugin configuration is saving");
                try
                {
                    Log.WriteInfo("OBS plugin cx after save:", data.Parent.GetFloat("cx"));
                    Properties.Settings.Default.Save();
                }
                catch( Exception e)
                {
                    Log.WriteError("OBS plugin configuration save error: {0} {1}", e.Message, e.StackTrace);
                    return true;
                }
                Log.WriteInfo("OBS plugin configuration is saved");
                return true;
            }
            else
            {
                Log.WriteInfo("OBS plugin configuration is canceled");
                return false;
            }
        }

        public override ImageSource Create(XElement data)
        {
            return new Ubiquitous2Source(data);
        }
    }
}
