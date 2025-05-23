using Microsoft.EntityFrameworkCore;
using Models;

public class ContextDb : DbContext
{
    public ContextDb(DbContextOptions<ContextDb> options) : base(options) { }

    // DbSet properties for each entity that maps to a database table
    public DbSet<Address> Addresses { get; set; } = default!;
    public DbSet<Restaurant> Restaurants { get; set; } = default!;
    public DbSet<Cuisine> Cuisines { get; set; } = default!;
    public DbSet<Review> Reviews { get; set; } = default!;
    public DbSet<User> Users { get; set; } = default!; // Ensure User is also included


}