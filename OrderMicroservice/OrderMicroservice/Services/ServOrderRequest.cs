using OrderRequestMicroservice.Data;
using OrderRequestMicroservice.DTOs;
using OrderRequestMicroservice.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using System.Net.Http.Json;
using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace OrderRequestMicroservice.Services
{
    public class ServOrderRequest
    {
        private readonly ApplicationDbContext _dataContext;
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;
        private readonly string _inventoryUrl;
        private readonly string _userUrl;
        private readonly ILogger<ServOrderRequest> _logger;

        public ServOrderRequest(
            ApplicationDbContext dataContext,
            IConfiguration configuration,
            IHttpClientFactory httpClientFactory,
            ILogger<ServOrderRequest> logger)
        {
            _dataContext = dataContext;
            _configuration = configuration;
            _httpClient = httpClientFactory.CreateClient();
            _logger = logger;

            _inventoryUrl = _configuration["Services:InventoryUrl"] ?? "http://localhost:5131/inventory";
            _userUrl = _configuration["Services:UserUrl"] ?? "http://localhost:5218/users";
            
            _logger.LogInformation($"[DEBUG] ServOrderRequest inicializado");
            _logger.LogInformation($"[DEBUG] InventoryUrl: {_inventoryUrl}");
            _logger.LogInformation($"[DEBUG] UserUrl: {_userUrl}");
        }

        public async Task<OrderRequestResponseDTO> CreateOrderRequest(CreateOrderRequestDTO request, string token)
        {
            try
            {
                _logger.LogInformation($"[DEBUG] Iniciando CreateOrderRequest");
                _logger.LogInformation($"[DEBUG] Token recebido: {token.Substring(0, Math.Min(15, token.Length))}...");
                _logger.LogInformation($"[DEBUG] ProductId: {request.ProductId}, Quantity: {request.Quantity}");

                // Validar usuário
                var userId = GetUserIdFromToken(token);
                _logger.LogInformation($"[DEBUG] UserId extraído do token: {userId}");

                var userIsValid = await ValidateUser(userId);
                _logger.LogInformation($"[DEBUG] Resultado da validação do usuário: {userIsValid}");

                if (!userIsValid)
                {
                    _logger.LogWarning($"[DEBUG] Validação de usuário falhou para userId: {userId}");
                    throw new UnauthorizedAccessException("Usuário não encontrado ou inválido");
                }

                // Obter informações do produto
                _logger.LogInformation($"[DEBUG] Obtendo informações do produto {request.ProductId}");
                var product = await GetProduct(request.ProductId);
                
                if (product == null)
                {
                    _logger.LogWarning($"[DEBUG] Produto não encontrado: {request.ProductId}");
                    throw new InvalidOperationException("Produto não encontrado");
                }

                _logger.LogInformation($"[DEBUG] Produto encontrado: {product.Name}, Quantidade disponível: {product.Quantity}");

                // Verificar quantidade disponível
                if (product.Quantity < request.Quantity)
                {
                    _logger.LogWarning($"[DEBUG] Quantidade insuficiente. Solicitado: {request.Quantity}, Disponível: {product.Quantity}");
                    throw new InvalidOperationException($"Quantidade insuficiente em estoque. Disponível: {product.Quantity}");
                }

                // Atualizar quantidade no inventário
                _logger.LogInformation($"[DEBUG] Atualizando quantidade no inventário");
                var success = await UpdateProductQuantity(request.ProductId, request.Quantity);
                
                if (!success)
                {
                    _logger.LogError("[DEBUG] Falha ao atualizar o inventário");
                    throw new Exception("Erro ao atualizar o inventário");
                }

                // Criar ordem
                _logger.LogInformation($"[DEBUG] Criando ordem de pedido");
                var orderRequest = new OrderRequest
                {
                    ProductId = request.ProductId,
                    UserId = userId,
                    Quantity = request.Quantity,
                    TotalPrice = product.Price * request.Quantity,
                    ProductName = product.Name,
                    CreatedAt = DateTime.UtcNow
                };

                _dataContext.OrderRequests.Add(orderRequest);
                await _dataContext.SaveChangesAsync();
                _logger.LogInformation($"[DEBUG] Ordem criada com sucesso: {orderRequest.Id}");

                // Retornar DTO de resposta
                var response = new OrderRequestResponseDTO
                {
                    Quantity = orderRequest.Quantity,
                    ProductName = orderRequest.ProductName,
                    TotalPrice = orderRequest.TotalPrice,
                    UserId = orderRequest.UserId,
                    CreatedAt = orderRequest.CreatedAt
                };

                _logger.LogInformation($"[DEBUG] Retornando resposta do pedido");
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError($"[DEBUG] Erro em CreateOrderRequest: {ex.Message}");
                _logger.LogError($"[DEBUG] StackTrace: {ex.StackTrace}");
                throw;
            }
        }

        private async Task<bool> ValidateUser(int userId)
        {
            try
            {
                _logger.LogInformation($"[DEBUG] Iniciando requisição HTTP para validar usuário: {userId}");
                _logger.LogInformation($"[DEBUG] URL completa: {_userUrl}/{userId}");

                var stopwatch = Stopwatch.StartNew();
                var response = await _httpClient.GetAsync($"{_userUrl}/{userId}");
                stopwatch.Stop();

                _logger.LogInformation($"[DEBUG] Tempo de resposta: {stopwatch.ElapsedMilliseconds}ms");
                _logger.LogInformation($"[DEBUG] Status code: {response.StatusCode}");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    _logger.LogInformation($"[DEBUG] Conteúdo da resposta: {content}");
                }
                else
                {
                    _logger.LogWarning($"[DEBUG] Resposta não sucedida: {response.StatusCode}");
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning($"[DEBUG] Erro content: {errorContent}");
                }

                return response.IsSuccessStatusCode;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError($"[DEBUG] Erro HTTP ao validar usuário: {ex.Message}");
                _logger.LogError($"[DEBUG] StackTrace: {ex.StackTrace}");
                throw new Exception($"Erro ao validar usuário. Serviço de usuários indisponível. Detalhes: {ex.Message}");
            }
        }

        private async Task<Product?> GetProduct(int id)
        {
            try
            {
                _logger.LogInformation($"[DEBUG] Iniciando requisição para obter produto: {id}");
                _logger.LogInformation($"[DEBUG] URL: {_inventoryUrl}/products/{id}");

                var stopwatch = Stopwatch.StartNew();
                var response = await _httpClient.GetAsync($"{_inventoryUrl}/products/{id}");
                stopwatch.Stop();

                _logger.LogInformation($"[DEBUG] Tempo de resposta: {stopwatch.ElapsedMilliseconds}ms");
                _logger.LogInformation($"[DEBUG] Status code: {response.StatusCode}");

                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    _logger.LogWarning($"[DEBUG] Produto não encontrado: {id}");
                    return null;
                }

                response.EnsureSuccessStatusCode();
                var product = await response.Content.ReadFromJsonAsync<Product>();
                _logger.LogInformation($"[DEBUG] Produto obtido com sucesso: {product?.Name}");
                return product;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError($"[DEBUG] Erro HTTP ao obter produto: {ex.Message}");
                _logger.LogError($"[DEBUG] StackTrace: {ex.StackTrace}");
                throw new Exception("Erro ao obter produto. Serviço de inventário indisponível.");
            }
        }

        private async Task<bool> UpdateProductQuantity(int id, int quantity)
        {
            try
            {
                _logger.LogInformation($"[DEBUG] Iniciando atualização de quantidade do produto: {id}");
                _logger.LogInformation($"[DEBUG] Quantidade a ser subtraída: {quantity}");

                var updateDto = new UpdateQuantityDTO
                {
                    Quantity = quantity,
                    Operation = "subtract"
                };

                var stopwatch = Stopwatch.StartNew();
                var response = await _httpClient.PutAsJsonAsync(
                    $"{_inventoryUrl}/products/{id}/quantity",
                    updateDto);
                stopwatch.Stop();

                _logger.LogInformation($"[DEBUG] Tempo de resposta: {stopwatch.ElapsedMilliseconds}ms");
                _logger.LogInformation($"[DEBUG] Status code: {response.StatusCode}");

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning($"[DEBUG] Erro ao atualizar quantidade: {errorContent}");
                }

                return response.IsSuccessStatusCode;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError($"[DEBUG] Erro HTTP ao atualizar quantidade: {ex.Message}");
                _logger.LogError($"[DEBUG] StackTrace: {ex.StackTrace}");
                throw new Exception("Erro ao atualizar quantidade. Serviço de inventário indisponível.");
            }
        }

        private int GetUserIdFromToken(string token)
        {
            try
            {
                _logger.LogInformation($"[DEBUG] Iniciando extração de userId do token");
                _logger.LogInformation($"[DEBUG] Token recebido: {token.Substring(0, Math.Min(15, token.Length))}...");

                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Secret"]);

                _logger.LogInformation($"[DEBUG] Chave JWT configurada: {_configuration["Jwt:Secret"] != null}");

                var cleanToken = token.Replace("Bearer ", "");
                _logger.LogInformation($"[DEBUG] Token limpo (sem 'Bearer'): {cleanToken.Substring(0, Math.Min(15, cleanToken.Length))}...");

                tokenHandler.ValidateToken(cleanToken, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);

                var jwtToken = (JwtSecurityToken)validatedToken;
                var userId = int.Parse(jwtToken.Claims.First(x => x.Type == "unique_name").Value);

                _logger.LogInformation($"[DEBUG] UserId extraído com sucesso: {userId}");
                return userId;
            }
            catch (Exception ex)
            {
                _logger.LogError($"[DEBUG] Erro ao extrair userId do token: {ex.Message}");
                _logger.LogError($"[DEBUG] StackTrace: {ex.StackTrace}");
                throw new UnauthorizedAccessException($"Token inválido. Detalhes: {ex.Message}");
            }
        }
    }
}