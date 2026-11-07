using ewms_common.Attributes;
using System.ComponentModel.DataAnnotations;

namespace ewms_common.DTOs.Request
{
    public class DeleteManyRequest
    {
        [RemoveDuplicateStrings]
        [MinLength(1)]
        public List<string> ids { get; set; } = new List<string>();
    }
}
