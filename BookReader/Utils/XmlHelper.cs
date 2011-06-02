using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.Serialization;
using System.Xml;

namespace PdfBookReader.Utils
{
    public static class XmlHelper
    {
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
            using (FileStream inStream = new FileStream(filename, FileMode.Open))
            {
                using (XmlReader reader = XmlReader.Create(inStream))
                {
                    return (T)s.ReadObject(reader, false);
                }
            }
        }
    }

    
}
