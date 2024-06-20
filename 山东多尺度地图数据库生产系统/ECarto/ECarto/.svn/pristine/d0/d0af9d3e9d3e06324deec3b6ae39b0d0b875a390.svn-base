using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ESRI.ArcGIS.SystemUI;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Threading;

namespace SMGI.Common
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public sealed class SMGIHotKeyAttribute : Attribute
    {
        // See the attribute guidelines at 
        //  http://go.microsoft.com/fwlink/?LinkId=85236
        public SMGIHotKeyAttribute(bool Ctrl, bool Shift, bool Alt, Keys KeyCode)
        {
            this.Key = new HotKeyInfo(Ctrl,Shift,Alt,KeyCode);
        }
        public HotKeyInfo Key { get; private set; }
    }
    [Serializable]
    public class HotKeyInfo
    {
        public bool Alt;
        public bool Ctrl;
        public bool Shift;
        public Keys KeyCode;

        public HotKeyInfo(string keyString) {
            Alt = false; Ctrl = false; Shift = false;
            if (keyString.StartsWith("Ctrl+")) {
                Ctrl = true;
                keyString = keyString.Substring(5);
            }
            if (keyString.StartsWith("Shift+"))
            {
                Shift = true;
                keyString = keyString.Substring(6);
            }
            if (keyString.StartsWith("Alt+"))
            {
                Alt = true;
                keyString = keyString.Substring(4);
            }
            KeyCode = (Keys)Enum.Parse(KeyCode.GetType(), keyString,true);
        }
        public HotKeyInfo(bool Ctrl, bool Shift, bool Alt, Keys KeyCode)
        {
            this.Alt = Alt;
            this.Ctrl = Ctrl;
            this.Shift = Shift;
            this.KeyCode = KeyCode;
        }

        public HotKeyInfo(KeyEventArgs e) {
            this.Alt = e.Alt;
            this.Ctrl = e.Control;
            this.Shift = e.Shift;
            this.KeyCode = e.KeyCode;
        }

        public bool IsHotKey(KeyEventArgs e)
        {
            return this.Alt == e.Alt
            &&this.Ctrl == e.Control
            &&this.Shift == e.Shift
            &&this.KeyCode == e.KeyCode;
        }
        public static string GetString(bool Alt, bool Ctrl, bool Shift, Keys KeyCode)
        {
            string r = Enum.Format(KeyCode.GetType(), KeyCode, "G");
            if (Alt)
            {
                r = "Alt+" + r;
            }
            if (Shift)
            {
                r = "Shift+" + r;
            }
            if (Ctrl)
            {
                r = "Ctrl+" + r;
            }
            return r;
        }
        public override string ToString()
        {
            if (this == Empty)
                return string.Empty;
            return GetString(Alt, Ctrl, Shift, KeyCode);
        }
        public override int GetHashCode()
        {
            return this.ToString().GetHashCode();
        }
        public override bool Equals(object obj)
        {
            if (!(obj is HotKeyInfo))
                return false;
            var other = obj as HotKeyInfo;

            return this.Alt == other.Alt && this.Ctrl == other.Ctrl
                && this.Shift == other.Shift && this.KeyCode == other.KeyCode;
        }

        public static HotKeyInfo Empty = new HotKeyInfo(false, false, false, Keys.F24);
    }
}
