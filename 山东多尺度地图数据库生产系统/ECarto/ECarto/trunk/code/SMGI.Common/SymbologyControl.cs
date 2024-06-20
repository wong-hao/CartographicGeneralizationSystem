using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using SMGI.Common;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.DisplayUI;
using ESRI.ArcGIS.CartoUI;

namespace SMGI.Common
{
    public partial class SymbologyControl : UserControl
    {
        private static string IMAGECOLUMNIDX = "ColumnSymbol";

        private GApplication application;
        private Size previewSize;
        StyleKnowledgeConfig SKC;
        string currentType = "点";

        public SymbologyControl()
            : base()
        {
            InitializeComponent();
        }
        public void InitializeWithGApplication(GApplication app)
        {
            
            application = app;
            previewSize = pictureBox_Preview.Size;
            comboBox1.Items.AddRange(new string[] { "点", "线", "面" });
            comboBox1.SelectedIndex = 0;
            currentType = comboBox1.SelectedItem.ToString();
        }

        public override void Refresh()
        {
            base.Refresh();
            ShowStyleGallery();
        }

        public void InitStyleGallery()
        {
            if (application == null) { return; }
            ShowStyleGallery();
        }

        private void ShowStyleGallery()
        {
            if (application == null) return;
            SKC = application.StyleKnowledgeBases.ActiveStyleConfig;
            if (SKC == null) { return; }
            ShowInfo(SKC);

            FillDGVUsingItems(SKC.SymbolsBase.ToArray(), currentType);
        }

        private void ShowInfo(StyleKnowledgeConfig skc)
        {
            if (skc == null) { return; }
            label_name.Text = "符号库名称: " + skc.StyleKnowledgeName;
            labelField.Text = "依附字段: " + skc.RenderField;
            textBox_filePath.Text = skc.StyleFile;
        }


        private IStyleGalleryItem getItemForSymbolBase(SymbolKnowledgeItem it)
        {
            IStyleGalleryItem item = null;
            try
            {
                if (it.StyleItem != null)
                {
                    item = it.StyleItem;
                    throw new Exception();
                }

                if (it["GeoType"] == null || it["StyleID"] == null) { throw new Exception(); }
                string gtype = it["GeoType"].ToString();
                string symStr = string.Empty;
                if (gtype.EndsWith("点"))
                {
                    symStr = SymbolClassString.点符号;
                }
                else if (gtype.EndsWith("线"))
                {
                    symStr = SymbolClassString.线符号;
                }
                else if (gtype.EndsWith("面"))
                {
                    symStr = SymbolClassString.面符号;
                }
                else
                {

                }
                if (symStr == string.Empty) { throw new Exception(); }

                item = application.StyleMgr.getStyleItemFromCache(
                    SKC.StyleFile, symStr, it["StyleID"].ToString());
                it.StyleItem = item;
            }
            catch
            { }
            return item;
        }


        private void FillDGVUsingItems(SymbolKnowledgeItem[] items, string styleType)
        {

            if (items != null && items.Length > 0)
            {
                try
                {
                    dataGridView1.Rows.Clear();

                    foreach (var item in items)
                    {
                        try
                        {
                            if (item != null && item["GeoType"].ToString().EndsWith(styleType))
                            {
                                AddANewRow(item);
                            }
                        }
                        catch
                        {
                            continue;
                        }
                    }
                }
                catch
                {

                }
            }
        }
        private void AddANewRow(SymbolKnowledgeItem item)
        {
            Image img = null;
            if (item != null)
            {
                IStyleGalleryItem symit = getItemForSymbolBase(item);

                if (symit != null && symit.Item != null && symit.Item is ISymbol)
                {
                    img = SymbolForm.ImageFromSymbol(symit.Item as ISymbol, 50, 28, 2);
                }
                else
                {
                    img = null;
                }

                int i = dataGridView1.Rows.Add(new object[] { item["StyleID"], img, item["GB"], item["GBName"] });
                dataGridView1.Rows[i].Tag = item;
                dataGridView1.Rows[i].Height = 30;
            }
        }



        private void pictureBox_Preview_Click(object sender, EventArgs e)
        {

        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if ((sender as ComboBox).SelectedIndex > -1)
            {
                currentType = (sender as ComboBox).SelectedItem.ToString();
                Refresh();
                dataGridView1.Focus();
            }
        }

        private void buttonNew_Click(object sender, EventArgs e)
        {
            ISymbol sym = null;
            string symType = string.Empty;
            if (currentType == "点")
            {
                sym = RenderFeatureLayer.createDefaultSimpleSymbol(ESRI.ArcGIS.Geometry.esriGeometryType.esriGeometryPoint);
            }
            else if (currentType == "线")
            {
                sym = RenderFeatureLayer.createDefaultSimpleSymbol(ESRI.ArcGIS.Geometry.esriGeometryType.esriGeometryPolyline);
            }
            else if (currentType == "面")
            {
                sym = RenderFeatureLayer.createDefaultSimpleSymbol(ESRI.ArcGIS.Geometry.esriGeometryType.esriGeometryPolygon);
            }
            if (sym != null)
            {
                SymbolEditorClass SEC = new SymbolEditorClass();
                if (SEC.EditSymbol(ref sym, application.MapControl.hWnd))
                {
                    IStyleGalleryItem SGI = new ServerStyleGalleryItemClass();
                    SGI.Item = sym;
                    SGI.Name = "Sym" + DateTime.Now.ToString("yyyyMMddHHmmss");
                    SymbolKnowledgeItem ski = new SymbolKnowledgeItem(SGI);

                    //三者应该有联动更新
                    application.StyleMgr.AddStyleItem(SKC.StyleFile, application.StyleMgr.StyleGallery, SGI);
                    SKC.SymbolsBase.Add(ski);
                    AddANewRow(ski);
                }
            }

        }

