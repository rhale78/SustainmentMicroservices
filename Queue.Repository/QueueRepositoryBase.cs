using Common.DAL;
using Microsoft.Extensions.Configuration;

namespace Queue.Repository
{
    public abstract class QueueRepositoryBase<T> : DALObjectBase<T> where T : new()
    {
        public QueueRepositoryBase(IConfiguration configuration) : base(configuration)
        {
        }

        protected override string ConnectionString => Configuration.GetConnectionString("Queue");
    }
    
    public class QueueRepository: QueueRepositoryBase<QueueRepository>
    {
        public QueueRepository() : base(null)
        {
        }
        public QueueRepository(string queueName,string queueStatus) : base(null)
        {
            QueueName = queueName;
            QueueStatus = queueStatus;
        }
        public QueueRepository(string queueName,string queueStatus, IConfiguration configuration) : base(configuration)
        {
            QueueName = queueName;
            QueueStatus = queueStatus;
        }

        protected  string QueueName { get; init; }
        protected  string QueueStatus { get; init; }
        protected override string TableName => QueueName+QueueStatus+"Queue";

        protected override List<string> FieldNames => throw new NotImplementedException();

        protected override List<string> FieldSQLTypes => throw new NotImplementedException();

        protected override List<object> GetFieldValues(QueueRepository instance)
        {
            throw new NotImplementedException();
        }

        protected override void LoadObjectInternal(QueueRepository instance)
        {
            throw new NotImplementedException();
        }

        protected override Task PostTableCreate()
        {
            throw new NotImplementedException();
        }
    }
}
