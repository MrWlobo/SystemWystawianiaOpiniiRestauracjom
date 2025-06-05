using Microsoft.EntityFrameworkCore;
using Models;

namespace Db
{
    public class ContextDb : DbContext
{
    public ContextDb(DbContextOptions<ContextDb> options) : base(options) { }


    public DbSet<Address> Addresses { get; set; } = default!;
    public DbSet<Restaurant> Restaurants { get; set; } = default!;
    public DbSet<Cuisine> Cuisines { get; set; } = default!;
    public DbSet<Review> Reviews { get; set; } = default!;
    public DbSet<User> Users { get; set; } = default!;


}
}