        private void buttonSave_Click(object sender, EventArgs e)
        {
            application.StyleKnowledgeBases.Save();
        }

        private void buttonEdit_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count > 0)
            {
                DataGridViewRow dr = dataGridView1.SelectedRows[0];
                if (dr.Tag != null && dr.Tag is SymbolKnowledgeItem)
                {
                    SymbolEditorClass SEC = new SymbolEditorClass();
                    ISymbol sym = (ISymbol)(dr.Tag as SymbolKnowledgeItem).StyleItem.Item;
                    if (sym == null) return;
                    if (SEC.EditSymbol(ref sym, application.MapControl.hWnd))
                    {
                        (dr.Tag as SymbolKnowledgeItem).StyleItem.Item = sym;
                        application.StyleMgr.UpdateStyleItem(SKC.StyleFile, application.StyleMgr.StyleGallery, (dr.Tag as SymbolKnowledgeItem).StyleItem);
                        dr.Cells[IMAGECOLUMNIDX].Value = SymbolForm.ImageFromSymbol(sym, 50, 28, 2);
                    }
                }
            }
        }

        private void buttonDelete_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count > 0)
            {
                DataGridViewRow dr = dataGridView1.SelectedRows[0];
                if (dr.Tag != null && dr.Tag is SymbolKnowledgeItem)
                {
                    SKC.SymbolsBase.Remove(dr.Tag as SymbolKnowledgeItem);
                    application.StyleMgr.DeleteStyleItem(SKC.StyleFile, application.StyleMgr.StyleGallery, (dr.Tag as SymbolKnowledgeItem).StyleItem);
                    dataGridView1.Rows.Remove(dr);
                }
            }

        }

        private void dataGridView1_SelectionChanged(object sender, EventArgs e)
        {
            DataGridView dgv = sender as DataGridView;
            DataGridViewRow r = null;
            if (dgv.SelectedRows.Count < 1) { return; }
            if ((r = dgv.SelectedRows[0]) != null)
            {
                if (r.Tag != null && r.Tag is SymbolKnowledgeItem)
                {
                    IStyleGalleryItem SGI = getItemForSymbolBase(r.Tag as SymbolKnowledgeItem);
                    if (SGI != null && SGI.Item is ISymbol)
                    {
                        pictureBox_Preview.Image =
                            SymbolForm.ImageFromSymbol(SGI.Item as ISymbol,
                            previewSize.Width - 3,
                            previewSize.Height - 3, 3, 4);
                    }
                }
            }
        }

        private void dataGridView1_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            DataGridView dgv = sender as DataGridView;
            DataGridViewRow r = null;
            if ((r = dgv.Rows[e.RowIndex]) != null)
            {
                if (r.Tag != null && r.Tag is SymbolKnowledgeItem)
                {
                    (r.Tag as SymbolKnowledgeItem)["GB"] = r.Cells[2].Value;
                    (r.Tag as SymbolKnowledgeItem)["GBName"] = r.Cells[3].Value;
                    application.StyleKnowledgeBases.Save();
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {

            IPictureMarkerSymbol pms = new PictureMarkerSymbolClass();
            MarkerPictureSymbolDailog fmps = new MarkerPictureSymbolDailog();
            fmps.ShowDialog();
            string extension=System.IO.Path.GetExtension(fmps.picFilePath);
            extension = extension.ToLower();
            esriIPictureType picturetype = esriIPictureType.esriIPictureBitmap;
            if (extension == "bmp")
            {
                picturetype = esriIPictureType.esriIPictureBitmap;
            }
            else if(extension=="jpg")
            {
                picturetype = esriIPictureType.esriIPictureJPG;
            }
            else if (extension == "png")
            {
                picturetype = esriIPictureType.esriIPicturePNG;
            }
            else if(extension=="emf")
            {
                picturetype = esriIPictureType.esriIPictureEMF;
            }
            pms.CreateMarkerSymbolFromFile(picturetype, fmps.picFilePath);
            pms.Size = fmps.symbolSize;
            (pms as ISymbolRotation).RotateWithTransform = true;

            if (pms != null)
            {
                SymbolEditorClass SEC = new SymbolEditorClass();
               
                    IStyleGalleryItem SGI = new ServerStyleGalleryItemClass();
                    SGI.Item = pms;
                    SGI.Name = "Sym" + DateTime.Now.ToString("yyyyMMddHHmmss");
                    SymbolKnowledgeItem ski = new SymbolKnowledgeItem(SGI);

                    //三者应该有联动更新
                    application.StyleMgr.AddStyleItem(SKC.StyleFile, application.StyleMgr.StyleGallery, SGI);
                    SKC.SymbolsBase.Add(ski);
                    AddANewRow(ski);
                
            }
         
        }
    }
}
