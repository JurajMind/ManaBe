using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.VisualStudio.Services.Account;
using smartHookah.Models;

namespace smartHookah.Services.Gear
{
    public class GearService : IGearService
    {
        private readonly SmartHookahContext _db;

        public GearService(SmartHookahContext db)
        {
            _db = db;
        }

        public List<PipeAccesory> GetPersonAccessories(int personId)
        {
            var person = _db.Persons.FirstOrDefault(a => a.Id == personId);
            if (person == null) throw new AccountNotFoundException();
            return person?.OwnedPipeAccesories.Select(a => a.PipeAccesory).ToList();
        }
    }
}