using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SMGI.Common
{
    [Serializable]
     public class PaperSeting         
    {
        [NonSerialized]
        GWorkspace w;
        public double Width {
            get {
                double widthInches, heightInches;
                w.Map.GetPageSize(out widthInches, out heightInches);
                return widthInches;
            }
            set {
                w.Map.SetPageSize(value, this.Height);
            }
        }
        public double Height
        {
            get
            {
                double widthInches, heightInches;
                w.Map.GetPageSize(out widthInches, out heightInches);
                return widthInches;
            }
            set
            {
                w.Map.SetPageSize(this.Width,value);
            }
        }
        public double mapX;
        public double mapY;
        PaperSeting(GWorkspace workspace) {
             w = workspace;
            
        }

    }
}
