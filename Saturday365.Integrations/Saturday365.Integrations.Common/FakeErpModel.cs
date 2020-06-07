using System;

namespace Saturday365.Integrations.Common
{

    public enum RecordTypes
    {
        Contact, Account
    }
    public class FakeErpModel
    {
        public Guid ID { get; set; }
        public RecordTypes RecordType { get; set; }
        public string AccountName { get; set; }
        public string AccountNumber { get; set; }
        public string ContactFirstName { get; set; }
        public object ContactLastName { get; set; }
    }
}
