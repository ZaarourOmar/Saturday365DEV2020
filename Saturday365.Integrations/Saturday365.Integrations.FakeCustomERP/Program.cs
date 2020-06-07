using Microsoft.Azure.ServiceBus;
using Newtonsoft.Json;
using Saturday365.Integrations.Common;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Saturday365.Integrations.FakeCustomERPSender
{
    class Program
    {

        static Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

        // create a topic client
        static ITopicClient _topicClient = null;
        static async Task Main(string[] args)
        {

            var connString = config.AppSettings.Settings["TOPIC_CONN_STRING"].Value;
            var topicName = config.AppSettings.Settings["TOPIC_NAME"].Value;

            string command = "";
            FakeErpModel fakeErpModel = new FakeErpModel();

            while (command != "quit")
            {
                #region Generate Fake data
                Console.WriteLine("Press Enter an option to Send some fake data");
                Console.WriteLine("1) Creates a new account.");
                Console.WriteLine("2) Creates a new contact.");
                Console.WriteLine("3) Updates an existing account.");
                Console.WriteLine("4) Updates an existin contact");


                command = Console.ReadLine();

                Random accountNumber = new Random();


                if (command == "1")
                {
                    fakeErpModel.ID = Guid.NewGuid();
                    fakeErpModel.RecordType = RecordTypes.Account;
                    fakeErpModel.AccountName = $"New Account {DateTime.Now.ToString()}";
                    fakeErpModel.AccountNumber = accountNumber.Next(100, 10000).ToString();
                }
                else if (command == "2")
                {
                    fakeErpModel.ID = Guid.NewGuid();
                    fakeErpModel.RecordType = RecordTypes.Contact;
                    fakeErpModel.ContactFirstName = $"New Contact";
                    fakeErpModel.ContactLastName = DateTime.Now.ToString();
                }
                else if (command == "3")
                {
                    fakeErpModel.ID = existingAccountId;
                    fakeErpModel.RecordType = RecordTypes.Account;
                    fakeErpModel.AccountName = $"Updated Account {DateTime.Now.ToString()}";
                    fakeErpModel.AccountNumber = accountNumber.Next(100, 10000).ToString();
                }
                else if (command == "4")
                {
                    fakeErpModel.ID = existingContactId;
                    fakeErpModel.RecordType = RecordTypes.Contact;
                    fakeErpModel.ContactFirstName = $"Updated Contact";
                    fakeErpModel.ContactLastName = DateTime.Now.ToString();

                }
                else
                {
                    Console.WriteLine("You have entered a wrong option");
                    continue;
                }
                #endregion

                try
                {
                    // TO DO - Post to SB here
                    _topicClient = new TopicClient(connString, topicName);
                    await _topicClient.SendAsync(BuildMessage(fakeErpModel));
                    await _topicClient.CloseAsync();


                }
                catch (ServiceBusCommunicationException sbEx)
                {
                    Console.WriteLine(sbEx.Message);
                }
                catch (Exception generalEx)
                {
                    Console.WriteLine(generalEx.Message);
                }
               

            }


        }

        private static Message BuildMessage(FakeErpModel fakeErpModel)
        {
            var json = JsonConvert.SerializeObject(fakeErpModel);
            var bytes = Encoding.UTF8.GetBytes(json);
            return new Message(bytes);
        }




        #region Hidden

        static Guid existingAccountId = Guid.Parse("9e39fbd9-eca1-ea11-a812-000d3af46865");
        static Guid existingContactId = Guid.Parse("ecb347e4-f0a1-ea11-a812-000d3af46865");
        #endregion
    }
}
