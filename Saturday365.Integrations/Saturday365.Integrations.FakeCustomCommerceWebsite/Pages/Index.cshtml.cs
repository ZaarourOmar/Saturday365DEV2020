using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Saturday365.Integrations.Common;
using Saturday365.Integrations.FakeWebsiteReceiver.Integrations;

namespace Saturday365.Integrations.FakeWebsiteReceiver.Pages
{
    public class IndexModel : PageModel
    {
        IDBContext _db = null;

        public IndexModel(IDBContext db)
        {
            _db = db;
        }
        public List<AccountModel> AllAccounts = new List<AccountModel>();
        public void OnGet()
        {
            AllAccounts = _db.GetAllAccounts();
        }
    }
}
