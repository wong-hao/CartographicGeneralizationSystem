using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using SMGI.Common;
using ESRI.ArcGIS.Geodatabase;
using System.Runtime.InteropServices;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geometry;
using SMGI.Common;
using ESRI.ArcGIS.esriSystem;
using SMGI.Plugin.EmergencyMap.Common;
namespace SMGI.Plugin.EmergencyMap
{
    public partial class FrmLbExport : Form
    {
        public FrmLbExport()
        {
            InitializeComponent();
        }
        IGraphicsContainer gc = null;
        IActiveView act = null;
        string fieldName=string.Empty;
        public  void RegisterVersion(IWorkspace ws)
        {
            IEnumDataset enumDataset = ws.get_Datasets(esriDatasetType.esriDTAny);
            enumDataset.Reset();
            IDataset dataset = enumDataset.Next();
            while (dataset != null)
            {
                if (dataset is IFeatureDataset)
                {
                    if (!(dataset as IVersionedObject3).IsRegisteredAsVersioned)
                    {
                        try
                        {
                            (dataset as IVersionedObject3).RegisterAsVersioned3(true);
                        }
                        catch (Exception ex)
                        {
                            if ((ex is COMException) && (ex as COMException).ErrorCode == -2147220989)
                            {
                                //针对该种情况进行特殊处理：原要素集在注册版本时没有选择moveEditsToBase项，
                                //而后在编辑过程中新增或替换了数据集中某一要素类，这样将会导致整个数据集未完全注册,
                                //若此时注册版本，并选择moveEditsToBase将会导致com组件错误【该实现不支持此操作。 [此对象的状态或所包含的对象不允许将编辑内容移动到基表中。]】
                                //这里尝试采取的策略是：先注册版本（不选择moveEditsToBase），然后再取消注册（选择moveEditsToBase，以保存先前的编辑成果到基表），最后再注册版本（选择moveEditsToBase）

                                (dataset as IVersionedObject).RegisterAsVersioned(true);//注册版本

                                (dataset as IVersionedObject3).UnRegisterAsVersioned3(true);//反注册版本

                                (dataset as IVersionedObject3).RegisterAsVersioned3(true);//注册版本

                            }
                            else
                            {
                                throw ex;
                            }
                        }
                    }
                    else
                    {
                        (dataset as IVersionedObject3).UnRegisterAsVersioned3(true);

                        (dataset as IVersionedObject3).RegisterAsVersioned3(true);
                    }
                }

                if (dataset is IFeatureClass)
                {
                    if (!(dataset as IVersionedObject3).IsRegisteredAsVersioned)
                    {
                        try
                        {
                            (dataset as IVersionedObject3).RegisterAsVersioned3(true);
                        }
                        catch (Exception ex)
                        {
                            if ((ex is COMException) && (ex as COMException).ErrorCode == -2147220989)
                            {
                                (dataset as IVersionedObject).RegisterAsVersioned(true);//注册版本

                                (dataset as IVersionedObject3).UnRegisterAsVersioned3(true);//反注册版本

                                (dataset as IVersionedObject3).RegisterAsVersioned3(true);//注册版本
                            }
                            else
                            {
                                throw ex;
                            }
                        }
                    }
                    else
                    {
                        (dataset as IVersionedObject3).UnRegisterAsVersioned3(true);

                        (dataset as IVersionedObject3).RegisterAsVersioned3(true);
                    }
                }

                if (dataset is ITable)
                {
                    if (!(dataset as IVersionedObject3).IsRegisteredAsVersioned)
                    {
                        try
                        {
                            (dataset as IVersionedObject3).RegisterAsVersioned3(true);
                        }
                        catch (Exception ex)
                        {
                            if ((ex is COMException) && (ex as COMException).ErrorCode == -2147220989)
                            {
                                (dataset as IVersionedObject).RegisterAsVersioned(true);//注册版本

                                (dataset as IVersionedObject3).UnRegisterAsVersioned3(true);//反注册版本

                                (dataset as IVersionedObject3).RegisterAsVersioned3(true);//注册版本
                            }
                            else
                            {
                                throw ex;
                            }
                        }
                    }
                    else
                    {
                        try
                        {
                            (dataset as IVersionedObject3).UnRegisterAsVersioned3(true);
                        }
                        catch (Exception ex)
                        {
                            if (ex.Message.Contains("Table is not multiversion, but must be for this operation"))//表未进行多版本化，但此操作要求必须采用多版本
                            {
                                (dataset as IVersionedObject).RegisterAsVersioned(false);
                            }
                            else
                            {
                                throw ex;
                            }
                            
                        }

                        (dataset as IVersionedObject3).RegisterAsVersioned3(true);
                    }
                }
                dataset = enumDataset.Next();
            }
            System.Runtime.InteropServices.Marshal.ReleaseComObject(enumDataset);
        }
        private void ExportSDE()
        {
            if (cmbDb.SelectedItem == null || cmblyr.SelectedItem == null || cmbField.SelectedItem == null)
            {
                MessageBox.Show("请设置服务参数！");
                return;
            }

            fieldName = this.cmbField.SelectedItem.ToString();
            var dbInfo = thematicList.Where(t => t.Name == cmbDb.SelectedItem.ToString()).FirstOrDefault();
            IWorkspace ws = GApplication.Application.GetWorkspacWithSDEConnection(dbInfo.IP, dbInfo.UserName, dbInfo.Password, dbInfo.DataBase);
            RegisterVersion(ws);
            IFeatureWorkspace sdeFeatureWorkspace = ws as IFeatureWorkspace;
            var sdeEditor = ws as IWorkspaceEdit;
            sdeEditor.StartEditing(true);
            sdeEditor.StartEditOperation();
            try
            {

                var fc = sdeFeatureWorkspace.OpenFeatureClass(dbInfo.DataBase + ".sde." + this.cmblyr.SelectedItem.ToString());
                var pload = fc as IFeatureClassLoad;
                pload.LoadOnlyMode = true;

                IFeatureCursor fCursor = fc.Insert(true);
                IFeatureBuffer fb = fc.CreateFeatureBuffer();
                act = GApplication.Application.ActiveView;
                gc = act as IGraphicsContainer;
                gc.Reset();
                IElement ele = null;
                while ((ele = gc.Next()) != null)
                {
                    if ((ele is IGroupElement))
                    {
                        var groupEle = ele as IGroupElement;
                        if ((groupEle as IElementProperties).Type == LabelType.ConnectLine.ToString())
                        {
                            IPoint anchor = new PointClass();
                            string txtName = string.Empty;
                            for (int i = 0; i < groupEle.ElementCount; i++)
                            {
                                #region
                                IElement ee = groupEle.get_Element(i);
                                switch ((ee as IElementProperties).Name)
                                {
                                    case "锚点":
                                        var anchorGeo = (ee.Geometry as IClone).Clone() as IGeometry;
                                        if (anchorGeo is IPolygon)
                                        {
                                            anchor = (anchorGeo as IArea).Centroid as IPoint;
                                        }
                                        else if (anchorGeo is IPolyline)
                                        {
                                            var gcs = anchorGeo as IGeometryCollection;
                                            var pcs = anchorGeo as IPointCollection;
                                            if (pcs.PointCount != 2)
                                            {
                                                anchor = pcs.get_Point(1);
                                            }
                                            else
                                            {
                                                (anchorGeo as IPolyline).QueryPoint(esriSegmentExtension.esriNoExtension, 0.5, true, anchor);
                                            }

                                        }
                                        break;
                                    case "文本":
                                        var txtElement = ee as ITextElement;
                                        txtName = txtElement.Text;
                                        break;
                                    default:
                                        break;

                                }

                                #endregion
                            }
                            if(txtName!=""&&!anchor.IsEmpty)
                            {
                                anchor.SpatialReference = GApplication.Application.MapControl.SpatialReference;
                                anchor.Project((fc as IGeoDataset).SpatialReference);
                                fb.set_Value(fc.FindField(fieldName), txtName);
                                fb.Shape = anchor;
                                fCursor.InsertFeature(fb as IFeatureBuffer);
                            }
                        }
                    }
                    else
                    {
                        if ((ele as IElementProperties).Type == LabelType.BallCallout.ToString())
                        {
                            IFormattedTextSymbol pTextSymbol = (ele as ITextElement).Symbol as IFormattedTextSymbol;
                            IBalloonCallout pBllCallout = pTextSymbol.Background as IBalloonCallout;
                            IPoint anchor = pBllCallout.AnchorPoint;
                            string txtName = (ele as ITextElement).Text;
                            if (txtName != "")
                            {

                                anchor.SpatialReference = GApplication.Application.MapControl.SpatialReference;
                                anchor.Project((fc as IGeoDataset).SpatialReference);
                                fb.set_Value(fc.FindField(fieldName), txtName);
                                fb.Shape = anchor;
                                fCursor.InsertFeature(fb as IFeatureBuffer);
                            }
                        }
                    }


                }

                fCursor.Flush();
                System.Runtime.InteropServices.Marshal.ReleaseComObject(fCursor);
                pload.LoadOnlyMode = false;
                sdeEditor.StopEditOperation();
                sdeEditor.StopEditing(true);
                MessageBox.Show("处理完成！");
            }
            catch(Exception ex)
            {
                MessageBox.Show("处理异常："+ex.Message);
            }
        }
        private void ExportLocal()
        {
          try
          {
            if (this.txtShp.Text.IndexOf(".shp")==-1)
            {
                MessageBox.Show("请选择保存文件!");
                return;
            }
            ShapeFileWriter resultFile = null;
            if (resultFile == null)
            {
                //建立结果文件
                resultFile = new ShapeFileWriter();
                Dictionary<string, int> fieldName2Len = new Dictionary<string, int>();
                fieldName2Len.Add("信息", 50);
                resultFile.createErrorResutSHPFile(this.txtShp.Text,GApplication.Application.ActiveView.FocusMap.SpatialReference, esriGeometryType.esriGeometryPoint, fieldName2Len);
            }
            #region
            act = GApplication.Application.ActiveView;
            gc = act as IGraphicsContainer;
            gc.Reset();
            IElement ele = null;
            while ((ele = gc.Next()) != null)
            {
                if ((ele is IGroupElement))
                {
                    var groupEle = ele as IGroupElement;
                    if ((groupEle as IElementProperties).Type == LabelType.ConnectLine.ToString())
                    {
                        IPoint anchor = new PointClass();
                        string txtName = string.Empty;
                        for (int i = 0; i < groupEle.ElementCount; i++)
                        {
                            #region
                            IElement ee = groupEle.get_Element(i);
                            switch ((ee as IElementProperties).Name)
                            {
                                case "锚点":
                                    var anchorGeo = (ee.Geometry as IClone).Clone() as IGeometry;
                                    if (anchorGeo is IPolygon)
                                    {
                                        anchor = (anchorGeo as IArea).Centroid as IPoint;
                                    }
                                    else if (anchorGeo is IPolyline)
                                    {
                                        var gcs = anchorGeo as IGeometryCollection;
                                        var pcs = anchorGeo as IPointCollection;
                                        if (pcs.PointCount != 2)
                                        {
                                            anchor = pcs.get_Point(1);
                                        }
                                        else
                                        {
                                            (anchorGeo as IPolyline).QueryPoint(esriSegmentExtension.esriNoExtension, 0.5, true, anchor);
                                        }

                                    }
                                    break;
                                case "文本":
                                    var txtElement = ee as ITextElement;
                                    txtName = txtElement.Text;
                                    break;
                                default:
                                    break;

                            }

                            #endregion
                        }
                        if(txtName!=""&&!anchor.IsEmpty)
                        {
                                Dictionary<string, string> fieldName2FieldValue = new Dictionary<string, string>();
                                fieldName2FieldValue.Add("信息", txtName);
                                resultFile.addErrorGeometry(anchor, fieldName2FieldValue);
                                
                        }
                    }
                }
                else
                {
                    if ((ele as IElementProperties).Type == LabelType.BallCallout.ToString())
                    {
                        IFormattedTextSymbol pTextSymbol = (ele as ITextElement).Symbol as IFormattedTextSymbol;
                        IBalloonCallout pBllCallout = pTextSymbol.Background as IBalloonCallout;
                        IPoint anchor = pBllCallout.AnchorPoint;
                        string txtName = (ele as ITextElement).Text;
                        if (txtName != "")
                        {
                                Dictionary<string, string> fieldName2FieldValue = new Dictionary<string, string>();
                                fieldName2FieldValue.Add("信息", txtName);
                                resultFile.addErrorGeometry(anchor, fieldName2FieldValue);
                        }
                    }
                }


            }
            #endregion
            //保存结果文件
            if (resultFile != null)
            {
                resultFile.saveErrorResutSHPFile();
            }
            MessageBox.Show("处理完成！");
           }
           catch(Exception ex)
           {
                MessageBox.Show("处理异常："+ex.Message);
           }
            
        }
        private void btOk_Click(object sender, EventArgs e)
        {
            if (!rbLocal.Checked)
            {
                ExportSDE();
            }
            else
            {
                ExportLocal();
            }
             
        }

        
        void btClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        List<ThematicDataInfo> thematicList=null;
        private void FrmLbExport_Load(object sender, EventArgs e)
        { 
            //获取专题数据库信
            //thematicList = ThematicDataClass.GetThemticElement(GApplication.Application);
            //foreach(var t in thematicList)
            //{
            //    this.cmbDb.Items.Add(t.Name);
            //    this.cmbDb.SelectedIndex=0;
            //}
            tabControl1.SelectedIndex = 0;
        }

