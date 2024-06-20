using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using SMGI.Common;
using System.Xml.Linq;
namespace SMGI.Plugin.EmergencyMap
{
    public partial class FrmBaseMapSet : Form
    {
        GApplication _app;
        public ServiceInfo MapServiceInfo = new ServiceInfo();
        public FrmBaseMapSet()
        {
            InitializeComponent();
        }

        private void FrmMapServiceLoad_Load(object sender, EventArgs e)
        {
            _app = GApplication.Application;

            var envFileName = @"专家库\地图服务\BaseMap.xml";
          
            XDocument doc = XDocument.Load(_app.Template.Root + @"\" + envFileName);
           
            {
                 
                var content = doc.Element("Template").Elements("Content");
                var elements = content.Elements("MapService").Elements("Item");
                int i = 0;
                foreach (var mapService in elements)
                {
                    var mapUrl = mapService.Element("MapServiceUrl").Attribute("type").Value;
                    if (mapUrl == "Local")
                        mapUrl = "本地数据库";
                    else
                        mapUrl = "SDE数据库";
                    var mapName = mapService.Element("MapServiceName");
                    var mapTxt = mapService.Element("MapServiceDes");
                    object[] dataObj = new object[4];
                    dataObj[0] = mapUrl;
                    dataObj[1] = mapName.Value;
                    if (mapTxt != null)
                    {
                        dataObj[2] = mapTxt.Value;
                    }
                    dataObj[3] = false;
                    dataGV_reuslt.Rows.Insert(i, dataObj);
                    dataGV_reuslt.Rows[i].HeaderCell.Value = (i + 1).ToString();
                    dataGV_reuslt.Rows[i].Tag = mapService;
                    i++;
                }  
            
            }
            
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            MapServiceInfo = null;
            for(int i=0;i<dataGV_reuslt.Rows.Count;i++)
            {
                DataGridViewRow dr = dataGV_reuslt.Rows[i];
                bool val;
                bool.TryParse(dr.Cells["Selected"].Value.ToString(),out val);
                if (val)
                {
                    string url = dr.Cells["ServiceUrl"].Value.ToString();
                    string name = dr.Cells["ServiceName"].Value.ToString();
                    ServiceInfo info = new ServiceInfo { Name = name, Url = url,Element=dr.Tag as XElement };
                    MapServiceInfo = info;
                    break;
                }
            }
            if (MapServiceInfo == null)
            {
                MessageBox.Show("请选择服务！");
                return;

            }
            DialogResult = DialogResult.OK;
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {

        }

        private void dataGV_reuslt_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            DataGridView dg = sender as DataGridView;
            if (e.RowIndex < 0 || e.ColumnIndex < 0)
                return;

            DataGridViewCheckBoxCell checkboxCell = dg.Rows[e.RowIndex].Cells[e.ColumnIndex] as DataGridViewCheckBoxCell;
            if (checkboxCell != null)
            {
                bool flag = Convert.ToBoolean(checkboxCell.EditedFormattedValue);
                if (flag)
                {
                    for( int i=0;i<dg.Rows.Count;i++)
                    {
                        if (i != e.RowIndex)
                        {
                            DataGridViewCheckBoxCell cbCell = dg.Rows[i].Cells[e.ColumnIndex] as DataGridViewCheckBoxCell;
                            cbCell.Value = false;
                        }
                    }
                }
            }
        }
    }

    public class ServiceInfo
    {
        public XElement Element;
        public string Name;
        public string Url;
    }
    
}
