﻿using smartHookah.Models.Db;
using System.Collections.Generic;

namespace smartHookah.Controllers
{
    public class SaveSmokeMetadataModel
    {
        public int DbSmokeSessionId { get; set; }

        public int MetaDataId { get; set; }

        public int TobacoMixId { get; set; }

        public string TobacoMixName { get; set; }

        public string TobaccoBrand { get; set; }

        public string TobaccoFlavor { get; set; }

        public string Hookah { get; set; }

        public string Bowl { get; set; }

        public string HeatManagement { get; set; }

        public int PackType { get; set; }

        public string Coal { get; set; }

        public double CoalsCount { get; set; }

        public int TobacoWeight { get; set; }

        public int PersonCount { get; set; }

        public List<TobacoMixPart> TobacosMix { get; set; }

        public SmokeSessionMetaData SmokeSessionMetaData { get; set; }

        public int LayerMethod { get; set; } = 0;
    }
}