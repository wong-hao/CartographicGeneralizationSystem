using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;

namespace SMGI.Common
{
    /// <summary>
    /// 窗口停靠隐藏类
    /// 使用方法
    /// private FormDock formDock = null;
    /// formDock = new FormDock(this,300);
    /// </summary>
    public class FormDock
    {
        #region 自定义声明
        /// <summary>
        /// 定义计时器
        /// </summary>
        private Timer StopRectTimer = new Timer();

        /// <summary>
        /// 贴边设置
        /// </summary>
        internal AnchorStyles StopAanhor = AnchorStyles.None;

        /// <summary> 
        /// 父级窗口实例 
        /// </summary> 
        private Form parentForm = null;

        private Point m_TempPoiont;//临时点位置
        private Point m_LastPoint;//窗体最小化前的坐标点位置

        #endregion

        #region 构造函数
        /// <summary> 
        /// 自动停靠
        /// </summary> 
        /// <param name="frmParent">父窗口对象</param> 
        public FormDock(Form frmParent)
        {
            parentForm = frmParent;
            parentForm.LocationChanged += new EventHandler(parentForm_LocationChanged);
            StopRectTimer.Tick += new EventHandler(timer1_Tick);    //注册事件
            StopRectTimer.Interval = 500;                           //计时器执行周期
            StopRectTimer.Start();                                  //计时器开始执行
        }
        /// <summary>
        /// 自动停靠 
        /// </summary>
        /// <param name="frmParent">父窗口对象</param>
        /// <param name="_trimInterval">时钟周期</param>
        public FormDock(Form frmParent, int _trimInterval)
        {
            parentForm = frmParent;
            parentForm.LocationChanged += new EventHandler(parentForm_LocationChanged);
            StopRectTimer.Tick += new EventHandler(timer1_Tick);    //注册事件
            StopRectTimer.Interval = _trimInterval;                           //计时器执行周期
            StopRectTimer.Start();                                  //计时器开始执行
        }
        #endregion

        /// <summary>
        /// 时钟的开始
        /// </summary>
        public void TimerStart()
        {
            StopRectTimer.Start();
        }

        /// <summary>
        /// 时钟的停止
        /// </summary>
        public void TimerStop()
        {
            StopRectTimer.Stop();
        }

        #region 窗口位置改变事件
        /// <summary>
        /// 窗口位置改变事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void parentForm_LocationChanged(object sender, EventArgs e)
        {
            if (parentForm.Location.X == -32000 && parentForm.Location.Y == -32000)
            {
                m_LastPoint = m_TempPoiont;//最小化了，m_LastPoint就是最小化前的位置。
            }
            else
            {
                m_TempPoiont = parentForm.Location;
            }

            this.mStopAnthor();
        }
        #endregion

        #region 计时器 周期事件
        /// <summary>
        /// 计时器 周期事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void timer1_Tick(object sender, EventArgs e)
        {
            if (parentForm.Bounds.Contains(Cursor.Position))
            {
                this.FormShow();
            }
            else
            {
                this.FormHide();
            }
        }
        #endregion

        #region 窗口停靠位置计算
        /// <summary>
        /// 窗口停靠位置计算
        /// </summary>
        private void mStopAnthor()
        {
            if (parentForm.Top <= 0)
            {
                StopAanhor = AnchorStyles.Top;
            }
            else if (parentForm.Left <= 0)
            {
                StopAanhor = AnchorStyles.Left;
            }
            else if (parentForm.Left >= Screen.PrimaryScreen.Bounds.Width - parentForm.Width)
            {
                StopAanhor = AnchorStyles.Right;
            }
            else
            {
                StopAanhor = AnchorStyles.None;
            }
        }
        #endregion

        #region 窗体不贴边显示
        /// <summary>
        /// 窗体不贴边显示
        /// </summary>
        public void FormShow()
        {
            switch (this.StopAanhor)
            {
                case AnchorStyles.Top:
                    parentForm.Location = new Point(parentForm.Location.X, 0);
                    break;
                case AnchorStyles.Left:
                    parentForm.Location = new Point(0, parentForm.Location.Y);
                    break;
                case AnchorStyles.Right:
                    parentForm.Location = new Point(Screen.PrimaryScreen.Bounds.Width - parentForm.Width, parentForm.Location.Y);
                    break;
            }
        }
        #endregion

        #region 窗体贴边隐藏
        /// <summary>
        /// 窗体贴边隐藏
        /// </summary>
        private void FormHide()
        {
            switch (this.StopAanhor)
            {
                case AnchorStyles.Top:
                    if (parentForm.WindowState == FormWindowState.Minimized)
                    {
                        parentForm.Location = this.m_LastPoint;
                        break;
                    }
                    parentForm.Location = new Point(parentForm.Location.X, (parentForm.Height - 2) * (-1));
                    break;
                case AnchorStyles.Left:
                    parentForm.Location = new Point((-1) * (parentForm.Width - 2), parentForm.Location.Y);
                    break;
                case AnchorStyles.Right:
                    parentForm.Location = new Point(Screen.PrimaryScreen.Bounds.Width - 2, parentForm.Location.Y);
                    break;
            }
        }
        #endregion
    }
}
