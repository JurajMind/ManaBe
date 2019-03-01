using System;
using System.Collections.Generic;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using smartHookah.Models;
using smartHookah.Models.Db;
using smartHookahCommon;

namespace smartHookah.Controllers.Api
{
    using System.Web;

    using smartHookah.Services.Redis;

    public class PingController : ApiController
    {

        private readonly SmartHookahContext db;

        private readonly IRedisService redisService;
        public PingController(SmartHookahContext db, IRedisService redisService)
        {
            this.db = db;
            this.redisService = redisService;
        }

        [HttpGet]
        [OptionalHttps(true)]
        [ActionName("DefaultAction")]
        public bool DoPing(string id=null,string version=null)
        {
            if (id == null)
            {
                return true;
            }

           

            var hookah = db.Hookahs.FirstOrDefault(a => a.Code == id);

          

            if (hookah == null)
                return true;

            if (hookah.Offline)
            {
                throw new HttpResponseException(HttpStatusCode.Forbidden);
            }

            try
            {
                try
                {
                    var adress = ((HttpContextWrapper)this.Request.Properties["MS_HttpContext"]).Request.UserHostAddress;
                    this.redisService.StoreAdress(hookah.Code + "_acces", adress);
                }
                catch (Exception e)
                {
            
                }

                RedisHelper.SetConnectionTime(id);

                var versionInt = Helper.UpdateVersionToInt(version);

                if (hookah.Version != versionInt)
                {
                    hookah.Version = versionInt;
                    db.Hookahs.AddOrUpdate(hookah);
                    db.SaveChanges();
                }
            }
            catch (Exception e)
            {
                return true;
            }
          



            return true;
        }
    }
}