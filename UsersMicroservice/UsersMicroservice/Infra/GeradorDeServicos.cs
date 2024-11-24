using UsersMicroservice.Data;

namespace UsersMicroservice.Infra
{
    public class GeradorDeServicos
    {
        public static ServiceProvider ServiceProvider;
        public static ApplicationDbContext CarregarContexto()
        {
            return ServiceProvider.GetService<ApplicationDbContext>();
        }
    }
}
