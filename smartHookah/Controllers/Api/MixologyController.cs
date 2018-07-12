using smartHookah.Helpers;
using smartHookah.Models;
using smartHookah.Models.Dto;
using System;
using System.Collections.Generic;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using Tobacco = smartHookah.Models.Dto.Tobacco;

namespace smartHookah.Controllers.Api
{
    [System.Web.Http.RoutePrefix("api/Mixology")]
    public class MixologyController : ApiController
    {
        private readonly SmartHookahContext _db;

        public MixologyController(SmartHookahContext db)
        {
            this._db = db;
        }
        
        #region Getters

        [System.Web.Http.HttpGet]
        [System.Web.Http.Route("GetMixes")]
        public async Task<MixListDTO> GetMixes(int page = 0, int pageSize = 50, string author = "me", string orderBy = "name", string order = "asc")
        {
            var query = from a in _db.TobaccoMixs select a;
            if (_db.Brands.Any(a => a.TobaccoMixBrand && a.Name.ToLower() == author.ToLower()))
            {
                query = from m in query where m.Brand.Name.ToLower() == author.ToLower() select m;
            } else if (author == "me")
            {
                var userId = UserHelper.GetCurentPerson(_db).Id;
                query = from m in query where m.Author.Id == userId select m;
            }

            switch (orderBy.ToLower())
            {
                case "name":
                    query = order == "asc"
                        ? from a in query orderby a.AccName ascending select a
                        : from a in query orderby a.AccName descending select a;
                    break;
                case "used":
                    query = order == "asc"
                        ? from a in query orderby a.Statistics.Used ascending select a
                        : from a in query orderby a.Statistics.Used descending select a;
                    break;
                case "rating":
                    query = order == "asc"
                        ? from a in query orderby a.Statistics.Overall ascending select a
                        : from a in query orderby a.Statistics.Overall descending select a;
                    break;
                case "time":
                    query = order == "asc"
                        ? from a in query orderby a.Statistics.SmokeDurationTick ascending select a
                        : from a in query orderby a.Statistics.SmokeDurationTick descending select a;
                    break;
                default:
                    return new MixListDTO() { Success = false, Message = "Invalid OrderBy value, select \"name\", \"used\", \"rating\" or \"time\"." };
            }
            query = pageSize > 0 && page >= 0 ? query.Skip(pageSize * page).Take(pageSize) : query.Take(50);

            var res = query.ToList();

            if(res.Count > 0)
            {
                var result = new MixListDTO() { Success = true, Message = $"{res.Count} mixes found." };
                foreach(var r in res)
                {
                    var m = new Mix()
                    {
                        Id = r.Id,
                        AccName = r.AccName,
                        Overall = r.Statistics?.Overall ?? -1,
                        Used = r.Statistics?.Used ?? -1
                    };
                    foreach (var x in r.Tobaccos)
                    {
                        var t = new Models.Dto.Tobacco()
                        {
                            Id = x.TobaccoId,
                            AccName = x.Tobacco.AccName,
                            BrandName = x.Tobacco.BrandName,
                            Fraction = x.Fraction
                        };

                        m.Tobaccos.Add(t);
                    }
                    result.Mixes.Add(m);
                }
                return result;
            }
            return new MixListDTO() { Success = false, Message = "No mixes found." };
        }

        [System.Web.Http.HttpGet]
        [System.Web.Http.Route("GetMixCreators")]
        public async Task<MixCreatorsDTO> GetFeaturedMixCreators(int page = 0, int pageSize = 50, string orderBy = "name", string order = "asc")
        {
            var query = from b in _db.Brands
                        where b.TobaccoMixBrand
                        select b;
            
            switch (orderBy.ToLower())
            {
                case "name":
                    query = order.ToLower() == "asc" ? from a in query orderby a.DisplayName ascending select a : from a in query orderby a.DisplayName descending select a;
                    break;
                case "count":
                    query = order.ToLower() == "asc" ? from a in query orderby a.PipeAccesories.Count(x => x is TobaccoMix) ascending select a : from a in query orderby a.PipeAccesories.Count(x => x is TobaccoMix) descending select a;
                    break;
                default:
                    return new MixCreatorsDTO() { Success = false, Message = "Invalid OrderBy value, select \"name\" or \"count\"." };
            }
            query = pageSize > 0 && page >= 0 ? query.Skip(pageSize * page).Take(pageSize) : query.Take(50);

            var res = query.ToList();

            if (res.Count() > 0)
            {
                var result = new MixCreatorsDTO() { Success = true, Message = $"{res.Count()} mix creators found." };
                foreach (var m in res)
                {
                    var creator = new MixCreator()
                    {
                        Name = m.Name,
                        DisplayName = m.DisplayName,
                        Picture = m.Picture,
                        MixCount = m.PipeAccesories.Count(a => a is TobaccoMix)
                    };

                    result.MixCreatorsList.Add(creator);
                }
                return result;
            }

            return new MixCreatorsDTO() { Success = false, Message = "No mix creators found." };
        }
        #endregion

