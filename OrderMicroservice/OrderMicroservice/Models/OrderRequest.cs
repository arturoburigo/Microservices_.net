namespace OrderRequestMicroservice.Models
{
    public class OrderRequest
    {
        public  int Id { get; set; }
        public required int ProductId { get; set; }
        public required int UserId { get; set; }
        public required int Quantity { get; set; }
        public  decimal TotalPrice { get; set; }
        public required string ProductName { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    public class Product
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
    }
}