using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace DatabaseUpdate
{
  public partial class MatchResultForm : Form
  {
    public MatchResultForm()
    {
      InitializeComponent();
    }

    private void button1_Click(object sender, EventArgs e)
    {
      if (dataGridView1.SelectedRows.Count < dataGridView1.RowCount)
        dataGridView1.SelectAll();
      else
        dataGridView1.ClearSelection();
    }

    private void button2_Click(object sender, EventArgs e)
    {
      foreach (DataGridViewRow item in dataGridView1.Rows)
      {
        item.Selected = !item.Selected;
      }
    }

    private void button3_Click(object sender, EventArgs e)
    {
      foreach (DataGridViewRow item in dataGridView1.SelectedRows)
      {
        dataGridView1.Rows.Remove(item);
      }
    }

  }
}
