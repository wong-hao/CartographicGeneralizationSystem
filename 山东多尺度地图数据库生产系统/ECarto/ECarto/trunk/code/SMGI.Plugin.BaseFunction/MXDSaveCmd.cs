using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SMGI.Common;
using ESRI.ArcGIS.Carto;
using System.Windows.Forms;

namespace SMGI.Plugin.BaseFunction
{
    public class MXDSaveCmd : SMGICommand
    {
        public MXDSaveCmd()
        {
            m_caption = "保存MXD";
        }
      public override bool Enabled
      {
          get
          {
              return m_Application != null;
          }
      }
      public override void OnClick()
      {
          IMxdContents pMxdC;
          pMxdC = m_Application.MapControl.Map as IMxdContents; 
          IMapDocument pMapDocument = new MapDocumentClass();
          if (m_Application.MapControl.DocumentFilename == null)
          {
              SaveFileDialog saveFileDialog1 = new SaveFileDialog();
              saveFileDialog1.Filter = "符号文件(*.mxd)|*.mxd";
             if (saveFileDialog1.ShowDialog() == DialogResult.OK)
             {
                 string fileName = saveFileDialog1.FileName;
                 pMapDocument.New(fileName);
             }
             
              IActiveView pActiveView = m_Application.MapControl.Map as IActiveView;
              pMapDocument.ReplaceContents(pMxdC);
              pMapDocument.Save(true, true);
          }
          else
          {
              pMapDocument.Open(m_Application.MapControl.DocumentFilename, "");
              IActiveView pActiveView = m_Application.MapControl.Map as IActiveView;
              pMapDocument.ReplaceContents(pMxdC);
              pMapDocument.Save(true, true);
          }
      }
    }
}
