using System.ComponentModel.DataAnnotations;
using System.Linq;
namespace PolicyTracker.DTO
{
    public class FilterSet
    {
        public string[] ProductLines { get; set; }
        public string[] Branches { get; set; }
        public string[] UnderwriterIds { get; set; }
        public string[] Brokers { get; set; }

        public FilterSet()
        {
            ProductLines = Enumerable.Empty<string>().ToArray();
            Branches = Enumerable.Empty<string>().ToArray();
            UnderwriterIds = Enumerable.Empty<string>().ToArray();
            Brokers = Enumerable.Empty<string>().ToArray();
        }
    }

    public class UnderwriterQuickAssign
    {
        [Required]
        public int Id { get; set; }
        [Required]
        public int UnderwriterId { get; set; }
        [Required]
        public int UnderwriterAssistantId { get; set; }
    }
}