        private void cmbDb_SelectedIndexChanged(object sender, EventArgs e)
        {
            var list = thematicList.Where(t => t.Name == cmbDb.SelectedItem.ToString()).FirstOrDefault();
            if (list != null)
                this.cmblyr.Items.Clear();
            foreach(var lyr in list.LyrsType)
            {
                if(lyr.Value=="点")
                {
                    this.cmblyr.Items.Add(lyr.Key);
                    this.cmblyr.SelectedIndex=0;
                }
            }
        }

        private void cmblyr_SelectedIndexChanged(object sender, EventArgs e)
        {
            var list = thematicList.Where(t => t.Name == cmbDb.SelectedItem.ToString()).FirstOrDefault();
            if (list != null)
                this.cmbField.Items.Clear();
            var fields = list.LyrsFields[this.cmblyr.SelectedItem.ToString()];
            foreach (var kv in fields)
            {
                this.cmbField.Items.Add(kv);
                this.cmbField.SelectedIndex = 0;
            }
            if (cmbField.Items.Count > 0)
            {
                this.cmbField.SelectedIndex = cmbField.Items.IndexOf("name");
            }

        }

        private void rbLocal_CheckedChanged(object sender, EventArgs e)
        {
            if ((sender as RadioButton).Checked)
            {
                tabControl1.SelectedIndex = 0;
            }
        }

        private void rbSDE_CheckedChanged(object sender, EventArgs e)
        {
            if ((sender as RadioButton).Checked)
            {
                tabControl1.SelectedIndex = 1;
            }
        }

        private void btView_Click(object sender, EventArgs e)
        {
            SaveFileDialog sf = new SaveFileDialog();
            sf.Filter = "保存文件|*.shp";
            sf.FileName = "标注结果";
            sf.RestoreDirectory = true;
            DialogResult dr = sf.ShowDialog();
            if (dr != DialogResult.OK)
                return;
            this.txtShp.Text = sf.FileName;
        }
    }
}
