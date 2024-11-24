using OrderRequestMicroservice.Data;
using OrderRequestMicroservice.DTOs;
using OrderRequestMicroservice.Models;
using OrderRequestMicroservice.Infra;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace OrderRequestMicroservice.Services
{
    public class ServOrderRequest
    {
        private readonly ApplicationDbContext _dataContext;
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;
        private readonly string _inventoryUrl;
        private readonly string _userUrl;

        public ServOrderRequest()
        {
            _dataContext = GeradorDeServicos.CarregarContexto();
            _configuration = GeradorDeServicos.CarregarConfiguration();
            _httpClient = GeradorDeServicos.CarregarHttpClient();

            _inventoryUrl = _configuration["Services:InventoryUrl"] ?? "http://localhost:5000/inventory";
            _userUrl = _configuration["Services:UserUrl"] ?? "http://localhost:5001/users";
        }

        public async Task<OrderRequestResponseDTO> CreateOrderRequest(CreateOrderRequestDTO request, string token)
        {
            // Validar usuário
            var userId = GetUserIdFromToken(token);
            if (!await ValidateUser(userId))
                throw new UnauthorizedAccessException("Usuário não encontrado ou inválido");

            // Obter informações do produto
            var product = await GetProduct(request.ProductId);
            if (product == null)
                throw new InvalidOperationException("Produto não encontrado");

            // Verificar quantidade disponível
            if (product.Quantity < request.Quantity)
                throw new InvalidOperationException($"Quantidade insuficiente em estoque. Disponível: {product.Quantity}");

            // Atualizar quantidade no inventário
            var success = await UpdateProductQuantity(request.ProductId, request.Quantity);
            if (!success)
                throw new Exception("Erro ao atualizar o inventário");

            // Criar ordem
            var orderRequest = new OrderRequest
            {
                ProductId = request.ProductId,
                UserId = userId,
                Quantity = request.Quantity,
                TotalPrice = product.Price * request.Quantity,
                ProductName = product.Name
            };

            _dataContext.OrderRequests.Add(orderRequest);
            await _dataContext.SaveChangesAsync();

            // Retornar DTO de resposta
            return new OrderRequestResponseDTO
            {
                Quantity = orderRequest.Quantity,
                ProductName = orderRequest.ProductName,
                TotalPrice = orderRequest.TotalPrice,
                UserId = orderRequest.UserId,
                CreatedAt = orderRequest.CreatedAt
            };
        }

        private async Task<bool> ValidateUser(int userId)
        {
            var response = await _httpClient.GetAsync($"{_userUrl}/{userId}");
            return response.IsSuccessStatusCode;
        }

        private async Task<Product> GetProduct(int id)
        {
            var response = await _httpClient.GetAsync($"{_inventoryUrl}/products/{id}");

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                return null;

            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<Product>();
        }

        private async Task<bool> UpdateProductQuantity(int id, int quantity)
        {
            var updateDto = new UpdateQuantityDTO
            {
                Quantity = quantity,
                Operation = "subtract"
            };

            var response = await _httpClient.PutAsJsonAsync(
                $"{_inventoryUrl}/products/{id}/quantity",
                updateDto);

            return response.IsSuccessStatusCode;
        }

        private int GetUserIdFromToken(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Secret"]);

            try
            {
                tokenHandler.ValidateToken(token.Replace("Bearer ", ""), new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);

                var jwtToken = (JwtSecurityToken)validatedToken;
                return int.Parse(jwtToken.Claims.First(x => x.Type == "unique_name").Value);
            }
            catch
            {
                throw new UnauthorizedAccessException("Token inválido");
            }
        }
    }
}
