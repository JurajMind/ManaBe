using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNet.SignalR;

using smartHookah.Models;

namespace smartHookah.Hubs
{
    public class SmokeSessionHub : Hub
    {
        public void Puf(string hookahCode , int direction)
        {

        }

        public Task JoinSession(string sessionId)
        {
            return this.Groups.Add(Context.ConnectionId, sessionId);
        }

        public Task LeaveSession(string sessionId)
        {
            return this.Groups.Remove(Context.ConnectionId, sessionId);
        }

        public async Task JoinLounge(int id)
        {
            using (var db = new SmartHookahContext())
            {
                //var person = UserHelper.GetCurentPerson(db, int.Parse(id));
                var place = db.Places.Where(a => a.Id == id).Include(a => a.Person.Hookahs).FirstOrDefault();
                if(place == null)
                    return;
                var hookahs = place.Person.Hookahs;

                foreach (var personHookah in hookahs)
                {
                    await Groups.Add(Context.ConnectionId, personHookah.Code);
                }

            }
        }

        public Task JoinHookah(string hookahId)
        {
            return Groups.Add(Context.ConnectionId, hookahId);
        }

    }

    public class ReservationHub : Hub
    {
        public async Task JoinPlace(int id)
        {
            await Groups.Add(Context.ConnectionId, id.ToString());
        }
    }
}