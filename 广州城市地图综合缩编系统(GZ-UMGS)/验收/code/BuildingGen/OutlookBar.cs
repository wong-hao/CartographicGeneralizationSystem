using System;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;
namespace OutlookBar {
    public class OutlookBar : Panel {
        private int buttonHeight;
        private int selectedBand;
        private int selectedBandHeight;
        private List<BandTagInfo> infos;

        public event EventHandler SelectBandChanged;
        public int ButtonHeight {
            get {
                return buttonHeight;
            }

            set {
                buttonHeight = value;
                // do recalc layout for entire bar
            }
        }

        public int SelectedBand {
            get {
                return selectedBand;
            }
            set {
                SelectBand(value);
                if (SelectBandChanged != null) {
                    SelectBandChanged(this, new EventArgs());
                }
            }
        }

        public OutlookBar() {
            buttonHeight = 25;
            selectedBand = 0;
            selectedBandHeight = 0;
            infos = new List<BandTagInfo>();
        }

        public void Initialize() {
            // parent must exist!
            Parent.SizeChanged += new EventHandler(SizeChangedEvent);
        }

        public void AddBand(string caption, ContentPanel content) {
            content.outlookBar = this;
            int index = infos.Count;
            BandTagInfo bti = new BandTagInfo(this, index, caption, content);
            infos.Add(bti);
            UpdateBarInfo();
            RecalcLayout(bti.band, index);
        }

        public void SelectBand(int index) {
            selectedBand = index;
            RedrawBands();
        }

        private void RedrawBands() {
            for (int i = 0; i < Controls.Count; i++) {
                BandPanel bp = Controls[i] as BandPanel;
                RecalcLayout(bp, i);
            }
        }

        private void UpdateBarInfo() {
            selectedBandHeight = ClientRectangle.Height - (Controls.Count * buttonHeight);
        }

        private void RecalcLayout(BandPanel bandPanel, int index) {
            int vPos = (index <= selectedBand) ? buttonHeight * index : buttonHeight * index + selectedBandHeight;
            int height = selectedBand == index ? selectedBandHeight + buttonHeight : buttonHeight;

            // the band dimensions
            bandPanel.Location = new Point(0, vPos);
            bandPanel.Size = new Size(ClientRectangle.Width, height);

            // the contained button dimensions
            bandPanel.Controls[0].Location = new Point(0, 0);
            bandPanel.Controls[0].Size = new Size(ClientRectangle.Width, buttonHeight);

            // the contained content panel dimensions
            bandPanel.Controls[1].Location = new Point(0, buttonHeight);
            bandPanel.Controls[1].Size = new Size(ClientRectangle.Width - 2, height - 8);
        }

        private void SizeChangedEvent(object sender, EventArgs e) {
            Size = new Size(Size.Width, ((Control)sender).ClientRectangle.Size.Height);
            UpdateBarInfo();
            RedrawBands();
        }
    }

    internal class BandPanel : Panel {
        public BandPanel(string caption, ContentPanel content, BandTagInfo bti) {
            BandButton bandButton = new BandButton(caption, bti);
            Controls.Add(bandButton);
            Controls.Add(content);
        }
    }

    internal class BandButton : Button {
        private BandTagInfo bti;

        public BandButton(string caption, BandTagInfo bti) {
            Text = caption;
            FlatStyle = FlatStyle.Standard;
            Visible = true;
            this.bti = bti;
            Click += new EventHandler(SelectBand);
        }

        private void SelectBand(object sender, EventArgs e) {
            bti.OutlookBar.SelectBand(bti.Index);
        }
    }

    public abstract class ContentPanel : Panel {
        public OutlookBar outlookBar;

        public ContentPanel() {
            // initial state
            Visible = true;
        }
    }

    public class GridPanel : ContentPanel {
        Size gridSize;
        List<Panel> panels;
        ESRI.ArcGIS.Controls.AxMapControl mapControl;
        public GridPanel(ESRI.ArcGIS.Controls.AxMapControl mapControl) {
            panels = new List<Panel>();
            this.mapControl = mapControl;
            Timer tr = new Timer();
            tr.Interval = 200;
            tr.Tick += new EventHandler(tr_Tick);
            tr.Start();
            mapControl.HandleDestroyed += (s, e) => { tr.Stop(); };
        }

