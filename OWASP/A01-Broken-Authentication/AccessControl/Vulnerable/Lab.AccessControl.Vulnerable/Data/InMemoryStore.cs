using Lab.AccessControl.Vulnerable.Models;

namespace Lab.AccessControl.Vulnerable.Data
{
    public class InMemoryStore
    {
        public List<User> Users { get; } = new()
        {
            new User("1", "alice", "password", "User"),
            new User("2", "bob", "password", "User"),
            new User("3", "admin", "password", "Admin")
        };

        public List<Order> Orders { get; } = new()
        {
            new Order("o1", "1", "Widget A", 10.00m),
            new Order("o2", "1", "Widget B", 15.50m),
            new Order("o3", "2", "Gadget X", 42.00m)
        };
    }
}