        #region Setters

        [System.Web.Http.HttpPost]
        [System.Web.Http.Route("AddToMix")]
        public async Task<TobaccoMixDTO> AddToMix([Bind(Include = "Id,Fraction,MixId,AccName")] MixPartDTO mixPart)
        {
            var tobacco = _db.Tobaccos.Find(mixPart.Id);

            if (mixPart.Fraction < 1 || mixPart.Fraction > 40 || tobacco == null)
            {
                return new TobaccoMixDTO(){Success = false, Message = "Tobacco not fount or fraction not within acceptable range."};
            }

            var mix = _db.TobaccoMixs.Find(mixPart.MixId);
            if (mix != null)
            {
                var t = new TobacoMixPart()
                {
                    TobaccoId = mixPart.Id,
                    Fraction = mixPart.Fraction
                };
                if (mix.Tobaccos.Any(a => a.TobaccoId == mixPart.Id)) return new TobaccoMixDTO(){Success = false, Message = "This tobacco was already added to mix. "};
                try
                {
                    mix.Tobaccos.Add(t);
                    _db.TobaccoMixs.AddOrUpdate(mix);
                    _db.SaveChanges();
                    var response = new TobaccoMixDTO()
                    {
                        Success = true,
                        Message = $"Tobacco with id {mixPart.Id} added to mix.",
                        Id = mix.Id,
                        AccName = mix.AccName
                    };
                    foreach (var m in mix.Tobaccos)
                    {
                        var x = new Tobacco()
                        {
                            Id = m.Tobacco.Id,
                            Fraction = m.Fraction,
                            AccName = m.Tobacco.AccName,
                            BrandName = m.Tobacco.BrandName
                        };
                        response.Tobaccos.Add(x);
                    }
                    return response;
                }
                catch(Exception e)
                {
                    return new TobaccoMixDTO(){Success = false, Message = e.Message};
                }
            }

            var author = UserHelper.GetCurentPerson(_db);
            mix = new TobaccoMix()
            {
                AccName = mixPart.AccName,
                Author = author,
                CreatedAt = DateTimeOffset.UtcNow,
                BrandName = author.AssignedBrandId ?? "OwnBrand"
            };
            if (mix.Tobaccos.Any(a => a.TobaccoId == mixPart.Id)) return new TobaccoMixDTO() { Success = false, Message = "This tobacco was already added to mix. " };
            mix.Tobaccos.Add(new TobacoMixPart(){TobaccoId = mixPart.Id, Fraction = mixPart.Fraction});

            try
            {
                _db.TobaccoMixs.AddOrUpdate(mix);
                _db.SaveChanges();
                var response = new TobaccoMixDTO()
                {
                    Success = true,
                    Message = $"Tobacco with id {mixPart.Id} added to new mix.",
                    Id = mix.Id,
                    AccName = mix.AccName
                };
                foreach (var m in mix.Tobaccos)
                {
                    var x = new Tobacco()
                    {
                        Id = m.Tobacco.Id,
                        Fraction = m.Fraction,
                        AccName = m.Tobacco.AccName,
                        BrandName = m.Tobacco.BrandName
                    };
                    response.Tobaccos.Add(x);
                }
                return response;
            }
            catch (Exception e)
            {
                return new TobaccoMixDTO(){Success = false, Message = e.Message};
            }
        }

        #endregion
    }
}