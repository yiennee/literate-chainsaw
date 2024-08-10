using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO.IsolatedStorage;


namespace ZephyrInnovations
{
    public enum SerializedFormat
    {
        /// <summary>
        /// Binary serialization format.
        /// </summary>
        Binary,

        /// <summary>
        /// Document serialization format.
        /// </summary>
        Document
    }

    public class ObjectXMLSerializer
    {
        /// <summary>
        /// Private serializer to prevent instanciation for 'static' class.
        /// </summary>
        private ObjectXMLSerializer()
        {

        }

        public static XmlNode ConvertToXmlNode(Object serializableObject)
        {
            XmlNode xmlNode;
            xmlNode = SaveToXmlNode(serializableObject, null);
            return xmlNode;
        }

        #region Load_methods

        /// <summary>
        /// Loads an object from an XML file in Document format.
        /// </summary>
        /// <example>
        /// <code>
        /// // Always create a new object prior to passing to ObjectXMLSerializer.Load method.
        /// SerializableObject serializableObject = new SerializableObject();
        /// 
        /// serializableObject = (SerializableObject)ObjectXMLSerializer.Load(serializableObject, @"C:\XMLObjects.xml");
        /// </code>
        /// </example>
        /// <param name="serializableObject">Serializable object to be loaded from file.</param>
        /// <param name="path">Path of the file to load the object from.</param>
        /// <returns>Object loaded from an XML file in Document format.</returns>
        public static Object Load(Object serializableObject, string path)
        {
            serializableObject = LoadFromDocumentFormat(serializableObject, null, path, null);
            return serializableObject;
        }

        /// <summary>
        /// Loads an object from an XML file using a specified serialized format.
        /// </summary>
        /// <example>
        /// <code>
        /// // Always create a new object prior to passing to ObjectXMLSerializer.Load method.
        /// SerializableObject serializableObject = new SerializableObject();
        /// 
        /// serializableObject = (SerializableObject)ObjectXMLSerializer.Load(serializableObject, @"C:\XMLObjects.xml", SerializedFormat.Binary);
        /// </code>
        /// </example>		
        /// <param name="serializableObject">Serializable object to be loaded from file.</param>
        /// <param name="path">Path of the file to load the object from.</param>
        /// <param name="serializedFormat">XML serialized format used to load the object.</param>
        /// <returns>Object loaded from an XML file using the specified serialized format.</returns>
        public static Object Load(Object serializableObject, string path, SerializedFormat serializedFormat)
        {
            switch (serializedFormat)
            {
                case SerializedFormat.Binary:
                    serializableObject = LoadFromBinaryFormat(serializableObject, path, null);
                    break;

                case SerializedFormat.Document:
                default:
                    serializableObject = LoadFromDocumentFormat(serializableObject, null, path, null);
                    break;
            }

            return serializableObject;
        }

        /// <summary>
        /// Loads an object from an XML file in Document format, supplying extra data types to enable deserialization of custom types within the object.
        /// </summary>
        /// <example>
        /// <code>
        /// // Always create a new object prior to passing to ObjectXMLSerializer.Load method.
        /// SerializableObject serializableObject = new SerializableObject();
        /// 
        /// serializableObject = (SerializableObject)ObjectXMLSerializer.Load(serializableObject, @"C:\XMLObjects.xml", new Type[] { typeof(MyCustomType) });
        /// </code>
        /// </example>
        /// <param name="serializableObject">Serializable object to be loaded from file.</param>
        /// <param name="path">Path of the file to load the object from.</param>
        /// <param name="extraTypes">Extra data types to enable deserialization of custom types within the object.</param>
        /// <returns>Object loaded from an XML file in Document format.</returns>
        public static Object Load(Object serializableObject, string path, System.Type[] extraTypes)
        {
            serializableObject = LoadFromDocumentFormat(serializableObject, extraTypes, path, null);
            return serializableObject;
        }

