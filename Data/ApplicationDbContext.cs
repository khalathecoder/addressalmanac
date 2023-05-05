using ContactPro_Crucible.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ContactPro_Crucible.Data
{
    public class ApplicationDbContext : IdentityDbContext<AppUser> //"type parameter" by adding <AppUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Contact> Contacts { get; set; } = default!; //reps our table in db and mirror it
        public virtual DbSet<Category> Categories { get; set; } = default!; 
    }
}