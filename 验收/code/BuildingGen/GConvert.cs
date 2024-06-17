using System;
using System.Collections.Generic;
using System.Text;
using ESRI.ArcGIS.esriSystem;
using System.Runtime.Serialization.Formatters.Soap;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Xml;

namespace BuildingGen
{
    [Serializable]
    public class GConverInfo
    {
        public string DataType { get; set; }
        public string GUID { get; set; }
        public string XML { get; set; }
    }
    /// <summary>
    /// 转换类，能转换 xml 和 带 [Serializable] 属性的托管类型 或者 带 IPersistStream 接口的 com 对象
    /// </summary>
    public class GConvert
    {
        public static string ObjectToXml(object obj)
        {
            GConverInfo info = ObjectToXmlInfo(obj);
            return WriteToSoap(info);
        }
        public static object XmlToObject(string xml)
        {
            GConverInfo info = ReadFromSoap(xml) as GConverInfo;
            return XmlInfoToObject(info);
        }
        public static GConverInfo ObjectToXmlInfo(object obj)
        {
            GConverInfo info = new GConverInfo();
            Type t = obj.GetType();
            info.DataType = t.ToString();
            if (t.IsCOMObject)
            {
                if (obj is IPersistStream)
                {
                    IMemoryBlobStream blob = new MemoryBlobStreamClass();
                    (obj as IPersistStream).Save(blob as IStream, 0);
                    IMemoryBlobStreamVariant pVar = (IMemoryBlobStreamVariant)blob;
                    object bytes;
                    pVar.ExportToVariant(out bytes);
                    byte[] bs = bytes as byte[];
                    info.XML = WriteToSoap(bs);
                    Guid gu;
                    (obj as IPersistStream).GetClassID(out gu);
                    info.GUID = gu.ToString();
                }
                else
                {
                    throw new Exception("无法转换");
                }
            }
            else
            {
                info.XML = WriteToSoap(obj);
            }
            return info;

        }
        public static object XmlInfoToObject(GConverInfo info)
        {
            string v = info.XML;
            string datatype = info.DataType;
            if (datatype == "System.__ComObject")
            {
                string guid = info.GUID;
                Guid g = new Guid(guid);
                Type t = Type.GetTypeFromCLSID(g);

                IMemoryBlobStream blob = new MemoryBlobStreamClass();
                (blob as IMemoryBlobStreamVariant).ImportFromVariant(ReadFromSoap(v as string));
                IPersistStream result = Activator.CreateInstance(t) as IPersistStream;
                result.Load(blob as IStream);

                return result;
            }
            else
            {
                return ReadFromSoap(v as string);
            }

        }
        public static string WriteToSoap(object instance)
        {
            SoapFormatter SoapFmater = new SoapFormatter();
            using (MemoryStream ms = new MemoryStream())
            {
                SoapFmater.Serialize(ms, instance);
                ms.Position = 0;
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(ms);
                return xmlDoc.InnerXml;
            }
        }
        public static object ReadFromSoap(string str)
        {
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(str);
            SoapFormatter SoapFmater = new SoapFormatter();
            using (MemoryStream ms = new MemoryStream())
            {
                xmlDoc.Save(ms);
                ms.Position = 0;
                object instance = SoapFmater.Deserialize(ms);
                return instance;
            }
        }

        public static byte[] WriteToBinary(object instance)
        {
            BinaryFormatter formatter = new BinaryFormatter();
            using (MemoryStream ms = new MemoryStream())
            {
                formatter.Serialize(ms, instance);
                return ms.ToArray();
            }
        }
        public static object ReadFromBinary(byte[] data)
        {
            BinaryFormatter formatter = new BinaryFormatter();
            using (MemoryStream ms = new MemoryStream(data))
            {                
                return formatter.Deserialize(ms);
            }
        }
    }
}
