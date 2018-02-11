using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.OData;
using System.Web.Mvc;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.Azure.Mobile.Server;
using Microsoft.Azure.Mobile.Server.Tables;
using smartHookah.Helpers;
using smartHookah.Models;


namespace smartHookah.Controllers.Mobile
{
    public class SmokeSessionsDtoController : TableController<SmokeSessionDto>
    {
        private SmartHookahContext context;
        protected override void Initialize(HttpControllerContext controllerContext)
        {
            base.Initialize(controllerContext);
            SmartHookahContext context = new SmartHookahContext();
            //DomainManager = new EntityDomainManager<CustomerDto>(context, Request, Services);
            DomainManager = new SimpleMappedEntityDomainManager<SmokeSessionDto, SmokeSession>(context, Request,
                p => p.SessionId);

            this.context = new SmartHookahContext();

        }
        [ExpandProperty("Hookah")]
        [ExpandProperty("SmokeSessionMetaData")]

        public IQueryable<SmokeSessionDto> GetAllSmokeSession()
        {
            var smokeSession = this.context.SmokeSessions.Include("Hookah").Include("MetaData").Project().To<SmokeSessionDto>();
            return smokeSession;
        }
        [ExpandProperty("Hookah")]
        [ExpandProperty("MetaData")]

        public SingleResult<SmokeSessionDto> GetSmokeSession(string id)
        {
            return Lookup(id);
        }
        public Task<SmokeSessionDto> PatchSmokeSessionDto(string id, Delta<SmokeSessionDto> patch)
        {
            return UpdateAsync(id, patch);
        }
        public async Task<IHttpActionResult> PostSmokeSession(SmokeSessionDto item)
        {
            SmokeSessionDto current = await InsertAsync(item);
            return CreatedAtRoute("Tables", new { id = current.Id }, current);
        }
        public Task DeleteSmokeSession(string id)
        {
            return DeleteAsync(id);
        }
    }

    public class SmokeSessionDto : EntityData
    {
        public Hookah Hookah { get; set; }
        public int HookahId { get; set; }

        public SmokeSessionMetaData MetaData { get; set; }

        public  SmokeSessionStatistics Statistics { get; set; }

        public TobaccoReview Review { get; set; }
        public string Token { get; set; }
    }


    public class SimpleMappedEntityDomainManager<TData, TModel>
    : MappedEntityDomainManager<TData, TModel>
    where TData : class, ITableData, new()
    where TModel : class
    {
        private Expression<Func<TModel, object>> dbKeyProperty;
        public SimpleMappedEntityDomainManager(DbContext context,
            HttpRequestMessage request,Expression<Func<TModel, object>> dbKeyProperty)
            : base(context, request,false)
        {
            this.dbKeyProperty = dbKeyProperty;
        }
        public override SingleResult<TData> Lookup(string id)
        {
            return this.LookupEntity(GeneratePredicate(id));
        }
        public override Task<TData> UpdateAsync(string id, Delta<TData> patch)
        {
            return this.UpdateEntityAsync(patch, ConvertId(id));
        }
        public override Task<bool> DeleteAsync(string id)
        {
            return this.DeleteItemAsync(ConvertId(id));
        }
        private static Expression<Func<TModel, bool>> GeneratePredicate(string id)
        {
            var m = Mapper.FindTypeMapFor<TModel, TData>();
            var pmForId = m.GetExistingPropertyMapFor(new AutoMapper.Impl.PropertyAccessor(typeof(TData).GetProperty("Id")));
            var keyString = pmForId.CustomExpression;
            var predicate = Expression.Lambda<Func<TModel, bool>>(
                Expression.Equal(keyString.Body, Expression.Constant(id)),
                keyString.Parameters[0]);
            return predicate;
        }
        private object ConvertId(string id)
        {
            var m = Mapper.FindTypeMapFor<TData, TModel>();
            var keyPropertyAccessor = GetPropertyAccessor(this.dbKeyProperty);
            var pmForId = m.GetExistingPropertyMapFor(new AutoMapper.Impl.PropertyAccessor(keyPropertyAccessor));
            TData tmp = new TData() { Id = id };
            var convertedId = pmForId.CustomExpression.Compile().DynamicInvoke(tmp);
            return convertedId;
        }
        private PropertyInfo GetPropertyAccessor(Expression exp)
        {
            if (exp.NodeType == ExpressionType.Lambda)
            {
                var lambda = exp as LambdaExpression;
                return GetPropertyAccessor(lambda.Body);
            }
            else if (exp.NodeType == ExpressionType.Convert)
            {
                var convert = exp as UnaryExpression;
                return GetPropertyAccessor(convert.Operand);
            }
            else if (exp.NodeType == ExpressionType.MemberAccess)
            {
                var propExp = exp as System.Linq.Expressions.MemberExpression;
                return propExp.Member as PropertyInfo;
            }
            else
            {
                throw new InvalidOperationException("Unexpected expression node type: " + exp.NodeType);
            }
        }
    }

}