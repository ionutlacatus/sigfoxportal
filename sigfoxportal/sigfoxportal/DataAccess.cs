using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;

namespace sigfoxportal
{

    public class DataAccess : DbContext
    {
        public DataAccess() : base("DataAccess") { }
        public DbSet<Subscription> Subscriptions { get; set; }
        public DbSet<PerUserTokenCache> PerUserTokenCacheList { get; set; }
    }
    public class DataAccessInitializer : System.Data.Entity.DropCreateDatabaseIfModelChanges<DataAccess>
    {

    }

    public class PerUserTokenCache
    {
        [Key]
        public int Id { get; set; }
        public string webUserUniqueId { get; set; }
        public byte[] cacheBits { get; set; }
        public DateTime LastWrite { get; set; }
    }

    public class Subscription
    {
        public string Id { get; set; }
        [NotMapped]
        public string DisplayName { get; set; }
        [NotMapped]
        public bool IsConnected { get; set; }
        public DateTime ConnectedOn { get; set; }
        public string ConnectedBy { get; set; }
        [NotMapped]
        public bool AzureAccessNeedsToBeRepaired { get; set; }
    }
}