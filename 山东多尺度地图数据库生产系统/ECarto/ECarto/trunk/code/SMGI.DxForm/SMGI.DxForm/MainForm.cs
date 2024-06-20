using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using DevExpress.Skins;
using DevExpress.LookAndFeel;
using DevExpress.UserSkins;
using DevExpress.XtraBars;
using DevExpress.XtraBars.Ribbon;
using DevExpress.XtraBars.Helpers;
using SMGI.Common;
using System.Xml.Linq;
using ESRI.ArcGIS.Controls;
using ESRI.ArcGIS.SystemUI;
using DevExpress.XtraBars.Docking;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.DisplayUI;
using ESRI.ArcGIS.Display;

namespace SMGI.DxForm
{
    public partial class MainForm : RibbonForm, IPluginHost, ISMGIMainForm
    {
        SMGI.Common.GApplication app;

        public SMGI.Common.GApplication App
        {
            get { return app; }
            set { app = value; }
        }

        public MainForm()
        {
            InitializeComponent();
            InitSkinGallery();

            this.Activated += new EventHandler(MainForm_Activated);
            this.MapControl.MouseCaptureChanged += new EventHandler(MapControl_MouseCaptureChanged);

            axToolbarControl1.AddItem(new ControlsSnappingToolbarClass());//增加捕捉工具条

            this.Ribbon.Toolbar.ShowCustomizeItem = false;

            DevExpress.Utils.AppearanceObject.DefaultFont = new System.Drawing.Font("Tahoma", 9);//默认"Tahoma", 9
            this.barAndDockingController1.AppearancesRibbon.FormCaption.Font = new System.Drawing.Font("Tahoma", 15);
            this.barAndDockingController1.AppearancesRibbon.PageHeader.Font = new System.Drawing.Font("Tahoma", 10);
            //this.barAndDockingController1.AppearancesRibbon.PageGroupCaption.Font = new System.Drawing.Font("Tahoma", 10);
            this.barAndDockingController1.AppearancesRibbon.FormCaption.TextOptions.VAlignment = DevExpress.Utils.VertAlignment.Center;
        }

        void MapControl_MouseCaptureChanged(object sender, EventArgs e)
        {
            if (LayoutState == Common.LayoutState.MapControl)
            {
                bool f = MapControl.Focus();
            }
        }

        void MainForm_Activated(object sender, EventArgs e)
        {
            if (LayoutState == Common.LayoutState.MapControl)
            {
                bool f = MapControl.Focus();
            }
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            axTOCControl1.SetBuddyControl(axMapControl1);

            this.WindowState = FormWindowState.Maximized;
            this.MouseWheel += new MouseEventHandler(MainForm_MouseWheel);
        }

        void MainForm_MouseWheel(object sender, MouseEventArgs e)
        {
            MapControl.Focus();
        }


        void InitSkinGallery()
        {
            SkinHelper.InitSkinGallery(rgbiSkins, true);
        }

        #region IPluginHost
        public AxMapControl MapControl
        {
            get { return this.axMapControl1; }
        }

        public AxTOCControl TocControl
        {
            get { return this.axTOCControl1; }
        }

        public AxPageLayoutControl PageLayoutControl
        {
            get
            {
                return this.axPageLayoutControl1;
            }
        }
        public event EventHandler<LayoutChangedArgs> MapLayoutChanged;
        public LayoutState LayoutState
        {
            get
            {
                return (tabPage.SelectedTabPage == tpMap) ? Common.LayoutState.MapControl : Common.LayoutState.PageLayoutControl;
            }
            set
            {
                tabPage.SelectedTabPage = (value == Common.LayoutState.MapControl) ? tpMap : tpPage;
            }
        }

        private void tabPage_SelectedIndexChanged(object sender, EventArgs e)
        {
            LayoutState prev, current;
            if (tabPage.SelectedTabPage == tpMap)
            {
                prev = LayoutState.PageLayoutControl;
                current = LayoutState.MapControl;
            }
            else
            {
                current = LayoutState.PageLayoutControl;
                prev = LayoutState.MapControl;
            }
            if (MapLayoutChanged != null)
            {
                MapLayoutChanged(this, new LayoutChangedArgs(current, prev));
            }
        }

