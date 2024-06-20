using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.esriSystem;
namespace SMGI.Common {
    public partial class RulerForm : UserControl {
        struct ShowUnit {
            internal double size_in_map_distance;
            internal int short_line_count;
            internal bool show_mid_line;
            internal bool show_in_mm;
            internal ShowUnit(double size_in_distance, int short_line_count, bool show_center_line) {
                this.size_in_map_distance = size_in_distance;
                this.short_line_count = short_line_count;
                this.show_mid_line = show_center_line;
                this.show_in_mm = (size_in_map_distance < 0.9);
            }
        }
        enum LineStyle { 
            long_line,
            mid_line,
            short_line
        }
        List<ShowUnit> show_array;
        int ruleWidth;
        public int RuleWidth {
            get { return ruleWidth; }
            set {
                ruleWidth = value;
                this.axMapControl.Top = ruleWidth;
                this.axMapControl.Left = ruleWidth;
                this.axMapControl.Width = this.Width - ruleWidth;
                this.axMapControl.Height = this.Height - ruleWidth;
            }
        }
        public RulerForm() {
            InitializeComponent();
            this.RuleWidth = 20;
            show_array = new List<ShowUnit>();
            double[] val = new double[] {1.0,2.0,5.0 };
            double[] rate = new double[] { 0.1, 1.0, 10.0 };
            int[] counts = new int[] { 4, 4, 0 };
            bool[] is_show = new bool[] { true,false,true};
            foreach (var item in rate) {
                for (int i = 0; i < 3; i++) {
                    show_array.Add(new ShowUnit(item * val[i], counts[i], is_show[i]));
                }
            }
            this.DoubleBuffered = true;
            MapControl.OnAfterDraw += new ESRI.ArcGIS.Controls.IMapControlEvents2_Ax_OnAfterDrawEventHandler(MapControl_OnAfterDraw);
            this.Paint += new PaintEventHandler(RulerForm_Paint);
        }

        void RulerForm_Paint(object sender, PaintEventArgs e) {
            MyPaint();
        }

        void MyPaint() {
            if (ruleWidth <= 0)
                return;
            if (MapControl.MapScale == 0) {
                return;
            }

            UnitConverterClass cvt = new UnitConverterClass();
            double point_distance = 0.0;
            ShowUnit unit = show_array[show_array.Count - 1];
            foreach (var item in show_array) {
                double real_world_distance = item.size_in_map_distance * MapControl.ReferenceScale / MapControl.MapScale;
                point_distance = cvt.ConvertUnits(real_world_distance, esriUnits.esriMillimeters, esriUnits.esriPoints);
                if (point_distance >= 8) {
                    unit = item;
                    break;
                }
            }
            if (point_distance < 8)
                return;
            List<LineStyle> lines = new List<LineStyle>();
            lines.Add(LineStyle.long_line);
            for (int i = 0; i < unit.short_line_count; i++) {
                lines.Add(LineStyle.short_line);
            }
            if (unit.show_mid_line) {
                lines.Add(LineStyle.mid_line);
                for (int i = 0; i < unit.short_line_count; i++) {
                    lines.Add(LineStyle.short_line);
                }
            }
            Graphics g = this.CreateGraphics();
            g.Clear(this.BackColor);
            Pen pen = new Pen(this.ForeColor);
            g.DrawString(unit.show_in_mm ? "mm" : "cm", this.Font,
                new SolidBrush(this.ForeColor), new RectangleF(0, 0, RuleWidth, RuleWidth));
            double current_dis = 0;
            for (int i = 0; ; i++) {
                int current_pos = (int)(i * point_distance + RuleWidth);
                if (current_pos > this.Width) break;
                LineStyle s = lines[i % lines.Count];
                float from = 0.0f;
                if (s == LineStyle.short_line) {
                    from += RuleWidth - (RuleWidth / 3);
                }
                if (s == LineStyle.mid_line) {
                    from += RuleWidth - (RuleWidth / 2);
                }
                g.DrawLine(pen, current_pos, from, current_pos, this.Height);
                if (s == LineStyle.long_line) {
                    g.DrawString(System.Convert.ToInt32(current_dis).ToString(), this.Font,
                        new SolidBrush(this.ForeColor), current_pos + 0f, 0f);
                }
                current_dis += unit.show_in_mm ? unit.size_in_map_distance : (unit.size_in_map_distance / 10.0);
            }
            current_dis = 0;
            for (int i = 0; ; i++) {
                int current_pos = (int)(i * point_distance + RuleWidth);
                if (current_pos > this.Height) break;
                LineStyle s = lines[i % lines.Count];
                float from = 0.0f;
                if (s == LineStyle.short_line) {
                    from += RuleWidth - (RuleWidth / 3);
                }
                if (s == LineStyle.mid_line) {
                    from += RuleWidth - (RuleWidth / 2);
                }
                g.DrawLine(pen, from, current_pos, this.Width, current_pos);
                if (s == LineStyle.long_line) {
                    g.DrawString(System.Convert.ToInt32(current_dis).ToString(), this.Font,
                        new SolidBrush(this.ForeColor), 0f, current_pos + 0f);
                }
                current_dis += unit.show_in_mm ? unit.size_in_map_distance : (unit.size_in_map_distance / 10.0);
            }
        }
        void MapControl_OnAfterDraw(object sender, ESRI.ArcGIS.Controls.IMapControlEvents2_OnAfterDrawEvent e) {
            if ((ESRI.ArcGIS.Carto.esriViewDrawPhase)e.viewDrawPhase != ESRI.ArcGIS.Carto.esriViewDrawPhase.esriViewBackground)
                return;

            MyPaint();
        }
        public ESRI.ArcGIS.Controls.AxMapControl MapControl { get { return axMapControl; } }

    }
}
