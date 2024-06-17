using System;
using System.Collections.Generic;
using System.Text;

namespace BuildingGen
{
    class Undo : BaseGenCommand
    {
        private ESRI.ArcGIS.Controls.ControlsUndoCommandClass undo;
        public Undo()
        {
            undo = new ESRI.ArcGIS.Controls.ControlsUndoCommandClass();
            base.m_category = "GEdit";
            base.m_caption = "撤销";
            base.m_message = "撤销";
            base.m_toolTip = "撤销";
            base.m_name = "Undo";
        }

        public override int Bitmap
        {
            get
            {
                return undo.Bitmap;
            }
        }
        public override void OnCreate(object hook)
        {
            undo.OnCreate(hook);
        }
        public override bool Enabled
        {
            get
            {
                return undo.Enabled;
            }
        }
        public override void OnClick()
        {
            undo.OnClick();
        }
    }
}
