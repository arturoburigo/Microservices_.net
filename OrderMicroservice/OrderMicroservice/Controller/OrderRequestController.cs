using Microsoft.AspNetCore.Mvc;
using OrderRequestMicroservice.Services;
using OrderRequestMicroservice.DTOs;
using Microsoft.AspNetCore.Authorization;

namespace OrderRequestMicroservice.Controllers
{
    [ApiController]
    [Route("order-requests")]
    [Authorize]
    public class OrderRequestController : ControllerBase
    {
        private readonly ServOrderRequest _servOrderRequest;

        public OrderRequestController()
        {
            _servOrderRequest = new ServOrderRequest();
        }

        [HttpPost]
        public async Task<ActionResult<OrderRequestResponseDTO>> CreateOrderRequest(
            [FromBody] CreateOrderRequestDTO request)
        {
            try
            {
                var token = Request.Headers["Authorization"].ToString();
                var response = await _servOrderRequest.CreateOrderRequest(request, token);
                return Ok(response);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }
    }
}