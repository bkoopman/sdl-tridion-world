using System;
using System.Configuration;
using System.ServiceModel;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Configuration;
using Microsoft.IdentityModel.Protocols.WSTrust;

namespace LocalStsService
{
    /// <summary>
    /// The sts service.
    /// </summary>
    public class StsService
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="StsService"/> class.
        /// </summary>
        public StsService()
        {
            WindowsSecurityTokenServiceConfiguration config = new WindowsSecurityTokenServiceConfiguration();
            config.TrustEndpoints.Add(new ServiceHostEndpointConfiguration(typeof(IWSTrust13AsyncContract), new WS2007HttpBinding(), ConfigurationManager.AppSettings["IssuerName"]));

            Task.Factory.StartNew(() =>
                {
                    try
                    {
                        WSTrustServiceHost host = new WSTrustServiceHost(config);
                        host.Opened += HostOpened;
                        host.Faulted += HostFaulted;
                        host.Closed += HostClosed;
                        host.Open();
                        Console.WriteLine(@"Local STS started on port : {0}", ConfigurationManager.AppSettings["IssuerName"]);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(
                            @"Error there is a isue with the host({0} : {1} ",
                            ConfigurationManager.AppSettings["IssuerName"],
                            ex);
                    }
                });
        }

        /// <summary>
        /// The host_ faulted.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        private static void HostFaulted(object sender, EventArgs e)
        {
            Console.WriteLine(@"SimpleActiveSTS Faulted,{0}", e);
        }

        /// <summary>
        /// The host_ opened.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        private static void HostOpened(object sender, EventArgs e)
        {
            Console.WriteLine(@"SimpleActiveSTS Opened,{0}", e);
        }

        /// <summary>
        /// The host_ closed.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="eventArgs">
        /// The event args.
        /// </param>
        private static void HostClosed(object sender, EventArgs eventArgs)
        {
            Console.WriteLine(@"SimpleActiveSTS stoped,{0}", eventArgs);
        }
    }
}
