using System;
using UnityEngine;

namespace HR_Toolkit.Thresholds
{
    public abstract class Thresholds : MonoBehaviour
    {
        public enum Warning
        {
            Good,
            Ok,
            Critical, 
            NotSet
        }
        
        public Warning actualWarningState = Warning.NotSet;

        protected Warning CheckForWarnings()
        {
            // TODO
            throw new NotImplementedException();
        }

        public void LogWarnings(LogFile logFile)
        {
            // TODO
            throw new NotImplementedException();
        }
        
    }
}
