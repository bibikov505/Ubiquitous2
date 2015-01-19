using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using UB.Model;
using UB.Properties;

namespace UB.Utils
{
    public class Theme
    {
        public static void SwitchTheme( string themeName )
        {
            Uri themeUri = null;
            Uri sharedUri = null;

            if( IsLocalThemeExist( themeName ) )
            {
                themeUri = new Uri("/Ubiquitous2;component/Skins/" + themeName + "/Skin.xaml", UriKind.RelativeOrAbsolute);
            }
            else if( IsCustomThemeExist( themeName ))
            {
                try
                {
                    themeUri = new Uri(CustomThemesFolder + themeName + "/Theme.xaml", UriKind.Absolute);
                    //sharedUri = new Uri("/Ubiquitous2;component/Skins/Shared/Shared.xaml", UriKind.RelativeOrAbsolute);
                }
                catch { }
            }
            else
            {
                SwitchTheme("Main");
                return;
            }
            

            if( themeUri != null )
            {
                Application.Current.Resources.MergedDictionaries.Clear();
                foreach (var uri in new Uri[] { sharedUri, themeUri })
                {
                    if( uri != null )
                    {
                        Application.Current.Resources.MergedDictionaries.Add(new ResourceDictionary()
                        {
                            Source = themeUri
                        });
                    }
                }
                Ubiquitous.Default.Config.AppConfig.ThemeName = themeName;
            }
        }
        public static string CurrentTheme
        {
            get
            {
                return Ubiquitous.Default.Config.AppConfig.ThemeName;
            }
        }
        public static List<ThemeDescription> ThemesList
        {
            get {
                var result = new List<ThemeDescription>();
                var resources = Resource.GetResourceNames();
                try
                {
                    var localThemes = resources.Where( res => Regex.IsMatch(res,  @"skins\/.*?\/skin\..aml") )
                        .Select( res => 
                            {
                                var title = (Re.GetSubString(res, @"skins\/(.*?)\/skin\..aml") ?? "No title").Capitalize();
                                return new ThemeDescription()
                                {
                                    Title = title,
                                    Thumbnail = @"/Ubiquitous2;component/Skins/" + title + "/Thumbnail.png",
                                };
                            });

                    var userThemes = Directory.GetDirectories(CustomThemesFolder)
                        .Where(folder => File.Exists(CustomThemesFolder + Path.GetFileName(folder) + @"\Theme.xaml"))
                        .Select(folder => 
                            {
                                var title = Path.GetFileName(folder).Capitalize();
                                return new ThemeDescription()
                                {
                                    Title = title,
                                    Thumbnail = CustomThemesFolder + title + @"\Thumbnail.png"
                                };
                        })
                        .Where( theme => !theme.Title.Equals("SampleTheme",StringComparison.InvariantCultureIgnoreCase));

                    if (localThemes != null)
                        result = result.Union(localThemes).ToList();

                    if (userThemes != null)
                        result = result.Union(userThemes).ToList();

                }
                catch( Exception e)
                {
                    Log.WriteError("Theme enumeration error: {0} ", e.Message);
                }

                return result;

            }
        }

        public static string CustomThemesFolder
        {
            get { 
                return AppDomain.CurrentDomain.GetData("DataDirectory") + @"\Themes\";
            }
        }

        public static bool IsCustomThemeExist(string themeName)
        {
            return File.Exists(CustomThemesFolder + themeName + @"\Theme.xaml");
        }

        public static bool IsLocalThemeExist(string themeName)
        {
            var resources = Resource.GetResourceNames();
            if (resources.Any( title => title.Contains("/" + themeName.ToLower() + "/")))
                return true;
            else
                return false;
        }
    }

    public class ThemeDescription
    {
        public string Title { get; set; }
        public string Thumbnail { get; set; }
        public bool IsCurrent { get; set; }
    }
}
