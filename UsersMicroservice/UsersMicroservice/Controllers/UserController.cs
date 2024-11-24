using Microsoft.AspNetCore.Mvc;
using UserMicroservice.Services;
using UsersMicroservice.DTO;
using UsersMicroservice.Models;

namespace UsersMicroservice.Controllers
{
    [ApiController]
    [Route("users")]
    public class UserController : ControllerBase
    {
        private readonly ServUsuario _servUsuario;
        private readonly ServAutenticacao _servAutenticacao;

        public UserController(IConfiguration configuration)
        {
            _servUsuario = new ServUsuario(configuration);
            _servAutenticacao = new ServAutenticacao(configuration);
        }

        [HttpPost("login")]
        public async Task<ActionResult<string>> Login([FromBody] UserLoginDTO loginDto)
        {
            try
            {
                var token = await _servAutenticacao.Autenticar(loginDto);
                return Ok(new { token });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
        }

        [HttpPost("register")]
        public async Task<ActionResult<User>> Register([FromBody] UserRegisterDTO registerDto)
        {
            try
            {
                var usuario = await _servUsuario.RegistrarUsuario(registerDto);
                return Ok(usuario);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("register/admin")]
        public async Task<ActionResult<User>> RegisterAdmin(
            [FromBody] UserRegisterDTO registerDto,
            [FromHeader(Name = "Authorization")] string authorization)
        {
            try
            {
                var token = authorization.Replace("Bearer ", "");
                var usuario = await _servUsuario.RegistrarAdmin(registerDto, token);
                return Ok(usuario);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
