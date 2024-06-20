using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SMGI.Common
{
    public class ProcessTrigger
    {
        private double percent;
        public double Percent
        {
            get { return percent; }
        }

        private bool matchMapConfig = false;
        public bool MatchMapConfig
        {
            get { return matchMapConfig; }
            set { 
                if(value != matchMapConfig)
                {
                    matchMapConfig = value;
                    CalculateProcess();
                }
            }
        }

        private bool waterLine = false;
        public bool WaterLine
        {
            get { return waterLine; }
            set {
                if(value != waterLine)
                {
                    waterLine = value;
                    CalculateProcess();
                }
            }
        }

        private bool dljProcess = false;
        public bool DljProcess
        {
            get { return dljProcess; }
            set {
                if(value != dljProcess)
                {
                    dljProcess = value;
                    CalculateProcess();
                }
            }
        }

        private bool calculatePIPL = false;
        public bool CalculatePIPL
        {
            get { return calculatePIPL; }
            set {
                if(value != calculatePIPL)
                {
                    calculatePIPL = value;
                    CalculateProcess();
                }
            }
        }

        private bool calculateAngle = false;
        public bool CalculateAngle
        {
            get { return calculateAngle; }
            set {
                if (value != calculateAngle)
                {
                    calculateAngle = value;
                    CalculateProcess();
                }
            }
        }

        private bool roadsetConflict = false;
        public bool RoadsetConflict
        {
            get { return roadsetConflict; }
            set {
                if (value != roadsetConflict)
                {
                    roadsetConflict = value;
                    CalculateProcess();
                }
            }
        }

        private bool disperseSymbol = false;
        public bool DisperseSymbol
        {
            get { return disperseSymbol; }
            set {
                if (value != disperseSymbol)
                {
                    disperseSymbol = value;
                    CalculateProcess();
                }
            }
        }

        private bool labelConfig = false;
        public bool LabelConfig
        {
            get { return labelConfig; }
            set {
                if (value != labelConfig)
                {
                    labelConfig = value;
                    CalculateProcess();
                }
            }
        }

        private bool convertAnnotation = false;
        public bool ConvertAnnotation
        {
            get { return convertAnnotation; }
            set {
                if (value != convertAnnotation)
                {
                    convertAnnotation = value;
                    CalculateProcess();
                }
            }
        }

        private bool annoFeatureConflict = false;
        public bool AnnoFeatureConflict
        {
            get { return annoFeatureConflict; }
            set {
                if (value != annoFeatureConflict)
                {
                    annoFeatureConflict = value;
                    CalculateProcess();
                }
            }
        }

        private bool maskConfig = false;
        public bool MaskConfig
        {
            get { return maskConfig; }
            set {
                if (value != maskConfig)
                {
                    maskConfig = value;
                    CalculateProcess();
                }
            }
        }

        private bool margin = false;
        public bool Margin
        {
            get { return margin; }
            set { 
                if(value != margin)
                {
                    margin = value;
                    CalculateProcess();
                }
            }
        }

        private bool outElement = false;
        public bool OutElement
        {
            get { return outElement; }
            set {
                if (value != outElement)
                {
                    outElement = value;
                    CalculateProcess();
                }
            }
        }

        private bool legend = false;
        public bool Legend
        {
            get { return legend; }
            set {
                if (value != legend)
                {
                    legend = value;
                    CalculateProcess();
                }
            }
        }

        private void CalculateProcess()
        {
            Type t = typeof(ProcessTrigger);
        }
    }
}
