using System.ServiceModel;

namespace Example.UiCoreService.Model.Progress
{
    public class ServiceProcessHelper : IExtension<InstanceContext>
    {
        public ServiceProcessHelper(ServiceProcess process)
        {
            Process = process;
        }

        public ServiceProcess Process { get; private set; }

        public void Attach(InstanceContext owner)
        {
        }

        public void Detach(InstanceContext owner)
        {
        }
    }
}