
using System.ComponentModel.DataAnnotations; //called directives
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;

namespace ContactPro_Crucible.Models
{
    public class Contact
    {
        //Primary Key - identifying piece of info about any individual record or "entity" in the db
        //ID is auto created by db ; could be any num unordered
        public int Id { get; set; }

        //Foreign Key - can appear multiple times in table, will refer to actual primary key of users table (who is logged in)
        //User table and contact table will be directly correlated
        //they work together to be able to look into the table and obtain their respective content
        [Required]
        public string? AppUserId { get; set; } //GUID stored for specific logged in user to get there specific contacts

        [Required]
        [Display(Name = "First Name")]
        [StringLength(50, ErrorMessage = "The {0} must be at least {2} and max {1} characters long.", MinimumLength = 2)]
        public string? FirstName { get; set; }

        [Required]
        [Display(Name = "Last Name")]
        [StringLength(50, ErrorMessage = "The {0} must be at least {2} and max {1} characters long.", MinimumLength = 2)]
        public string? LastName { get; set; }

        [NotMapped]
        public string? FullName { get { return $"{FirstName} {LastName}"; } }

        [Required]
        [DataType(DataType.Date)]
        public DateTime CreatedDate { get; set; }

        [Display(Name = "Birthday")]
        [DataType(DataType.Date)] //adds in calendar for user to select date easier
        public DateTime? DateOfBirth { get; set; }

        public string? Address1 { get; set; } //? makes nullable to avoid runtime errors; anything nullable it is not required by default
        public string? Address2 { get; set; }
        public string? City { get; set; }

        public Enums.States State { get; set; } //Enums "enumurates the data that is in it"

        [Display(Name = "Zip Code")]
        [DataType(DataType.PostalCode)]
        public int ZipCode { get; set; }

        [Required]
        [Display(Name = "Email Address")]
        [DataType(DataType.EmailAddress)]
        public string? Email { get; set; }

        [Required]
        [Display(Name = "Phone Number")]
        [DataType(DataType.PhoneNumber)]
        public string? PhoneNumber { get; set; }

        // Image Properties
        [NotMapped] //prevents items from going to the db
        public IFormFile? ImageFile { get; set; }
        public byte[]? ImageData { get; set; } //turns any image into byte array, more like a string
        public string? ImageType { get; set; }
        // Image Properties

        
        //Navigation Properties - means of which to navigate to another table
        public virtual AppUser? AppUser { get; set; } //works with AppUserId and Id to create foreign key, to be able to retrieve information from table; virtual overrides and allows for users information to repopulate when they login 

        public virtual ICollection<Category> Categories { get; set; } = new HashSet<Category>(); //list of categories in the contact table for any single contact
    }
}
