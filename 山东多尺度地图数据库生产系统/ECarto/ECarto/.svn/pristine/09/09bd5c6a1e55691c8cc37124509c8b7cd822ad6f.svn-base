using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.Xml;
using System.Collections;
using System.IO;

namespace SMGI.Common.StatusPaper
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
            [XmlElement("JobType")]
            public string JobType { get; set; }
            [XmlElement("JobState")]
            public string JobState { get; set; }
            [XmlElement("JobTime")]
            public string JobTime { get; set; }
            [XmlElement("JobLog")]
            public string JobLog { get; set; }
        }

}
