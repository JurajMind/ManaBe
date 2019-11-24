using Newtonsoft.Json;
using smartHookah.Models.Db;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace smartHookah.Helpers.ModelExtensions
{
    public static class SmokeSessionPufExtension
    {
        public static ICollection<DbPuf> StoredPufs(this SmokeSession session)
        {
            try
            {
                var sessions =
                    JsonConvert.DeserializeObject<List<Puf>>(
                        File.ReadAllText(HttpContext.Current.Server.MapPath(session.StorePath)));
                var dbPufs = sessions.Select(s => new DbPuf(s)).ToList();
                foreach (var dbPuf in dbPufs)
                {
                    dbPuf.SmokeSession = session;
                    dbPuf.SmokeSessionId = session.SessionId;
                }

                return dbPufs;

            }
            catch (Exception e)
            {
                return new List<DbPuf>();
            }
        }

        public static string StoredPufs(SmokeSession session, string storeKey)
        {
            try
            {
                string subPath = $"/SmokeSession/{storeKey}"; // your code goes here
                string serverPath = HttpContext.Current.Server.MapPath(subPath);
                bool exists = System.IO.Directory.Exists(serverPath);

                if (!exists)
                    Directory.CreateDirectory(HttpContext.Current.Server.MapPath(subPath));
                var path = System.IO.Path.Combine(serverPath, $"{session.SessionId}.json");
                var relativePath = subPath + $"/{session.SessionId}.json";
                foreach (var sessionDbPuf in session.DbPufs)
                {
                    sessionDbPuf.SmokeSession = null;
                }
                List<Puf> pufs = session.DbPufs.ToList().Select(s => new Puf(s)
                {

                }).ToList();

                var json = JsonConvert.SerializeObject(pufs, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });

                System.IO.File.WriteAllText(path, json);
                return relativePath;
            }
            catch (Exception e)
            {
                return null;
            }
        }
    }
}