using Gumani_Moila_ST10229429_CLDV7111w.Models;
using Microsoft.EntityFrameworkCore;


namespace Gumani_Moila_ST10229429_CLDV7111w.Data
{
    public class EventEaseContext  : DbContext
    {
    public EventEaseContext(DbContextOptions<EventEaseContext> options) : base(options)
    { }

    public DbSet<Event> Event { get; set; }
    public DbSet<Venue> Venue { get; set; }
    public DbSet<Booking> Booking { get; set; }

    public DbSet<User> User { get; set; }

    public DbSet<CustomerDetail> CustomerDetail { get; set; }
}


}