        /// <summary>
        /// Loads an object from an XML file in Document format, located in a specified isolated storage area.
        /// </summary>
        /// <example>
        /// <code>
        /// // Always create a new object prior to passing to ObjectXMLSerializer.Load method.
        /// SerializableObject serializableObject = new SerializableObject();
        /// 
        /// serializableObject = (SerializableObject)ObjectXMLSerializer.Load(serializableObject, "XMLObjects.xml", IsolatedStorageFile.GetUserStoreForAssembly());
        /// </code>
        /// </example>
        /// <param name="serializableObject">Serializable object to be loaded from file.</param>
        /// <param name="fileName">Name of the file in the isolated storage area to load the object from.</param>
        /// <param name="isolatedStorageDirectory">Isolated storage area directory containing the XML file to load the object from.</param>
        /// <returns>Object loaded from an XML file in Document format located in a specified isolated storage area.</returns>
        public static Object Load(Object serializableObject, string fileName, IsolatedStorageFile isolatedStorageDirectory)
        {
            serializableObject = LoadFromDocumentFormat(serializableObject, null, fileName, isolatedStorageDirectory);
            return serializableObject;
        }

        /// <summary>
        /// Loads an object from an XML file located in a specified isolated storage area, using a specified serialized format.
        /// </summary>
        /// <example>
        /// <code>
        /// // Always create a new object prior to passing to ObjectXMLSerializer.Load method.
        /// SerializableObject serializableObject = new SerializableObject();
        /// 
        /// serializableObject = (SerializableObject)ObjectXMLSerializer.Load(serializableObject, "XMLObjects.xml", IsolatedStorageFile.GetUserStoreForAssembly(), SerializedFormat.Binary);
        /// </code>
        /// </example>		
        /// <param name="serializableObject">Serializable object to be loaded from file.</param>
        /// <param name="fileName">Name of the file in the isolated storage area to load the object from.</param>
        /// <param name="isolatedStorageDirectory">Isolated storage area directory containing the XML file to load the object from.</param>
        /// <param name="serializedFormat">XML serialized format used to load the object.</param>        
        /// <returns>Object loaded from an XML file located in a specified isolated storage area, using a specified serialized format.</returns>
        public static Object Load(Object serializableObject, string fileName, IsolatedStorageFile isolatedStorageDirectory, SerializedFormat serializedFormat)
        {
            switch (serializedFormat)
            {
                case SerializedFormat.Binary:
                    serializableObject = LoadFromBinaryFormat(serializableObject, fileName, isolatedStorageDirectory);
                    break;

                case SerializedFormat.Document:
                default:
                    serializableObject = LoadFromDocumentFormat(serializableObject, null, fileName, isolatedStorageDirectory);
                    break;
            }

            return serializableObject;
        }

        /// <summary>
        /// Loads an object from an XML file in Document format, located in a specified isolated storage area, and supplying extra data types to enable deserialization of custom types within the object.
        /// </summary>
        /// <example>
        /// <code>
        /// // Always create a new object prior to passing to ObjectXMLSerializer.Load method.
        /// SerializableObject serializableObject = new SerializableObject();
        /// 
        /// serializableObject = (SerializableObject)ObjectXMLSerializer.Load(serializableObject, "XMLObjects.xml", IsolatedStorageFile.GetUserStoreForAssembly(), new Type[] { typeof(MyCustomType) });
        /// </code>
        /// </example>		
        /// <param name="serializableObject">Serializable object to be loaded from file.</param>
        /// <param name="fileName">Name of the file in the isolated storage area to load the object from.</param>
        /// <param name="isolatedStorageDirectory">Isolated storage area directory containing the XML file to load the object from.</param>
        /// <param name="extraTypes">Extra data types to enable deserialization of custom types within the object.</param>
        /// <returns>Object loaded from an XML file located in a specified isolated storage area, using a specified serialized format.</returns>
        public static Object Load(Object serializableObject, string fileName, IsolatedStorageFile isolatedStorageDirectory, System.Type[] extraTypes)
        {
            serializableObject = LoadFromDocumentFormat(serializableObject, null, fileName, isolatedStorageDirectory);
            return serializableObject;
        }


        public static Object Load(Object serializableObject, XmlNode xmlNode)
        {
            serializableObject = LoadFromXmlNodeFormat(serializableObject, null, xmlNode);
            return serializableObject;
        }
        #endregion

        #region Save_methods

