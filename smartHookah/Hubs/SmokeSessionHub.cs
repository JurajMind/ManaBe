﻿using Microsoft.AspNet.SignalR;
using smartHookah.Models.Db;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

namespace smartHookah.Hubs
{
    public class SmokeSessionHub : Hub
    {
        public void Puf(string hookahCode, int direction)
        {
        }

        public Task JoinSession(string sessionId)
        {
            return this.Groups.Add(this.Context.ConnectionId, sessionId);
        }

        public void Ping()
        {
            this.Clients.Client(this.Context.ConnectionId).Ping();
        }

        public Task JoinPerson(string personId)
        {
            return this.Groups.Add(this.Context.ConnectionId, personId);
        }

        public Task LeaveSession(string sessionId)
        {
            return this.Groups.Remove(this.Context.ConnectionId, sessionId);
        }

        public async Task JoinLounge(int id)
        {
            using (var db = new SmartHookahContext())
            {
                // var person = UserHelper.GetCurentPerson(db, int.Parse(id));
                var place = db.Places.Where(a => a.Id == id).Include(a => a.Person.Hookahs).FirstOrDefault();
                if (place == null) return;
                var hookahs = place.Person.Hookahs;

                foreach (var personHookah in hookahs)
                {
                    await this.Groups.Add(this.Context.ConnectionId, personHookah.Code);
                }
            }
        }

        public Task JoinHookah(string hookahId)
        {
            return this.Groups.Add(this.Context.ConnectionId, hookahId);
        }

        public async Task JoinPlace(int id)
        {
            await this.Groups.Add(this.Context.ConnectionId, $"place_{id.ToString()}");
        }
    }

    public class ReservationHub : Hub
    {
        public async Task JoinPlace(int id)
        {
            await this.Groups.Add(this.Context.ConnectionId, id.ToString());
        }
    }
}