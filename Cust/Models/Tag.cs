using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;

namespace Cust.Models
{
    public class Tag
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        [Display(Name = "Tag")]
        [StringLength(20)]
        public string Id { get; set; }
        public string Description { get; set; }
    }
}
