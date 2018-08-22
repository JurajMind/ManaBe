using System;
using System.Collections.Generic;
using smartHookah.Models;

namespace smartHookah.Services.Gear
{
    public interface IGearService
    {
        List<PipeAccesory> GetPersonAccessories(int? personId, string type);
        PipeAccesory GetPipeAccessory(int id);
        void Vote(int id, VoteValue value);

        List<Models.Dto.GearService.SearchPipeAccesory> SearchAccesories(string search, AccesoryType type,int page,int pageSize);
    }
}