        public ICommandPool CommandPool
        {
            get
            {
                return axToolbarControl1.CommandPool;
            }
        }
        Dictionary<IntPtr, DockPanel> dockedCtls = new Dictionary<IntPtr, DockPanel>();
        public void CloseChild(IntPtr handle)
        {
            if (dockedCtls.ContainsKey(handle))
            {
                var ctt = dockedCtls[handle];
                ctt.Close();
            }
        }
        public void ShowChild(IntPtr handle)
        {
            if (dockedCtls.ContainsKey(handle))
            {
                return;
            }
            var ctl = Control.FromHandle(handle);
            
            if (ctl.Parent != null)
            {
                try
                {
                    var p = ctl.Parent;
                    ctl.Parent = null;

                    (p as Form).Close();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex);
                }
            }

            {
                var ctt = dockManager1.AddPanel( DevExpress.XtraBars.Docking.DockingStyle.Right);
                ctt.Options.AllowDockBottom = false;
                ctt.Options.AllowDockFill = false;
                ctt.Options.AllowDockLeft = false;
                ctt.Options.AllowDockTop = false;
                ctt.Options.AllowFloating = false;
                ctt.Options.ShowAutoHideButton = false;

                if (ctl is Form)
                {
                    var f = ctl as Form;
                    f.TopLevel = false;
                    f.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
                }
                ctt.Text = ctl.Text;
                ctt.TabText = ctl.Text;
                //ctt.Controls.Clear();
                ctt.Controls.Add(ctl);
                ctt.Width = ctl.Width;
                ctl.Dock = DockStyle.Fill;

                ctl.Show();


                ctt.Show();
                //ctt.UpdateZOrder();
                ctt.ClosingPanel += (o, e) => 
                { 
                    ctt.Controls.Remove(ctl);
                    dockedCtls.Remove(handle);
                };
                //ctt.FormClosing += (o, e) => { Controls.Remove(ctt); ctt.Controls.Clear(); };
                dockedCtls.Add(handle, ctt);
            }
        }

        public void ShowChild2(IntPtr handle,FormLocation location)
        {
            if (dockedCtls.ContainsKey(handle))
            {
                return;
            }
            var ctl = Control.FromHandle(handle);

            if (ctl.Parent != null)
            {
                try
                {
                    var p = ctl.Parent;
                    ctl.Parent = null;
                    if (p is Form)
                    {
                        (p as Form).Close();
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex);
                }
            }

            {
                DockPanel ctt = null;
                switch (location)
                {
                    case FormLocation.Left:
                        ctt = dockManager1.AddPanel(DevExpress.XtraBars.Docking.DockingStyle.Left);
                        break;
                    case FormLocation.Right:
                        ctt = dockManager1.AddPanel(DevExpress.XtraBars.Docking.DockingStyle.Right);
                        break;
                    case FormLocation.Down:
                        ctt = dockManager1.AddPanel(DevExpress.XtraBars.Docking.DockingStyle.Bottom);
                        break;
                    case FormLocation.Top:
                        ctt = dockManager1.AddPanel(DevExpress.XtraBars.Docking.DockingStyle.Top);
                        break;
                }
                ctt.Options.AllowDockBottom = false;
                ctt.Options.AllowDockFill = false;
                ctt.Options.AllowDockRight = false;
                ctt.Options.AllowDockLeft = false;
                ctt.Options.AllowDockTop = false;
                ctt.Options.AllowFloating = false;
                //ctt.Options.ShowAutoHideButton = false;
                int frmWidth = ctl.Width;
                if (ctl is Form)
                {
                    var f = ctl as Form;
                    f.TopLevel = false;
                    f.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
                }
                ctt.Text = ctl.Text;
                ctt.TabText = ctl.Text;
                //ctt.Controls.Clear();
                ctt.Controls.Add(ctl);
                ctt.Width = frmWidth;
                ctl.Dock = DockStyle.Fill;

                ctl.Show();


                ctt.Show();
                //ctt.UpdateZOrder();
                ctt.ClosingPanel += (o, e) =>
                {
                    ctt.Controls.Remove(ctl);
                    dockedCtls.Remove(handle);
                };
                //ctt.FormClosing += (o, e) => { Controls.Remove(ctt); ctt.Controls.Clear(); };
                dockedCtls.Add(handle, ctt);
            }
        }
        public void SetupCommands(XDocument doc, Dictionary<string, PluginCommand> commands)
        {
            CommandGroup group = CommandGroup.ReadFromXML(doc.Element("Group"));
            this.Ribbon.Images = imageCollection1;
            this.Ribbon.LargeImages = imageCollection1;
            SetupCommands(group, null, commands,true);
            qbs.Sort((i1, i2) => { return i1.Item2.CompareTo(i2.Item2); });
            foreach (var item in qbs)
            {
                this.Ribbon.Toolbar.ItemLinks.Add(item.Item1);
            }
            this.timer1.Start();
        }
        List<BarButtonItem> bss = new List<BarButtonItem>();
        List<Tuple<BarButtonItem, int>> qbs = new List<Tuple<BarButtonItem, int>>();
        public void SetupCommands(CommandGroup group, RibbonPage parentPage, Dictionary<string, PluginCommand> commands,bool isRoot = false)
        {            
            RibbonPage page = parentPage;

            if (page == null )
            {
                page = new RibbonPage(group.Caption);
                this.Ribbon.Pages.Add(page);
            }

            RibbonPageGroup pageGroup = new RibbonPageGroup(group.Caption);

            page.Groups.Add(pageGroup);

            foreach (var item in group.Children)
            {
                if (item is CommandGroup)
                {
                    if(isRoot)
                        SetupCommands(item as CommandGroup, null, commands);
                    else
                        SetupCommands(item as CommandGroup, page, commands);
                }
                else
                {
                    if ((item as CommandItem).ClassName == "DevExpress.Skins.SkinManager.Default.Skins")//添加dev所支持的皮肤
                    {
                        SetupSkinsCommandItem(pageGroup);
                    }
                    else
                    {
                        SetupCommandItem(item as CommandItem, pageGroup, commands);
                    }
                }
            }

            if (pageGroup.ItemLinks.Count == 0)
            {
                page.Groups.Remove(pageGroup);
            }

            if (parentPage == null && page.Groups.Count == 0)
            {
                this.Ribbon.Pages.Remove(page);
            }
        }
        
