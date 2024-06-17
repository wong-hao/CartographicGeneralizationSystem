using System;
using System.Collections.Generic;
using System.Text;

namespace BuildingGen
{
    public class AddData:BaseGenCommand
    {
        AddDataDlg dlg;
        public AddData()
        {
            base.m_category = "GSystem";
            base.m_caption = "添加数据";
            base.m_message = "向工作区中添加数据（数据将被复制到工作区中）";
            base.m_toolTip = "向工作区中添加制定数据（数据将被复制到工作区中）";
            base.m_name = "AddData";
            base.m_bitmap = System.Drawing.Bitmap.FromHbitmap((IntPtr)(new ESRI.ArcGIS.Controls.ControlsAddDataCommandClass()).Bitmap);
            dlg = new AddDataDlg();
            
        }
        public override bool Enabled
        {
            get
            {
                return m_application.Workspace!= null;
            }
        }
        public override void OnClick()
        {            
            //Sdlg.LayerType = "All Type";
            if (dlg.ShowDialog() != System.Windows.Forms.DialogResult.OK)
            {
                return;
            }
            foreach (var item in m_application.Workspace.LayerManager.Layers) {
                if (item.LayerType == dlg.LayerType || item.Layer.Name == dlg.LayerName) {
                    System.Windows.Forms.MessageBox.Show("已经存在相同的图层。");
                    return;
                }
            }
            string wn;
            string fn;
            if (!dlg.LayerPath.Contains("|"))
            {
                wn = System.IO.Path.GetDirectoryName(dlg.LayerPath);
                fn = System.IO.Path.GetFileNameWithoutExtension(dlg.LayerPath);
            }
            else
            {
                string[] paths = dlg.LayerPath.Split(new char[] { '|' });
                wn = paths[0];
                fn = paths[paths.Length - 1];
            }
            m_application.SetBusy(true);
            m_application.Workspace.LayerManager.AddLayer(wn, fn, dlg.LayerName,dlg.LayerType);
            m_application.SetBusy(false);
            m_application.MapControl.Refresh();
        }
    }
}
