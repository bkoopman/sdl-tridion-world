using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AppData.Tridion;
using System.ServiceModel.Description;
using System.ServiceModel;


namespace AppData
{
    class Program
    {
        static ReadOptions DEFAULT_READ_OPTIONS = new ReadOptions();

        static string BINDING_NAME = "basicHttp1";
        static System.Xml.Linq.XNamespace TRIDION_NAMESPACE = "http://www.tridion.com/ContentManager/5.0";

        public delegate void ProcessApplicationData(string itemUri, ApplicationData data, ICoreService client);

        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("AppData <command> [parameters]");
                Console.WriteLine("  e.g.: AppData view <contextTcmUri> [applicationId]");
                Console.WriteLine("  e.g.: AppData delete <contextTcmUri> <applicationId>");
                Console.WriteLine("  e.g.: AppData set <TcmUri> <applicationId> [\"value\"]");
                Console.WriteLine("  e.g.: AppData x|export|download <TcmUri> <applicationId> \"<saveFilename>\"");
                Console.WriteLine("  e.g.: AppData u[pload]|import <TcmUri> <applicationId> \"<filename>\"");
                Environment.Exit(0);
            }

            if (args[0].Equals("view", StringComparison.InvariantCultureIgnoreCase) || args[0].ToLower() == "v")
            {
                if (args.Length != 2 && args.Length != 3)
                {
                    Console.WriteLine("AppData v[iew] <contextTcmUri> [applicationId]");
                    Environment.Exit(0);
                }
                string contextUri = args[1];
                string appId = args.Length > 2 ? args[2] : ".*";
                ViewAppData(contextUri, appId);
            }
            else if (args[0].Equals("delete", StringComparison.InvariantCultureIgnoreCase) || args[0].ToLower() == "d")
            {
                if (args.Length != 3)
                {
                    Console.WriteLine("AppData d[elete] <contextTcmUri> <applicationId>");
                    Environment.Exit(0);
                }
                string contextUri = args[1];
                string appId = args[2];
                DeleteAppData(contextUri, appId);
            }
            else if (args[0].Equals("set", StringComparison.InvariantCultureIgnoreCase) || args[0].ToLower() == "s")
            {
                if (args.Length <3 || args.Length > 4)
                {
                    Console.WriteLine("AppData s[et] <TcmUri> <applicationId> [\"value\"]");
                    Environment.Exit(0);
                }
                string itemUri = args[1];
                string appId = args[2];
                //string value = args.Skip(3).Aggregate((total, next) => total + " " + next);
                string value = args.Length == 4 ? args[3] : "";
                SetAppData(itemUri, appId, value);
            }
            else if (args[0].Equals("download", StringComparison.InvariantCultureIgnoreCase) || args[0].Equals("export", StringComparison.InvariantCultureIgnoreCase) || args[0].ToLower() == "x")
            {
                if (args.Length != 4)
                {
                    Console.WriteLine("AppData x|export|download <TcmUri> <applicationId> \"<saveFilename>\"");
                    Environment.Exit(0);
                }
                string itemUri = args[1];
                string appId = args[2];
                string fileName = args[3];
                DownloadAppData(itemUri, appId, fileName);
            }
            else if (args[0].Equals("upload", StringComparison.InvariantCultureIgnoreCase) || args[0].Equals("import", StringComparison.InvariantCultureIgnoreCase) || args[0].ToLower() == "u")
            {
                if (args.Length != 4)
                {
                    Console.WriteLine("AppData u[pload]|import <TcmUri> <applicationId> \"<filename>\"");
                    Environment.Exit(0);
                }
                string itemUri = args[1];
                string appId = args[2];
                string fileName = args[3];
                UploadAppData(itemUri, appId, fileName);
            }
        }

        // static ICoreService2010 GetNewClient()
        static ICoreService GetNewClient()
        {
            return new CoreServiceClient(BINDING_NAME);
        }

        static void ForEachApplicationData(string contextUri, ProcessApplicationData callback)
        {
            var client = GetNewClient();
            try
            {
                ForEachApplicationData(client, contextUri, callback);
            }
            finally
            {
                if (client is CoreServiceClient) ((CoreServiceClient)client).Close();
                ((IDisposable)client).Dispose();
            }
        }
        static void ForEachApplicationData(ICoreService client, string itemUri, ProcessApplicationData callback)
        {
            foreach (var appData in client.ReadAllApplicationData(itemUri))
            {
                callback(itemUri, appData, client);
            }
            var context = client.Read(itemUri, DEFAULT_READ_OPTIONS);
            if (context is OrganizationalItemData) // TODO: also handle Publication
            {
                var filter = new OrganizationalItemItemsFilterData();
                //filter.Recursive = true; // faster, but order is undefined
                var items = client.GetListXml(itemUri, filter).Elements(TRIDION_NAMESPACE + "Item");
                foreach (var element in items)
                {
                    var childUri = element.Attribute("ID").Value;
                    ForEachApplicationData(client, childUri, callback);
                }
            }
        }
        static void ForEachApplicationDataThatMatches(string contextUri, string applicationId, ProcessApplicationData callback)
        {
            System.Text.RegularExpressions.Regex regex = new System.Text.RegularExpressions.Regex(applicationId);
            ForEachApplicationData(contextUri, (itemUri, appData, client) =>
            {
                if (regex.IsMatch(appData.ApplicationId))
                {
                    callback(itemUri, appData, client);
                }
            });
        }


        static void ViewAppData(string contextUri, string applicationId)
        {
            ForEachApplicationDataThatMatches(contextUri, applicationId, (itemUri, appData, client) =>
            {
                bool isAscii = appData.Data.All(c => c >= 32 && c < 127);
                var value = isAscii ? new ASCIIEncoding().GetString(appData.Data) : "<The value is not an ASCII string. Save it to a file using AppData export ...>";
                Console.WriteLine(string.Format("{0}.{1}={2}", itemUri, appData.ApplicationId, value));
            });
        }

        static void DeleteAppData(string contextUri, string applicationId)
        {
            ForEachApplicationDataThatMatches(contextUri, applicationId, (itemUri, appData, client) =>
            {
                client.DeleteApplicationData(itemUri, appData.ApplicationId);
                Console.WriteLine(string.Format("Deleted {0}.{1}", itemUri, appData.ApplicationId));
            });
        }

        static void SetAppData(string itemUri, string applicationId, string value)
        {
            var client = GetNewClient();
            var appData = new ApplicationData();
            appData.ApplicationId = applicationId;
            appData.Data = new ASCIIEncoding().GetBytes(value);
            client.SaveApplicationData(itemUri, new ApplicationData[] { appData });
            //client.Close();
            if (client is CoreServiceClient) ((CoreServiceClient)client).Close();
            Console.WriteLine(string.Format("{0}.{1}={2}", itemUri, applicationId, value));
        }

        static void DownloadAppData(string itemUri, string applicationId, string fileName)
        {
            var client = GetNewClient();
            var appData = client.ReadApplicationData(itemUri, applicationId);
            System.IO.File.WriteAllBytes(fileName, appData.Data);
            if (client is CoreServiceClient) ((CoreServiceClient)client).Close();
            Console.WriteLine(string.Format("{0}.{1} written to {2}", itemUri, applicationId, fileName));
        }

        static void UploadAppData(string itemUri, string applicationId, string fileName)
        {
            var client = GetNewClient();
            var appData = new ApplicationData();
            appData.ApplicationId = applicationId;
            appData.Data = System.IO.File.ReadAllBytes(fileName);
            client.SaveApplicationData(itemUri, new ApplicationData[] { appData });
            if (client is CoreServiceClient) ((CoreServiceClient)client).Close();
            Console.WriteLine(string.Format("{0}.{1} read from {2}", itemUri, applicationId, fileName));
        }
    } // class Program
}
