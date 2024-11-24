using InventoryMicroservice.Data;
using InventoryMicroservice.DTOs;
using InventoryMicroservice.Models;
using InventoryMicroservice.Infra;
using Microsoft.EntityFrameworkCore;

namespace InventoryMicroservice.Services
{
    public class ServInventory
    {
        private readonly ApplicationDbContext _dataContext;

        public ServInventory(IConfiguration configuration)
        {
            _dataContext = GeradorDeServicos.CarregarContexto();
        }

        public async Task<Product> CreateProduct(CreateProductDTO productDto)
        {
            var product = new Product
            {
                Name = productDto.Name,
                Description = productDto.Description,
                Price = productDto.Price,
                Quantity = productDto.Quantity,
            };

            _dataContext.Products.Add(product);
            await _dataContext.SaveChangesAsync();

            return product;
        }

        public async Task<Product> UpdateProduct(int id, UpdateProductDTO productDto)
        {
            var product = await _dataContext.Products.FindAsync(id);
            if (product == null)
                throw new InvalidOperationException("Produto não encontrado");

            if (productDto.Name != null)
                product.Name = productDto.Name;
            if (productDto.Description != null)
                product.Description = productDto.Description;
            if (productDto.Price.HasValue)
                product.Price = productDto.Price.Value;
            if (productDto.Quantity.HasValue)
                product.Quantity = productDto.Quantity.Value;

            product.UpdatedAt = DateTime.UtcNow;
            await _dataContext.SaveChangesAsync();

            return product;
        }

        public async Task<bool> UpdateQuantity(int id, UpdateQuantityDTO updateDto)
        {
            var product = await _dataContext.Products.FindAsync(id);
            if (product == null)
                throw new InvalidOperationException("Produto não encontrado");

            if (updateDto.Operation.ToLower() == "subtract")
            {
                if (product.Quantity < updateDto.Quantity)
                    return false;
                product.Quantity -= updateDto.Quantity;
            }
            else if (updateDto.Operation.ToLower() == "add")
            {
                product.Quantity += updateDto.Quantity;
            }
            else
            {
                throw new InvalidOperationException("Operação inválida");
            }

            product.UpdatedAt = DateTime.UtcNow;
            await _dataContext.SaveChangesAsync();
            return true;
        }

        public async Task<Product?> GetProduct(int id)
        {
            return await _dataContext.Products.FindAsync(id);
        }

        public async Task<List<Product>> ListProducts(int? skip = null, int? take = null)
        {
            var query = _dataContext.Products.AsQueryable();

            if (skip.HasValue)
                query = query.Skip(skip.Value);
            if (take.HasValue)
                query = query.Take(take.Value);

            return await query.ToListAsync();
        }

        public async Task<bool> DeleteProduct(int id)
        {
            var product = await _dataContext.Products.FindAsync(id);
            if (product == null)
                return false;

            _dataContext.Products.Remove(product);
            await _dataContext.SaveChangesAsync();
            return true;
        }

        public async Task<bool> CheckAvailability(int id, int quantity)
        {
            var product = await _dataContext.Products.FindAsync(id);
            return product != null && product.Quantity >= quantity;
        }
    }
}