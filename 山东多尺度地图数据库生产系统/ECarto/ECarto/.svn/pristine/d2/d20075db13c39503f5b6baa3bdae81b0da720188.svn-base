using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using SMGI.UI.Ribbon35;

namespace SMGI.Common
{
    public partial class CommandIndexForm : Form
    {
        RibbonPanel rPanel;
        public CommandIndexForm(RibbonPanel pl)
        {
            InitializeComponent();
            rPanel = pl;
            bsCommand.DataSource = pl.Items;
            lbCommand.DisplayMember = "Text";
        }

        private void CommandIndexForm_Load(object sender, EventArgs e)
        {
            int i = rPanel.OwnerTab.Panels.IndexOf(rPanel);
            if (i == 0)
                btLeft.Enabled = false;
            if (i == rPanel.OwnerTab.Panels.Count - 1)
                btRight.Enabled = false;
        }

        private void btUp_Click(object sender, EventArgs e)
        {
            int idx = lbCommand.SelectedIndex;
            if (idx <= 0 || idx >= lbCommand.Items.Count)
            {
                return;
            }
            var c = bsCommand.Current;
            bsCommand.RemoveCurrent();
            bsCommand.Insert(idx - 1, c);
            bsCommand.Position = idx - 1;
            rPanel.Owner.ActiveTab = rPanel.OwnerTab;
        }

        private void btDown_Click(object sender, EventArgs e)
        {
            int idx = lbCommand.SelectedIndex;
            if (idx < 0 || idx >= lbCommand.Items.Count - 1)
            {
                return;
            }
            var c = bsCommand.Current;
            bsCommand.RemoveCurrent();
            bsCommand.Insert(idx + 1, c);
            bsCommand.Position = idx + 1;
            rPanel.Owner.ActiveTab = rPanel.OwnerTab;            
        }

        private void bsCommand_CurrentItemChanged(object sender, EventArgs e)
        {

        }

        private void lbCommand_SelectedIndexChanged(object sender, EventArgs e)
        {
            btDown.Enabled = true;
            btUp.Enabled = true;
            if (lbCommand.SelectedIndex == 0)
            {
                btUp.Enabled = false;
            }
            if (lbCommand.SelectedIndex == lbCommand.Items.Count - 1)
            {
                btDown.Enabled = false;
            }
        }

        private void btOk_Click(object sender, EventArgs e)
        {
            
        }

        private void btLeft_Click(object sender, EventArgs e)
        {
            int i = rPanel.OwnerTab.Panels.IndexOf(rPanel);
            if (i == 0)
                return;

            var tb = rPanel.OwnerTab;
            tb.Panels.RemoveAt(i);
            i--;
            tb.Panels.Insert(i , rPanel);

            if (i == 0)
                btLeft.Enabled = false;
            else
                btLeft.Enabled = true;

            if (i == tb.Panels.Count - 1)
                btRight.Enabled = false;
            else
                btRight.Enabled = true;

            rPanel.Owner.ActiveTab = rPanel.OwnerTab;   
        }

        private void btRight_Click(object sender, EventArgs e)
        {
            int i = rPanel.OwnerTab.Panels.IndexOf(rPanel);
            if (i == rPanel.OwnerTab.Panels.Count - 1)
                return;

            var tb = rPanel.OwnerTab;
            tb.Panels.RemoveAt(i);
            i++;
            tb.Panels.Insert(i, rPanel);

            if (i == 0)
                btLeft.Enabled = false;
            else
                btLeft.Enabled = true;

            if (i == tb.Panels.Count - 1)
                btRight.Enabled = false;
            else
                btRight.Enabled = true;

            rPanel.Owner.ActiveTab = rPanel.OwnerTab;
        }
    }
}
