
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

namespace SMGI.Common
{
    public enum SMGISubSystem { 
        基础功能 = 1,
        常规编辑=2,
        制图编辑 = 3,
        地形图专题 = 4,
        应急专题 = 5,  
        地图综合=6,
        整饰输出=7,
        地理国情=8,
        其他功能 = 0x7FFFFFFF,
    }
    public interface ISMGIPlugin {
        void setApplication(GApplication app);
        SMGISubSystem SubSystem { get; }
        event EventHandler PluginChanged;
    }

    public class MessageEventArgs : EventArgs
    {
        public MessageEventArgs(string m) {
            Message = m;
        }
        public string Message { get; set; }
    }
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public sealed class SMGIAutomaticCommandAttribute : Attribute
    {
        public SMGIAutomaticCommandAttribute()
        {
        }
    }
    public interface ISMGIAutomaticCommand
    {
        bool DoCommand(XElement args,Action<string> messageRaisedAction);
    }

//#if zhouqi
    public class PluginManager
    {
        GApplication application;
        IPluginHost hostForm;
        string pluginPath { get { return GApplication.PluginPath; } }

        private List<ToolStripMenuItem> contextMenuItems; //包含所有上下文菜单（弹出菜单）

        public Dictionary<string,PluginCommand> Commands { get; private set; }
        
        public ToolbarControlClass ESRIToolbar { get; private set; }
        ICommandPool2 commandPool;

        public TOCSeleteItem CurrentContextItem { get; set; }

        public PluginManager(GApplication app)
        {
            this.application = app;
            this.hostForm = app.MainForm;
            app.PluginManager = this;
            
            if (!Directory.Exists(pluginPath))
            {
                Directory.CreateDirectory(pluginPath);
            }

            contextMenuItems = new List<ToolStripMenuItem>();
            Commands = new Dictionary<string, PluginCommand>();

            hostForm.MapLayoutChanged += new EventHandler<LayoutChangedArgs>(hostForm_MapLayoutChanged);

            ESRIToolbar = new ToolbarControlClass();
            ESRIToolbar.CommandPool = hostForm.CommandPool;
            commandPool = hostForm.CommandPool as ICommandPool2;

            this.LoadPlugins();

            app.TOCControl.OnMouseUp += new ITOCControlEvents_Ax_OnMouseUpEventHandler(TOCControl_OnMouseUp);
        }

        void hostForm_MapLayoutChanged(object sender, LayoutChangedArgs e)
        {
            if (hostForm.LayoutState == LayoutState.MapControl)
            {
                ESRIToolbar.SetBuddyControl(this.application.MapControl.Object);
            }
            else
            {
                ESRIToolbar.SetBuddyControl(this.application.PageLayoutControl.Object);
            }
        }

        private void LoadPlugins()
        {
            if (application == null)
                return;
            
            List<string> assemblyNames = new List<string>();
            assemblyNames.AddRange(Directory.GetFiles(pluginPath, "*.dll", SearchOption.AllDirectories));

            List<PluginCommand> allCommandInfo = new List<PluginCommand>();
            #region 1.生成所有的CommandInfo
            for (int i = 0; i < assemblyNames.Count; i++)
            {
                try
                {
                    Assembly assembly = Assembly.LoadFrom(assemblyNames[i]);
                    if (assembly.ManifestModule.Name.StartsWith("SMGI.Plugin") )
                    {
                        var ab = assembly.GetCustomAttributes(typeof(SMGIProductAttribute), true);
                        string productName = "ECarto";
                        if (ab.Length > 0)
                        {
                            var at = ab.First() as SMGIProductAttribute;
                            if (at != null)
                                productName = at.SubSystem;
                        }

                        if (!application.Template.Products.Contains(productName))
                        {
                            continue;
                        }

                        var infos = TraversalAssembly(assembly);
                        allCommandInfo.AddRange(infos);
                    }
                }
                catch//可能出现原生DLL
                {
                    continue;
                }
            }
            #endregion
           
            #region 6.创建上下文菜单
            List<PluginCommand> cmds = new List<PluginCommand>();
            foreach (var info in allCommandInfo)
            {
                if (info.Command is ISMGIContextMenu)
                {
                    var ctxItem = CreateToolStripItem(info);
                    if (ctxItem is ToolStripMenuItem)
                        contextMenuItems.Add(ctxItem as ToolStripMenuItem);
                }
                else
                {
                    cmds.Add(info);
                }
            }
            this.Commands = allCommandInfo.ToDictionary((info) => { return info.ToString(); });

            #endregion
            #region 写下所有命令（输出载入的所有命令）
            #if zhouqi
            
                CommandGroup group = new CommandGroup
                {
                    Caption = "All Commands",
                    Children = (from x in cmds
                                select new CommandItem
                                {
                                    Caption = x.Command.Caption,
                                    ClassName = x.Command.GetType().FullName,
                                    ToolTip = x.Command.Tooltip
                                }).ToArray()
                };

                string v = group.WriteToXml().ToString();
            

            //if (application.TemplateManager.Template == null)
            //{
            //    application.MainForm.SetupCommands(group, Commands);
            //}
            #endif
            #endregion

            if (application.Template != null)
                application.MainForm.SetupCommands(application.Template.XCommandLayout, Commands);
            
        }

