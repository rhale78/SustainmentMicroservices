//using Microsoft.Extensions.Configuration;

//namespace Common.Repository
//{
//    public abstract class RootRepository
//    {
//        protected IConfiguration Configuration { get; set; }
//        internal static bool IsRepositoryReady { get; set; } = false;
//        internal static object LockObject { get; } = new object();

//        protected RootRepository(IConfiguration configuration)
//        {
//            Configuration = configuration;
//        }

//        protected async Task WaitForRepositoryToBeReady()
//        {
//            //RSH 2/12/24 - may want to add a log here if the time has been more than a few seconds
//            while (!IsRepositoryReady)
//            {
//                await Task.Delay(100).ConfigureAwait(false);
//            }
//        }
//    }
//}