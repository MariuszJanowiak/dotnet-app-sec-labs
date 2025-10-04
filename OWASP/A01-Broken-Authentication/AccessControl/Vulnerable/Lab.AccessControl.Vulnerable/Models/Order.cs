namespace Lab.AccessControl.Vulnerable.Models
{
    public class Order
    {
        public string Id { get; init; }

        public string UserId { get; init; }

        public string Product { get; init; }

        public decimal Price { get; init; }

        public Order(string id, string userId, string product, decimal price)
        {
            Id = id;
            UserId = userId;
            Product = product;
            Price = price;
        }
    }
}
