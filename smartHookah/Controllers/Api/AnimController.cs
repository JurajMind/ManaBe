using System;
using System.Collections.Generic;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using smartHookah.Models;
using smartHookah.Models.Db;

namespace smartHookah.Controllers.Api
{
    public class AnimController : ApiController
    {
        [HttpPost]
        [OptionalHttps(true)]
        [ActionName("DefaultAction")]
        public async Task<string> Update(string id)
        {
            string result = await Request.Content.ReadAsStringAsync();
            var bytes = Convert.FromBase64String(result);
            var str = System.Text.Encoding.Default.GetString(bytes);
            if (str.StartsWith("ani") && bytes.Length>=9)
            {
                int _idleAnimation = bytes[3];
                int _pufAnimation = bytes[4];
                int _blowAnimation = bytes[5];
                int _idleBr = bytes[6];
                int _pufBr = bytes[7];
                byte _hue = bytes[8];
                byte _sat = bytes[9];

                using (var db = new SmartHookahContext())
                {
                    var hookah = db.Hookahs.FirstOrDefault(a => a.Code == id);

                    var hookahSetting = hookah?.Setting;

                    if (hookahSetting == null)
                        return null;

                    hookahSetting.IdleAnimation = _idleAnimation;
                    hookahSetting.BlowAnimation = _blowAnimation;
                    hookahSetting.PufAnimation = _pufAnimation;
                    hookahSetting.IdleBrightness = _idleBr;
                    hookahSetting.PufBrightness = _pufBr;
                    hookahSetting.Color.Hue = _hue;
                    hookahSetting.Color.Saturation = _sat;
                    hookah.Setting = hookahSetting;
                    db.Hookahs.AddOrUpdate(hookah);
                    await db.SaveChangesAsync();
                    return "OK";

                }
            }
            return null;
        }
    }
}
