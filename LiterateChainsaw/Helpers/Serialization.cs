using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiterateChainsaw.Helpers
{
    public static class Serialization
    {
        public static bool Deserialize(string strXMLFile, Type type, out object obj, out string strErrMsg)
        {
            strErrMsg = "";
            obj = null;
            System.IO.TextReader reader = null;//= new System.IO.StreamReader(strXMLFile);

            try
            {
                if (!System.IO.File.Exists(strXMLFile))
                {
                    strErrMsg = string.Format("XML file \"{0}\" not found!", strXMLFile);
                    return false;
                }
                reader = new System.IO.StreamReader(strXMLFile);
                System.Xml.Serialization.XmlSerializer serializer = new System.Xml.Serialization.XmlSerializer(type);
                obj = serializer.Deserialize(reader);
                reader.Close();
                return true;
            }
            catch (Exception ex)
            {
                strErrMsg = string.Format("Deserialization Error. File={0}; \nException: {1}", strXMLFile, ex.ToString());
                if (reader != null)
                    reader.Close();
                return false;
            }
        }
        public static bool Serialize(string sTmpXMLFileName, Type type, object obj, out string strErrMsg)
        {
            strErrMsg = "";
            System.Xml.Serialization.XmlSerializer serializer = null;
            System.IO.FileStream stream = null;
            try
            {
                serializer = new System.Xml.Serialization.XmlSerializer(type);
                stream = new System.IO.FileStream(sTmpXMLFileName, System.IO.FileMode.Create, System.IO.FileAccess.Write);
                serializer.Serialize(stream, obj);
                return true;
            }
            catch (Exception ex)
            {
                strErrMsg = string.Format("Serialization Error. File={0}; \nException: {1}", sTmpXMLFileName, ex.ToString());
                return false;
            }
            finally
            {
                if (stream != null)
                    stream.Close();
            }
        }
    }
}