        /// <summary>
        /// Saves an object to an XML file in Document format.
        /// </summary>
        /// <example>
        /// <code>        
        /// SerializableObject serializableObject = new SerializableObject();
        /// 
        /// ObjectXMLSerializer.Save(serializableObject, @"C:\XMLObjects.xml");
        /// </code>
        /// </example>
        /// <param name="serializableObject">Serializable object to be saved to file.</param>
        /// <param name="path">Path of the file to save the object to.</param>
        public static void Save(Object serializableObject, string path)
        {
            SaveToDocumentFormat(serializableObject, null, path, null);
        }

        /// <summary>
        /// Saves an object to an XML file using a specified serialized format.
        /// </summary>
        /// <example>
        /// <code>
        /// SerializableObject serializableObject = new SerializableObject();
        /// 
        /// ObjectXMLSerializer.Save(serializableObject, @"C:\XMLObjects.xml", SerializedFormat.Binary);
        /// </code>
        /// </example>
        /// <param name="serializableObject">Serializable object to be saved to file.</param>
        /// <param name="path">Path of the file to save the object to.</param>
        /// <param name="serializedFormat">XML serialized format used to save the object.</param>
        public static void Save(Object serializableObject, string path, SerializedFormat serializedFormat)
        {
            switch (serializedFormat)
            {
                case SerializedFormat.Binary:
                    SaveToBinaryFormat(serializableObject, path, null);
                    break;

                case SerializedFormat.Document:
                default:
                    SaveToDocumentFormat(serializableObject, null, path, null);
                    break;
            }
        }

        /// <summary>
        /// Saves an object to an XML file in Document format, supplying extra data types to enable serialization of custom types within the object.
        /// </summary>
        /// <example>
        /// <code>        
        /// SerializableObject serializableObject = new SerializableObject();
        /// 
        /// ObjectXMLSerializer.Save(serializableObject, @"C:\XMLObjects.xml", new Type[] { typeof(MyCustomType) });
        /// </code>
        /// </example>
        /// <param name="serializableObject">Serializable object to be saved to file.</param>
        /// <param name="path">Path of the file to save the object to.</param>
        /// <param name="extraTypes">Extra data types to enable serialization of custom types within the object.</param>
        public static void Save(Object serializableObject, string path, System.Type[] extraTypes)
        {
            SaveToDocumentFormat(serializableObject, extraTypes, path, null);
        }

        /// <summary>
        /// Saves an object to an XML file in Document format, located in a specified isolated storage area.
        /// </summary>
        /// <example>
        /// <code>        
        /// SerializableObject serializableObject = new SerializableObject();
        /// 
        /// ObjectXMLSerializer.Save(serializableObject, "XMLObjects.xml", IsolatedStorageFile.GetUserStoreForAssembly());
        /// </code>
        /// </example>
        /// <param name="serializableObject">Serializable object to be saved to file.</param>
        /// <param name="fileName">Name of the file in the isolated storage area to save the object to.</param>
        /// <param name="isolatedStorageDirectory">Isolated storage area directory containing the XML file to save the object to.</param>
        public static void Save(Object serializableObject, string fileName, IsolatedStorageFile isolatedStorageDirectory)
        {
            SaveToDocumentFormat(serializableObject, null, fileName, isolatedStorageDirectory);
        }

        /// <summary>
        /// Saves an object to an XML file located in a specified isolated storage area, using a specified serialized format.
        /// </summary>
        /// <example>
        /// <code>        
        /// SerializableObject serializableObject = new SerializableObject();
        /// 
        /// ObjectXMLSerializer.Save(serializableObject, "XMLObjects.xml", IsolatedStorageFile.GetUserStoreForAssembly(), SerializedFormat.Binary);
        /// </code>
        /// </example>
        /// <param name="serializableObject">Serializable object to be saved to file.</param>
        /// <param name="fileName">Name of the file in the isolated storage area to save the object to.</param>
        /// <param name="isolatedStorageDirectory">Isolated storage area directory containing the XML file to save the object to.</param>
        /// <param name="serializedFormat">XML serialized format used to save the object.</param>        
        public static void Save(Object serializableObject, string fileName, IsolatedStorageFile isolatedStorageDirectory, SerializedFormat serializedFormat)
        {
            switch (serializedFormat)
            {
                case SerializedFormat.Binary:
                    SaveToBinaryFormat(serializableObject, fileName, isolatedStorageDirectory);
                    break;

                case SerializedFormat.Document:
                default:
                    SaveToDocumentFormat(serializableObject, null, fileName, isolatedStorageDirectory);
                    break;
            }
        }

