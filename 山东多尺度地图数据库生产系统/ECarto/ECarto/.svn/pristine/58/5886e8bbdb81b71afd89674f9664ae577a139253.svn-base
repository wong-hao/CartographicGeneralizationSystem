using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.Xml;
using System.Collections;
using System.IO;

namespace SMGI.Common.InformPaper
{
    public class root
    {
        [XmlElement("head")]
        public head head { get; set; }
        [XmlElement("content")]
        public content content { get; set; }
    }

    public class head
    {
        [XmlElement("MessageType")]
        public string MessageType { get; set; }
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
        [XmlElement("JobOperator")]
        public string JobOperator { get; set; }
        [XmlElement("JobReportTime")]
        public string JobReportTime { get; set; }
        [XmlElement("Result")]
        public string Result { get; set; }
        [XmlElement("RejectReason")]
        public string RejectReason { get; set; }
        [XmlElement("OutputPath")]
        public string OutputPath { get; set; }
        [XmlElement("InnerQResult")]
        public string InnerQResult { get; set; }
        [XmlElement("InnerQFileName")]
        public string InnerQFileName { get; set; }
        [XmlElement("OutFileCount")]
        public string OutFileCount { get; set; }
        [XmlElement("OutFileList")]
        public OutFileList OutFileList { get; set; }
    }

    public class OutFileList
    {
        [XmlElement("OutFile")]
        public List<OutFile> OutFiles { get; set; }
    }

    public class OutFile
    {
        [XmlElement("ProductID")]
        public string ProductID { get; set; }
        [XmlElement("ProductLevel")]
        public string ProductLevel { get; set; }
        [XmlElement("OutFileName")]
        public string OutFileName { get; set; }
        [XmlElement("OutFileSize")]
        public string OutFileSize { get; set; }
        [XmlElement("IfArchive")]
        public string IfArchive { get; set; }
        [XmlElement("OutFileType")]
        public string OutFileType { get; set; }
    }
}
