using System;
using System.Collections.Generic;
using System.Configuration;
using Amazon.SimpleSystemsManagement;
using Amazon.SimpleSystemsManagement.Model;

namespace Trianz.Enterprise.Operations
{
    public static class ParameterStoreConfigLoader
    {
        public static void LoadFromParameterStore()
        {
            try
            {
                string environment = ConfigurationManager.AppSettings["AppEnvironment"] ?? "Development";
                string parameterPath = ConfigurationManager.AppSettings["ParameterPath"] ?? $"/TrianzEnterpriseOperations/{environment}/";

                var ssmClient = new AmazonSimpleSystemsManagementClient();
                var request = new GetParametersByPathRequest
                {
                    Path = parameterPath,
                    Recursive = true,
                    WithDecryption = true
                };

                var parameters = new List<Parameter>();
                string nextToken = null;

                do
                {
                    request.NextToken = nextToken;
                    var response = ssmClient.GetParametersByPath(request);
                    parameters.AddRange(response.Parameters);
                    nextToken = response.NextToken;
                } while (!string.IsNullOrEmpty(nextToken));

                foreach (var param in parameters)
                {
                    string keyName = param.Name.Substring(parameterPath.Length).TrimStart('/');
                    if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings[keyName]))
                    {
                        ConfigurationManager.AppSettings[keyName] = param.Value;
                    }
                }
            }
            catch (Exception ex)
            {
                // Log error but don't crash - fall back to web.config values
                System.Diagnostics.Trace.TraceWarning("Failed to load configuration from Parameter Store: " + ex.Message);
            }
        }
    }
}
