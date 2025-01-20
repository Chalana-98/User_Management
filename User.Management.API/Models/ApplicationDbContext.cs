using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace User.Management.API.Models
{
    public class ApplicationDbContext : IdentityDbContext<IdentityUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base (options)
        {

        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            SeedRoles(builder);
        }

        private static void SeedRoles(ModelBuilder builder)
        {
            builder.Entity<IdentityRole>().HasData
            (
                new IdentityRole()
                {
                    Id = "3b098b51-725c-491c-85c5-e937c8ad6865",
                    Name = "Admin",
                    ConcurrencyStamp = "1",
                    NormalizedName = "Admin"
                },
                new IdentityRole()
                {
                    Id = "cd7d6b74-0fd3-4fc0-9d5d-61d2154d622d",
                    Name = "User",
                    ConcurrencyStamp = "2",
                    NormalizedName = "User"
                },
                new IdentityRole()
                {
                    Id = "83f8b40e-19e2-43b1-9d46-0dc0ac9205fb",
                    Name = "HR",
                    ConcurrencyStamp = "3",
                    NormalizedName = "HR"
                }
            );
        }


    }
}
