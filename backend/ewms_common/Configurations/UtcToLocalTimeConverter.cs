using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace ewms_common.Configurations
{
    public class UtcToLocalTimeConverter : ValueConverter<DateTime, DateTime>
    {
        public UtcToLocalTimeConverter()
            : base(
                v => v,
                v => DateTime.SpecifyKind(v, DateTimeKind.Local)
            )
        {
        }
    }
}
