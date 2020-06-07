using System;
using System.Collections.Generic;
using System.Text;

namespace Saturday365.Integrations.Common
{
    public class AccountModel
    {
        public AccountModel()
        {

        }

        public AccountModel(Guid iD, string accountNumber, string accountName)
        {
            ID = iD;
            AccountNumber = accountNumber;
            AccountName = accountName;
        }

        public Guid ID { get; set; }
        public string AccountNumber { get; set; }
        public string AccountName { get; set; }
    }
}
