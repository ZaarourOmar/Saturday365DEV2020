using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Microsoft.PowerPlatform.Cds.Client;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Newtonsoft.Json;
using Saturday365.Integrations.Common;

namespace Saturday365.Integrations.Functions
{
    public static class Function1
    {


        [FunctionName("ERP2CDS")]
        public static void Run([ServiceBusTrigger("systems2cds", "cds_sub", Connection = "system2CdsConnString")] string mySbMsg, ILogger log)
        {
            log.LogInformation($"C# ServiceBus topic trigger function processed message: {mySbMsg}");

            var client = CdsServiceClient();
            if (client.IsReady)
            {
                var fakeErpModel = JsonConvert.DeserializeObject<FakeErpModel>(mySbMsg);

                switch (fakeErpModel.RecordType)
                {
                    case RecordTypes.Account:
                        UpsertAccount(client, fakeErpModel);
                        break;

                    case RecordTypes.Contact:
                        UpsertContact(client, fakeErpModel);
                        break;
                }
            }


        }


        /// <summary>
        /// Helper method to update/insert accounts in CDS
        /// </summary>
        /// <param name="client"></param>
        /// <param name="fakeErpModel"></param>
        private static void UpsertAccount(CdsServiceClient client, FakeErpModel fakeErpModel)
        {
            try
            {
                QueryExpression accountQuery = new QueryExpression("account");
                accountQuery.ColumnSet = new ColumnSet("name", "accountnumber");
                accountQuery.Criteria.AddCondition("accountid", ConditionOperator.Equal, fakeErpModel.ID);
                var result = client.RetrieveMultiple(accountQuery);
                if (result == null || result.Entities.Count == 0)
                {
                    //create account
                    Entity newAccount = new Entity("account");
                    newAccount.Attributes["name"] = fakeErpModel.AccountName;
                    newAccount.Attributes["accountnumber"] = fakeErpModel.AccountNumber;
                    client.Create(newAccount);

                }
                else
                {
                    var existingAccount = result.Entities[0];
                    // update existing account
                    existingAccount.Attributes["name"] = fakeErpModel.AccountName;
                    existingAccount.Attributes["accountnumber"] = fakeErpModel.AccountNumber;
                    client.Update(existingAccount);
                }
            }
            catch (Exception ex)
            {
                //handle errors here
            }

        }


        /// <summary>
        /// Helper method to update/insert contacts in CDS
        /// </summary>
        /// <param name="client"></param>
        /// <param name="fakeErpModel"></param>
        private static void UpsertContact(CdsServiceClient client, FakeErpModel fakeErpModel)
        {
            try
            {
                QueryExpression contactQuery = new QueryExpression("contact");
                contactQuery.ColumnSet = new ColumnSet("firstname", "lastname");
                contactQuery.Criteria.AddCondition("contactid", ConditionOperator.Equal, fakeErpModel.ID);
                var result = client.RetrieveMultiple(contactQuery);
                if (result == null || result.Entities.Count == 0)
                {
                    //create a contact
                    Entity newContact = new Entity("contact");
                    newContact.Attributes["firstname"] = fakeErpModel.ContactFirstName;
                    newContact.Attributes["lastname"] = fakeErpModel.ContactLastName;
                    client.Create(newContact);

                }
                else
                {
                    var existingContact = result.Entities[0];
                    // update existing account
                    existingContact.Attributes["firstname"] = fakeErpModel.ContactFirstName;
                    existingContact.Attributes["lastname"] = fakeErpModel.ContactLastName;
                    client.Update(existingContact);
                }
            }
            catch (Exception ex)
            {

            }
        }

        private static CdsServiceClient CdsServiceClient()
        {
            string organizationUrl = "https://area5one-dev.crm3.dynamics.com";
            string clientId = "0fa61e53-9860-47b6-80c7-eb15f8675b51"; // Client Id
            string clientSecret = "bgX2EDj7uobDI.~~wmsaBkdK8N2KcxV~L0"; //Client Secret

            var client = new CdsServiceClient($@"AuthType=ClientSecret;url={organizationUrl};ClientId={clientId};ClientSecret={clientSecret}");

            return client;
        }

    }


}
