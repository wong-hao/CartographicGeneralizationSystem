using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using ESRI.ArcGIS.ADF.CATIDs;
using ESRI.ArcGIS.ADF.BaseClasses;
using System.Drawing;
using ESRI.ArcGIS.Framework;
using System.Windows.Forms;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geometry;
using SMGI.Common;
namespace SMGI.Plugin.CollaborativeWork
{
    public class DeleteFieldsCommand : SMGICommand
    {
        public DeleteFieldsCommand()
        {
        }

        public override bool Enabled
        {
            get
            {
                return m_Application != null && m_Application.Workspace != null && m_Application.EngineEditor.EditState== ESRI.ArcGIS.Controls.esriEngineEditState.esriEngineStateNotEditing;
            }
        }
       
        public override void OnClick()
        {
            frmInputField inputField = new frmInputField();
            inputField.ShowDialog();
            string delFieldName = "";
            if ((delFieldName = inputField.tbInfo.Text.Trim()) == "")
            {
                return;
            }

            IActiveView activeView = m_Application.ActiveView;
            IMap map = activeView.FocusMap;
            var layers = map.get_Layers();
            int delNum = 0;
            for (var layer = layers.Next(); layer != null; layer = layers.Next())
            {
                if (!(layer is ESRI.ArcGIS.Carto.IFeatureLayer))
                    continue;
                IFeatureClass fc = (layer as IFeatureLayer).FeatureClass;
                int delFieldIndex = fc.Fields.FindField(delFieldName);
                if (delFieldIndex != -1)
                {
                    IField field = fc.Fields.get_Field(delFieldIndex);
                    fc.DeleteField(field);
                    delNum++;
                }
            }
            MessageBox.Show("删除成功！\n共处理了【" + delNum.ToString() + "】个图层");
        }
    }
}
