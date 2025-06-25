using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace WebApplication1.Data;

public class ApplicationDbContext : IdentityDbContext<AppUser>
{
    public ApplicationDbContext(DbContextOptions options) : base(options)
    {
       
        
    }
    // no need to dbset app user cause alr using identitydbcontext
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        var roles = new IdentityRole[]
        {
            new IdentityRole
            {
                Id = "1",
                Name = "Admin",
                NormalizedName = "ADMIN",
                //Guid is a static class can use anywhere thats why no new
                ConcurrencyStamp = Guid.NewGuid().ToString()
            },
            new IdentityRole
            {
                Id = "2",
                Name = "Supervisor",
                NormalizedName = "SUPERVISOR",
                ConcurrencyStamp = Guid.NewGuid().ToString()
            },
            new IdentityRole
            {
                Id = "3",
                Name = "Member",
                NormalizedName = "MEMBER",
                ConcurrencyStamp = Guid.NewGuid().ToString()
            },
        };

        builder.Entity<IdentityRole>((b) => 
            b.HasData(roles));
        
    }
}
