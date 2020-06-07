using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Saturday365.Integrations.Plugins
{
    public class AzureAwarePlugin : IPlugin
    {
        // The id of the previously configured service endpoint. 
        private Guid serviceEndpointId;

        public AzureAwarePlugin(string config)
        {
            if (String.IsNullOrEmpty(config) || !Guid.TryParse(config, out serviceEndpointId))
            {
                throw new InvalidPluginExecutionException("Service endpoint ID should be passed as config.");
            }
        }

        public void Execute(IServiceProvider serviceProvider)
        {
            // Retrieve the execution context.
            IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));

            // Assemble the org service
            IOrganizationServiceFactory serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            IOrganizationService orgService = serviceFactory.CreateOrganizationService(context.UserId);


            // Extract the tracing service.
            ITracingService tracingService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
            if (tracingService == null)
                throw new InvalidPluginExecutionException("Failed to retrieve the tracing service.");

            // get a reference to the Service End point
            IServiceEndpointNotificationService cloudService = (IServiceEndpointNotificationService)serviceProvider.GetService(typeof(IServiceEndpointNotificationService));
            if (cloudService == null)
                throw new InvalidPluginExecutionException("Failed to retrieve the service bus service.");

            try
            {
                // Modify writable properties (if needed)
                //context.SharedVariables.Add("YouVariable", "Some Value");

                // What if we want to get all attributes even for an update message? An example of we need a plugin not a service endpoint only
                var entity = context.InputParameters["Target"] as Entity;
                if (entity != null && entity.LogicalName == "account" && context.MessageName == "Update")
                {
                    var accountRecord = orgService.Retrieve("account", entity.Id, new ColumnSet("name", "accountnumber"));
                    if (!entity.Attributes.ContainsKey("name"))
                    {
                        entity.Attributes.Add("name", accountRecord.GetAttributeValue<string>("name"));
                    }
                    if (!entity.Attributes.ContainsKey("accountnumber"))
                    {
                        entity.Attributes.Add("accountnumber", accountRecord.GetAttributeValue<string>("accountnumber"));
                    }
                    
                    context.InputParameters["Target"] = entity;
                }

                tracingService.Trace("Posting the execution context.");
                string response = cloudService.Execute(new EntityReference("serviceendpoint", serviceEndpointId), context);
                if (!String.IsNullOrEmpty(response))
                {
                    tracingService.Trace("Response = {0}", response);
                }
                tracingService.Trace("Done.");
            }
            catch (Exception e)
            {
                tracingService.Trace("Exception: {0}", e.ToString());
                throw;
            }
        }
    }
}
