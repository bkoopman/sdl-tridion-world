using System;

namespace Example.UiCoreService.Model.Progress
{
    public class ServiceProcess
    {
        private readonly object _lock = new object();

        public ServiceProcess()
        {
            Id = Guid.NewGuid().ToString();
            SetStatus(Resources.ProgressStatusInitializing);
        }

        public string Id { get; set; }
        public string Status { get; protected set; }
        public int PercentComplete { get; set; }
        public bool Failed { get; set; }

        public void Complete()
        {
            Complete(Resources.ProgressStatusComplete);
        }

        public void Complete(string status)
        {
            SetCompletePercentage(100);
            SetStatus(status);
        }

        public void SetCompletePercentage(int percent)
        {
            lock (_lock)
            {
                PercentComplete = percent;
            }
        }

        public void IncrementCompletePercentage()
        {
            lock (_lock)
            {
                PercentComplete++;
            }
        }

        public void IncrementCompletePercentageBy(int percent)
        {
            lock (_lock)
            {
                PercentComplete += percent;
            }
        }

        public void SetStatus(string status)
        {
            lock (_lock)
            {
                Status = status;
            }
        }
    }
}