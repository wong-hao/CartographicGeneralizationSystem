using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ESRI.ArcGIS.esriSystem;
using System.Runtime.Serialization.Formatters.Soap;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace SMGI.Common
{
    [Serializable]
    internal class ConverInfo
    {
        public Guid ClassID { get; set; }
        public string Value { get; set; }
    }

    [Serializable]
    internal class ConverInfoByte
    {
        //public Type Type { get; set; }
        public Guid ClassID { get; set; }
        public byte[] Value { get; set; }
    }

    /// <summary>
    /// 转换类，能转换 xml 和 带 [Serializable] 属性的托管类型 或者 带 IPersistStream 接口的 com 对象
    /// </summary>
    public static class GConvert
    {
        public static string ObjectToXml(object obj)
        {
            ConverInfo info = ObjectToXmlInfo(obj);
            return WriteToSoap(info);
        }
        public static object XmlToObject(string xml)
        {
            ConverInfo info = ReadFromSoap(xml) as ConverInfo;
            return XmlInfoToObject(info);
        }
        internal static ConverInfo ObjectToXmlInfo(object obj)
        {
            ConverInfo info = new ConverInfo();
            Type t = obj.GetType();
            info.ClassID = t.GUID;
            if (t.IsCOMObject)
            {
                if (obj is IPersist)
                {
                    Guid guid;
                    (obj as IPersist).GetClassID(out guid);
                    info.ClassID = guid;
                }
                if (obj is IPersistStream)
                {
                    IMemoryBlobStream blob = new MemoryBlobStreamClass();
                    (obj as IPersistStream).Save(blob as IStream, 0);
                    IMemoryBlobStreamVariant pVar = (IMemoryBlobStreamVariant)blob;
                    object bytes;
                    pVar.ExportToVariant(out bytes);
                    byte[] bs = bytes as byte[];
                    info.Value = WriteToSoap(bs);
                    Guid gu;
                    (obj as IPersistStream).GetClassID(out gu);
                }
                else
                {
                    throw new Exception("无法转换");
                }
            }
            else
            {
                info.Value = WriteToSoap(obj);
            }
            return info;

        }
        internal static object XmlInfoToObject(ConverInfo info)
        {
            string v = info.Value;
            Type t = Type.GetTypeFromCLSID(info.ClassID);
            if (t.IsCOMObject)
            {
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
        internal static string WriteToSoap(object instance)
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
        internal static object ReadFromSoap(string str)
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

        //为有泛型的类准备的序列化函数
        public static string ObjectToXML4Generic(object obj)
        {
            string result = "";
            try
            {
                XmlSerializer xmlMachine = new XmlSerializer(obj.GetType());
                MemoryStream ms = new MemoryStream();
                xmlMachine.Serialize(ms, obj);
                byte[] bs = ms.ToArray();
                result = System.Convert.ToBase64String(bs);
                ms.Close(); ms.Dispose();
            }catch
            { 
            
            }
            return result;
        }
        public static object XMLToObject4Generic(string str, Type t)
        {
            object obj = null;
            try
            {
                XmlSerializer xs = new XmlSerializer(t);
                MemoryStream ms = new MemoryStream(System.Convert.FromBase64String(str));
                obj = xs.Deserialize(ms);
                ms.Close(); ms.Dispose();
            }
            catch
            { }
            return obj;
        }


        internal static ConverInfoByte ObjectToByteInfo(object obj)
        {
            ConverInfoByte info = new ConverInfoByte();
            Type t = obj.GetType();
            info.ClassID = Guid.Empty;
            if (t.IsCOMObject)
            {
                if (obj is IPersist)
                { 
                    Guid guid;
                    (obj as IPersist).GetClassID(out guid);
                    info.ClassID = guid;
                }
                if (obj is IPersistStream)
                {
                    IMemoryBlobStream blob = new MemoryBlobStreamClass();
                    (obj as IPersistStream).Save(blob as IStream, 0);
                    IMemoryBlobStreamVariant pVar = (IMemoryBlobStreamVariant)blob;
                    object bytes;
                    pVar.ExportToVariant(out bytes);
                    byte[] bs = bytes as byte[];
                    info.Value = bs;
                }
                else
                {
                    throw new Exception("无法转换");
                }
            }
            else
            {
                info.Value = WriteToBinary(obj);
            }
            return info;
        }
        internal static object ByteInfoToObject(ConverInfoByte info)
        {
            //System.Diagnostics.Debug.Print(info.ClassID.ToString());
            byte[] v = info.Value;
            
            if (info.ClassID != Guid.Empty)
            {
                Type t = Type.GetTypeFromCLSID(info.ClassID);
                IMemoryBlobStream blob = new MemoryBlobStreamClass();
                (blob as IMemoryBlobStreamVariant).ImportFromVariant(v);
                
                IPersistStream result = Activator.CreateInstance(t) as IPersistStream;
                
                //IPersistStream result = Activator.CreateComInstanceFrom(info.AssembleName, info.DataType) as IPersistStream;
                result.Load(blob as IStream);

                return result;
            }
            else
            {
                return ReadFromBinary(v);
            }
        }
        public static string ObjectToBase64(object obj) {
            try
            {
                ConverInfoByte info = ObjectToByteInfo(obj);
                return System.Convert.ToBase64String(WriteToBinary(info));
            }
            catch {
                return string.Empty;
            }
        }
        public static object Base64ToObject(string base64code) {
            try
            {
                ConverInfoByte info = ReadFromBinary(System.Convert.FromBase64String(base64code)) as ConverInfoByte;
                return ByteInfoToObject(info);
            }
            catch
            {
                return null;
            }
        }
    }

}
