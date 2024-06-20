using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ESRI.ArcGIS.Controls;
using System.ComponentModel;
using System.Xml.Linq;

namespace SMGI.Common
{
    public interface ICommandInfo
    { 
    }
    public enum LayoutState { 
        MapControl,
        PageLayoutControl
    }
    public class LayoutChangedArgs:EventArgs
    {
        public LayoutState CurrentState { get; set; }
        public LayoutState PrevState { get; set; }
        public LayoutChangedArgs(LayoutState currentState, LayoutState prevState)
        {
            this.CurrentState = currentState;
            this.PrevState = prevState;
        }
    }
    public interface IESRIEnvironment
    {
        AxMapControl MapControl { get; }
        AxPageLayoutControl PageLayoutControl { get; }
        AxTOCControl TocControl { get; }
        ICommandPool CommandPool { get; }
    }
    public interface IFormEnvironment
    {
        IntPtr Handle { get; }
        string Title { get; set; }
        bool BusyStatus { set; }
        void ShowStatus(string status);
        void ShowToolDes(string status);
        void ShowChild(IntPtr handle);
        void CloseChild(IntPtr handle);
        event EventHandler<CancelEventArgs> Closing;
    }
    public enum FormLocation
    {
        Right=0,
        Left=1,
        Top=2,
        Down=3,
    }
    public interface IFormEnvironment2
    {
        void ShowChild2(IntPtr handle,FormLocation location); 
    }
    public interface ILayoutState
    {
        LayoutState LayoutState { get; set; }
        event EventHandler<LayoutChangedArgs> MapLayoutChanged;
    }
    public interface IPluginHost : IESRIEnvironment, IFormEnvironment, ILayoutState,IFormEnvironment2
    {
        void SetupCommands(XDocument commandinfo, Dictionary<string, PluginCommand> commands);        
    }

    public interface ISMGIMainForm
    {
        AxToolbarControl MapHBar { get; }
        AxToolbarControl MapVBar { get; }
        AxToolbarControl PageHBar { get; }
        AxToolbarControl PageVBar { get; }
        object DockManager { get; }
    }
}
