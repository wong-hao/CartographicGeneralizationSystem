using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace SMGI.Common
{
    [Serializable]
    public  class LegendSetting
    {
            #region  参数
            public int row = 0;
            public double hdistance = 0.0;
            public double ldistance = 0.0;
            public double textdistance = 0.0;
            public double legendwidth = 0.0;
            public double legendheight = 0.0;
            public double textwidth = 0.0;
            public List<string> legendlist = null;
            #endregion
            long mapscale;
            public LegendSetting()
            {
                SetToDefault();
            }
            public void SetToDefault()  //设置默认
            {
                row = 2;
                hdistance = 6;
                ldistance = 43;
                textdistance = 2;
                legendwidth = 18;
                legendheight = 9;
                textwidth = 17;
                legendlist = new List<string>();
            }
            public void LoadDefaultList()
            {
                legendlist.Clear();
                legendlist.Add("310200");
                legendlist.Add("310500");
                legendlist.Add("310300");
//                legendlist.Add("1000000");
//                legendlist.Add("1000000");
//                legendlist.Add("1000000");
//                legendlist.Add("1000000");
//                legendlist.Add("1000000");
                legendlist.Add("410102");
                legendlist.Add("410101");
                legendlist.Add("430102");
                legendlist.Add("430101");
                legendlist.Add("430200");
                legendlist.Add("420101");
                legendlist.Add("420201");
                legendlist.Add("620201");
                legendlist.Add("630201");
                legendlist.Add("640201");
                legendlist.Add("650201");
                legendlist.Add("360400");
                legendlist.Add("360100");
                legendlist.Add("340303");
                legendlist.Add("350201");
                legendlist.Add("340101");
                legendlist.Add("340102");
                legendlist.Add("460401");
                legendlist.Add("460300");
                legendlist.Add("地面河流");
                legendlist.Add("湖泊");
                legendlist.Add("水库");
                legendlist.Add("明礁（单个的、丛礁）");
                legendlist.Add("暗礁（单个的、丛礁）");
                legendlist.Add("干出礁（单个的、丛礁）");
                legendlist.Add("孤峰、峰丛");
                legendlist.Add("成林");
            }
        }
}
