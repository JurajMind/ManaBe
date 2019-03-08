using System;
using Newtonsoft.Json.Converters;

namespace smartHookah.Helpers.Formaters
{
    public class ShortIsoDateFormatConverter : DateTimeConverterBase
    {
        public override object ReadJson(Newtonsoft.Json.JsonReader reader, Type objectType, object existingValue, Newtonsoft.Json.JsonSerializer serializer)
        {
            return DateTime.Parse(reader.Value.ToString());
        }

        public override void WriteJson(Newtonsoft.Json.JsonWriter writer, object value, Newtonsoft.Json.JsonSerializer serializer)
        {
            writer.WriteValue(((DateTime)value).ToString(" yyyy-mm-ddThh:mm:ssssssTZD"));
        }
    }
}