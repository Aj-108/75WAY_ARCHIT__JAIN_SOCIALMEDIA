using System;
namespace Social_Media
{
    public interface IDbConnection
    {
        string GetConnectionString();
    }


    public class DbConnection : IDbConnection
    {
        public IConfiguration Config { get; set; }

        public DbConnection(IConfiguration _config)
        {
            Config = _config;
        }


        public string GetConnectionString()
        {
            Console.WriteLine(Config.GetSection("ConnectionStrings").GetSection("DefaultConnection").Value);
            return Config.GetSection("ConnectionStrings").GetSection("DefaultConnection").Value;
        }
    }
}
