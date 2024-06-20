using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.Xml;
using System.Collections;
using System.IO;

namespace SMGI.Common
{
    [Serializable]
    public class root
    {
        [XmlElement("head")]
        public head head {get;set;}
        [XmlElement("content")]
        public content content { get; set; }
    }

    [Serializable]
    public class head
    { 
        [XmlElement("MessageType")]
        public string MessageType {get;set;}
        [XmlElement("MessageID")]
        public string MessageID { get; set; }
        [XmlElement("OriginatorAddress")]
        public string OriginatorAddress { get; set; }
        [XmlElement("RecipientAddress")]
        public string RecipientAddress { get; set; }
        [XmlElement("CreationTime")]
        public string CreationTime { get; set; }
    }

    [Serializable]
    public class content
    {
        [XmlElement("TaskID")]
        public string TaskID { get; set; }
        [XmlElement("JobID")]
        public string JobID { get; set; }
        [XmlElement("MJMJobType")]
        public string MJMJobType { get; set; }
        [XmlElement("SatelliteID")]
        public string SatelliteID { get; set; }
        [XmlElement("TaskPriority")]
        public string TaskPriority { get; set; }
        [XmlElement("ProductCount")]
        public string ProductCount { get; set; }
        [XmlElement("ProductList")]
        public ProductList ProductList { get; set; }
        [XmlElement("ProductOutPath")]
        public string ProductOutPath {get;set;}
        [XmlElement("ProcessOutPath")]
        public string ProcessOutPath { get; set; }
        [XmlElement("HelperOutPath")]
        public string HelperOutPath { get; set; }
        [XmlElement("OutJobStatusFileName")]
        public string OutJobStatusFileName { get; set; }
        [XmlElement("OutJobCompleteFileName")]
        public string OutJobCompleteFileName { get; set; }
        [XmlElement("JobOperator")]
        public string JobOperator { get; set; }
        [XmlElement("JobStartTime")]
        public string JobStartTime { get; set; }
        [XmlElement("JobEndTime")]
        public string JobEndTime { get; set; }
        [XmlElement("IfRedo")]
        public string IfRedo { get; set; }
        [XmlElement("QMCFilePathName")]
        public string QMCFilePathName { get; set; }
        [XmlElement("QMCFileName")]
        public string QMCFileName { get; set; }
        [XmlElement("InputCount")]
        public string InputCount { get; set; }
        [XmlElement("InputFileList")]
        public InputFileList InputFileList { get; set; }
    }

    [Serializable]
    public class ProductList
    { 
        [XmlElement("Product")]
        public List<Product> Products { get; set; }
    }

    [Serializable]
    public class Product
    {
        [XmlElement("ProductID")]
        public string ProductID { get; set; }
        [XmlElement("ProductLevel")]
        public string ProductLevel { get; set; }
        [XmlElement("ExtentType")]
        public string ExtentType { get; set; }
        [XmlElement("MapScale")]
        public string MapScale { get; set; }
        [XmlElement("MapNumber")]
        public string MapNumber { get; set; }
        [XmlElement("PgProductFormat")]
        public string PgProductFormat { get; set; }
        [XmlElement("EarthModel")]
        public string EarthModel {get;set;}
        [XmlElement("CoordinateSys")]
        public string CoordinateSys {get;set;}
        [XmlElement("MapProjection")]
        public string MapProjection {get;set;}
        [XmlElement("DatumLevel")]
        public string DatumLevel {get;set;}
        [XmlElement("BursaSevenParameters")]
        public string BursaSevenParameters {get;set;}
        [XmlElement("ProjectionParameters")]
        public string ProjectionParameters {get;set;}
        [XmlElement("TjFirstLevelName")]
        public string TjFirstLevelName {get;set;}
        [XmlElement("TjDistrictName")]
        public string TjDistrictName {get;set;}
        [XmlElement("TjType")]
        public string TjType {get;set;}
        [XmlElement("PaperSize")]
        public string PaperSize {get;set;}
        [XmlElement("FJSM")]
        public string FJSM {get;set;}
        [XmlElement("Is4dCQ")]
        public string Is4dCQ {get;set;}
        [XmlElement("BQInformation")]
        public string BQInformation {get;set;}
        [XmlElement("SCZInformation")]
        public string SCZInformation {get;set;}
        [XmlElement("MLevel")]
        public string MLevel {get;set;}
        [XmlElement("SpecialIntroduce")]
        public string SpecialIntroduce { get; set; }
    }

    [Serializable]
    public class InputFileList
    {
        [XmlElement("InputFile")]
        public List<InputFile> InputFiles { get; set; }
    }

    [Serializable]
    public class InputFile
    {
        [XmlElement("DataID")]
        public string DataID { get; set; }
        [XmlElement("InputFileName")]
        public string InputFileName { get; set; }
        [XmlElement("InputFileType")]
        public string InputFileType { get; set; }
        [XmlElement("InputFileSize")]
        public string InputFileSize { get; set; }
        [XmlElement("EarthModel")]
        public string EarthModel { get; set; }
        [XmlElement("MapProjection")]
        public string MapProjection { get; set; }
        [XmlElement("ZoneType")]
        public string ZoneType { get; set; }
        [XmlElement("ZoneNo")]
        public string ZoneNo { get; set; }
    }
    
    public class XMLDecode
    {
        //public static List<string> sourcedir = new List<string>();
        //public static List<root> workpapers = new List<root>();
        //public static bool ischeck = true;
        public static List<string> sourcedir = new List<string>();
        private GApplication app;
        public XMLDecode(GApplication app, List<string> xmls)
        {
            //string path = sourcedir[0] + "\\4D出图软件作业通知单.xml";
            bool contain = false;
            try
            {
                foreach (var item in xmls)
                {
                    XmlDocument xmldoc = new XmlDocument();
                  //  System.Windows.Forms.MessageBox.Show(item);
                    xmldoc.Load(item);

                    MemoryStream ms = new MemoryStream();
                    xmldoc.Save(ms);
                    ms.Position = 0;

                    //声明序列化对象实例serializers
                    XmlSerializer serializer = new XmlSerializer(typeof(root));
                    //反序列化，并将反序列化结果值赋给变量i
                    root paperxml = (root)serializer.Deserialize(ms);
                    //workpapers.Add(paperxml);
                    foreach(root wpaper in app.workpapers)
                    {
                        if (paperxml.head.MessageID == wpaper.head.MessageID)
                        {
                            System.Windows.Forms.MessageBox.Show("任务单已存在！");
                            contain = true;
                            break;
                        } 
                    }
                    if (contain)
                    {
                        continue;
                    }
                    else
                    {
                        app.workpapers.Add(paperxml);
                    }
                }
            }
            catch(Exception e)
            {
                System.Windows.Forms.MessageBox.Show(e.ToString());
                //ischeck = false;
                return;
            }
        }

    }
}
