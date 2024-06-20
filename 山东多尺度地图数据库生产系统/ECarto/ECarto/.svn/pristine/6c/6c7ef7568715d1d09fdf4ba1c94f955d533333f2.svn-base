using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Controls;
using ESRI.ArcGIS.DisplayUI;
using System.IO;
using ESRI.ArcGIS.Editor;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Geometry;

namespace SMGI.Common
{
    public partial class SymboloryForm : Form
    {
        private IStyleGalleryItem3 pStyleGalleryItem;
        private IStyleGalleryItem3 currentStyleGalleryItem;
        //private ILegendClass pLegendClass;
        ISymbologyStyleClass pSSC;
        //private ILayer pLayer;
        public ISymbol pSymbol;
        public ISymbol pSymboltemp;
        public Image pSymbolImage;
        private AxMapControl _control;
        IList<IStyleGalleryItem3> pList;
        IList<IStyleGalleryItem3> pListstart;
        IList<string> pListstr;
        IList<string> pListstringEnd;
        IList<string> pListCato;
        IList<string> pListName;
        GApplication gapp;
        esriSymbologyStyleClass es;
        string ClassName;
        bool save = false;
        SymbolInfo pSymbolInfo;
        IList<SymbolInfo> pSymbolInfoStr;
        //菜单是否已经初始化标志
       // bool contextMenuMoreSymbolInitiated = false;
        public SymboloryForm(ISymbol symbol,GApplication app)
        {
            InitializeComponent();
            pSymboltemp = (symbol as IClone).Clone() as ISymbol;
            gapp = app;
            _control = app.MapControl;
            //_control.Map.Layers
          //  _control = pAxMapControl;
        }
        /// <summary>
        /// 初始化SymbologyControl的StyleClass,图层如果已有符号,则把符号添加到SymbologyControl中的第一个符号,并选中
        /// </summary>
        /// <param name="symbologyStyleClass"></param>
        private void SetFeatureClassStyle(esriSymbologyStyleClass symbologyStyleClass, string name, string Category,ISymbol Symbol )
        {                     
            this.axSymbologyControl1.StyleClass = symbologyStyleClass;
            ISymbologyStyleClass pSymbologyStyleClass = this.axSymbologyControl1.GetStyleClass(symbologyStyleClass);
                  
            currentStyleGalleryItem = new ServerStyleGalleryItemClass();
                // currentStyleGalleryItem = new ServerStyleGalleryItem();
                currentStyleGalleryItem.Name = name;
                currentStyleGalleryItem.Item = Symbol;
                currentStyleGalleryItem.Category = Category;
                currentStyleGalleryItem.Tags =new Guid().ToString();
               // pSymbolInfo = new SymbolInfo(currentStyleGalleryItem);                        
                pSymbologyStyleClass.AddItem(currentStyleGalleryItem, 0);
                pList.Add(currentStyleGalleryItem);
                if (!pListCato.Contains(Category))
                {
                    pListCato.Add(Category);
                    comboBox1.Items.Add(Category);
                }
                if (!pListName.Contains(name))
                {
                    pListName.Add(name);
                }                               

            pSymbologyStyleClass.SelectItem(0);
            this.axSymbologyControl1.Refresh();
        }
        /// <summary>
        /// 第一次加载面板，把图层符号显示在picture中
        /// </summary>
        /// <param name="symbologyStyleClass"></param>
        /// <param name="name"></param>
        /// <param name="Category"></param>
        public void InitPicture(esriSymbologyStyleClass symbologyStyleClass, string name, string Category)
        {
            this.axSymbologyControl1.StyleClass = symbologyStyleClass;
            ISymbologyStyleClass pSymbologyStyleClass = this.axSymbologyControl1.GetStyleClass(symbologyStyleClass);


            currentStyleGalleryItem = new ServerStyleGalleryItemClass();
                currentStyleGalleryItem.Name = name;
                currentStyleGalleryItem.Item = pSymboltemp;
                currentStyleGalleryItem.Category = Category;
               
            this.pSymbol = (ISymbol)currentStyleGalleryItem.Item;
          
            System.Drawing.Image image = System.Drawing.Image.FromHbitmap(new System.IntPtr(this.axSymbologyControl1.GetStyleClass(this.axSymbologyControl1.StyleClass).PreviewItem(currentStyleGalleryItem, this.pictureBox1.Width, this.pictureBox1.Height).Handle));
            this.pictureBox1.Image = image;
            this.textBox2.Text = name;
            this.textBox3.Text = Category;
            
        }
        /// <summary>
        /// 从数据库中读出符号
        /// </summary>
        /// <param name="symbolname"></param>
        private void GetStyleFromGDB(esriSymbologyStyleClass symbologyStyleClass,string symbolname)
        {
            //this.axSymbologyControl1.StyleClass = symbologyStyleClass;
            //ISymbologyStyleClass pSymbologyStyleClass = this.axSymbologyControl1.GetStyleClass(symbologyStyleClass);
            int inde = -1;
            int i=0;
            this.axSymbologyControl1.StyleClass = symbologyStyleClass;
            ISymbologyStyleClass pSymbologyStyleClass = this.axSymbologyControl1.GetStyleClass(symbologyStyleClass);
            pListstr = gapp.AppConfig[this.GetType().ToString() + symbolname] as List<string>;
            SymbolInfo pysm;
           pList = new List<IStyleGalleryItem3>();
           pListName = new List<string >();
            pListCato= new List<string >();
            if (pListstr != null)
            {
                foreach (string s in pListstr)
                {
                    i++;
                    pysm = GConvert.Base64ToObject(s) as SymbolInfo;
                    if (pysm == null)
                        continue;
                    if (pysm.Symbol == pSymboltemp)
                    {
                        inde = i - 1;
                    }
                    pList.Add(pysm.SymbolGalleryItem);
                    if (!pListCato.Contains(pysm.Category))
                    {
                        pListCato.Add(pysm.Category);
                    }
                    if (!pListName.Contains(pysm.Name))
                    {
                        pListName.Add(pysm.Name);
                    }
                    // IStyleGalleryItem sy = GConvert.Base64ToObject(s) as IStyleGalleryItem;
                    
                    pSymbologyStyleClass.AddItem(pysm.SymbolGalleryItem, i - 1);
                }
                foreach (string s in pListCato)
                {
                    comboBox1.Items.Add(s);
                }
            }
            else
            {
                pListstr = new List<string>();
            }
            pListstart = pList;
            if (inde != -1)
            {
                pSymbologyStyleClass.SelectItem(inde);
            }

          // comboBox1.Items = pListCato;
            //symbols = gapp.AppConfig[this.GetType().ToString() + "Symbols"] as List<string>;
        }
        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="symbologyStyleClass"></param>
        /// <param name="symbolname"></param>
        private void GetFeatureClassStyle(esriSymbologyStyleClass symbologyStyleClass,string symbolname)
        {
            this.axSymbologyControl1.StyleClass = symbologyStyleClass;
            ISymbologyStyleClass pSymbologyStyleClass = this.axSymbologyControl1.GetStyleClass(symbologyStyleClass);



            int count = pSymbologyStyleClass.ItemCount;
            List<string> symbols = new List<string>();
            for (int i = 0; i < count; i++)
            {
               // pList = new List<ISymbol>();
                IStyleGalleryItem sb = pSymbologyStyleClass.GetItem(i);
               // pList.Add(sb);
                symbols.Add(GConvert.ObjectToBase64(sb));
            }
           // gapp.EngineEditor
            gapp.AppConfig[this.GetType().ToString() + symbolname] = symbols;
            //symbols = gapp.AppConfig[this.GetType().ToString() + "Symbols"] as List<string>;
        }
        /// <summary>
        /// 移除符号
        /// </summary>
        /// <param name="symbologyStyleClass"></param>
        /// <param name="currentStyleGalleryItem"></param>
        private void  GetStyleIndex(esriSymbologyStyleClass symbologyStyleClass, IStyleGalleryItem3 StyleGalleryItem)
        {
            this.axSymbologyControl1.StyleClass = symbologyStyleClass;
            ISymbologyStyleClass pSymbologyStyleClass = this.axSymbologyControl1.GetStyleClass(symbologyStyleClass);


            if (StyleGalleryItem == null)
            {
                return;
            }
            int count = pSymbologyStyleClass.ItemCount;
           // List<string> symbols = new List<string>();
            for (int i = 0; i < count; i++)
            {
                if (pSymbologyStyleClass.GetItem(i).Equals(StyleGalleryItem))
                {
                    pSymbologyStyleClass.RemoveItem(i);
                    break;
                }
               
              //  pList = new List<ISymbol>();
               // ISymbol sb = (ISymbol)pSymbologyStyleClass.GetItem(i);
               // pList.Add(sb);
               // symbols.Add(GConvert.ObjectToBase64(sb));
            }
            pListCato.Add(StyleGalleryItem.Category);

            comboBox1.Items.Remove(StyleGalleryItem.Category);
            pListName.Add(StyleGalleryItem.Name);
                    
            pList.Remove(StyleGalleryItem);
            //gapp.AppConfig[this.GetType().ToString() + "Symbols"] = symbols;
            //symbols = gapp.AppConfig[this.GetType().ToString() + "Symbols"] as List<string>;
        }
        /// <summary>
        /// 把选中并设置好的符号在picturebox控件中预览
        /// </summary>
        private void PreviewImage(string name, string Category)
        {
            System.Drawing.Image image = System.Drawing.Image.FromHbitmap(new System.IntPtr(this.axSymbologyControl1.GetStyleClass(this.axSymbologyControl1.StyleClass).PreviewItem(pStyleGalleryItem, this.pictureBox1.Width, this.pictureBox1.Height).Handle));
            this.pictureBox1.Image = image;
            this.textBox2.Text = name;
            this.textBox3.Text = Category;
        }
        private void SymboloryForm_Load(object sender, EventArgs e)
        {
            
            comboBox1.Items.Add("全部");
            if (pSymboltemp is IMarkerSymbol)
            {
                es = esriSymbologyStyleClass.esriStyleClassMarkerSymbols;
                ClassName = "Marker Symbols";
                this.GetStyleFromGDB(es, ".MarkerSymbols");
                InitPicture(es,"当前符号","点");
            }
            else if (pSymboltemp is ITextSymbol)
            {
                es = esriSymbologyStyleClass.esriStyleClassTextSymbols;
                ClassName = "Text Symbols";
                this.GetStyleFromGDB(es, ".TextSymbols");
                InitPicture(es, "当前符号", "注记");
            }
            else if (pSymboltemp is ILineSymbol)
            {
                es = esriSymbologyStyleClass.esriStyleClassLineSymbols;
                ClassName = "Line Symbols";
                this.GetStyleFromGDB(es, ".LineSymbols");
                 InitPicture(es, "当前符号", "线");
            }
            else if (pSymboltemp is IFillSymbol)
            {
                es = esriSymbologyStyleClass.esriStyleClassFillSymbols;
                ClassName = "Fill Symbols";
                this.GetStyleFromGDB(es, ".FillSymbols");
                InitPicture(es, "当前符号", "面");
                //       // this.SetFeatureClassStyle(es, "当前符号", "面");
            }

         
        }
        /// <summary>
        /// 将ArcGIS Engine中的IRgbColor接口转换至.NET中的Color结构
        /// </summary>
        /// <param name="pRgbColor">IRgbColor</param>
        /// <returns>.NET中的System.Drawing.Color结构表示ARGB颜色</returns>
        public Color ConvertIRgbColorToColor(IRgbColor pRgbColor)
        {
            return ColorTranslator.FromOle(pRgbColor.RGB);
        }
        public Color ConvertIColorToColor(IColor pRgbColor)
        {
            return ColorTranslator.FromOle(pRgbColor.RGB);
        }
        /// <summary>
        /// 将.NET中的Color结构转换至于ArcGIS Engine中的IColor接口
        /// </summary>
        /// <param name="color">.NET中的System.Drawing.Color结构表示ARGB颜色</param>
        /// <returns>IColor</returns>
        public IColor ConvertColorToIColor(Color color)
        {
            IColor pColor = new RgbColorClass();
            pColor.RGB = color.B * 65536 + color.G * 256 + color.R;
            return pColor;
        }
        private void axSymbologyControl1_OnItemSelected(object sender, ISymbologyControlEvents_OnItemSelectedEvent e)
        {
            save = true;
            pStyleGalleryItem = (IStyleGalleryItem3)e.styleGalleryItem;
           // pStyleGalleryItem.
            this.pSymbol = (ISymbol)pStyleGalleryItem.Item;
            Color color;
            switch (this.axSymbologyControl1.StyleClass)
            {
                //点符号
                case esriSymbologyStyleClass.esriStyleClassMarkerSymbols:
                    color = this.ConvertIColorToColor(((IMarkerSymbol)pStyleGalleryItem.Item).Color );
                    //设置点符号角度和大小初始值
                  //  this.nudAngle.Value = (decimal)((IMarkerSymbol)this.pStyleGalleryItem.Item).Angle;
                  //  this.nudSize.Value = (decimal)((IMarkerSymbol)this.pStyleGalleryItem.Item).Size;
                    break;
                //点符号
                case esriSymbologyStyleClass.esriStyleClassTextSymbols:
                    color = this.ConvertIColorToColor(((ITextSymbol)pStyleGalleryItem.Item).Color);
                    //设置点符号角度和大小初始值
                    //  this.nudAngle.Value = (decimal)((IMarkerSymbol)this.pStyleGalleryItem.Item).Angle;
                    //  this.nudSize.Value = (decimal)((IMarkerSymbol)this.pStyleGalleryItem.Item).Size;
                    break;
                //线符号
                case esriSymbologyStyleClass.esriStyleClassLineSymbols:
                    color = this.ConvertIColorToColor(((ILineSymbol)pStyleGalleryItem.Item).Color );
                    //设置线宽初始值
                  //  this.nudWidth.Value = (decimal)((ILineSymbol)this.pStyleGalleryItem.Item).Width;
                    break;
                //面符号
                case esriSymbologyStyleClass.esriStyleClassFillSymbols:

                    // color = ((IFillSymbol)pStyleGalleryItem.Item).Color.;
                    // IRgbColor rgb = c as IRgbColor;
                    color = this.ConvertIColorToColor(((IFillSymbol)pStyleGalleryItem.Item).Color);
                  //  this.btnOutlineColor.BackColor = this.ConvertIRgbColorToColor(((IFillSymbol)pStyleGalleryItem.Item).Outline.Color as IRgbColor);
                    //设置外框线宽度初始值
                   // this.nudWidth.Value = (decimal)((IFillSymbol)this.pStyleGalleryItem.Item).Outline.Width;
                    break;
                default:
                    color = Color.Black;
                    break;
            }
            //设置按钮背景色
           // this.button1.BackColor = color;
            //预览符号
            this.PreviewImage(pStyleGalleryItem.Name, pStyleGalleryItem.Category);
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            if (this.pSymbol == null)
            {
                MessageBox.Show("请选择要编辑的符号");
                return;
            }
            ISymbol sb = this.pSymbol;
            //ISymbolSelector ss = new SymbolSelectorClass();
            //// ss.
            //ss.AddSymbol(sb);
            //if (ss.SelectSymbol(0))
            //{
            //    sb = ss.GetSymbolAt(0);
            //}
            //IMarkerSymbol 
           //// irep
        
          //  ISymbolSelector ss = PM as ISymbolSelector;
            //IRepresentationGraphics pg = new RepresentationGraphicsClass();
           // rm.DoModal(0,pg);
            //rm.Marker;
            ISymbolEditor ISE = new SymbolEditorClass();
            ISE.EditSymbol(ref sb, 0);
            this.pSymbol = sb;
            if (pStyleGalleryItem == null)
            {
                pStyleGalleryItem = currentStyleGalleryItem;
            }
            pStyleGalleryItem.Item = sb;

            Color color;
            switch (this.axSymbologyControl1.StyleClass)
            {
                //点符号
                case esriSymbologyStyleClass.esriStyleClassMarkerSymbols:
                    color = this.ConvertIColorToColor(((IMarkerSymbol)sb).Color as IColor);
                    //设置点符号角度和大小初始值

                    break;
                case esriSymbologyStyleClass.esriStyleClassTextSymbols:
                    color = this.ConvertIColorToColor(((ITextSymbol)sb).Color as IColor);
                    //设置点符号角度和大小初始值

                    break;
                //线符号
                case esriSymbologyStyleClass.esriStyleClassLineSymbols:
                    color = this.ConvertIColorToColor(((ILineSymbol)sb).Color as IColor);
                    //设置线宽初始值

                    break;
                //面符号
                case esriSymbologyStyleClass.esriStyleClassFillSymbols:

                    // color = ((IFillSymbol)pStyleGalleryItem.Item).Color.;
                    // IRgbColor rgb = c as IRgbColor;
                    color = this.ConvertIColorToColor(((IFillSymbol)sb).Color as IColor);

                    break;
                default:
                    color = Color.Black;
                    break;
            }
            //设置按钮背景色
            // this.button1.BackColor = color;
            //预览符号
            //switch (this.axSymbologyControl1.StyleClass)
            //{
            //    //点符号
            //    case esriSymbologyStyleClass.esriStyleClassMarkerSymbols:
            //        ((IMarkerSymbol)this.pStyleGalleryItem.Item).Color = this.ConvertColorToIColor(color);
            //        break;
            //    //线符号
            //    case esriSymbologyStyleClass.esriStyleClassLineSymbols:
            //        ((ILineSymbol)this.pStyleGalleryItem.Item).Color = this.ConvertColorToIColor(color);
            //        break;
            //    //面符号
            //    case esriSymbologyStyleClass.esriStyleClassFillSymbols:
            //        ((IFillSymbol)this.pStyleGalleryItem.Item).Color = this.ConvertColorToIColor(color);
            //        break;
            //}
            this.PreviewImage(pStyleGalleryItem.Name, pStyleGalleryItem.Category);
            this.axSymbologyControl1.Refresh();
            //this.Refresh();
            // this.pSymbol=ISE.
        }
        /// <summary>
        /// 新建符号
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button3_Click(object sender, EventArgs e)
        {
            try
            {
                //
            SymboloryNameForm snf = new SymboloryNameForm(pListCato);
            if (snf.ShowDialog() == DialogResult.OK){
               
                string name = snf.name;
                string Category = snf.caot;
                if (name == "")
                {
                    MessageBox.Show("请输入符号名字");
                    return;
                }
                if (Category == "")
                {
                    MessageBox.Show("请输入符号类别");
                    return;
                }
                switch (this.axSymbologyControl1.StyleClass)
                {
                    case esriSymbologyStyleClass.esriStyleClassMarkerSymbols:
                        this.SetFeatureClassStyle(esriSymbologyStyleClass.esriStyleClassMarkerSymbols, name, Category, pSymboltemp);

                        break;
                    case esriSymbologyStyleClass.esriStyleClassLineSymbols:
                        this.SetFeatureClassStyle(esriSymbologyStyleClass.esriStyleClassLineSymbols, name, Category, pSymboltemp);

                        break;
                    case esriSymbologyStyleClass.esriStyleClassFillSymbols:
                        this.SetFeatureClassStyle(esriSymbologyStyleClass.esriStyleClassFillSymbols, name, Category, pSymboltemp);

                        break;
                    case esriSymbologyStyleClass.esriStyleClassTextSymbols:
                        this.SetFeatureClassStyle(esriSymbologyStyleClass.esriStyleClassTextSymbols, name, Category, pSymboltemp);

                        break;
                    default:
                        this.Close();
                        break;
                }
            }
            // GetStyleIndex(es, pStyleGalleryItem);
                saveItem();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            //snf.Show();
            //if (snf.ShowDialog() == DialogResult.OK)
            //{
            //   
            //}
            
        }
        /// <summary>
        /// 保存符号到面板
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button7_Click(object sender, EventArgs e)
        {
            try
            {

            this.axSymbologyControl1.StyleClass = es;
            ISymbologyStyleClass pSymbologyStyleClass = this.axSymbologyControl1.GetStyleClass(es);


            if (pStyleGalleryItem == null)
            {
                pStyleGalleryItem = currentStyleGalleryItem;
            }
            int count = pSymbologyStyleClass.ItemCount;
            for (int i = 0; i < count; i++)
            {
                ISymbol Symbol=pSymbologyStyleClass.GetItem(i).Item as ISymbol;
               if( Symbol.Equals(this.pSymbol))
               {
                   MessageBox.Show("当前符号已存在，请修改在保存");
                   return;
               }
                
            }
            if (pStyleGalleryItem.Equals(currentStyleGalleryItem))
            {
                SymboloryNameForm snf = new SymboloryNameForm(pListCato);
                if (snf.ShowDialog() == DialogResult.OK)
                {
                   // IGeoFeatureLayer pGeoFeatureLayer = (IGeoFeatureLayer)pLayer;
                    string name = snf.name;
                    string Category = snf.caot;
                    switch (this.axSymbologyControl1.StyleClass)
                    {
                        case esriSymbologyStyleClass.esriStyleClassMarkerSymbols:
                            this.SetFeatureClassStyle(esriSymbologyStyleClass.esriStyleClassMarkerSymbols, name, Category, this.pSymbol);
                            break;
                        case esriSymbologyStyleClass.esriStyleClassLineSymbols:
                            this.SetFeatureClassStyle(esriSymbologyStyleClass.esriStyleClassLineSymbols, name, Category, this.pSymbol);

                            break;
                        case esriSymbologyStyleClass.esriStyleClassFillSymbols:
                            this.SetFeatureClassStyle(esriSymbologyStyleClass.esriStyleClassFillSymbols, name, Category, this.pSymbol);

                            break;
                        case esriSymbologyStyleClass.esriStyleClassTextSymbols:
                            this.SetFeatureClassStyle(esriSymbologyStyleClass.esriStyleClassTextSymbols, name, Category, this.pSymbol);

                            break;
                        default:
                            this.Close();
                            break;
                    }
                }
            }
          else  if (save)
            {
                // List<string> symbols = new List<string>();
                for (int i = 0; i < count; i++)
                {
                    if (pSymbologyStyleClass.GetItem(i).Equals(pStyleGalleryItem))
                    {
                        pSymbologyStyleClass.GetItem(i).Item = this.pSymbol;
                    }

                    //  pList = new List<ISymbol>();
                    // ISymbol sb = (ISymbol)pSymbologyStyleClass.GetItem(i);
                    // pList.Add(sb);
                    // symbols.Add(GConvert.ObjectToBase64(sb));
                }
            }
            else
            {
                SymboloryNameForm snf = new SymboloryNameForm(pListCato);
                if (snf.ShowDialog() == DialogResult.OK)
                {
                       // IGeoFeatureLayer pGeoFeatureLayer = (IGeoFeatureLayer)pLayer;
                        string name = snf.name;
                        string Category = snf.caot;
                        switch (this.axSymbologyControl1.StyleClass)
                        {
                            case esriSymbologyStyleClass.esriStyleClassMarkerSymbols:
                                this.SetFeatureClassStyle(esriSymbologyStyleClass.esriStyleClassMarkerSymbols, name, Category, this.pSymbol);

                                break;
                            case esriSymbologyStyleClass.esriStyleClassLineSymbols:
                                this.SetFeatureClassStyle(esriSymbologyStyleClass.esriStyleClassLineSymbols, name, Category, this.pSymbol);

                                break;
                            case esriSymbologyStyleClass.esriStyleClassFillSymbols:
                                this.SetFeatureClassStyle(esriSymbologyStyleClass.esriStyleClassFillSymbols, name, Category, this.pSymbol);

                                break;
                            case esriSymbologyStyleClass.esriStyleClassTextSymbols:
                                this.SetFeatureClassStyle(esriSymbologyStyleClass.esriStyleClassTextSymbols, name, Category, this.pSymbol);

                                break;
                            default:
                                this.Close();
                                break;
                        
                    }
                }
            }
           
                // GetStyleIndex(es, pStyleGalleryItem);
                saveItem();
              
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            this.axSymbologyControl1.Refresh();
           // this.PreviewImage();
        }
        /// <summary>
        /// 导出符号
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button1_Click(object sender, EventArgs e)
        {
            saveFileDialog1.Filter = "符号文件(*.sbs)|*.sbs";
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                string fileName = saveFileDialog1.FileName;
                StreamWriter SW;
                if (File.Exists(fileName))
                {
                    MessageBox.Show("文件已存在");
                }
                else
                {
                    try
                    {
                        SW = File.CreateText(fileName);
                        this.axSymbologyControl1.StyleClass = es;
                        ISymbologyStyleClass pSymbologyStyleClass = this.axSymbologyControl1.GetStyleClass(es);

                        int count = pSymbologyStyleClass.ItemCount;
                        //  List<string> symbols = new List<string>();
                        for (int i = 0; i < count; i++)
                        {
                           // pList = new List<ISymbol>();
                            IStyleGalleryItem sb = (IStyleGalleryItem)pSymbologyStyleClass.GetItem(i);
                            // pList.Add(sb);
                            // symbols.Add(GConvert.ObjectToBase64(sb));
                            SW.WriteLine(GConvert.ObjectToBase64(sb));
                        }


                        SW.Close();
                        MessageBox.Show("导出成功");
                    }
                    catch(Exception ex)
                    {
                    }
                }
                //SaveFile(fileName);
            }        
           //this.axSymbologyControl1.
            //if (saveFileDialog1.ShowDialog()== DialogResult.OK)
            //{
            //   IStyleGalleryItem styleGalleryItem =
            //}
        }
        /// <summary>
        /// 加载符号
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button6_Click(object sender, EventArgs e)
        {
            openFileDialog1.Filter = "符号文件(*.sbs)|*.sbs|(*.mxd)|*.mxd|(*.ServerStyle)|*.ServerStyle|(*.style)|*.style";
            IStyleGalleryItem styleGalleryItem;
            IEnumStyleGalleryItem enumStyleGalleryItem = null;
            if (openFileDialog1.ShowDialog(this) == DialogResult.OK)
            {
                try
                {
                    this.axSymbologyControl1.StyleClass = es;
                    ISymbologyStyleClass pSymbologyStyleClass = this.axSymbologyControl1.GetStyleClass(es);
                    string name = openFileDialog1.FileName.Substring(openFileDialog1.FileName.LastIndexOf(".") + 1);
                    if (name == "sbs")
                    {
                        StreamReader sr = new StreamReader(openFileDialog1.FileName, Encoding.Default);
                        string s;
                        
                        StringBuilder ss = new StringBuilder();
                        while ((s = sr.ReadLine()) != null)
                        {
                            // ss.Append(s);
                            IStyleGalleryItem3 syy = GConvert.Base64ToObject(s.ToString()) as IStyleGalleryItem3;
                            pSymbologyStyleClass.AddItem(syy, 0);
                            pList.Add(syy);
                            if (!pListCato.Contains(syy.Category))
                            {
                                pListCato.Add(syy.Category);
                                comboBox1.Items.Add(syy.Category);
                            }
                            if (!pListName.Contains(syy.Name))
                            {
                                pListName.Add(syy.Name);
                            }


                        }

                        // GetStyleIndex(es, pStyleGalleryItem);
                        saveItem();
                        // IGeoFeatureLayer pGeoFeatureLayer = (IGeoFeatureLayer)pLayer;
                   }
                    else if (name == "ServerStyle")
                    {
                        IStyleGallery pStyleGaller = new ServerStyleGalleryClass();

                        IStyleGalleryStorage pStyleGalleryStorage = pStyleGaller as IStyleGalleryStorage;
                        //使用IStyleGalleryStorage接口的AddFile方法加载ServerStyle文件
                        IStyleGalleryItem pStyleGallerItem = null;
                        IEnumStyleGalleryItem pEnumSyleGalleryItem = null;
                        IStyleGalleryClass pStyleGalleryClass = null;
                        pStyleGalleryStorage.AddFile(openFileDialog1.FileName);

                        
                        for (int i = 0; i < pStyleGaller.ClassCount; i++)
                        {
                            pStyleGalleryClass = pStyleGaller.get_Class(i);

                            if (pStyleGalleryClass.Name != ClassName)
                                continue;

                            pEnumSyleGalleryItem = pStyleGaller.get_Items(ClassName, openFileDialog1.FileName, "");
                            pEnumSyleGalleryItem.Reset();

                            //遍历pEnumSyleGalleryItem
                            pStyleGallerItem = pEnumSyleGalleryItem.Next();
                            while (pStyleGallerItem != null)
                            {
                                //获取符号
                                IStyleGalleryItem3 sg = pStyleGallerItem as IStyleGalleryItem3;
                                pSymbologyStyleClass.AddItem(sg, 0);
                                pList.Add(sg);
                                if (!pListCato.Contains(sg.Category))
                                {
                                    pListCato.Add(sg.Category);
                                    comboBox1.Items.Add(sg.Category);
                                }
                                if (!pListName.Contains(sg.Name))
                                {
                                    pListName.Add(sg.Name);
                                }
                                   // ISymbol pSymbol = pStyleGallerItem.Item as ISymbol;                                                                                                                             

                                pStyleGallerItem = pEnumSyleGalleryItem.Next();

                            }
                            System.Runtime.InteropServices.Marshal.ReleaseComObject(pEnumSyleGalleryItem);

                            System.Runtime.InteropServices.Marshal.ReleaseComObject(pStyleGalleryClass);
                        }
                        saveItem();
                    }
                    else if (name == "style")
                    {
                        IStyleGallery styleGalley = new ESRI.ArcGIS.Framework.StyleGalleryClass();
                        IStyleGalleryStorage styleGalleryStorage = styleGalley as IStyleGalleryStorage;
                        IStyleGalleryItem pStyleGallerItem = null;
                        IEnumStyleGalleryItem pEnumSyleGalleryItem = null;
                        IStyleGalleryClass pStyleGalleryClass = null;
                        styleGalleryStorage.AddFile(openFileDialog1.FileName);
                        for (int i = 0; i < styleGalley.ClassCount; i++)
                        {
                            pStyleGalleryClass = styleGalley.get_Class(i);

                            if (pStyleGalleryClass.Name != ClassName)
                                continue;

                            pEnumSyleGalleryItem = styleGalley.get_Items(ClassName, openFileDialog1.FileName, "");
                            pEnumSyleGalleryItem.Reset();

                            //遍历pEnumSyleGalleryItem
                            pStyleGallerItem = pEnumSyleGalleryItem.Next();
                            while (pStyleGallerItem != null)
                            {
                                //获取符号
                                IStyleGalleryItem3 sg = pStyleGallerItem as IStyleGalleryItem3;
                                pSymbologyStyleClass.AddItem(sg, 0);
                                pList.Add(sg);
                                if (!pListCato.Contains(sg.Category))
                                {
                                    pListCato.Add(sg.Category);
                                    comboBox1.Items.Add(sg.Category);
                                }
                                if (!pListName.Contains(sg.Name))
                                {
                                    pListName.Add(sg.Name);
                                }
                                // ISymbol pSymbol = pStyleGallerItem.Item as ISymbol;                                                                                                                             

                                pStyleGallerItem = pEnumSyleGalleryItem.Next();

                            }
                            System.Runtime.InteropServices.Marshal.ReleaseComObject(pEnumSyleGalleryItem);

                            System.Runtime.InteropServices.Marshal.ReleaseComObject(pStyleGalleryClass);
                        }
                        saveItem();
                        }
                    else
                    {
                        IMapDocument pMapDoc = new MapDocumentClass();
                        pMapDoc.Open(openFileDialog1.FileName, "");
                        for (int i = 0; i < pMapDoc.MapCount; i++)
                        {

                            IMap map = pMapDoc.get_Map(i);//遍历每一个Map
                            IEnumLayer pEnumLayer = GetFeatureLayers(map);
                            ILayer layer = null;
                            while ((layer = pEnumLayer.Next()) != null)
                            {
                                IFeatureLayer pFeatureLayer = layer as IFeatureLayer;
                                IGeoFeatureLayer pgeo = pFeatureLayer as IGeoFeatureLayer;
                                if (pgeo == null)
                                {
                                    continue;
                                }
                                IFeatureRenderer pFeatureRenderer = pgeo.Renderer as IFeatureRenderer;


                                if (pFeatureRenderer is IUniqueValueRenderer)
                                {
                                    IUniqueValueRenderer pSimpleRenderer = pFeatureRenderer as IUniqueValueRenderer;

                                    for (int j = 0; j < pSimpleRenderer.ValueCount; j++)
                                    {
                                        string s = pSimpleRenderer.get_Value(j);

                                        ISymbol ps = pSimpleRenderer.get_Symbol(s);
                                        if (!getLayer(pSymbologyStyleClass, ps))
                                        {
                                            break;
                                        }
                                        SetFeatureClassStyle(pSymbologyStyleClass.StyleClass, s, pFeatureLayer.Name, ps);
                                        //  SetFeatureClassStyle(pSymbologyStyleClass, s, pFeatureLayer.Name, pSimpleRenderer.get_Symbol(s));
                                    }
                                }
                                else if (pFeatureRenderer is ISimpleRenderer)
                                {

                                    ISimpleRenderer pSimpleRenderer = pFeatureRenderer as ISimpleRenderer;
                                    if (!getLayer(pSymbologyStyleClass, pSimpleRenderer.Symbol))
                                    {
                                        continue;
                                    }
                                    SetFeatureClassStyle(pSymbologyStyleClass.StyleClass, pFeatureLayer.Name, pFeatureLayer.Name, pSimpleRenderer.Symbol);
                                }

                                //  pSimpleRenderer.
                                // pFeatureRenderer.
                            }

                            // pListMap.Add(map);
                            // comboBox1.Items.Add(openFileDialog1.SafeFileName.Substring(0, openFileDialog1.SafeFileName.LastIndexOf(".")));
                        }

                        saveItem();
                    }
                  
                }
               


                catch (Exception ex)
                {
                  //  MessageBox.Show(ex.Message);
                }
            }

            //if (openFileDialog1.FileName != "")
            //{
            //    this.axSymbologyControl1.LoadStyleFile(openFileDialog1.FileName);
            //}
            //this.ShowDialog();

        }
        public bool getLayer(ISymbologyStyleClass pSymbologyStyleClass, ISymbol ps)
        {
            if (pSymbologyStyleClass.StyleClass == esriSymbologyStyleClass.esriStyleClassMarkerSymbols && ps is IMarkerSymbol)
            {
                return true;
            }
            else if (pSymbologyStyleClass.StyleClass == esriSymbologyStyleClass.esriStyleClassLineSymbols && ps is ILineSymbol)
            {
                return true;
            }
            else if (pSymbologyStyleClass.StyleClass == esriSymbologyStyleClass.esriStyleClassFillSymbols && ps is IFillSymbol)
            {
                return true;
            }
            else
            {
                return false;
            }

        }
        public IEnumLayer GetFeatureLayers(IMap map)
        {
            UID uid = new UIDClass();
            uid.Value = "{40A9E885-5533-11d0-98BE-00805F7CED21}";//FeatureLayer
            IEnumLayer layers = map.get_Layers(uid, true);
            return layers;
        }
        /// <summary>
        /// 移除符号
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button8_Click(object sender, EventArgs e)
        {
            try
            {

                GetStyleIndex(es, pStyleGalleryItem);
                
                saveItem();
              //  this.axSymbologyControl1.StyleClass = es;
              //  ISymbologyStyleClass pSymbologyStyleClass = this.axSymbologyControl1.GetStyleClass(es);
              //int hash=  pStyleGalleryItem.GetHashCode();
              // // pStyleGalleryItem.ID
              //  pSymbologyStyleClass.
               // pSymbologyStyleClass.
              //   this.axSymbologyControl1.s
                //pSymbologyStyleClass.RemoveItem(0);
            }
            catch (Exception ex){
                MessageBox.Show(ex.Message);
            }
        }
        /// <summary>
        /// 删除符号
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button9_Click(object sender, EventArgs e)
        {
            try
            {
                //GetStyleIndex(es, pStyleGalleryItem);
                //IGeoFeatureLayer pGeoFeatureLayer = (IGeoFeatureLayer)pLayer;
                //switch (((IFeatureLayer)pLayer).FeatureClass.ShapeType)
                //{
                //    case ESRI.ArcGIS.Geometry.esriGeometryType.esriGeometryPoint:
                //        es = esriSymbologyStyleClass.esriStyleClassMarkerSymbols;
                //        this.GetFeatureClassStyle(es, "MarkerSymbols");

                //        break;
                //    case ESRI.ArcGIS.Geometry.esriGeometryType.esriGeometryPolyline:
                //        es = esriSymbologyStyleClass.esriStyleClassLineSymbols;
                //        this.GetFeatureClassStyle(es, "LineSymbols");

                //        break;
                //    case ESRI.ArcGIS.Geometry.esriGeometryType.esriGeometryPolygon:
                //        es = esriSymbologyStyleClass.esriStyleClassFillSymbols;
                //        this.GetFeatureClassStyle(es, "FillSymbols");

                //        break;
                //    case ESRI.ArcGIS.Geometry.esriGeometryType.esriGeometryMultiPatch:
                //        es = esriSymbologyStyleClass.esriStyleClassFillSymbols;
                //        this.GetFeatureClassStyle(es, "FillSymbols");

                //        break;
                //    default:
                //        this.Close();
                //        break;
                //}
               
            }
            catch (Exception ex)
            {
            }
        }
        /// <summary>
        /// 保存符号
        /// </summary>
        public void saveItem()
        {
            pListstringEnd=new List<string>();
            SymbolInfo pSymbolInfo1;
            foreach (IStyleGalleryItem3 item in pList)
            {
                pSymbolInfo1 = new SymbolInfo(item);

                pListstringEnd.Add(GConvert.ObjectToBase64(pSymbolInfo1));
            }
            switch (this.axSymbologyControl1.StyleClass)
            {
                case esriSymbologyStyleClass.esriStyleClassMarkerSymbols:
                    //  es = esriSymbologyStyleClass.esriStyleClassMarkerSymbols;
                    // this.GetFeatureClassStyle(es, "MarkerSymbols");
                    gapp.AppConfig[this.GetType().ToString() + ".MarkerSymbols"] = pListstringEnd;
                    break;
                case esriSymbologyStyleClass.esriStyleClassLineSymbols:
                    //es = esriSymbologyStyleClass.esriStyleClassLineSymbols;
                    //this.GetFeatureClassStyle(es, "LineSymbols");
                    gapp.AppConfig[this.GetType().ToString() + ".LineSymbols"] = pListstringEnd;
                    break;
                case esriSymbologyStyleClass.esriStyleClassFillSymbols:
                    //es = esriSymbologyStyleClass.esriStyleClassFillSymbols;
                    //this.GetFeatureClassStyle(es, "FillSymbols");
                    gapp.AppConfig[this.GetType().ToString() + ".FillSymbols"] = pListstringEnd;
                    break;
                case esriSymbologyStyleClass.esriStyleClassTextSymbols:
                    //es = esriSymbologyStyleClass.esriStyleClassFillSymbols;
                    //this.GetFeatureClassStyle(es, "FillSymbols");
                    gapp.AppConfig[this.GetType().ToString() + ".TextSymbols"] = pListstringEnd;
                    break;
                default:
                    this.Close();
                    break;
            }
        }
/// <summary>
///符号保存数据库
/// </summary>
/// <param name="sender"></param>
/// <param name="e"></param>
        private void button4_Click(object sender, EventArgs e)
        {
            try
            {
               // GetStyleIndex(es, pStyleGalleryItem);
                saveItem();
               // IGeoFeatureLayer pGeoFeatureLayer = (IGeoFeatureLayer)pLayer;
                switch (this.axSymbologyControl1.StyleClass)
                {
                    case esriSymbologyStyleClass.esriStyleClassMarkerSymbols:
                        //  es = esriSymbologyStyleClass.esriStyleClassMarkerSymbols;
                        // this.GetFeatureClassStyle(es, "MarkerSymbols");
                        gapp.AppConfig[this.GetType().ToString() + "MarkerSymbols"] = pListstringEnd;
                        break;
                    case esriSymbologyStyleClass.esriStyleClassLineSymbols:
                        //es = esriSymbologyStyleClass.esriStyleClassLineSymbols;
                        //this.GetFeatureClassStyle(es, "LineSymbols");
                        gapp.AppConfig[this.GetType().ToString() + "LineSymbols"] = pListstringEnd;
                        break;
                    case esriSymbologyStyleClass.esriStyleClassFillSymbols:
                        //es = esriSymbologyStyleClass.esriStyleClassFillSymbols;
                        //this.GetFeatureClassStyle(es, "FillSymbols");
                        gapp.AppConfig[this.GetType().ToString() + "FillSymbols"] = pListstringEnd;
                        break;
                    case esriSymbologyStyleClass.esriStyleClassTextSymbols:
                        //es = esriSymbologyStyleClass.esriStyleClassFillSymbols;
                        //this.GetFeatureClassStyle(es, "FillSymbols");
                        gapp.AppConfig[this.GetType().ToString() + "TextSymbols"] = pListstringEnd;
                        break;
                    default:
                        this.Close();
                        break;
                }
                MessageBox.Show("保存成功");

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
           
        }
        /// <summary>
        /// 确定
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button2_Click(object sender, EventArgs e)
        {
           

            this.Close();

        }
        /// <summary>
        /// 取消
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button5_Click(object sender, EventArgs e)
        {
            try
            {

                ISymbologyStyleClass pSymbologyStyleClass = this.axSymbologyControl1.GetStyleClass(es);
                // pSymbologyStyleClass.RemoveAll();
                pSymbologyStyleClass.UnselectItem();
                int count = pSymbologyStyleClass.ItemCount;
                // List<string> symbols = new List<string>();
                for (int i = count - 1; i > -1; i--)
                {
                    pList.Remove(pSymbologyStyleClass.GetItem(i) as IStyleGalleryItem3);
                    pSymbologyStyleClass.RemoveItem(i);
                    this.Refresh();

                }
                saveItem();

                //  pSymbologyStyleClass.
            }
            catch
            {
            }
        }
        /// <summary>
        /// 搜索
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button10_Click(object sender, EventArgs e)
        {
            try
            {
                IList<IStyleGalleryItem> index = new List<IStyleGalleryItem>();
                this.axSymbologyControl1.StyleClass = es;
                ISymbologyStyleClass pSymbologyStyleClass = this.axSymbologyControl1.GetStyleClass(es);
                if (comboBox1.Text == "" && textBox1.Text == "")
                {
                    MessageBox.Show("请选择符号种类或者符号名字");
                }

                else
                {
                    if (comboBox1.Text != "" && textBox1.Text != "")
                    {

                        foreach (IStyleGalleryItem s in pList)
                        {
                            if (comboBox1.Text == "全部")
                            {
                                if (s.Name == textBox1.Text)
                                {
                                    //pSymbologyStyleClass.RemoveItem(i);
                                    index.Add(s);
                                }
                            }

                            else
                            {
                                if (s.Category == comboBox1.Text && s.Name == textBox1.Text)
                                {
                                    //pSymbologyStyleClass.RemoveItem(i);
                                    index.Add(s);
                                }
                            }
                        }

                    }
                    else if (comboBox1.Text != "" && textBox1.Text == "")
                    {

                        foreach (IStyleGalleryItem s in pList)
                        {
                            if (comboBox1.Text == "全部")
                            {
                                index.Add(s);

                            }

                            else
                            {
                                if (s.Category == comboBox1.Text)
                                {
                                    index.Add(s); // index.Add(i);  pSymbologyStyleClass.RemoveItem(i);
                                }
                            }

                        }
                    }
                    else
                    {

                        foreach (IStyleGalleryItem s in pList)
                        {
                            if (s.Name == textBox1.Text)
                            {
                                index.Add(s);// pSymbologyStyleClass.RemoveItem(i);
                            }
                        }
                    }
                }
                pSymbologyStyleClass.RemoveAll();
                foreach (IStyleGalleryItem i in index)
                {
                    pSymbologyStyleClass.AddItem(i);
                }
                pSymbologyStyleClass.SelectItem(0);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }
        
       
    }
}
