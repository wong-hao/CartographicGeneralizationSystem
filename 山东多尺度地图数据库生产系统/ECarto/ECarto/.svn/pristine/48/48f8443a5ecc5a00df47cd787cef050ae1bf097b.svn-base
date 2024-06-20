using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using ESRI.ArcGIS.Controls;
using System.Reflection;
using ESRI.ArcGIS.SystemUI;
using ESRI.ArcGIS.Carto;
using System.Xml.Linq;
using System.Runtime.Serialization.Json;
using System.Runtime.Serialization;
using System.Drawing;
namespace SMGI.Common
{
    public static class Until
    {
        public static string ToSafeString(this object obj)
        {
            if (obj == null)
                return string.Empty;
            else
                return obj.ToString();
        }
    }

    public class PluginCommand
    {
        public string ToolTip { get; set; }
        public string Caption { get; set; }
        public ICommand Command { get; set; }
        public int SubTypeIndex { get; set; }
        private GApplication app { get; set; }

        public PluginCommand(ICommand cmd, int idx, GApplication app)
        {
            this.Command = cmd;
            this.SubTypeIndex = idx;
            this.app = app;

            //this.SetupSMGIPlugin();
        }
        public static string GetString(ICommand cmd, int idx)
        {
            return cmd.GetType().FullName + "@" + idx.ToString();
        }
        public override string ToString()
        {
            if (Command is ICommandSubType)
            {
                return Command.GetType().FullName + "@" + SubTypeIndex.ToString();
            }
            else
            {
                return Command.GetType().FullName;
            }
        }

        private void SetupSMGIPlugin()
        {
            SetCurrent();
            (this.Command as ISMGIPlugin).setApplication(app);
        }
        private void SetCurrent()
        {
            ICommandSubType sub = Command as ICommandSubType;
            if (sub != null)
            {
                sub.SetSubType(SubTypeIndex);
            }
        }
        public event EventHandler Clicked;
        public void OnClick()
        {
            try
            {                
                SetCurrent();
                if (!this.Enabled)
                {
                    return;
                }
                if (app.Workspace != null)
                {
                    app.Workspace.LayerManager.UpdateCurrentRepresentation();
                }
                //设置工具、命令的提示信息
                this.app.MainForm.ShowToolDes("" + this.Caption + ":" + this.ToolTip);
              
                if (Command is ITool)
                {
                    if (app.LayoutState == LayoutState.MapControl)
                    {
                        if (app.MapControl.CurrentTool != Command)
                        {
                            if (app.MapControl.CurrentTool != null)
                                app.MapControl.CurrentTool.Deactivate();
                            app.MapControl.CurrentTool = Command as ITool;
                        }
                    }
                    else
                    {
                        if (app.PageLayoutControl.CurrentTool != Command)
                        {
                            if (app.PageLayoutControl.CurrentTool != null)
                                app.PageLayoutControl.CurrentTool.Deactivate();
                            app.PageLayoutControl.CurrentTool = Command as ITool;
                        }
                    }
                }
                else
                {
                    Command.OnClick();
                }
                if (Clicked != null)
                    Clicked(this, new EventArgs());
            }
            catch (Exception ex)
            {
                ThreadExceptionDialog dlg = new ThreadExceptionDialog(ex);
                dlg.Text = "异常";
                dlg.ShowDialog();

            }
        }
        public bool Enabled
        {
            get {
                SetCurrent();
                return Command.Enabled;
            }
        }
        public bool Selected
        {
            get {
                if (app.LayoutState == LayoutState.MapControl)
                    return (Command.Equals(app.MapControl.CurrentTool));
                else
                    return (Command.Equals(app.PageLayoutControl.CurrentTool));
            }
        }
        public override int GetHashCode()
        {
            return this.ToString().GetHashCode();
        }
        public override bool Equals(object obj)
        {
            return obj is PluginCommand && this.ToString() == obj.ToString();
        }

        public bool IsAutomaticCommand
        {
            get
            {                
                 var att = this.GetType().GetCustomAttributes(typeof(SMGIAutomaticCommandAttribute), false);
                 return (att == null || att.Length == 0);
            }
        }
        
    }
}
