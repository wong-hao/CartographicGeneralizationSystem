using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq;
namespace NetDesign.Com
{
    public class BreakClass
    {
        private static BreakClass application;
        public static BreakClass Application
        {
            get
            {
                if (application == null)
                    application = new BreakClass();
                return application;
            }
        }
        public Dictionary<string, string> RegionBreakValues(Dictionary<string, double> gdbs, List<double> breaks)
        {
          

            Dictionary<string, string> grade = new Dictionary<string, string>();

            foreach (var kv in gdbs)
            {
                for (int i = 0; i < breaks.Count; i++)
                {
                    if (kv.Value <= breaks[i])
                    {
                        if (i == 0)
                        {
                            grade[kv.Key] = "0—" + breaks[i];
                        }
                        else
                        {
                            grade[kv.Key] = breaks[i - 1] + "—" + breaks[i];
                        }
                        break;

                    }
                }
                if (!grade.ContainsKey(kv.Key))
                {
                    grade[kv.Key] = breaks[breaks.Count - 1] + "—";
                }
            }
            return grade;
        }

        public Dictionary<string, string> BreakValues(Dictionary<string, double> gdbs, int num)
        {
            double max = gdbs.Max(t => t.Value);
            double min = gdbs.Min(t => t.Value);
            double step = (max - min) / num;
            List<double> breaks = new List<double>();
            for(int i=1;i<num;i++)
            {
                breaks.Add(Math.Round(min + i * step,2));
            }

            Dictionary<string, string> grade = new Dictionary<string, string>();

            foreach (var kv in gdbs)
            {
                for (int i = 0; i < breaks.Count; i++)
                {
                    if (kv.Value <= breaks[i])
                    {
                        if (i == 0)
                        {
                            grade[kv.Key] = "0—" + breaks[i];
                        }
                        else
                        {
                            grade[kv.Key] = breaks[i - 1] + "—" + breaks[i];
                        }
                        break;

                    }
                }
                if (!grade.ContainsKey(kv.Key))
                {
                    grade[kv.Key] = breaks[breaks.Count - 1] + "—";
                }
            }
            return grade;
        }
    }
}