        /// <summary>
        /// Saves an object to an XML file in Document format, located in a specified isolated storage area, and supplying extra data types to enable serialization of custom types within the object.
        /// </summary>
        /// <example>
        /// <code>
        /// SerializableObject serializableObject = new SerializableObject();
        /// 
        /// ObjectXMLSerializer.Save(serializableObject, "XMLObjects.xml", IsolatedStorageFile.GetUserStoreForAssembly(), new Type[] { typeof(MyCustomType) });
        /// </code>
        /// </example>		
        /// <param name="serializableObject">Serializable object to be saved to file.</param>
        /// <param name="fileName">Name of the file in the isolated storage area to save the object to.</param>
        /// <param name="isolatedStorageDirectory">Isolated storage area directory containing the XML file to save the object to.</param>
        /// <param name="extraTypes">Extra data types to enable serialization of custom types within the object.</param>
        public static void Save(Object serializableObject, string fileName, IsolatedStorageFile isolatedStorageDirectory, System.Type[] extraTypes)
        {
            SaveToDocumentFormat(serializableObject, null, fileName, isolatedStorageDirectory);
        }

        #endregion

        #region Private

        private static FileStream CreateFileStream(IsolatedStorageFile isolatedStorageFolder, string path)
        {
            FileStream fileStream = null;

            if (isolatedStorageFolder == null)
                fileStream = new FileStream(path, FileMode.OpenOrCreate);
            else
                fileStream = new IsolatedStorageFileStream(path, FileMode.OpenOrCreate, isolatedStorageFolder);

            return fileStream;
        }

        private static Object LoadFromBinaryFormat(Object serializableObject, string path, IsolatedStorageFile isolatedStorageFolder)
        {
            using (FileStream fileStream = CreateFileStream(isolatedStorageFolder, path))
            {
                BinaryFormatter binaryFormatter = new BinaryFormatter();
                serializableObject = binaryFormatter.Deserialize(fileStream);
            }

            return serializableObject;
        }

        private static Object LoadFromDocumentFormat(Object serializableObject, System.Type[] extraTypes, string path, IsolatedStorageFile isolatedStorageFolder)
        {
            using (TextReader textReader = CreateTextReader(isolatedStorageFolder, path))
            {
                XmlSerializer xmlSerializer = CreateXmlSerializer(serializableObject, extraTypes);
                serializableObject = xmlSerializer.Deserialize(textReader);
            }

            return serializableObject;
        }

        private static object LoadFromXmlNodeFormat(Object serializableObject, System.Type[] extraTypes, XmlNode xmlNode)
        {
            try
            {
                using (XmlNodeReader nodeReader = CreateXmlReader(xmlNode))
                {
                    XmlSerializer xmlSerializer = CreateXmlSerializer(serializableObject, extraTypes);
                    serializableObject = xmlSerializer.Deserialize(nodeReader);
                }
            }
            catch
            {
                serializableObject = null;

                //if (nodeReader.Name == "ERROR")
                //   serializableObject = null;
            }

            return serializableObject;
        }

        private static TextReader CreateTextReader(IsolatedStorageFile isolatedStorageFolder, string path)
        {
            TextReader textReader = null;

            if (isolatedStorageFolder == null)
                textReader = new StreamReader(path);
            else
                textReader = new StreamReader(new IsolatedStorageFileStream(path, FileMode.Open, isolatedStorageFolder));

            return textReader;
        }

        private static XmlNodeReader CreateXmlReader(XmlNode xmlNode)
        {
            XmlNodeReader nodeReader = null;
            nodeReader = new XmlNodeReader(xmlNode);
            return nodeReader;
        }

        private static TextWriter CreateTextWriter(IsolatedStorageFile isolatedStorageFolder, string path)
        {
            TextWriter textWriter = null;

            if (isolatedStorageFolder == null)
                textWriter = new StreamWriter(path);
            else
                textWriter = new StreamWriter(new IsolatedStorageFileStream(path, FileMode.OpenOrCreate, isolatedStorageFolder));

            return textWriter;
        }

