using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;

namespace SMSS.DBContext
{
    public class DBContextCon
    {
        private readonly IConfiguration _configuration;
        private readonly string _connectionString;

        public DBContextCon(IConfiguration configuration)
        {
            _configuration = configuration;
            _connectionString = _configuration.GetConnectionString("DefaultConnection");
        }
        public IDbConnection CreateConnection() => new SqlConnection(_connectionString);
    }
}
