using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.Serialization.Json;

namespace PDFViewer.Reader.Utils
{
    public static class JsonHelper
    {
        /// <summary>
        /// Serialize an object to a JSON file
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <param name="filename"></param>
        public static void Serialize<T>(T obj, String filename)
        {
            using (FileStream fs = new FileStream(filename, FileMode.OpenOrCreate))
            {
                DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(T));
                
                serializer.WriteObject(fs, obj);
            }
        }

        /// <summary>
        /// Deserialize an object from JSON file
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="filename"></param>
        /// <returns></returns>
        public static T Deserialize<T>(String filename)
        {
            T obj;
            using (FileStream fs = new FileStream(filename, FileMode.Open))
            {
                DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(T));
                obj = (T)serializer.ReadObject(fs);
            }
            return obj; 
        }
    }
}
