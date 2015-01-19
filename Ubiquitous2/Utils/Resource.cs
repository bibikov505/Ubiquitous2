using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace UB.Utils
{
    public static class Resource
    {
        public static string[] GetResourceNames()
        {
            var assembly = Assembly.GetExecutingAssembly();
            string resName = assembly.GetName().Name + ".g.resources";
            using (var stream = assembly.GetManifestResourceStream(resName))
            {
                using (var reader = new System.Resources.ResourceReader(stream))
                {
                    return reader.Cast<DictionaryEntry>().Select(entry =>
                             (string)entry.Key).ToArray();
                }
            }
        }
    }
}