        public void SetupCommandItem(CommandItem item, RibbonPageGroup group, Dictionary<string, PluginCommand> commands)
        {
            if (!commands.ContainsKey(item.ClassName))
            {
                return;
            }
            var cmd = commands[item.ClassName];
            BarButtonItem bi = new BarButtonItem();
            bi.Caption = item.Caption;
            bi.Hint = item.ToolTip;
            cmd.ToolTip = item.ToolTip;
            cmd.Caption = item.Caption;
            if (item.HotKey != string.Empty)
            {                
                try
                {
                    KeysConverter c = new KeysConverter();
                    
                    var ks = c.ConvertFromString(item.HotKey);
                    
                    
                    BarShortcut bc = new BarShortcut((Keys)ks);
                    
                    bc.DisplayString = item.HotKey;
                    bi.ItemShortcut = bc;
                }
                catch
                { 
                }                
            }

            bi.Tag = cmd;
            bi.ItemClick += (o, e) => { 
                cmd.OnClick();
                if(cmd.Command != null)
                    bi.Down = cmd.Command.Checked; 
            };
            Image img = item.ReadImage();
            var idx = imageCollection1.Images.Add(img);            
            bi.ImageIndex = idx;
            bi.LargeImageIndex = idx;
            


            bi.RibbonStyle = RibbonItemStyles.All;
            bi.ButtonStyle = BarButtonStyle.Check;
            if (cmd.Command is ITool)
            {
                bi.ButtonStyle =  BarButtonStyle.Check;
            }
            group.ItemLinks.Add(bi);

            if (item.QOrder >= 0)
            {
                qbs.Add(new Tuple<BarButtonItem, int>(bi, item.QOrder));
            }
            bss.Add(bi);
            
        }