        private static XmlSerializer CreateXmlSerializer(Object serializableObject, System.Type[] extraTypes)
        {
            Type ObjectType = serializableObject.GetType();

            XmlSerializer xmlSerializer = null;

            if (extraTypes != null)
                xmlSerializer = new XmlSerializer(ObjectType, extraTypes);
            else
                xmlSerializer = new XmlSerializer(ObjectType);

            return xmlSerializer;
        }

        private static void SaveToDocumentFormat(Object serializableObject, System.Type[] extraTypes, string path, IsolatedStorageFile isolatedStorageFolder)
        {
            using (TextWriter textWriter = CreateTextWriter(isolatedStorageFolder, path))
            {
                XmlSerializer xmlSerializer = CreateXmlSerializer(serializableObject, extraTypes);
                xmlSerializer.Serialize(textWriter, serializableObject);
            }
        }

        private static void SaveToBinaryFormat(Object serializableObject, string path, IsolatedStorageFile isolatedStorageFolder)
        {
            using (FileStream fileStream = CreateFileStream(isolatedStorageFolder, path))
            {
                BinaryFormatter binaryFormatter = new BinaryFormatter();
                binaryFormatter.Serialize(fileStream, serializableObject);
            }
        }

        private static XmlNode SaveToXmlNode(Object serializableObject, System.Type[] extraTypes)
        {
            XmlNode xmlNode;

            using (MemoryStream mem = new MemoryStream())
            {
                XmlSerializer xmlSerializer = CreateXmlSerializer(serializableObject, null);
                xmlSerializer.Serialize(mem, serializableObject);
                mem.Position = 0;
                System.Xml.XmlDocument xmlDoc = new System.Xml.XmlDocument();
                xmlDoc.Load(mem);
                xmlNode = xmlDoc.DocumentElement;
            }

            return xmlNode;
        }

        #endregion
    }

    public class XMLUtils
    {
        public bool WriteObjectToXMLFile(object dataObj, string filePath, string fileName, out string errMsg)
        {
            errMsg = "";

            string file = string.Format("{0}{1}.xml", filePath, fileName);

            try
            {
                XmlNode xmlNode = ObjectXMLSerializer.ConvertToXmlNode(dataObj);

                if (File.Exists(file))
                    File.Delete(file);

                XmlTextWriter xmlTextWriter = new XmlTextWriter(file, System.Text.Encoding.UTF8);
                xmlNode.WriteTo(xmlTextWriter);
                xmlTextWriter.Flush();
                xmlTextWriter.Close();
                xmlTextWriter = null;

                return true;
            }
            catch (Exception ex)
            {
                qGlobal.WriteRunLog(true, ex.Message, "ERROR");
                errMsg = string.Format("{0}{1}", "Failed to save ", file);

                return false;
            }
        }

        public object LoadData(object dataObj, string file, out string errMsg)
        {
            errMsg = "";

            try
            {
                if (!File.Exists(file))
                {
                    errMsg = string.Format("{0}{1}", file, " not exist");
                    return null;
                }

                dataObj = (object)ObjectXMLSerializer.Load(dataObj, file);

                if (dataObj == null)
                {
                    errMsg = string.Format("{0}{1}", "Failed to load ", file);
                    return null;
                }

                return dataObj;
            }
            catch (Exception ex)
            {
                qGlobal.WriteRunLog(true, ex.Message, "ERROR");
                errMsg = string.Format("{0}{1}", "Failed to load ", file);

                return null;
            }
        }

        public object LoadXMLData(object myObj, string filePath, string fileName, out string errMsg)
        {
            errMsg = "";

            string file = string.Format("{0}{1}.xml", filePath, fileName);

            try
            {
                if (!File.Exists(file))
                {
                    errMsg = string.Format("{0}{1}", file, " not exist");
                    return null;
                }

                myObj = (object)ObjectXMLSerializer.Load(myObj, file);

                if (myObj == null)
                {
                    errMsg = string.Format("{0}{1}", "Failed to load ", file);
                    return null;
                }

                return myObj;
            }
            catch (Exception ex)
            {
                qGlobal.WriteRunLog(true, ex.Message, "ERROR");
                errMsg = string.Format("{0}{1}", "Failed to load ", file);

                return null;
            }
        }
    }
}
