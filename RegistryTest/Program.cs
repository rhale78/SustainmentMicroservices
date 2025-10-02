namespace RegistryTest
{
    class Program
    {
        static async Task Main(string[] args)
        {
        }
    }
}
//            IConfigurationRoot configuration = new ConfigurationBuilder()
//                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
//                .AddJsonFile("appsettings.json")
//                .Build();

//            ApplicationRegistryRepository applicationRegistryRepository = new ApplicationRegistryRepository(configuration);
//            //await applicationRegistryRepository.Purge();

//            //ApplicationRegistryInstanceRepository repository = new ApplicationRegistryInstanceRepository(configuration);

//            ApplicationRegistryService applicationRegistryService = new ApplicationRegistryService(configuration, applicationRegistryRepository);

//            long elapsed = 0;
//            for (int j = 0; j < 2500; j++)
//            {
//                Stopwatch stopwatch = Stopwatch.StartNew();
//                ApplicationRegistryEntry registryEntry = await Helpers.GetRegistryEntry(LogInstance.CreateLog(), configuration);
//                ApplicationRegistryResult result = await applicationRegistryService.Register(registryEntry);
//                ApplicationDiscoveryService applicationDiscoveryService = new ApplicationDiscoveryService(configuration);

//                List<ApplicationDiscoveryEntry> entries = Helpers.GetDiscoveryEntries(result.ApplicationInstanceID, result.ApplicationVersionID);
//                foreach (ApplicationDiscoveryEntry entry in entries)
//                {
//                    entry.ApplicationVersionID = result.ApplicationVersionID;
//                    entry.ApplicationInstanceID = result.ApplicationInstanceID;
//                    await applicationDiscoveryService.AddDiscoveryRecords(entry);
//                }
//                List<string> urls = await applicationDiscoveryService.GetURLsForFriendlyName("Test", true);
//                ApplicationDiscoveryRoutes routes = await applicationDiscoveryService.GetRoutes("Test");
//                //Console.WriteLine($"Elapsed Time: {stopwatch.ElapsedMilliseconds}");
//                elapsed += stopwatch.ElapsedMilliseconds;
//            }
//            Console.WriteLine($"Elapsed Time: {elapsed}");
//            int b = 0;

//            //ApplicationRegistryInstance applicationRegistryInstance = new ApplicationRegistryInstance
//            //{
//            //    ApplicationPath = @"C:\Test",
//            //    IsActive = true,
//            //    MachineName = Environment.MachineName,
//            //    NumberHeartbeats = 1,
//            //    LastHeartbeatDateTime = DateTimeOffset.Now,
//            //    LastStartDateTime = DateTimeOffset.Now,
//            //    InstallDateTime = DateTimeOffset.Now
//            //};

//            //CycleDetector cycleDetector = new CycleDetector();
//            //await repository.Save(applicationRegistryInstance, cycleDetector);

//            ApplicationRegistryInfo applicationRegistryInfo = await applicationRegistryRepository.CreateOrFind("ApplicationRegistry", "1.0.0.0", @"C:\Program Files\Sustainment\ApplicationRegistry", Environment.MachineName, "12345");
//            ApplicationRegistryApplicationIDs ids = await applicationRegistryRepository.Save(applicationRegistryInfo);
//            applicationRegistryInfo = await applicationRegistryRepository.CreateOrFind("ApplicationRegistry", "1.0.0.0", @"C:\Program Files\Sustainment\ApplicationRegistry", Environment.MachineName, "1234a");
//            ids = await applicationRegistryRepository.Save(applicationRegistryInfo);
//            int c = 0;
//            await applicationRegistryRepository.GetApplicationRegistryInstanceByID(ids.ApplicationRegistryInstanceID);


//            //var instances = await repository.GetAll();
//            int a = 0;
//        }
//    }
//}