        #region setupskin
        private void SetupSkinsCommandItem(RibbonPageGroup group)
        {
            GalleryItemGroup gi = new GalleryItemGroup();
            gi.Caption = "所有皮肤";

            RibbonGalleryBarItem rgb = new RibbonGalleryBarItem();
            rgb.Gallery.AllowHoverImages = true;
            rgb.Gallery.FixedHoverImageSize = false;
            rgb.Gallery.Groups.AddRange(new GalleryItemGroup[] { gi });
            rgb.Gallery.ImageSize = new System.Drawing.Size(32, 32);
            rgb.Gallery.ItemImageLocation = DevExpress.Utils.Locations.Top;
            rgb.Gallery.ItemCheckMode = DevExpress.XtraBars.Ribbon.Gallery.ItemCheckMode.SingleCheck;
            rgb.Gallery.ColumnCount = 16;
            rgb.Gallery.InitDropDownGallery += new InplaceGalleryEventHandler(Gallery_InitDropDownGallery);
            rgb.Gallery.PopupClose += new InplaceGalleryEventHandler(Gallery_PopupClose);
            rgb.Gallery.ItemClick += new GalleryItemClickEventHandler(Gallery_ItemClick);

            DevExpress.XtraEditors.SimpleButton button = new DevExpress.XtraEditors.SimpleButton();
            foreach (SkinContainer container in SkinManager.Default.Skins)
            {
                button.LookAndFeel.SetSkinStyle(container.SkinName);
                GalleryItem item = new GalleryItem();
                rgb.Gallery.Groups[0].Items.Add(item);
                item.Caption = container.SkinName;
                item.Hint = container.SkinName;
                item.Image = GetSkinImage(button, 32, 32, 2);
                item.HoverImage = GetSkinImage(button, 64, 64, 4);

                if (item.Caption == UserLookAndFeel.Default.SkinName)
                {
                    item.Checked = true;
                }
            }
            group.ItemLinks.Add(rgb);
        }

        private void Gallery_InitDropDownGallery(object sender, InplaceGalleryEventArgs e)
        {
        }

        private void Gallery_PopupClose(object sender, InplaceGalleryEventArgs e)
        {
            foreach (GalleryItemGroup group in e.Item.Gallery.Groups)
            {
                foreach (GalleryItem item in group.Items)
                {
                    if (item.Caption == UserLookAndFeel.Default.SkinName)
                    {
                        item.Checked = true;
                        break;
                    }
                }
            }
        }

        private void Gallery_ItemClick(object sender, GalleryItemClickEventArgs e)
        {
            UserLookAndFeel.Default.SetSkinStyle(e.Item.Caption);
            e.Item.Checked = true;
        }

        private Bitmap GetSkinImage(DevExpress.XtraEditors.SimpleButton button, int width, int height, int indent)
        {
            Bitmap image = new Bitmap(width, height);

            using (Graphics g = Graphics.FromImage(image))
            {
                DevExpress.Utils.Drawing.StyleObjectInfoArgs info = new DevExpress.Utils.Drawing.StyleObjectInfoArgs(new DevExpress.Utils.Drawing.GraphicsCache(g));
                info.Bounds = new Rectangle(0, 0, width, height);
                button.LookAndFeel.Painter.GroupPanel.DrawObject(info);
                button.LookAndFeel.Painter.Border.DrawObject(info);
                info.Bounds = new Rectangle(indent, indent, width - indent * 2, height - indent * 2);
                button.LookAndFeel.Painter.Button.DrawObject(info);
            }

            return image;
        }
        #endregion


        public string Title
        {
            get
            {
                return this.Text;
            }
            set
            {
                this.Text = value;
            }
        }


        public bool BusyStatus
        {
            set
            {
                //do nothing
            }
        }
        public void ShowToolDes(string des)
        {
            ToolDes.Caption = des;
        }
        public void ShowStatus(string status)
        {
            siStatus.Caption = status;
        }

