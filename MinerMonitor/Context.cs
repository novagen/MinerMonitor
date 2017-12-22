using Monitor.Models;
using System.Data.Entity;

namespace Monitor
{
	public class Context : DbContext
	{
		static Context()
		{
			// Database initialize
			Database.SetInitializer<Context>(new DbInitializer());
			using (Context db = new Context())
			{
				db.Database.Initialize(false);
			}
		}

		public DbSet<Miner> Miners { get; set; }
		public DbSet<Pool> Pools { get; set; }
		public DbSet<Proxy> Proxies { get; set; }
	}

	internal class DbInitializer : CreateDatabaseIfNotExists<Context>
	{
	}
}