        void tr_Tick(object sender, EventArgs e) {
            foreach (Panel item in panels) {
                if (mapControl.CurrentTool == item.Tag) {
                    item.BorderStyle = BorderStyle.Fixed3D;
                }
                else if (item.BorderStyle == BorderStyle.Fixed3D) {
                    item.BorderStyle = BorderStyle.FixedSingle;
                }
                item.Enabled = (item.Tag as ESRI.ArcGIS.SystemUI.ICommand).Enabled;
            }
        }
        public void AddCommand(ESRI.ArcGIS.SystemUI.ICommand command) {
            Panel panel = new Panel();
            panel.BackColor = Color.White;
            panel.BorderStyle = BorderStyle.FixedSingle;
            panel.Size = gridSize;
            panel.Location = PLoacation(panels.Count);
            Font font = new Font(panel.Font.FontFamily, 12, FontStyle.Bold);
            panel.Font = font;
            panel.ForeColor = Color.Black;
            panel.Tag = command;
            Label label = new Label();
            label.Text = command.Caption;
            label.Dock = DockStyle.Fill;
            label.TextAlign = ContentAlignment.MiddleCenter;
            panel.Controls.Add(label);

            label.MouseEnter += new EventHandler(panel_MouseEnter);
            label.MouseLeave += new EventHandler(panel_MouseLeave);
            EventHandler handler = (sender, arg) => {
                panel.BorderStyle = BorderStyle.Fixed3D;
                if (command is ESRI.ArcGIS.SystemUI.ITool) {
                    mapControl.CurrentTool = command as ESRI.ArcGIS.SystemUI.ITool;
                    //command.OnClick();                    
                }
                else {
                    command.OnClick();
                }
            };
            ToolTip tip = new ToolTip();
            label.MouseEnter += (obj, e) => {
                //tip.ToolTipTitle = command.Tooltip;
                tip.Show(command.Tooltip, label);
            };
            label.MouseLeave += (obj, e) => {
                tip.Hide(label);
            };
            label.Click += handler;
            this.Controls.Add(panel);
            panels.Add(panel);
        }

        void panel_MouseLeave(object sender, EventArgs e) {
            Panel panel = (sender as Label).Parent as Panel;
            panel.BackColor = Color.White;
        }

        void panel_MouseEnter(object sender, EventArgs e) {
            Panel panel = (sender as Label).Parent as Panel;
            panel.BackColor = Color.Azure;
        }
        Point PLoacation(int index) {
            int x = (index % 2 == 0) ? 0 : gridSize.Width + 1;
            int y = (gridSize.Height + 1) * (index / 2);
            return new Point(x, y);
        }
        protected override void OnResize(EventArgs eventargs) {
            base.OnResize(eventargs);
            gridSize.Width = (this.ClientSize.Width - 2) / 2;
            gridSize.Height = gridSize.Width / 2;
        }
    }
    public class IconPanel : ContentPanel {
        protected int iconSpacing;
        protected int margin;

        public int IconSpacing {
            get {
                return iconSpacing;
            }
        }

        public int Margin {
            get {
                return margin;
            }
        }

        public IconPanel() {
            margin = 10;
            iconSpacing = 32 + 15 + 10;	// icon height + text height + margin
            //BackColor=Color;
            AutoScroll = true;
        }


        public void AddIcon(string caption, Image image, EventHandler onClickEvent) {
            int index = Controls.Count / 2;	// two entries per icon
            PanelIcon panelIcon = new PanelIcon(this, image, index, onClickEvent);
            Controls.Add(panelIcon);

            Label label = new Label();
            label.Text = caption;
            label.Visible = true;
            label.Location = new Point(0, margin + image.Size.Height + index * iconSpacing);
            label.Size = new Size(Size.Width, 15);
            label.TextAlign = ContentAlignment.TopCenter;
            label.Click += onClickEvent;
            label.Tag = panelIcon;
            Controls.Add(label);
        }
    }

    public class PanelIcon : PictureBox {
        public int index;
        public IconPanel iconPanel;

        private Color bckgColor;
        private bool mouseEnter;

        public int Index {
            get {
                return index;
            }
        }

        public PanelIcon(IconPanel parent, Image image, int index, EventHandler onClickEvent) {
            this.index = index;
            this.iconPanel = parent;
            Image = image;
            Visible = true;
            Location = new Point(iconPanel.outlookBar.Size.Width / 2 - image.Size.Width / 2,
                            iconPanel.Margin + index * iconPanel.IconSpacing);
            Size = image.Size;
            Click += onClickEvent;
            Tag = this;

            MouseEnter += new EventHandler(OnMouseEnter);
            MouseLeave += new EventHandler(OnMouseLeave);
            MouseMove += new MouseEventHandler(OnMouseMove);

            bckgColor = iconPanel.BackColor;
            mouseEnter = false;
        }

        private void OnMouseMove(object sender, MouseEventArgs args) {
            if ((args.X < Size.Width - 2) &&
                (args.Y < Size.Width - 2) &&
                (!mouseEnter)) {
                BackColor = Color.LightCyan;
                BorderStyle = BorderStyle.FixedSingle;
                Location = Location - new Size(1, 1);
                mouseEnter = true;
            }
        }

        private void OnMouseEnter(object sender, EventArgs e) {
        }

        private void OnMouseLeave(object sender, EventArgs e) {
            if (mouseEnter) {
                BackColor = bckgColor;
                BorderStyle = BorderStyle.None;
                Location = Location + new Size(1, 1);
                mouseEnter = false;
            }
        }
    }

    public class BandTagInfo {
        public OutlookBar OutlookBar { get; private set; }
        public int Index { get; private set; }
        public string Caption { get; private set; }
        public ContentPanel Content { get; private set; }
        internal BandPanel band;
        public BandTagInfo(OutlookBar ob, int index, string caption, ContentPanel content) {
            OutlookBar = ob;
            this.Index = index;
            Caption = caption;
            Content = content;
            band = new BandPanel(caption, content, this);
            ob.Controls.Add(band);
        }
    }

}
