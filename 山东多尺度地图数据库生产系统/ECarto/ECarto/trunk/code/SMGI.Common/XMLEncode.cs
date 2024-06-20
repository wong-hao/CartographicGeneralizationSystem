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
    public class XMLEncode
    {
        private XmlSerializer serializer;
        private string papername;
        public XMLEncode(string encodetype,GApplication app)
        {
            try
            {
                if (encodetype == "complete")
                {
                    //string[] strings = XMLDecode.sourcedir[0].Split(new Char[] { '\\' });
                    //for (int i = 0; i < strings.Length - 1; i++)
                    //{
                    //    papername = papername + strings[i] + "\\";
                    //}
                    papername = app.Workspace.Auto4DMapinfo.Content.OutJobCompleteFileName;
                    serializer = new XmlSerializer(typeof(SMGI.Common.InformPaper.root));
                    TextWriter writer = new StreamWriter(papername);
                    SMGI.Common.InformPaper.root completepaper = new InformPaper.root();
                    SMGI.Common.InformPaper.head cmphead = new InformPaper.head();
                    SMGI.Common.InformPaper.content cmpcontent = new InformPaper.content();
                    //head
                    cmphead.MessageType = "4DCTJobNoticeReport";
                    cmphead.MessageID = DateTime.Now.ToString("yyyyMMddHHmmss");
                    cmphead.OriginatorAddress = "4DCT";
                    cmphead.RecipientAddress = "MJM";
                    cmphead.CreationTime = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss");
                    //content
                    cmpcontent.TaskID = app.Workspace.Auto4DMapinfo.Content.TaskID;
                    cmpcontent.JobID = app.Workspace.Auto4DMapinfo.Content.JobID;
                    cmpcontent.JobOperator = app.Workspace.Auto4DMapinfo.Content.JobOperator;
                    cmpcontent.JobReportTime = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss");
                    cmpcontent.Result = "0";
                    cmpcontent.RejectReason = "null";
                    cmpcontent.OutputPath = app.Workspace.Auto4DMapinfo.Content.ProductOutPath;
                    cmpcontent.InnerQResult = "0";
                    cmpcontent.InnerQFileName = "null";
                    cmpcontent.OutFileCount = app.Workspace.Auto4DMapinfo.Content.ProductCount;
                    //outputfilelist
                    SMGI.Common.InformPaper.OutFileList outfilelist = new InformPaper.OutFileList();
                    List<SMGI.Common.InformPaper.OutFile> outfiles = new List<InformPaper.OutFile>();
                    outfilelist.OutFiles = outfiles;
                    SMGI.Common.InformPaper.OutFile outfile = new InformPaper.OutFile();
                    outfile.IfArchive = "0";
                    outfile.OutFileType = "0";
                    outfile.ProductID = "0";
                    outfile.ProductLevel = "DLG";
                    outfile.OutFileSize = "0";
                    outfile.OutFileName = "null";
                    outfilelist.OutFiles.Add(outfile);
                    cmpcontent.OutFileList = outfilelist;
                    //xmlgen
                    completepaper.content = cmpcontent;
                    completepaper.head = cmphead;
                    serializer.Serialize(writer, completepaper);
                    writer.Close();
                }
                else if (encodetype == "status")
                {
                    //string[] strings = XMLDecode.sourcedir[0].Split(new Char[] { '\\' });
                    //for (int i = 0; i < strings.Length - 1; i++)
                    //{
                    //    papername = papername + strings[i] + "\\";
                    //}
                    papername = app.Workspace.Auto4DMapinfo.Content.OutJobStatusFileName;
                    serializer = new XmlSerializer(typeof(SMGI.Common.StatusPaper.root));
                    TextWriter writer = new StreamWriter(papername);
                    SMGI.Common.StatusPaper.root statuspaper = new StatusPaper.root();
                    SMGI.Common.StatusPaper.head stshead = new StatusPaper.head();
                    SMGI.Common.StatusPaper.content stscontent = new StatusPaper.content();
                    //head
                    stshead.MessageType = "JobLogFile";
                    stshead.MessageID = DateTime.Now.ToString("yyyyMMddHHmmss");
                    stshead.OriginatorAddress = "4DCT";
                    stshead.RecipientAddress = "MJM";
                    stshead.CreationTime = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss");
                    //content
                    stscontent.TaskID = app.Workspace.Auto4DMapinfo.Content.TaskID;
                    stscontent.JobID = app.Workspace.Auto4DMapinfo.Content.JobID;
                    stscontent.JobType = app.Workspace.Auto4DMapinfo.Content.MJMJobType;
                    stscontent.JobState = "1";
                    stscontent.JobTime = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss");
                    stscontent.JobLog = "null";
                    //xmlgen
                    statuspaper.content = stscontent;
                    statuspaper.head = stshead;
                    serializer.Serialize(writer, statuspaper);
                    writer.Close();
                }
            }
            catch
            { }
        }
    }
}
