using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.OleDb;
using System.Windows.Forms;
using System.Xml;
using SMGI.Common;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.DataSourcesGDB;
using System.Xml.Linq;
using System.IO;
using System.Data;
using ESRI.ArcGIS.Maplex;
using System.Runtime.InteropServices;
using ESRI.ArcGIS.Controls;
using System.Drawing;
using ESRI.ArcGIS.Display;
using stdole;
using ESRI.ArcGIS.ADF.COMSupport;
using ESRI.ArcGIS.Geoprocessor;
using ESRI.ArcGIS.esriSystem;
using System.Diagnostics;
using System.Threading;
namespace SMGI.Plugin.EmergencyMap
{
    public class AuoExportMapCommand : SMGICommand
    {
        public AuoExportMapCommand()
        {
            m_caption = "一键输出地图";
            m_toolTip = "一键输出地图";
        }

        public override bool Enabled
        {
            get
            {
                return m_Application != null && m_Application.Workspace == null &&
                    ClipElement.GetClipRangeElement(m_Application.MapControl.ActiveView.GraphicsContainer) != null;
            }
        }
        private  bool attachMap = false;
        private  string GetAppDataPath()
        {
            if (System.Environment.OSVersion.Version.Major <= 5)
            {
                return System.IO.Path.GetFullPath(
                    System.IO.Path.GetDirectoryName(System.Windows.Forms.Application.ExecutablePath) + @"\..");
            }

            var dp = System.Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var di = new System.IO.DirectoryInfo(dp);
            var ds = di.GetDirectories("SMGI");
            if (ds == null || ds.Length == 0)
            {
                var sdi = di.CreateSubdirectory("SMGI");
                return sdi.FullName;
            }
            else
            {
                return ds[0].FullName;
            }
        }
        private void WaitForTimerCheck(int id)
        {
            try
            {
                while (true)
                {
                    using (var p = Process.GetProcessById(id))
                    {
                        Thread.Sleep(200);
                    }
                }
            }
            catch
            {
                return;
            }
        }
        public override void OnClick()
        {
            FrmAutoMap frm = new FrmAutoMap(m_Application);
            DialogResult dr=  frm.ShowDialog();
            if (dr != DialogResult.OK)
                return;
            
            WaitOperation wo = m_Application.SetBusy();
            try
            {
                XDocument doc = new XDocument();

                XElement root = new XElement("Arg");
                root.Add(new XElement("Product", m_Application.Template.ClassName));
                root.Add(new XElement("Template", m_Application.Template.Caption));

                XElement cmds = new XElement("Commands");
                XElement cmd = null;
                Process p = null;
                ProcessStartInfo si = null;
                //
                #region 第一步 数据获取
                wo.SetText("数据获取...当前步骤：1/5");
                string sourceGDB = frm.SourceGDB;

                var paramContent = EnvironmentSettings.getContentElement(m_Application);
                var mapScale = paramContent.Element("MapScale");//比例尺
                var content = EnvironmentSettings.getContentElement(m_Application);
                var server = content.Element("Server");
                string ipAddress = server.Element("IPAddress").Value;
                string userName = server.Element("UserName").Value;
                string passWord = server.Element("Password").Value;
                string databaseName = content.Element("DatabaseName").Value;
                ISpatialReference targetSpatialReference = CommonMethods.getClipSpatialRef();
                if (null == targetSpatialReference)
                    return;
                IGeometry clipGeo = ClipElement.GetClipRangeElement(m_Application.MapControl.ActiveView.GraphicsContainer).Geometry;
                if (null == clipGeo)
                {
                    MessageBox.Show("无效的裁切面！");
                    return;
                }
                //页面矩形
                IGeometry pageGeo = ClipElement.createPageRect(m_Application, targetSpatialReference,"出版范围");
                //内图廓
                IGeometry InlineGeo = ClipElement.createInlinePolyline(m_Application, targetSpatialReference, "内图廓");
                //外图廓
                IGeometry OutlineGeo = ClipElement.createInlinePolyline(m_Application, targetSpatialReference, "外图廓");
                //丁字线
                IList<IGeometry> ListTlineGeo = ClipElement.createMapTlinePolyline(m_Application, targetSpatialReference);
                //压盖
                IGeometry OverlapGeo = ClipElement.createOvelapRect(InlineGeo, pageGeo);
                //附区
                IGeometry attachGeo = new PolygonClass();

                if ((pageGeo as IRelationalOperator).Equals(clipGeo))
                {
                   
                }
                else
                {
                    IGeometry pageClone = (pageGeo as IClone).Clone() as IGeometry;
                    pageClone.Project(clipGeo.SpatialReference);
                    attachGeo = (pageClone as ITopologicalOperator).Difference(clipGeo);
                }
              
                //数据下载
                cmd = new XElement("SMGI.Plugin.EmergencyMap.DataBaseDownLoadCmd");
                cmd.Add(new XElement("Height", CommonMethods.PaperHeight));//
                cmd.Add(new XElement("Width", CommonMethods.PaperWidth));//
                cmd.Add(new XElement("BaseMap", frm.BaseMapStyle));
                cmd.Add(new XElement("Attach", frm.AttachMap));//
                cmd.Add(new XElement("OutPutGDB", sourceGDB));//
                cmd.Add(new XElement("IP", ipAddress));//
                cmd.Add(new XElement("UserName", userName));
                cmd.Add(new XElement("PassWord", passWord));
                cmd.Add(new XElement("Scale", mapScale.Value));//
                cmd.Add(new XElement("SpatialReference", CommonMethods.clipSpatialRefFileName));//
                cmd.Add(new XElement("ClipGeoJson", CommonMethods.GeometryToJsonString(clipGeo)));
                cmd.Add(new XElement("PageGeoJson", CommonMethods.GeometryToJsonString(pageGeo)));
                cmd.Add(new XElement("InlineGeoJson", CommonMethods.GeometryToJsonString(InlineGeo)));
                cmd.Add(new XElement("OutlineGeoJson", CommonMethods.GeometryToJsonString(OutlineGeo)));
                XElement ListTlineGeoJson = new XElement("ListTlineGeoJson");
                foreach (var geo in ListTlineGeo)
                {
                    ListTlineGeoJson.Add(new XElement("TlineGeoJson", CommonMethods.GeometryToJsonString(geo)));
                }
                //专题数据
                if (frm.ThematicInfo != null)
                {
                    var thematic = new XElement("Thematic");
                    var item= new XElement("Item");
                    item.Add(new XElement("Name", frm.ThematicInfo.Name));
                    item.Add(new XElement("IP", frm.ThematicInfo.IP));
                    item.Add(new XElement("UserName", frm.ThematicInfo.UserName));
                    item.Add(new XElement("Password", frm.ThematicInfo.Password));
                    item.Add(new XElement("DataBase", frm.ThematicInfo.DataBase));

                    //图层
                    var lyrs = new XElement("Layers");
                    foreach (var kv in frm.ThematicInfo.Lyrs)
                    {
                        if (kv.Value)
                        {
                            lyrs.Add(new XElement("Layer", kv.Key));
                        }
                    }
                    item.Add(lyrs);
                  
                    thematic.Add(item);
                    cmd.Add(thematic);
                }
                cmd.Add(ListTlineGeoJson);
                cmd.Add(new XElement("OverlapGeoJson", CommonMethods.GeometryToJsonString(OverlapGeo)));
                cmd.Add(new XElement("AttachGeoJson", CommonMethods.GeometryToJsonString(attachGeo)));
                cmds.Add(cmd);
                //数据升级
                cmd = new XElement("SMGI.Plugin.EmergencyMap.DataBaseStructUpgradeCmd");
                cmd.Add(new XElement("SourceGDB", sourceGDB));//
                cmd.Add(new XElement("OutPutGDB", frm.UpgradeGDB));//
                cmd.Add(new XElement("MapScale", mapScale.Value));//
                cmd.Add(new XElement("AttachMap", frm.AttachMap));//
                cmd.Add(new XElement("BaseMapTemplate", frm.BaseMapStyle));
                cmd.Add(new XElement("BaseMap", frm.BaseMapStyle));
                cmds.Add(cmd);
                //结尾
                root.Add(cmds);
                doc.Add(root);
                string tempxml = GetAppDataPath() + "\\autoStep1.xml";
                if (File.Exists(tempxml))
                {
                    File.Delete(tempxml);
                }
                doc.Save(tempxml);
                int pid = -1;
                using (p = new Process())
                {
                    si = new ProcessStartInfo();
                    si.FileName = GApplication.ExePath + "\\" + "SMGI.Shell.exe";
                    si.Arguments = string.Format("\"{0}\"", tempxml);
                    si.UseShellExecute = false;
                    si.CreateNoWindow = true;

                    p.StartInfo = si;
                    p.Start();
                    pid = p.Id;
                }
                WaitForTimerCheck(pid);
                #endregion
               
                string gdb = frm.UpgradeGDB;
                //
                #region 第一组 注记生成 耗内存单独由一个shell处理
                wo.SetText("注记生成...当前步骤：2/5");
                doc = new XDocument();

                root = new XElement("Arg");
                root.Add(new XElement("Product", m_Application.Template.ClassName));
                root.Add(new XElement("Template", m_Application.Template.Caption));

                cmds = new XElement("Commands");
                cmd = null;
                // 1.打开数据库 必须放在前面
                cmd = new XElement("SMGI.Plugin.BaseFunction.GDBWorkspaceOpenCmd");
                cmd.SetValue(gdb);
                cmds.Add(cmd);
                //2.图廓线生成 是注记，图廓线的基础:已经处理完成
                // cmd = new XElement("SMGI.Plugin.EmergencyMap.FrmMapLineCmd");
                // cmds.Add(cmd);
                
                //3.注记生成
                if (frm.MaplexAnnotateCmd.Checked)
                {
                    cmd = frm.AnnoEle;
                    cmds.Add(cmd);
                }
                //4.境界处理
                if (frm.BoulSkipDrawCmd.Checked)
                {
                    cmd = frm.BOULSkipEle;
                    cmds.Add(cmd);
                }
                root.Add(cmds);
                doc.Add(root);
                tempxml = GetAppDataPath() + "\\autoStep2.xml";
                if (File.Exists(tempxml))
                {
                    File.Delete(tempxml);
                }
                doc.Save(tempxml);
                pid = -1;
                using (p = new Process())
                {
                    si = new ProcessStartInfo();
                    si.FileName = GApplication.ExePath + "\\" + "SMGI.Shell.exe";
                    si.Arguments = string.Format("\"{0}\"", tempxml);
                    si.UseShellExecute = false;
                    si.CreateNoWindow = true;
                    p.StartInfo = si;
                    p.Start();
                    pid = p.Id;
                }
                WaitForTimerCheck(pid);
                
                #endregion
                System.Threading.Thread.Sleep(1500);
                #region 第二组图面整饰相关
                wo.SetText("图面整饰...当前步骤：3/5");
                doc = new XDocument();

                root = new XElement("Arg");
                root.Add(new XElement("Product", m_Application.Template.ClassName));
                root.Add(new XElement("Template", m_Application.Template.Caption));

                cmds = new XElement("Commands");
                cmd = null;
                //1.打开GDB
                cmd = new XElement("SMGI.Plugin.BaseFunction.GDBWorkspaceOpenCmd");
                cmd.SetValue(gdb);
                cmds.Add(cmd);
                //2.图名
                cmd = new XElement("SMGI.Plugin.EmergencyMap.MapNameSetCmd");
                {
                    cmd.Add(new XElement("MapName", frm.MapName));
                    cmd.Add(new XElement("MapNameSize", frm.MapNameInfos.MapNameSize));
                    cmd.Add(new XElement("FontName", frm.MapNameInfos.MapNameFont));
                    cmd.Add(new XElement("MapNameColor", frm.MapNameInfos.MapNameColor));
                    cmd.Add(new XElement("IsArtStyle", frm.MapNameInfos.IsArtStyle));
                    cmd.Add(new XElement("MapNameSpace", frm.MapNameInfos.MapNameSpace));
                    cmd.Add(new XElement("MapNameWidth", frm.MapNameInfos.MapNameWidth));
                    cmd.Add(new XElement("MapNameDis", frm.MapNameInfos.MapNameDis));
                    cmd.Add(new XElement("MapNameInterval", 8));
                    cmd.Add(new XElement("ProductName", frm.MapProductName));
                    cmd.Add(new XElement("ProductSize", 6));
                }
                DateTime dt = DateTime.Now;
                string productTime = dt.Year + "年" + dt.Month + "月";
                cmd.Add(new XElement("ProductTime", productTime));
                cmd.Add(new XElement("TimeSize", 6));
                cmds.Add(cmd);
                // 3 花边
                if (frm.FootBorderCmd.Checked)
                {
                    cmd = frm.FootBorderEle; ;
                    cmds.Add(cmd);
                }
                // 4 色带面
                if (frm.SDMColorCmd.Checked)
                {
                    cmd = frm.SDMElement;
                    cmds.Add(cmd);
                }
                if (frm.NorthCmd.Checked)
                {
                    cmd = frm.CompassCmdEle;
                    cmds.Add(cmd);
                }
                // 6 图例   在比例尺生成前面
                if (frm.LengendDyCreateCmd.Checked)
                {
                    cmd = new XElement("SMGI.Plugin.EmergencyMap.LengendDyCreateCmd");
                    cmds.Add(cmd);
                }
                //7.比例尺 
                if (frm.ScaleBarCmd.Checked)
                {
                    cmd = frm.ScaleBarEle;
                    cmds.Add(cmd);
                }


                root.Add(cmds);
                doc.Add(root);
                tempxml = GetAppDataPath() + "\\autoStep3.xml";
                if (File.Exists(tempxml))
                {
                    File.Delete(tempxml);
                }
                doc.Save(tempxml);
                pid = -1;
                using (p = new Process())
                {
                    si = new ProcessStartInfo();
                    si.FileName = GApplication.ExePath + "\\" + "SMGI.Shell.exe";
                    si.Arguments = string.Format("\"{0}\"", tempxml);
                    si.UseShellExecute = false;
                    si.CreateNoWindow = true;

                    p.StartInfo = si;
                    p.Start();
                    pid = p.Id;
                }
                WaitForTimerCheck(pid);
                #endregion
                System.Threading.Thread.Sleep(1500);
                #region 第三组地图处理
                doc = new XDocument();
                wo.SetText("地图处理...当前步骤：4/5");
                root = new XElement("Arg");
                root.Add(new XElement("Product", m_Application.Template.ClassName));
                root.Add(new XElement("Template", m_Application.Template.Caption));

                cmds = new XElement("Commands");
                cmd = null;
                //1.打开数据库
                cmd = new XElement("SMGI.Plugin.BaseFunction.GDBWorkspaceOpenCmd");
                cmd.SetValue(gdb);
                cmds.Add(cmd);
                //1 境界普色
                if (frm.ZoneColorCmd.Checked)
                {
                    cmd = frm.BouaColorEle;
                    cmds.Add(cmd);
                }
                //2.地名冲突
                if (frm.SymbolProcessCmd.Checked)
                {
                    cmd = new XElement("SMGI.Plugin.EmergencyMap.SymbolProcessCmd");
                    cmds.Add(cmd);
                }
                //3.水系面边线提取
                if (frm.RiverBoundayExtractCmd.Checked)
                {
                    cmd = new XElement("SMGI.Plugin.EmergencyMap.RiverBoundayExtractCmd");
                    cmds.Add(cmd);
                }
                //4.调整有向点
                if (frm.DirectionPointAdjustmentCmd.Checked)
                {
                    cmd = new XElement("SMGI.Plugin.EmergencyMap.DirectionPointAdjustmentCmd");
                    cmds.Add(cmd);
                }
                //5.涵洞符号
                if (frm.JustifyCulvertCmd.Checked)
                {
                    cmd = new XElement("SMGI.Plugin.EmergencyMap.JustifyCulvertCmd");
                    cmds.Add(cmd);
                }
                //6.水系线消音
                if (frm.HydlMaskProcessCmd.Checked)
                {
                    cmd = new XElement("SMGI.Plugin.EmergencyMap.HydlMaskProcessCmd");
                    cmds.Add(cmd);
                }
               

                //7.河流渐变
                if (frm.RiverAutoGradualCmd.Checked)
                {
                    cmd = new XElement("SMGI.Plugin.EmergencyMap.RiverAutoGradualCmd");
                    cmds.Add(cmd);
                }
                //8.道路端头处理
                if (frm.RoadSymEndsCmd.Checked)
                {
                    cmd = new XElement("SMGI.Plugin.EmergencyMap.RoadEndsProcessCmd");
                    cmds.Add(cmd);
                }
                //***************
                root.Add(cmds);
                doc.Add(root);
                pid = -1;
                tempxml = GetAppDataPath() + "\\autoStep4.xml";
                if (File.Exists(tempxml))
                {
                    File.Delete(tempxml);
                }
                doc.Save(tempxml);
                using (p = new Process())
                {
                    si = new ProcessStartInfo();
                    si.FileName = GApplication.ExePath + "\\" + "SMGI.Shell.exe";
                    si.Arguments = string.Format("\"{0}\"", tempxml);
                    si.UseShellExecute = false;
                    si.CreateNoWindow = true;
                    p.StartInfo = si;
                    p.Start();
                    pid = p.Id;
                }
                WaitForTimerCheck(pid);
               
                #endregion
                System.Threading.Thread.Sleep(1500);
                #region 其他组 位置图处理 比较耗内存单独处理
                doc = new XDocument();
                wo.SetText("其他处理...当前步骤：5/5");
                root = new XElement("Arg");
                root.Add(new XElement("Product", m_Application.Template.ClassName));
                root.Add(new XElement("Template", m_Application.Template.Caption));

                cmds = new XElement("Commands");
                cmd = null;
                //1.打开GDB
                cmd = new XElement("SMGI.Plugin.BaseFunction.GDBWorkspaceOpenCmd");
                cmd.SetValue(gdb);
                cmds.Add(cmd);
                if (frm.AddFigureMapCmdXJ.Checked)
                {
                    //2.位置图
                    cmd = frm.AddFigureMapCmdEle;
                    cmds.Add(cmd);
                }
                //3.地图输出
                cmd = new XElement("SMGI.Plugin.EmergencyMap.DataMapExportCmd");
                cmd.Add(new XElement("Height", 770));
                cmd.Add(new XElement("Width", 1070));
                cmd.Add(new XElement("FileName", frm.m_FileName));
                cmd.Add(new XElement("Resolution", 300));
                cmds.Add(cmd);

                root.Add(cmds);
                doc.Add(root);
                pid = -1;
                tempxml = GetAppDataPath() + "\\autoStep5.xml";
                if (File.Exists(tempxml))
                {
                    File.Delete(tempxml);
                }
                doc.Save(tempxml);
                using( p = new Process())
                {
                    si = new ProcessStartInfo();
                    si.FileName = GApplication.ExePath + "\\" + "SMGI.Shell.exe";
                    si.Arguments = string.Format("\"{0}\"", tempxml);
                    si.UseShellExecute = false;
                    si.CreateNoWindow = true;
                    p.StartInfo = si;
                    p.Start();
                    pid = p.Id;
                }
                WaitForTimerCheck(pid);
                
                #endregion



            }
            catch
            {
                wo.Dispose();
            }
            wo.Dispose();
            if (MessageBox.Show(string.Format("输出完成：{0}\n是否打开该文件？", frm.m_FileName), "完成", MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes)
            {
                System.Diagnostics.Process.Start(frm.m_FileName);
            }
        }
        public void CreateANNO(object sender, EventArgs e)
        {
        }
        private string GetStringByColor(IColor color)
        {
            ICmykColor cmykColor = new CmykColorClass { CMYK = color.CMYK };
            string cmykString = string.Empty;
            if (cmykColor.Cyan != 0)
                cmykString += "C" + cmykColor.Cyan.ToString();
            if (cmykColor.Magenta != 0)
                cmykString += "M" + cmykColor.Magenta.ToString();
            if (cmykColor.Yellow != 0)
                cmykString += "Y" + cmykColor.Yellow.ToString();
            if (cmykColor.Black != 0)
                cmykString += "K" + cmykColor.Black.ToString();
            return cmykString;
        }

    }
}
