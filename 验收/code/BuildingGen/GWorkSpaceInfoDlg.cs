using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace BuildingGen {
  public partial class GWorkSpaceInfoDlg : Form {
    GWorkspace workspace;
    public GWorkSpaceInfoDlg(GWorkspace w) {
      InitializeComponent();
      this.workspace = w;
    }
    public double OrgScale {
      get {
        try {
          string text = cbOrgScale.Text;
          string[] v = text.Split(new char[] { ':', '：' });
          if (v.Length > 1) {
            string v2 = v[1];
            return Convert.ToDouble(v2);
          }
          else return Convert.ToDouble(text);   
        }
        catch {
          return 2000;
        }
      }
      set {
        cbOrgScale.Text = string.Format("1:{0}", Convert.ToInt32(value).ToString("n0"));     
      }
    }
    public double GenScale {
      get {
        try {
          string text = cbGenScale.Text;
          string[] v = text.Split(new char[] { ':', '：' });
          if (v.Length > 1) {
            string v2 = v[1];
            return Convert.ToDouble(v2);
          }
          else return Convert.ToDouble(text); 
        }
        catch {
          return 2000;
        }
      }
      set {
        cbGenScale.Text = string.Format("1:{0}", Convert.ToInt32(value).ToString("n0"));
      }
    }
    private void btSave_Click(object sender, EventArgs e) {
      workspace.MapConfig["OrgScale"] = this.OrgScale;
      workspace.MapConfig["GenScale"] = this.GenScale;
      this.DialogResult = DialogResult.OK;
    }

    private void GWorkSpaceInfoDlg_Load(object sender, EventArgs e) {
      this.OrgScale = (double)workspace.MapConfig["OrgScale"];
      this.GenScale = (double)workspace.MapConfig["GenScale"];
      this.tbPath.Text = workspace.Workspace.PathName;
    }
  }
}
