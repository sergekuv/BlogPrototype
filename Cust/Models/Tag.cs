using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;
using Newtonsoft.Json;

namespace Cust.Models
{
    public class Tag
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        [Display(Name = "Tag")]
        [StringLength(20)]
        public string Id { get; set; }
        public bool Disabled { get; set; }

        //To avoid JsonException: "A possible object cycle was detected" in API project.
        // Other possible methods mentioned here:
        //https://makolyte.com/system-text-json-jsonexception-a-possible-object-cycle-was-detected-which-is-not-supported/ 
        [System.Text.Json.Serialization.JsonIgnore] 
        public ICollection<Article>? Articles { get; set; }
    }
}
