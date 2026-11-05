using System.Text.Json;
using System.Text.Json.Serialization;

namespace ewms_common.Configurations
{
    public class LocalDateTimeConverter : JsonConverter<DateTime>
    {
        public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var dateTime = reader.GetDateTime();

            return dateTime.Kind switch
            {
                DateTimeKind.Unspecified => DateTime.SpecifyKind(dateTime, DateTimeKind.Local),
                DateTimeKind.Utc => dateTime.ToLocalTime(),
                _ => dateTime
            };
        }

        public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
        {
            var localTime = value.Kind switch
            {
                DateTimeKind.Unspecified => DateTime.SpecifyKind(value, DateTimeKind.Local),
                DateTimeKind.Utc => value.ToLocalTime(),
                _ => value
            };

            writer.WriteStringValue(
                localTime.ToString("yyyy-MM-ddTHH:mm:ss.fffffffK")
            );
        }
    }
}
