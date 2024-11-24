using UsersMicroservice.DTO;
using UsersMicroservice.Infra;
using UsersMicroservice.Models;
using UsersMicroservice.Data;
using Microsoft.EntityFrameworkCore;


namespace UserMicroservice.Services
{
    public class ServUsuario
    {
        private readonly ApplicationDbContext _dataContext;
        private readonly ServAutenticacao _servAutenticacao;

        public ServUsuario(IConfiguration configuration)
        {
            _dataContext = GeradorDeServicos.CarregarContexto();
            _servAutenticacao = new ServAutenticacao(configuration);
        }

        public async Task<User> RegistrarUsuario(UserRegisterDTO registerDto)
        {
            if (await _dataContext.Users.AnyAsync(u => u.Email == registerDto.Email))
                throw new InvalidOperationException("Email já registrado");

            var usuario = new User
            {
                Email = registerDto.Email,
                Name = registerDto.Name,
                Password = BCrypt.Net.BCrypt.HashPassword(registerDto.Password),
                Role = "USER"
            };

            _dataContext.Users.Add(usuario);
            await _dataContext.SaveChangesAsync();

            return usuario;
        }

        public async Task<User> RegistrarAdmin(UserRegisterDTO registerDto, string adminToken)
        {
            _servAutenticacao.ValidarTokenAdmin(adminToken);

            if (await _dataContext.Users.AnyAsync(u => u.Email == registerDto.Email))
                throw new InvalidOperationException("Email já registrado");

            var usuario = new User
            {
                Email = registerDto.Email,
                Name = registerDto.Name,
                Password = BCrypt.Net.BCrypt.HashPassword(registerDto.Password),
                Role = "ADMIN"
            };

            _dataContext.Users.Add(usuario);
            await _dataContext.SaveChangesAsync();

            return usuario;
        }

        public async Task<List<User>> ListarUsuarios()
        {
            return await _dataContext.Users
                .Select(u => new User
                {
                    Id = u.Id,
                    Email = u.Email,
                    Name = u.Name,
                    Role = u.Role,
                    Password = "" // Não retorna a senha
                })
                .ToListAsync();
        }
    }
}