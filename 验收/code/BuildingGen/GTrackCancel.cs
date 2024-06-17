///验证不行的代码
using System;
using System.Collections.Generic;
using System.Text;
using ESRI.ArcGIS.esriSystem;
namespace BuildingGen {
    
    class GTrackCancel :ITrackCancel,IStepProgressor{
        WaitOperation wo;
        public GTrackCancel(WaitOperation wo) {
            StepValue = 1;
            this.wo = wo;
        }
        #region IStepProgressor 成员

        public void Hide() {
            wo.Step(0);
        }

        public int MaxRange {
            get;
            set;
        }


        public string Message {
            get;
            set;
        }

        public int MinRange {
            get;
            set;
        }

        public int OffsetPosition(int offsetValue) {
            return offsetValue;
        }

        public int Position {
            get;
            set;
        }

        public void Show() {
            wo.Step(0);
        }

        public void Step() {
            wo.Step((MaxRange - MinRange) /  StepValue);
        }

        public int StepValue {
            get;
            set;
        }

        #endregion

        #region ITrackCancel 成员

        public void Cancel() {            
        }

        public bool CancelOnClick {
            get;
            set;
        }

        public bool CancelOnKeyPress {
            get;
            set;
        }

        public int CheckTime {
            get;
            set;
        }

        public bool Continue() {
            return true;
        }

        public bool ProcessMessages {
            get;
            set;
        }

        public IProgressor Progressor {
            get {
                return this;
            }
            set { 
            }
        }

        public void Reset() {            

        }

        public void StartTimer(int hWnd, int milliseconds) {
            
        }

        public void StopTimer() {
            
        }

        public bool TimerFired {
            get;
            set;
        }

        #endregion
    }
}
