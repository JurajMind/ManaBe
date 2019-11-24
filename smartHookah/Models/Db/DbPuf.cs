using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace smartHookah.Models.Db
{
    public class DbPuf : Puf
    {
        [Key]
        public int Id { get; set; }

        public int? SmokeSession_Id { get; set; }

        [ForeignKey("SmokeSession_Id")]
        public virtual SmokeSession SmokeSession { get; set; }
        public DbPuf()
        { }

        public DbPuf(Puf a)
        {
            this.DateTime = a.DateTime;
            this.SmokeSessionId = a.SmokeSessionId;
            this.Type = a.Type;
            this.Milis = a.Milis;
            this.Presure = a.Presure;
        }
    }
}