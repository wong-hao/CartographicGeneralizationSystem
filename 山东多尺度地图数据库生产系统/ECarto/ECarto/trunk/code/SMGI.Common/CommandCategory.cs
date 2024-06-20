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
using SMGI.UI.Ribbon35;

namespace SMGI.Common
{
    public class CategoryInfo
    {
        public CategoryInfoContext Context { get; set; }

        public Dictionary<string, CommandInfo> Commands { get; set; }
        public CategoryInfo()
        {
            Commands = new Dictionary<string, CommandInfo>();
        }
        public RibbonPanel RibbonPanel { get; private set; }
        public void CreateUIEx()
        {
            CreateRibbon();
            AddComandItemEx();
        }
        private void CreateRibbon()
        {
            RibbonPanel = new RibbonPanel(Context.Name);
            RibbonPanel.Tag = this;
            RibbonPanel.ButtonMoreClick += new EventHandler(RibbonPanel_ButtonMoreClick);
            
        }

        void RibbonPanel_ButtonMoreClick(object sender, EventArgs e)
        {
            var f = new CommandIndexForm(sender as RibbonPanel);
            f.ShowDialog();
            //RibbonPanel.Owner.ActiveTab = RibbonPanel.OwnerTab;
        }
        private void AddComandItemEx()
        {
            var cis = from ci in Commands.Values
                      orderby ci.Context.IndexInToolbar
                      select ci;
            foreach (var ci in cis)
            {
                ci.SetupSMGIPlugin();
                ci.CreateUIEx();
                RibbonPanel.Items.Add(ci.RibbonItem);
            }
        }
        public void SyncContext()
        {
            this.Context.Commands.Clear();
            for (int i = 0; i < RibbonPanel.Items.Count; i++)
            {
                var it = RibbonPanel.Items[i].Tag as CommandInfo;
                it.Context.IndexInToolbar = i;
                this.Context.Commands.Add(it.ToString(), it.Context);
            }
            this.Context.RibbonPanelIndex = RibbonPanel.OwnerTab.Panels.IndexOf(RibbonPanel);
        }
    }
    [Serializable]
    public class CategoryInfoContext
    {
        public string Name { get; set; }
        public SMGISubSystem SMGISystem { get; set; }
        public bool ToolBarVisiable { get; set; }
        public int RibbonPanelIndex { get; set; }
        public CategoryInfoContext(string name,SMGISubSystem sys = Common.SMGISubSystem.其他功能)
        {
            Name = name;
            ToolBarVisiable = true;
            SMGISystem = sys;
            RibbonPanelIndex = int.MaxValue;
            Commands = new Dictionary<string, CommandInfoContext>();
        }
        public Dictionary<string, CommandInfoContext> Commands { get; set; }
    }
  
}
