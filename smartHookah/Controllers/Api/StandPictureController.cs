using Autofac.Integration.WebApi;
using smartHookah.Models.Db;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web.Http;

namespace smartHookah.Controllers.Api
{
    [AutofacControllerConfiguration]
    public class StandPictureController : ApiController
    {

        private readonly SmartHookahContext db;

        public StandPictureController(SmartHookahContext db)
        {
            this.db = db;
        }

        public StandPictureController()
        {

        }

        [OptionalHttps(true)]
        [ActionName("DefaultAction")]
        public HttpResponseMessage GetStandPicture(string id)
        {

            var hookah = db.Hookahs.FirstOrDefault(a => a.Code == id);

            if (hookah == null || hookah.Setting == null || hookah.Setting.Picture == null)
                return null;

            var picture = hookah.Setting.Picture;
            //var result =
            //    "34:48:AAAAAAAAAAAAAADAAQAAAMABAAAAwAEAAACAAAAAAIAAAAAAgAAAAAD4DwAAAIAAAAAAgAAAAACAAAAAAIAAAAAAgAAAAACAAAAAAIAAAAAAgAAAAACAAA8AAIDAMQAAgGBgAACAGEAAAIAMgAAAwAOAAADAAYAAAMABfgAAwAHBAADAgeAAAMBBIAEA4MMYAQD4jwcBAP4fAAEA/z8AAQD/fwABAP9/AAEA/z+AAAD+P4AAAPgPQAAAwAEgAAAAADAAAAAACAAAAAAGAAAAgAEAAAB4AADA/wcAAAAAAAAAAAAAAAAAAAAAAAAAAAAA:";

            var result = $"{picture.Width}:{picture.Height}:{picture.PictueString}:";

            string yourJson = result;
            var response = Request.CreateResponse(HttpStatusCode.OK);
            response.Content = new StringContent(yourJson, Encoding.UTF8, "application/json");
            return response;
        }

    }





}