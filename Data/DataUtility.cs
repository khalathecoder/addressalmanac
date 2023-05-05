using ContactPro_Crucible.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace ContactPro_Crucible.Data
{
	public static class DataUtility
	{
		public static string GetConnectionString(IConfiguration configuration)
		{
			string? connectionString = configuration.GetConnectionString("DefaultConnection");
			string? databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");

			return string.IsNullOrEmpty(databaseUrl) ? connectionString! : BuildConnectionString(databaseUrl);
			//if (string.IsNullOrEmpty(databaseUrl))
			//{
			//    return connectionString;
			//}
			//else
			//{
			//    return BuildConnectionString(databaseUrl);
			//}
		}
		private static string BuildConnectionString(string databaseUrl)
		{
			var databaseUri = new Uri(databaseUrl);
			var userInfo = databaseUri.UserInfo.Split(':');
			var builder = new NpgsqlConnectionStringBuilder()
			{
				Host = databaseUri.Host,
				Port = databaseUri.Port,
				Username = userInfo[0],
				Password = userInfo[1],
				Database = databaseUri.LocalPath.TrimStart('/'),
				SslMode = SslMode.Require,
				TrustServerCertificate = true
			};
			return builder.ToString();
		}

		public static async Task ManageDataAsync(IServiceProvider svcProvider)
		{
			// obtaining the necessary services based on the IServiceProvider parameter
			var dbContextSvc = svcProvider.GetRequiredService<ApplicationDbContext>();
			var userManagerSvc = svcProvider.GetRequiredService<UserManager<AppUser>>();

			// align the db by checking migrations
			await dbContextSvc.Database.MigrateAsync();

			//seed demo user(s)
			await SeedDemoUsersAsync(userManagerSvc);

		}

		//Demo users Seed Method
		private static async Task SeedDemoUsersAsync(UserManager<AppUser> userManagerSvc)
		{
			AppUser demoUser = new AppUser()
			{
				UserName = "demologin@contactpro.com",
				Email = "demologin@contactpro.com",
				FirstName = "Demo",
				LastName = "User",
				EmailConfirmed = true
			};

			try
			{
				AppUser? user = await userManagerSvc.FindByEmailAsync(demoUser.Email);

				if (user == null)
				{
					await userManagerSvc.CreateAsync(demoUser,"Abc&123!");
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine("*************  ERROR  *************");
				Console.WriteLine("Error Seeding Demo Login User.");
				Console.WriteLine(ex.Message);
				Console.WriteLine("***********************************");
				throw;
			}
		}
	}

}
