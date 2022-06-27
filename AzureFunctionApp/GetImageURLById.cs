using System;
using System.Net;
using System.Data.SqlClient;

using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace AzureFunctionApp
{
    public class GetImageURLById
    {
        private readonly ILogger _logger;

        public GetImageURLById(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<GetImageURLById>();
        }

        [Function("GetImageURLById")]
        public HttpResponseData Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequestData req)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request.");

            var query = System.Web.HttpUtility.ParseQueryString(req.Url.Query);
            string itemId = query["Id"]?.ToString();

            var response = req.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("Content-Type", "text/plain; charset=utf-8");

            SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder();
                       
            var dbUserId = Environment.GetEnvironmentVariable("KeyVaultSQLDBUserId", EnvironmentVariableTarget.Process);
            var dbPassword = Environment.GetEnvironmentVariable("KeyVaultSQLDBPassword", EnvironmentVariableTarget.Process);
            var dbDataSource = Environment.GetEnvironmentVariable("KeyVaultSQLDataSource", EnvironmentVariableTarget.Process);
            var dbName = Environment.GetEnvironmentVariable("KeyVaultSQLInitialCatalog", EnvironmentVariableTarget.Process);
            var tblName = Environment.GetEnvironmentVariable("KeyVaultSQLTableName", EnvironmentVariableTarget.Process);
            var conString = Environment.GetEnvironmentVariable("KeyVaultSQLConnectionString", EnvironmentVariableTarget.Process);

            response.WriteString($"DB User Id: {dbUserId} \n");
            response.WriteString($"DB Password: {dbPassword} \n");
            response.WriteString($"DB Data Source: {dbDataSource} \n");
            response.WriteString($"DB Name: {dbName} \n");
            response.WriteString($"Main Table Name: {tblName} \n\n");
            response.WriteString($"Full Connection String:\n {conString} \n\n");

            //Azure Cloud DB
            builder.DataSource = dbDataSource;
            builder.UserID = dbUserId;
            builder.Password = dbPassword;
            builder.InitialCatalog = dbName;

            //builder.ConnectionString = conString;

            response.WriteString($"Query String Parameter URL Id : {itemId} \n\n");

            using (SqlConnection connection = new SqlConnection(builder.ConnectionString))
            {
                string sql = string.Empty;

                if(string.IsNullOrEmpty(itemId))
                    sql = $"SELECT * FROM {tblName}";
                else
                    sql = $"SELECT * FROM {tblName} WHERE Id={itemId}";

                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            response.WriteString($"{reader.GetInt32(0)} - {reader.GetString(1)} - {reader.GetString(2)} \n");
                        }
                    }
                }
            }

            return response;
        }
    }
}
