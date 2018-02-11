using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using AutoMapper;
using smartHookah.Helpers;
using smartHookah.Models;
//using smartHookah.Models.Dto;
using smartHookahCommon;


namespace smartHookah.Controllers.Mobile
{
    [Authorize]
    public class PersonController : ApiController
    {
        private SmartHookahContext _db;
        public PersonController(SmartHookahContext db)
        {
            _db = db;
        }

        [HttpGet]
        [Route("api/Mobile/Person/")]
        public async Task<GetPersonActiveSession> GetHome()
        {
            var persons = UserHelper.GetCurentPersonIQuerable(_db);

            var person = persons.Include(a => a.SmokeSessions.Select(b => b.MetaData))
                .Include(a => a.SmokeSessions.Select(b => b.Statistics)).FirstOrDefault();

            var model = new GetPersonActiveSession();

            var session = person.SmokeSessions.Where(a => a.StatisticsId == null);
            //model.Hookahs = person.Hookahs.Select(a => new HookahDTO(a)).ToList();


            //var dynamic = session.Select(a => RedisHelper.GetSmokeStatistic(sessionId: a.SessionId));

            //foreach (var dynamicSmokeStatistic in dynamic)
            //{
                
            //}

            return model;
        }
    }

    public class GetPersonActiveSession
    {
        //public List<HookahDTO> Hookahs { get; set; }

        public List<SmokeSessionDto> ActiveSessions { get; set; }

    }
}

