using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using Accord.MachineLearning;
using Newtonsoft.Json;
using smartHookah.Models;
using smartHookah.Support;

namespace smartHookah.Controllers
{
    public class MachineController : Controller
    {
        private readonly SmartHookahContext db;

        public MachineController(SmartHookahContext db)
        {
            this.db = db;
        }

        // GET: Learning
        public ActionResult Index()
        {
            return View();
        }

        public void GetAllData()
        {
            var dbSmokeSession = db.SmokeSessions.Select(a => a.Id).ToList();
            var result = new List<SessionMLData>();

            foreach (var session in dbSmokeSession)
            {
                var pufs = db.DbPufs.Where(a => a.SmokeSession_Id == session).ToList();
                var clusters = pufs.GetClusterPuf().ToArray();
                result.Add(new SessionMLData()
                {
                    Id =  session,
                    Data = clusters
                });

            }

            string json = JsonConvert.SerializeObject(result);
            Response.Clear();
            Response.ContentType = "application/json; charset=utf-8";
            Response.Write(json);
            Response.End();

        }

        public async Task<ActionResult> Clusters(int? id,int ? persons)
        {
            if (id == null)
            {
                RedirectToAction("Index", "Home");
            }
            var session = db.SmokeSessions.Find(id);

            if(session == null)
            {
                RedirectToAction("Index", "Home");
            }

            var cpufs = session.DbPufs.ToList().GetClusterPuf().Where(a => a.Presure > 0).ToArray();
            var observations = cpufs.Select(a => new double[] {a.Presure, a.Duration.TotalMilliseconds}).ToArray();

            Accord.Math.Random.Generator.Seed = 0;
            if (persons == null)
            {
                persons = session.Persons.Count + session.MetaData.AnonymPeopleCount;
            }
            KMeans kmeans = new KMeans(persons.Value);
            KMeansClusterCollection clusters = kmeans.Learn(observations);

            int[] labels = clusters.Decide(observations);

            var model = new LearningSessionViewModel();

            for (int i = 0; i < cpufs.Length; i++)
            {
                cpufs[i].Cluster = labels[i];
            }

            model.Cpufs = cpufs;
            model.persons = persons;
            model.SessionId=id.Value;
            return View(model);
        }

        public void GetData(int? id)
        {
            var dbSmokeSession = db.SmokeSessions.Where(a => a.Id == id).Select(a => a.Id).ToList();
            var result = new List<SessionMLData>();

            foreach (var session in dbSmokeSession)
            {
                var pufs = db.DbPufs.Where(a => a.SmokeSession_Id == session).ToList();
                var clusters = pufs.GetClusterPuf().ToArray();
                result.Add(new SessionMLData()
                {
                    Id = session,
                    Data = clusters
                });

            }

            string json = JsonConvert.SerializeObject(result);
            Response.Clear();
            Response.ContentType = "application/json; charset=utf-8";
            Response.Write(json);
            Response.End();
        }

        class SessionMLData
        {
            public int Id { get; set; }

            public CPuf[] Data { get; set; }


        }
    }

    public class LearningSessionViewModel
    {
        public int? persons;
        public CPuf[] Cpufs { get; set; }

        public int SessionId { get; set; }
    }
}