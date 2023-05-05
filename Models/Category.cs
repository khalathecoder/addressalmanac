using System.ComponentModel.DataAnnotations;

namespace ContactPro_Crucible.Models
{
    public class Category
    {
        //primary key
        public int Id { get; set; } //first need identifier when creating new class, db is smart and knows to add this to db based on id

        [Required]
        public string? AppUserId { get; set; } //GUID is alphanumeric, that is reason for string

        [Required]
        [Display(Name="Category Name")]
        public string? Name { get; set; }

        //Navigation Property
        public virtual AppUser? AppUser { get; set; } //this is needed in order  to create relation between AppUserId and Id to work together to retrieve users info
        public virtual ICollection<Contact> Contacts { get; set; } = new HashSet<Contact>(); //category has respective list of contacts anytime i load from db; many to many relationship; 
    }
}
