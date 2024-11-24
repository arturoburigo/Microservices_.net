using Microsoft.AspNetCore.Mvc;
using InventoryMicroservice.Services;
using InventoryMicroservice.DTOs;
using InventoryMicroservice.Models;
using Microsoft.AspNetCore.Authorization;

namespace InventoryMicroservice.Controllers
{
    [ApiController]
    [Route("inventory")]
    public class InventoryController : ControllerBase
    {
        private readonly ServInventory _servInventory;

        public InventoryController(IConfiguration configuration)
        {
            _servInventory = new ServInventory(configuration);
        }

        [HttpPost("products")]
        [Authorize(Roles = "ADMIN")]
        public async Task<ActionResult<Product>> CreateProduct([FromBody] CreateProductDTO productDto)
        {
            try
            {
                var product = await _servInventory.CreateProduct(productDto);
                return Ok(product);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("products/{id}")]
        [Authorize(Roles = "ADMIN")]
        public async Task<ActionResult<Product>> UpdateProduct(int id, [FromBody] UpdateProductDTO productDto)
        {
            try
            {
                var product = await _servInventory.UpdateProduct(id, productDto);
                return Ok(product);
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("products/{id}/quantity")]
        public async Task<ActionResult> UpdateQuantity(int id, [FromBody] UpdateQuantityDTO updateDto)
        {
            try
            {
                var success = await _servInventory.UpdateQuantity(id, updateDto);
                if (!success)
                    return BadRequest(new { message = "Quantidade insuficiente em estoque" });
                return Ok(new { message = "Quantidade atualizada com sucesso" });
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("products/{id}")]
        public async Task<ActionResult<Product>> GetProduct(int id)
        {
            var product = await _servInventory.GetProduct(id);
            if (product == null)
                return NotFound(new { message = "Produto não encontrado" });
            return Ok(product);
        }

        [HttpGet("products")]
        public async Task<ActionResult<List<Product>>> ListProducts([FromQuery] int? skip, [FromQuery] int? take)
        {
            var products = await _servInventory.ListProducts(skip, take);
            return Ok(products);
        }

        [HttpDelete("products/{id}")]
        [Authorize(Roles = "ADMIN")]
        public async Task<ActionResult> DeleteProduct(int id)
        {
            var success = await _servInventory.DeleteProduct(id);
            if (!success)
                return NotFound(new { message = "Produto não encontrado" });
            return Ok(new { message = "Produto deletado com sucesso" });
        }

        [HttpGet("products/{id}/check-availability")]
        public async Task<ActionResult<bool>> CheckAvailability(int id, [FromQuery] int quantity)
        {
            var available = await _servInventory.CheckAvailability(id, quantity);
            return Ok(available);
        }
    }
}