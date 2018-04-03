using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.Linq;

namespace smartHookah.Models
{
    public class SmartHookahInitializer : System.Data.Entity.DropCreateDatabaseIfModelChanges<SmartHookahContext>
    {
        protected override void Seed(SmartHookahContext context)
        {
            var hookah = new List<Hookah>
            {
                new Hookah() {Name = "Kaya",Code = "kaya"},
                new Hookah() {Name = "Virtual hookah", Code = "hookahTest1"},
                new Hookah() {Name = "Prototype 1", Code ="beta1" }
            };

            foreach (var hookah1 in hookah)
            {
                context.Hookahs.Add(hookah1);
            }
            //var lounge = new Lounge()
            //{
            //    Name = "U mimi",
            //    Address = new Address()
            //    {
            //        City = "Praha",
            //        Number = "9",
            //        ZIP = "11 000",
            //        Street = "Dlouhá"

            //    },
            //    Descriptions = "Sukromna vodnofajkaren",
             
            //};

            //context.Lounges.Add(lounge);
            try
            {
                context.SaveChanges();
            }
            catch (DbEntityValidationException e)
            {
                foreach (var eve in e.EntityValidationErrors)
                {
                    Console.WriteLine("Entity of type \"{0}\" in state \"{1}\" has the following validation errors:",
                        eve.Entry.Entity.GetType().Name, eve.Entry.State);
                    foreach (var ve in eve.ValidationErrors)
                    {
                        Console.WriteLine("- Property: \"{0}\", Error: \"{1}\"",
                            ve.PropertyName, ve.ErrorMessage);
                    }
                }
                throw;
            }
          
        }
    }
}