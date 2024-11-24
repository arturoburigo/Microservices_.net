using OrderRequestMicroservice.Data;

namespace OrderRequestMicroservice.Infra
{
    public class GeradorDeServicos
    {
        public static ServiceProvider ServiceProvider;

        public static ApplicationDbContext CarregarContexto()
        {
            return ServiceProvider.GetService<ApplicationDbContext>();
        }

        public static IConfiguration CarregarConfiguration()
        {
            return ServiceProvider.GetService<IConfiguration>();
        }

        public static HttpClient CarregarHttpClient()
        {
            return ServiceProvider.GetService<HttpClient>();
        }
    }
}