using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace Common.DAL
{
    public abstract class DALObjectBase
    {
        protected IConfiguration Configuration { get; set; }
        protected abstract string ConnectionString { get; }

        protected DALObjectBase(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        protected async Task<int> ExecuteRetrieveID(string query, List<string> parameters = null, List<object> values = null)
        {
            await using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                await using (SqlCommand command = new SqlCommand(query, connection))
                {
                    AddParameters(command, parameters, values);

                    await connection.OpenAsync().ConfigureAwait(false);
                    return (int)(decimal)await command.ExecuteScalarAsync().ConfigureAwait(false);
                }
            }
        }

        protected async Task<int> ExecuteNonQuery(string query, List<string> parameters = null, List<object> values = null)
        {
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    AddParameters(command, parameters, values);

                    await connection.OpenAsync().ConfigureAwait(false);
                    return await command.ExecuteNonQueryAsync().ConfigureAwait(false);
                }
            }
        }

        protected static void AddParameters(SqlCommand command, List<string> parameters, List<object> values)
        {
            if (parameters != null && values != null)
            {
                for (int i = 0; i < parameters.Count; i++)
                {
                    command.Parameters.AddWithValue(parameters[i], values[i]);
                }
            }
        }
    }
}