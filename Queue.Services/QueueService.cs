using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Queue.Interfaces;
using Queue.Repository;

namespace Queue.Services
{
    public class QueueService : IQueueService
    {
        public IConfiguration Configuration { get; }
        public ILogger<QueueService> Logger { get; }
        public IQueueRepository QueueRepository { get; }

        public QueueService(IConfiguration configuration, ILogger<QueueService> logger,IQueueRepository queueRepository)
        {
            Configuration = configuration;
            Logger = logger;
            QueueRepository = queueRepository;
        }

    }
}
