namespace smartHookah.Controllers
{
    using System;
    using System.Threading.Tasks;
    using System.Web.Mvc;
    using System.Web.Security;

    using Microsoft.TeamFoundation.WorkItemTracking.WebApi;
    using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
    using Microsoft.VisualStudio.Services.Common;
    using Microsoft.VisualStudio.Services.WebApi;
    using Microsoft.VisualStudio.Services.WebApi.Patch;
    using Microsoft.VisualStudio.Services.WebApi.Patch.Json;

    using smartHookah.Helpers;

    [Authorize]
    public class ReportController : Controller
    {
        // GET: Report
        public ActionResult Index()
        {
            if (Request.UrlReferrer != null) ViewBag.Url = Request.UrlReferrer.ToString();
           
            return this.View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> PostBug(PostBugModel model)
        {
            var usr = User.Identity.Name;
            var bug = new CreateBug();
            var footer = $"Author:{usr} Url:{model.Url}";
            var result = bug.CreateBugUsingClientLib(model.Title, model.Steps + footer,model.Priority);

            return RedirectToAction("Index", "Home");
        }
    }

    public class PostBugModel
    {
        public int Priority { get; set; }

        public string Url { get; set; }

        public string Title { get; set; }

        public string Steps { get; set; }
    }

    public class CreateBug
    {
        private readonly string _personalAccessToken;

        private readonly string _project;

        private readonly string _uri;

        /// <summary>
        ///     Constructor. Manually set values to match your account.
        /// </summary>
        public CreateBug()
        {
            this._uri = "https://hamornik.visualstudio.com";
            this._personalAccessToken = "bwau5qui5fxwbzzygt4b3teunzzkzdujpfqvixraljmwaqsxd5pq";
            this._project = "Smarthookah";
        }

        /// <summary>
        ///     Create a bug using the .NET client library
        /// </summary>
        /// <returns>Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models.WorkItem</returns>
        public WorkItem CreateBugUsingClientLib(string Title, string Steps, int priority = 3)
        {
            var uri = new Uri(this._uri);
            var personalAccessToken = this._personalAccessToken;
            var project = this._project;

            var credentials = new VssBasicCredential(string.Empty, this._personalAccessToken);
            var patchDocument = new JsonPatchDocument();

            // add fields and their values to your patch document
            patchDocument.Add(
                new JsonPatchOperation { Operation = Operation.Add, Path = "/fields/System.Title", Value = Title });

            patchDocument.Add(
                new JsonPatchOperation
                    {
                        Operation = Operation.Add,
                        Path = "/fields/Microsoft.VSTS.TCM.ReproSteps",
                        Value = Steps
                    });

            patchDocument.Add(
                new JsonPatchOperation
                    {
                        Operation = Operation.Add,
                        Path = "/fields/Microsoft.VSTS.Common.Priority",
                        Value = priority.ToString()
                    });

            patchDocument.Add(
                new JsonPatchOperation
                    {
                        Operation = Operation.Add,
                        Path = "/fields/Microsoft.VSTS.Common.Severity",
                        Value = "2 - High"
                    });
            var connection = new VssConnection(uri, credentials);
            var workItemTrackingHttpClient = connection.GetClient<WorkItemTrackingHttpClient>();

            try
            {
                var result = workItemTrackingHttpClient.CreateWorkItemAsync(patchDocument, project, "Bug").Result;

                Console.WriteLine("Bug Successfully Created: Bug #{0}", result.Id);

                return result;
            }
            catch (AggregateException ex)
            {
                Console.WriteLine("Error creating bug: {0}", ex.InnerException.Message);
                return null;
            }
        }
    }
}