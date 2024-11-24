namespace InventoryMicroservice.DTOs
{
    public class CreateProductDTO
    {
        public required string Name { get; set; }
        public required string Description { get; set; }
        public required decimal Price { get; set; }
        public required int Quantity { get; set; }
    }

    public class UpdateProductDTO
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public decimal? Price { get; set; }
        public int? Quantity { get; set; }
    }

    public class UpdateQuantityDTO
    {
        public required int Quantity { get; set; }
        public required string Operation { get; set; } // "add" ou "subtract"
    }
}