using System.Collections.Generic;
using System.Linq;
using smartHookah.Models;

namespace smartHookah.Services.Gear
{
    public interface IGearService
    {
        List<PipeAccesory> GetPersonAccessories(int personId);
    }
}