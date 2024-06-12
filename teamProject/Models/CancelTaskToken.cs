
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace teamProject.Models
{
    public class CancelTaskToken
    {
        private bool isCanceled = false;
        private bool isStopped = false;
        private bool isCompleted = false;

        public void Cancel()
        {
            isCanceled = true;
            isStopped = true;
        }

        public void Stop()
        {
            isStopped = true;
            isCanceled = true;
            isCompleted = true;
        }

        public bool IsCompleted()
        {
            return isCompleted;
        }

        public bool IsCancelRequested()
        {
            return isCanceled;
        }

        public bool IsStopped()
        {
            return isStopped;
        }
    }
}
