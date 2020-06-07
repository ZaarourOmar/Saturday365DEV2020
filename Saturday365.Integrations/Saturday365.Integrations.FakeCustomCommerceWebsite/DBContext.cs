using Microsoft.Xrm.Sdk;
using Saturday365.Integrations.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Saturday365.Integrations.FakeWebsiteReceiver
{
   
    public interface IDBContext
    {
        void AddAccount(AccountModel newAccount);
        List<AccountModel> GetAllAccounts();
        void UpsertContact(Entity contact);
        void UpsertAccount(Entity account);
    }
    public class DBContext : IDBContext
    {

        public DBContext()
        {
            DBAccounts = new List<AccountModel>();
        }
        static List<AccountModel> DBAccounts = new List<AccountModel>();


        public void AddAccount(AccountModel newAccount)
        {
            DBAccounts.Add(newAccount);
        }

        public List<AccountModel> GetAllAccounts()
        {
            return DBAccounts;
        }

        public void UpsertContact(Entity entity)
        {
            // This system does't care about contacts
        }

        public void UpsertAccount(Entity entity)
        {
            var account = DBAccounts.FirstOrDefault(x => x.ID == entity.Id);
            var accountName = entity.GetAttributeValue<string>("name");
            var accountNumber = entity.GetAttributeValue<string>("accountnumber");
            if (account == null)
            {
                AddAccount(new AccountModel() { ID = entity.Id, AccountName = accountName, AccountNumber = accountNumber });
            }
            else
            {
                account.AccountName = accountName;
                account.AccountNumber = accountNumber;
            }
        }
      
    }
}