        internal ToolStripItem CreateToolStripItem(PluginCommand info)
        {
            var Command = info.Command;
            EventHandler eh = (o, e) => info.OnClick();
            if (Command is IToolControl)
            {
                ToolStripControlHost ti = new ToolStripControlHost(
                    Control.FromHandle(new IntPtr((Command as IToolControl).hWnd))
                    );
                ti.Tag = info;
                ti.Click += eh;
                return ti;
            }
            else
            {
                ToolStripItem toolbutton = null;

                    ToolStripMenuItem tm = new ToolStripMenuItem();
                    toolbutton = tm;
                    tm.CheckOnClick = Command is ITool;
               
                toolbutton.Text = Command.Caption;
                toolbutton.Tag = info;
                toolbutton.AutoSize = true;
                //toolbutton.ToolTipText = obj.Tooltip;
                if (Command.Bitmap != 0)
                {
                    toolbutton.Image = System.Drawing.Image.FromHbitmap(new IntPtr(Command.Bitmap));
                }

                toolbutton.Click += eh;
                return toolbutton;
            }
        }

        void TOCControl_OnMouseUp(object sender, ITOCControlEvents_OnMouseUpEvent e)
        {
            if (e.button != 2)
                return;
            {
                esriTOCControlItem pItem = esriTOCControlItem.esriTOCControlItemNone;
                IBasicMap pMap = null;
                ILayer pLayer = null;
                object pOther = new object();
                object pIndex = new object();
                (sender as AxTOCControl).HitTest(e.x, e.y, ref pItem, ref pMap, ref pLayer, ref pOther, ref pIndex);
                System.Drawing.Point p = new System.Drawing.Point();
                p.X = e.x;
                p.Y = e.y;



                if (pItem == esriTOCControlItem.esriTOCControlItemNone)
                {
                    return;
                }

                ILegendClass cls = null;
                if (pItem == esriTOCControlItem.esriTOCControlItemLegendClass)
                {

                    cls = (pOther as ILegendGroup).get_Class(System.Convert.ToInt32(pIndex));
                }
                CurrentContextItem = new TOCSeleteItem(pItem, pMap, pLayer, pOther as ILegendGroup, cls);

                ContextMenuStrip menu = new ContextMenuStrip();
                var items = from mi in contextMenuItems
                            where (mi.Tag as PluginCommand).Command.Enabled
                            select mi;
                menu.Items.AddRange(items.ToArray());
                menu.Show(application.TOCControl, e.x, e.y);
            }
        }

        public List<PluginCommand> TraversalAssembly(Assembly assembly)
        {
            Module[] modules = assembly.GetModules(false);
            List<PluginCommand> infos = new List<PluginCommand>();
            foreach (Module module in modules)
            {
                Type[] types = module.GetTypes();
                foreach (Type type in types)
                {
                    try
                    {
                        Type icommandType = type.GetInterface(typeof(ICommand).Name);
                        
                        if (icommandType != null && type.IsClass)
                        {
                            var rinfos = CreateCommandInstance(type);
                            if (rinfos != null)
                                infos.AddRange(rinfos);
                        }
                    }
                    catch
                    {
                        continue;
                    }
                }
            }
            return infos;
        }

        private ICommand CreateCommand(Type type)
        {
            object obj = Activator.CreateInstance(type);
            //QI 
            ICommand command = obj as ICommand;
            (command as ISMGIPlugin).setApplication(this.application);
            if (!(command is ICommandSubType))
            {
                ESRIToolbar.AddItem(command);
            }
            if (!(commandPool.Exists(command)))
            {

                (commandPool as ICommandPoolEdit).AddCommand(command, null);
            }
            
            return command;
        }

        private List<PluginCommand> CreateCommandInstance(Type type)
        {
            try
            {
                /*Check for ICommand Interface, COMObject, Class，还要判断其为抽象类*/
                if (type.GetInterface(typeof(ICommand).Name) == null ||
                    type.GetInterface(typeof(ISMGIPlugin).Name) == null ||
                    !type.IsClass ||
                    type.IsAbstract ||
                    !type.IsPublic)
                {
                    return null;
                }

                //创建该命令的***********唯一实例************
                
                int subTypeCount;
                var command = CreateCommand(type);
                if (command is ICommandSubType)
                {
                    //QI for CommandSubType
                    ICommandSubType m_commandSubType = command as ICommandSubType;
                    subTypeCount = m_commandSubType.GetCount();
                }
                else
                {
                    subTypeCount = 1;
                }

                List<PluginCommand> infos = new List<PluginCommand>();

                infos.Add(new PluginCommand(command, 1, application));

                for (int i = 2; i <= subTypeCount; i++)
                {
                    PluginCommand info = new PluginCommand(CreateCommand(type), i, application);

                    infos.Add(info);

                }
                return infos;
            }
            catch (Exception e)
            {
                throw new Exception("未能成功创建命令");
            }
        }
    }
//#endif
}
