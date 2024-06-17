using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace DomapTool {
  public partial class LineSimplifyDlg : Form {
    public LineSimplifyDlg() {
      InitializeComponent();
      tbPara.Text = GGenPara.Para["曲线化简容差"].ToString();
    }
    public double ParaValue {
      get {
        try {
          double v = Convert.ToDouble(tbPara.Text);
          GGenPara.Para["曲线化简容差"] = v;
          return v;
        }
        catch {
          return 5;
        }
      }
    }
  }
}
