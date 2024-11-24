namespace OrderRequestMicroservice.DTOs
{
    public class CreateOrderRequestDTO
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
    }

    public class OrderRequestResponseDTO
    {
        public int Quantity { get; set; }
        public required string ProductName { get; set; }
        public decimal TotalPrice { get; set; }
        public int UserId { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class UpdateQuantityDTO
    {
        public required int Quantity { get; set; }
        public required string Operation { get; set; } // "add" ou "subtract"
    }
}