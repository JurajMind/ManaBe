using System;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using CsvHelper.Configuration.Attributes;
using smartHookah.Models.Db;
using ServiceStack.Common;

namespace smartHookah.Controllers.Api
{
    public class 
        PlaceImportModel
    {
        #region Properties

        [Index(0)]
        [MaxLength(255)]
        public string Name { get; set; }

        [Index(1)]
        public string LogoPath { get; set; }

        [Index(2)]
        [MaxLength(255)]
        public string ShortDescriptions { get; set; }

        [Index(3)]
        public string Descriptions { get; set; }

        [Index(4)]
        [MaxLength(25)]
        public string FriendlyUrl { get; set; }

        [Index(5)]
        public string PersonId { get; set; }

        [Index(6)]
        public string PhoneNumber { get; set; }

        [Index(7)]
        public string Facebook { get; set; }

        [Index(8)]
        public string FranchiseId { get; set; }

        #region Address

        [Index(9)]
        public string Street { get; set; }

        [Index(10)]
        public string City { get; set; }

        [Index(11)]
        public string Number { get; set; }

        [Index(12)]
        public string ZIP { get; set; }

        #endregion

        #region OpeningHours

        [Index(13)]
        public string MonOpen { get; set; }

        [Index(14)]
        public string MonClose { get; set; }

        [Index(15)]
        public string TueOpen { get; set; }

        [Index(16)]
        public string TueClose { get; set; }

        [Index(17)]
        public string WedOpen { get; set; }

        [Index(18)]
        public string WedClose { get; set; }

        [Index(19)]
        public string ThuOpen { get; set; }

        [Index(20)]
        public string ThuClose { get; set; }

        [Index(21)]
        public string FriOpen { get; set; }

        [Index(22)]
        public string FriClose { get; set; }

        [Index(23)]
        public string SatOpen { get; set; }

        [Index(24)]
        public string SatClose { get; set; }

        [Index(25)]
        public string SunOpen { get; set; }

        [Index(26)]
        public string SunClose { get; set; }

        #endregion        

        #endregion

        public static Place ToModel(PlaceImportModel model)
        {
            var address = new Address()
            {
                City = model.City,
                Number = model.Number,
                Street = model.Street,
                ZIP = model.ZIP
            };

            var hours = new Collection<BusinessHours>
            {
                new BusinessHours()
                {
                    Day = 0,
                    OpenTine = ParseTime(model.SunOpen),
                    CloseTime = ParseTime(model.SunClose)
                },

                new BusinessHours()
                {
                    Day = 1,
                    OpenTine = ParseTime(model.MonOpen),
                    CloseTime = ParseTime(model.MonClose)
                },

                new BusinessHours()
                {
                    Day = 2,
                    OpenTine = ParseTime(model.TueOpen),
                    CloseTime = ParseTime(model.TueClose)
                },

                new BusinessHours()
                {
                    Day = 3,
                    OpenTine = ParseTime(model.WedOpen),
                    CloseTime = ParseTime(model.WedClose)
                },

                new BusinessHours()
                {
                    Day = 4,
                    OpenTine = ParseTime(model.ThuOpen),
                    CloseTime = ParseTime(model.ThuClose)
                },

                new BusinessHours()
                {
                    Day = 5,
                    OpenTine = ParseTime(model.FriOpen),
                    CloseTime = ParseTime(model.FriClose)
                },

                new BusinessHours()
                {
                    Day = 6,
                    OpenTine = ParseTime(model.SatOpen),
                    CloseTime = ParseTime(model.SatOpen)
                }
            };

            if (Uri.TryCreate(model.LogoPath, UriKind.Absolute, out var uriResult)
                && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps))
            {
                using (var client = new WebClient())
                {
                    var extension = Path.GetExtension(uriResult.ToString());
                    if (!string.IsNullOrEmpty(extension))
                    {
                        const string path = "/Content/PlacePictures/";
                        var filePath = HttpContext.Current.Server.MapPath($"{path}{model.FriendlyUrl}{extension}");
                        client.DownloadFile(uriResult, filePath);
                        model.LogoPath = $"{path}{model.FriendlyUrl}{extension}";
                    }
                    else
                    {
                        model.LogoPath = "";
                    }
                    
                }
                
                
            }
            var result = new Place()
            {
                Address = address,
                HaveReservation = false,
                BusinessHours = hours,
                Facebook = model.Facebook,
                FranchiseId = int.Parse(model.FranchiseId),
                PersonId = int.Parse(model.PersonId),
                Descriptions = model.Descriptions,
                ShortDescriptions = model.ShortDescriptions,
                LogoPath = model.LogoPath,
                Name = model.Name,
                FriendlyUrl = model.FriendlyUrl,
                PhoneNumber = model.PhoneNumber,
                Public = false
            };
            return result;
        }

        private static TimeSpan ParseTime(string input) => input.IsNullOrEmpty()
            ? TimeSpan.Zero
            : TimeSpan.Parse(input);
    }


    public class PlaceImportModelMap
    {
        [Name("title")]
        public string Name { get; set; }

        [Name("url")]
        public string Url { get; set; }

        [Name("description")]
        public string PosibleAdress { get; set; }

        [Name("longitude")]
        public string Lng { get; set; }

        [Name("latitude")]
        public string Lat { get; set; }


}}