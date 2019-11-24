using System;

namespace smartHookah.Models.Factory
{
    using smartHookah.Controllers;
    using smartHookah.Models.Db;

    public static class PipeAccesoryFactory
    {
        public static PipeAccesory CreateFromRecort(ImportPipeAccesory record, Brand brand, string type)
        {
            var pipeAccesory = new PipeAccesory
            {
                AccName = record.Name,
                BrandName = brand.Name,
                CreatedAt = DateTimeOffset.UtcNow,
            };
            switch (type)
            {

                case "pipe":
                    return new Pipe(pipeAccesory);
                case "bowl":
                    return new Bowl(pipeAccesory);
                case "tobacco":
                    {
                        var protoTobaco = new Tobacco(pipeAccesory) { SubCategory = record.SubType };
                        return protoTobaco;
                    }
                case "heatmanagement":
                    {
                        return new HeatManagment(pipeAccesory);
                    }
                case "coal":
                    {
                        return new Coal(pipeAccesory);
                    }
                default:
                    return pipeAccesory;


            }
        }


    }
}