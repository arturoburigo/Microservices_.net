using UsersMicroservice.Infra;
using UsersMicroservice.DTO;
using UsersMicroservice.Models;
using UsersMicroservice.Data;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;

namespace UserMicroservice.Services

{
    public class ServAutenticacao
    {
        private readonly ApplicationDbContext _dataContext;
        private readonly IConfiguration _configuration;

        public ServAutenticacao(IConfiguration configuration)
        {
            _dataContext = GeradorDeServicos.CarregarContexto();
            _configuration = configuration;
        }

        public async Task<string> Autenticar(UserLoginDTO loginDto)
        {
            var usuario = await _dataContext.Users
                .FirstOrDefaultAsync(u => u.Email == loginDto.Email);

            if (usuario == null || !BCrypt.Net.BCrypt.Verify(loginDto.Password, usuario.Password))
                throw new UnauthorizedAccessException("Email ou senha inválidos");

            return GerarToken(usuario);
        }

        public string ValidarTokenAdmin(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Secret"]);

            try
            {
                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);

                var jwtToken = (JwtSecurityToken)validatedToken;
                var role = jwtToken.Claims.First(x => x.Type == ClaimTypes.Role).Value;

                if (role != "ADMIN")
                    throw new UnauthorizedAccessException("Acesso não autorizado");

                return role;
            }
            catch
            {
                throw new UnauthorizedAccessException("Token inválido");
            }
        }

        private string GerarToken(User usuario)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Secret"]);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.Name, usuario.Id.ToString()),
                    new Claim(ClaimTypes.Email, usuario.Email),
                    new Claim(ClaimTypes.Role, usuario.Role)
                }),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}