using System;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.ServiceModel.Web;
using Example.UiCoreService.Model.Progress;
using Example.UiCoreService.Model.CoreService;
using Tridion.ContentManager.CoreService.Client;

namespace Example.UiCoreService.Model.Services
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple)]
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Required)]
    [ServiceContract(Namespace = "Example.UiCoreService.Model.Services")]
    public class DecommissionService : BaseService
    {
        class DecommissionParameters
        {
            public string ItemUri { get; set; }
        }

        [OperationContract, WebGet(ResponseFormat = WebMessageFormat.Json)]
        public ServiceProcess Execute(string itemUri)
        {
            if (string.IsNullOrEmpty(itemUri))
            {
                throw new ArgumentNullException("itemUri");
            } 
            
            DecommissionParameters arguments = new DecommissionParameters { ItemUri = itemUri };
            return ExecuteAsync(arguments);
        }

        [OperationContract, WebGet(ResponseFormat = WebMessageFormat.Json)]
        public override ServiceProcess GetProcessStatus(string id)
        {
            return base.GetProcessStatus(id);
        }

        public override void Process(ServiceProcess process, object arguments)
        {
            DecommissionParameters parameters = (DecommissionParameters)arguments;
            using (var coreService = Client.GetCoreService())
            {
                process.SetCompletePercentage(25);
                try
                {
                    coreService.DecommissionPublicationTarget(parameters.ItemUri);
                    process.Complete();
                }
                catch (Exception e)
                {
                    process.SetStatus(e.Message);
                    process.Failed = true;
                }
            }
        }
    }
}
