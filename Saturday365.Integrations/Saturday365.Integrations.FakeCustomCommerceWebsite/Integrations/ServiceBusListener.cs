using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.InteropExtensions;
using Microsoft.Extensions.Logging;
using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using System.Text;

namespace Saturday365.Integrations.FakeWebsiteReceiver.Integrations
{

    public interface IServiceBusListener
    {
        void RegisterHandlerAndListen();
    }
    public class ServiceBusListener : IServiceBusListener
    {
        readonly ILogger _logger;
        readonly IConfiguration _config;
        ISubscriptionClient _subClient = null;
        IDBContext _db;

        public ServiceBusListener()
        {

        }
        public ServiceBusListener(IConfiguration config, ILogger<ServiceBusListener> logger, IDBContext db)
        {
            _config = config;
            _logger = logger;
            _db = db;
            InitializeSubsriptionClient();
        }

        private void InitializeSubsriptionClient()
        {
            //read connection parameters from the config setting
            var connString = _config.GetConnectionString("TOPIC_CONN_STRING");
            var topicName = _config.GetValue<string>("TOPIC_NAME");
            var subName = _config.GetValue<string>("SUBSCRIPTION_NAME");

            // init sub client here
            _subClient = new SubscriptionClient(connString, topicName, subName);
        }

        public void RegisterHandlerAndListen()
        {

            var options = new MessageHandlerOptions(OnError)
            {
                AutoComplete = false,
                MaxConcurrentCalls = 1
            };
            _subClient.RegisterMessageHandler(ProcessMessage, options);
        }

        private async Task ProcessMessage(Message msg, CancellationToken token)
        {
            switch (msg.ContentType)
            {
                case "application/json":
                    break;

                case "application/msbin1":
                    var remoteContext = msg.GetBody<RemoteExecutionContext>();
                    var entity = remoteContext.InputParameters["Target"] as Entity;
                    UpdateDatabase(entity);
                    await _subClient.CompleteAsync(msg.SystemProperties.LockToken);
                    break;
            }
        }

        private Task OnError(ExceptionReceivedEventArgs arg)
        {
            Console.WriteLine(arg.Exception.Message);
            return Task.CompletedTask;

        }


        /// <summary>
        /// Helper method to update a fake database
        /// </summary>
        /// <param name="entity"></param>
        private void UpdateDatabase(Entity entity)
        {
            if (entity.LogicalName == "account")
            {
                _db.UpsertAccount(entity);
            }
            else if (entity.LogicalName == "contact")
            {
                _db.UpsertContact(entity);
            }

        }
    }
}
