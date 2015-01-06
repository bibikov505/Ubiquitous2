using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UB.Model;

namespace UB.Utils
{
    public static class Json
    {
        public static JArray Sort( JArray jsonArray, string property )
        {
            if (jsonArray == null || String.IsNullOrWhiteSpace(property))
                return null;

            var result = new JArray(jsonArray.OrderBy(x => x[property]));
            return result;
        }

        public static JArray ParseArray( object obj )
        {
            if (obj is JArray)
                return obj as JArray;

            if (obj is string)
            {
                if (String.IsNullOrWhiteSpace(obj as string))
                    return null;
                else
                    return JArray.Parse(obj as string);
            }
            else
                return null;
        }
        public static void SerializeToStream<T>( T obj, Action<Stream> callback ) where T: class
        {
            JsonSerializer serializer = new JsonSerializer();

            using( MemoryStream stream = new MemoryStream())
            using( StreamWriter streamWriter = new StreamWriter(stream))
            using( JsonWriter writer = new JsonTextWriter(streamWriter))
            {

                serializer.Serialize(writer, obj);
                writer.Flush();
                stream.Position = 0;
                callback(stream);
            }
        }
        public static T DeserializeUrl<T>(string url) where T:class
        {
            using (WebClientBase wc = new WebClientBase())
            using (Stream stream = wc.DownloadToStream(url))
            {
                if (stream == null)
                    return null;

                using (StreamReader streamReader = new StreamReader(stream))
                using (JsonReader reader = new JsonTextReader(streamReader))
                {
                    JsonSerializer serializer = new JsonSerializer();
                    try
                    {
                        return serializer.Deserialize<T>(reader);
                    }
                    catch
                    {
                        Log.WriteError("Deserializing of {0} from url {1} failed", typeof(T).ToString(), url);
                        return null;
                    }
                }

            }

        }
        public static T DeserializeStream<T>(Stream stream) where T : class
        {
            if (stream == null || !stream.CanRead)
                return null;

            using (StreamReader streamReader = new StreamReader(stream))
            using (JsonReader reader = new JsonTextReader(streamReader))
            {
                JsonSerializer serializer = new JsonSerializer();
                try
                {
                    return serializer.Deserialize<T>(reader);
                }
                catch
                {
                    Log.WriteError("Deserializing of {0} failed", typeof(T).ToString());
                    return null;
                }
            }
        }
    }
}
