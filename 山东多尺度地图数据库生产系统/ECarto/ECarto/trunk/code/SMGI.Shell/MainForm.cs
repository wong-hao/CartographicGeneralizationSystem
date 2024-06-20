using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace SMGI.Shell
{
    public partial class MainForm : Form,SMGI.Common.IPluginHost
    {
        public MainForm()
        {
            InitializeComponent();
        }

        public void SetupCommands(System.Xml.Linq.XDocument commandinfo, Dictionary<string, Common.PluginCommand> commands)
        {
            //do nothing
        }

        public ESRI.ArcGIS.Controls.AxMapControl MapControl
        {
            get { return this.axMapControl1; }
        }

        public ESRI.ArcGIS.Controls.AxPageLayoutControl PageLayoutControl
        {
            get { return this.axPageLayoutControl1; }
        }

        public ESRI.ArcGIS.Controls.AxTOCControl TocControl
        {
            get { return this.axTOCControl1; }
        }

        public ESRI.ArcGIS.Controls.ICommandPool CommandPool
        {
            get { return this.axToolbarControl1.CommandPool; }
        }


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
            set {
                //do nothing
            }
        }

        public void ShowToolDes(string des)
        {
            // do nothing
        }

        public void ShowStatus(string status)
        {
            // do nothing
        }

        public void ShowChild(IntPtr handle)
        {
            // not surpport
            System.Console.WriteLine("不支持显示窗口，窗口ID:"+handle.ToString());
        }
        public void ShowChild2(IntPtr handle, SMGI.Common.FormLocation location)
        {
            // not surpport
            System.Console.WriteLine("不支持显示窗口，窗口ID:" + handle.ToString());
        }
        public void CloseChild(IntPtr handle)
        {
            System.Console.WriteLine("不支持关闭窗口，窗口ID:" + handle.ToString());
        }

        public new event EventHandler<CancelEventArgs> Closing;

        public Common.LayoutState LayoutState
        {
            get;
            set;
        }

        public event EventHandler<Common.LayoutChangedArgs> MapLayoutChanged;
    }
}
