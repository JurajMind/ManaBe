using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace smartHookah.Models.Db
{
    public class Friendship
    {
        public int Id { get; set; }
        public virtual  int AId { get; set; }
        public virtual Person A { get; set; }

        public virtual int BId { get; set; }
        public virtual Person B { get; set; }

        public DateTime Created { get; set; }

        public FriendshipStatus Status;
    }

    public enum FriendshipStatus
    {
        Ok,
        Waiting
    }
}