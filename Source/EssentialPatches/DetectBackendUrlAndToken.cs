using Newtonsoft.Json;
using System;

namespace StayInTarkov.EssentialPatches
{
    public class DetectBackendUrlAndToken
    {
        public string BackendUrl { get; }
        public string Version { get; }

        public string PHPSESSID { get; private set; }

        public DetectBackendUrlAndToken(string backendUrl, string version)
        {
            BackendUrl = backendUrl;
            Version = version;
        }

        private static DetectBackendUrlAndToken CreateBackendConnectionFromEnvVars()
        {
            string[] args = Environment.GetCommandLineArgs();
            if (args == null)
                return null;

            var beUrl = string.Empty;
            var php = string.Empty;

            // Get backend url
            foreach (string arg in args)
            {
                if (arg.Contains("BackendUrl"))
                {
                    string json = arg.Replace("-config=", string.Empty);
                    var item = JsonConvert.DeserializeObject<DetectBackendUrlAndToken>(json);
                    beUrl = item.BackendUrl;
                }
                if (arg.Contains("-token="))
                {
                    php = arg.Replace("-token=", string.Empty);
                }
            }

            if (!string.IsNullOrEmpty(php) && !string.IsNullOrEmpty(beUrl))
            {
                return new DetectBackendUrlAndToken(beUrl, php);
            }
            return null;
        }

        public static DetectBackendUrlAndToken GetBackendConnection()
        {
            return CreateBackendConnectionFromEnvVars();
        }
    }
}
