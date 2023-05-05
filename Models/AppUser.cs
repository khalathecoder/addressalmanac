using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace ContactPro_Crucible.Models
{
    public class AppUser : IdentityUser //"inheritance of IdentityUser class"
    {
        [Required]
        [Display(Name = "First Name")]
        [StringLength(50, ErrorMessage ="The {0} must be at least {2} and max {1} characters long.", MinimumLength = 2)]
        public string? FirstName { get; set; }

        [Required]
        [Display(Name = "Last Name")]
        [StringLength(50, ErrorMessage = "The {0} must be at least {2} and max {1} characters long.", MinimumLength = 2)]
        public string? LastName { get; set;}

        [NotMapped] //redunant to have stored in database when FN and LN are already required, therefore using notmapped
        public string? FullName { get {return $"{FirstName} {LastName}";}} //concatenation of FN AND LN properties

        //Navigation Properties
        public virtual ICollection<Contact> Contacts { get; set; } = new HashSet<Contact>();
        public virtual ICollection<Category> Categories { get; set; } = new HashSet<Category>();
    }
}
