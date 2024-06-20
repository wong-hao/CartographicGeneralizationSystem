using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ESRI.ArcGIS.Carto;

namespace SMGI.Common
{
    public class SelectDataDlg
    {
        ESRI.ArcGIS.Controls.ControlsAddDataCommandClass cmd;

        SelectDataForm form;

        private List<ILayer> listLyrs;

        public ILayer Layer
        {
            get;
            internal set;
        }

        public ILayer[] Layers
        {
            get { return listLyrs.ToArray(); }
        }

        internal SelectDataDlg()
        {
            form = new SelectDataForm();
            listLyrs = new List<ILayer>();
            cmd = new ESRI.ArcGIS.Controls.ControlsAddDataCommandClass();
            cmd.OnCreate(form.axMapControl1.Object);
        }

        public DialogResult ShowDialog()
        {

            cmd.OnClick();

            if (form.axMapControl1.LayerCount > 0)
            {
                listLyrs.Clear();
                for (int i = 0; i < form.axMapControl1.LayerCount; i++)
                {
                    Layer = form.axMapControl1.get_Layer(0);
                    listLyrs.Add(form.axMapControl1.get_Layer(i));
                }
                form.axMapControl1.ClearLayers();

                form.axMapControl1.Map = new MapClass();
                return DialogResult.OK;
            }
            else
            {
                return DialogResult.Cancel;
            }
        }
    }
}
