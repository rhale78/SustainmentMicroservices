using Microsoft.Extensions.Configuration;

namespace Common.Repository
{
    public abstract class RootRepository<T> where T:RootRepository<T>
    {
        protected IConfiguration Configuration { get; set; }
        protected static bool IsRepositoryReady { get; set; } = false;
        protected static object LockObject { get; } = new object();

        protected RootRepository(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        protected async Task WaitForRepositoryToBeReady()
        {
            //RSH 2/12/24 - may want to add a log here if the time has been more than a few seconds
            while (!IsRepositoryReady)
            {
                await Task.Delay(100).ConfigureAwait(false);
            }
        }

        public async Task UpgradeAndPurgeDatabase()
        {
            if (!IsRepositoryReady)
            {
                 lock (LockObject)
                {
                    //Monitor.Enter(LockObject);
                    UpgradeAndPurgeDatabaseInternal().GetAwaiter().GetResult();
                }
            }
        }

        private async Task UpgradeAndPurgeDatabaseInternal()
        {
            if (!IsRepositoryReady)
            {
                try
                {
                    await UpgradeDatabase();
                    await Purge();
                }
                finally
                {
                    IsRepositoryReady = true;
                }
            }
        }

        internal async Task UpgradeDatabase()
        {
            if (Configuration.GetBoolValueWithDefault(UpgradeDatabaseConfigKey, false))
            {
                await DropTables().ConfigureAwait(false);
                await CreateTables().ConfigureAwait(false);
            }
        }

        protected virtual string UpgradeDatabaseConfigKey => "UpgradeDatabase";

        public abstract Task DropTables();
        public abstract Task CreateTables();
        public abstract Task Purge();
    }
}