using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Http.Controllers;
using AutoMapper.QueryableExtensions;
using Microsoft.Azure.Mobile.Server;
using Microsoft.Azure.Mobile.Server.Tables;
using smartHookah.Helpers;
using smartHookah.Models;

namespace smartHookah.Controllers.Mobile
{
    public class PipeAccesoryMobileController : TableController<PipeAccesoryDto>
    {

        public PipeAccesoryMobileController(SmartHookahContext db)
        {
            this.context = db;
        }
        private SmartHookahContext context;
        protected override void Initialize(HttpControllerContext controllerContext)
        {
            base.Initialize(controllerContext);
          
            //DomainManager = new EntityDomainManager<CustomerDto>(context, Request, Services);
            DomainManager = new SimpleMappedEntityDomainManager<PipeAccesoryDto, PipeAccesory>(context, Request,
                p => p.Id);

            this.context = new SmartHookahContext();

        }

        public IQueryable<PipeAccesoryDto> GetAllPipeAccesory()
        {
            var pipeAccesory = this.context.PipeAccesories.Include("Brand").Include("Statistics").Project().To<PipeAccesoryDto>();
            return pipeAccesory;
        }

        [ExpandProperty("Brand")]
        [ExpandProperty("Statistics")]
        public SingleResult<PipeAccesoryDto> GetpipeAccesory(string id)
        {
            return Lookup(id);
        }
    }

    public class PipeAccesoryDto : EntityData
    {
        public string AccName { get; set; }

        public Brand Brand { get; set; }

        public PipeAccesoryStatistics Statistics { get; set; }

    }
}