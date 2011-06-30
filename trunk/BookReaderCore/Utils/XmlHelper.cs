using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.Serialization;
using System.Xml;
using System.Diagnostics;
using NLog;

namespace BookReader.Utils
{
    public static class XmlHelper
    {
        static readonly Logger log = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Serialize an object to XML file
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <param name="filename"></param>
        public static void Serialize<T>(T obj, String filename)
        {
            DataContractSerializer s = new DataContractSerializer(typeof(T));
            XmlWriterSettings sett = new XmlWriterSettings();
            sett.Indent = true;
            sett.CheckCharacters = false;

            using (FileStream outStream = new FileStream(filename, FileMode.Create))
            {
                using (XmlWriter writer = XmlWriter.Create(outStream, sett))
                {
                    s.WriteObject(writer, obj);
                }
            }
        }

        /// <summary>
        /// Deserialize an object from XML file
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="filename"></param>
        /// <returns></returns>
        public static T Deserialize<T>(String filename)
        {
            DataContractSerializer s = new DataContractSerializer(typeof(T));

            XmlReaderSettings sett = new XmlReaderSettings();
            sett.CheckCharacters = false;            
            
            using (FileStream inStream = new FileStream(filename, FileMode.Open))
            {
                using (XmlReader reader = XmlReader.Create(inStream, sett))
                {
                    return (T)s.ReadObject(reader, false);
                }
            }
        }

        /// <summary>
        /// Deserialize an object from XML file or return a default object.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="filename"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static T DeserializeOrDefault<T>(String filename, T defaultValue = default(T), bool ignoreErrors = true)
        {
            if (!File.Exists(filename)) { return defaultValue; }

            try
            {
                return Deserialize<T>(filename);
            }
            catch (Exception e)
            {
                log.ErrorException("DeseializeOrDefault failed reading: " + filename, e);
                if (!ignoreErrors) { throw; }
                return defaultValue;
            }
        }

    }

    
}