        public new event EventHandler<CancelEventArgs> Closing;

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (Closing != null)
            {
                Closing(this, e);
                if (e.Cancel)
                    return;
            }

        }
        #endregion

        #region ISMGIMainForm
        public AxToolbarControl MapHBar
        {
            get { return this.axToolbarControl1; }
        }
        public AxToolbarControl MapVBar
        {
            get { return this.axToolbarControl2; }
        }
        public AxToolbarControl PageHBar
        {
            get { return this.axToolbarControl3; }
        }
        public AxToolbarControl PageVBar
        {
            get { return this.axToolbarControl4; }
        }
        public object DockManager 
        {
            get { return this.dockManager1; }
        }
        #endregion


        private void timer1_Tick(object sender, EventArgs e)
        {
            foreach (var item in bss)
            {                
                item.Enabled = (item.Tag as PluginCommand).Enabled;

                item.Down = (item.Tag as PluginCommand).Command.Checked;

                if ((item.Tag as PluginCommand).Command is ITool)
                {
                    item.Down = (item.Tag as PluginCommand).Selected;
                }
            }
        }

        private void iAbout_ItemClick(object sender, ItemClickEventArgs e)
        {
            AboutECarto about = new AboutECarto();
            about.ShowDialog();
        }

        private void tabPage_SelectedPageChanged(object sender, DevExpress.XtraTab.TabPageChangedEventArgs e)
        {
            tabPage_SelectedIndexChanged(sender, e);
        }

        private void MainForm_Click(object sender, EventArgs e)
        {

        }
        Timer timer;
        private void searchEdit_TextChanged(object sender, EventArgs e)
        {
            if (timer == null)
            {
                timer = new Timer();
                timer.Interval = 500;
                timer.Tick += (o, ee) =>
                {
                    if (searchEdit.SelectedItem is TocItem)
                    {
                        SelectItemInToc(searchEdit.SelectedItem as TocItem);
                    }
                    else
                    {
                        SelectItemInToc(searchEdit.Text);
                    }
                    timer.Stop();
                    searchEdit.Select(searchEdit.Text.Length, 0);
                };
            }
            if (timer.Enabled)
            {
                timer.Stop();
            }
            timer.Start();           
        }



        class TocItem
        {
            public TocItem(esriTOCControlItem type,
                IBasicMap map = null, ILayer layer = null, ILegendGroup group = null, int cls = -1)
            {
                Type = type;
                Map = map;
                Layer = layer;
                Group = group;
                Class = cls;
            }
            public esriTOCControlItem Type { get; set; }
            public IBasicMap Map { get; set; }
            public ILayer Layer { get; set; }
            public ILegendGroup Group { get; set; }
            public int Class { get; set; }
            public override string ToString()
            {
                switch (Type)
                {
                    case esriTOCControlItem.esriTOCControlItemHeading:
                        return Group.Heading;
                    case esriTOCControlItem.esriTOCControlItemLayer:
                        return Layer.Name;
                    case esriTOCControlItem.esriTOCControlItemLegendClass:
                        return Group.get_Class(Class).Label;
                    case esriTOCControlItem.esriTOCControlItemMap:
                        return Map.Name;
                    case esriTOCControlItem.esriTOCControlItemNone:
                    default:
                        return string.Empty;;
                }
            }
        }
        private IEnumerable <TocItem> FindThingsInMap(IMap map, string text)
        {
            if (map.Name.ToLower().Contains(text.ToLower()))
                yield return new TocItem(esriTOCControlItem.esriTOCControlItemMap,
                    map as IBasicMap);

            var layers = map.get_Layers();
            layers.Reset();
            ILayer l = null;
            //搜索图层名称，制图表达名称
            while ((l = layers.Next()) != null)
            {
                #region
                if (l.Name.ToLower().Contains(text.ToLower()))
                {
                    yield return new TocItem(esriTOCControlItem.esriTOCControlItemLayer,
                        map as IBasicMap, l);
                }

                ILegendInfo info = l as ILegendInfo;
                if (info == null)
                    continue;

                for (int i = 0; i < info.LegendGroupCount; i++)
                {
                    var g = info.get_LegendGroup(i);
                    if (g.Heading.ToLower().Contains(text.ToLower()))
                        yield return new TocItem(esriTOCControlItem.esriTOCControlItemHeading,
                            map as IBasicMap, l, g);

                    for (int j = 0; j < g.ClassCount; j++)
                    {
                        var c = g.get_Class(j);
                        if (c.Label.ToLower().Contains(text.ToLower()))
                            
                            yield return new TocItem(esriTOCControlItem.esriTOCControlItemLegendClass,
                                    map as IBasicMap, l, g, j);
                    }
                }
                #endregion
            }

            //yield return new TocItem(esriTOCControlItem.esriTOCControlItemNone);
        }
        private void SelectItemInToc(string text,int index = 0)
        {
            if (text == null || text == string.Empty || text == "")
            {
                return;
            }

            ITOCControl2 toc = this.axTOCControl1.Object as ITOCControl2;
            var b = toc.ActiveView.FocusMap;
            if (b is IMap)
            {
                var items = FindThingsInMap(b as IMap, text);
                TocItem item = null;
                searchEdit.Items.Clear();
                foreach (var it in items)
                {
                    searchEdit.Items.Add(it);
                    if (item == null)
                        item = it;
                }
                SelectItemInToc(item);
            }
        }
        private void SelectItemInToc(TocItem item)
        {
            ITOCControl2 toc = this.axTOCControl1.Object as ITOCControl2;
            switch (item.Type)
            {
                case esriTOCControlItem.esriTOCControlItemHeading:
                    toc.SelectItem(item.Group);
                    //TocControl.Focus();
                    return;
                case esriTOCControlItem.esriTOCControlItemLayer:
                    toc.SelectItem(item.Layer);
                    //TocControl.Focus();
                    return;
                case esriTOCControlItem.esriTOCControlItemLegendClass:
                    toc.SelectItem(item.Group, item.Class);
                    //TocControl.Focus();
                    return;
                case esriTOCControlItem.esriTOCControlItemMap:
                    toc.SelectItem(item.Map);
                    //TocControl.Focus();
                    return;
                default:
                    break;
            }
        }

        private void searchEdit_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter && searchEdit.Items.Count > 0)
                if (searchEdit.SelectedIndex < searchEdit.Items.Count - 1)
                    searchEdit.SelectedIndex++;
                else
                    searchEdit.SelectedIndex = 0;
        }

       
    }

    public static class Helper
    {
        public static string GetSafeValue(this XElement el, XName childName)
        {
            if (el == null || el.Element(childName) == null)
            {
                return string.Empty;
            }
            else
            {                
                return el.Element(childName).Value;
            }
        }
        public static int GetSafeInterger(this XElement el, XName childName)
        {
            try
            {
                if (el.Element(childName)==null)
                    return -1;
                return Convert.ToInt32(el.Element(childName).Value);
            }
            catch
            {
                return -1;
            }
        }
    }

    public abstract class CommandLayoutItem
    {
        public string Caption { get; set; }
        public abstract XElement WriteToXml();
    }
    public class CommandItem : CommandLayoutItem
    {
        public string ClassName { get; set; }
        public string ToolTip { get; set; }
        public string Image { get; set; }
        public string HotKey { get; set; }
        public int QOrder { get; set; }
        public Image ReadImage()
        {
            string path = GApplication.ResourcePath + @"\";
            if (Image == null || Image == "")
            {
                path += Caption + ".png";
            }
            else
            {
                path += Image;
            }
            if (!System.IO.File.Exists(path))
            {
                return null;
            }
            try
            {
                return System.Drawing.Image.FromFile(path);
            }
            catch
            {
                return null;
            }
        }

        public override XElement WriteToXml()
        {
            return new XElement("Command",
                new XElement("ClassName", ClassName),
                new XElement("Caption", Caption),
                new XElement("ToolTip", ToolTip),
                new XElement("Image", Image),                
                new XElement("HotKey",HotKey),
                new XElement("QOrder", QOrder)
                );
        }
        
        public static CommandItem ReadFromXML(XElement el)
        {
            return new CommandItem
            {
                Caption = el.GetSafeValue("Caption"),
                ClassName = el.GetSafeValue("ClassName"),
                ToolTip = el.GetSafeValue("ToolTip"),
                Image = el.GetSafeValue("Image"),
                HotKey = el.GetSafeValue("HotKey"),
                QOrder = el.GetSafeInterger("QOrder")
            };
        }
    }
    public class CommandGroup : CommandLayoutItem
    {
        public CommandLayoutItem[] Children { get; set; }

        public override XElement WriteToXml()
        {
            return new XElement("Group",
                        new XElement("Caption", Caption),
                        new XElement("Children",
                            from x in Children
                            select x.WriteToXml()
                            )
                        );
        }

        public static CommandGroup ReadFromXML(XElement el)
        {
            return new CommandGroup
            {
                Caption = el.GetSafeValue("Caption"),
                Children = (from x in el.Element("Children").Elements()
                            select x.Name == "Group"
                            ? CommandGroup.ReadFromXML(x) as CommandLayoutItem
                            : CommandItem.ReadFromXML(x) as CommandLayoutItem
                            ).ToArray()
            };
        }
    }


}