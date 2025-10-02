
//using ApplicationRegistry.DomainModels;
//using Microsoft.Data.SqlClient;
//using Microsoft.Extensions.Configuration;
//using System.Text;

//namespace ApplicationRegistry.DAL
//{
//    public abstract class DALObjectBase<T> : DALObjectBase where T : new()
//    {
//        static T DummyInstance { get; } = new T();
//        private ObjectLoader ObjectLoader { get; set; }

//        protected DALObjectBase(IConfiguration configuration) : base(configuration)
//        {
//        }

//        public async Task<List<T>> GetAllInternal()
//        {
//            return await ExecuteSQLQuery($"SELECT * FROM {TableName} WITH (NOLOCK)").ConfigureAwait(false);
//        }
//        public async Task<T> GetByIDInternal(int id)
//        {
//            return DummyInstance is IIDObject
//                ? await ExecuteSQLQuerySingle($"SELECT * FROM {TableName} WITH (NOLOCK) WHERE ID=@ID", new List<string>() { "@ID" }, new List<object>() { id }).ConfigureAwait(false)
//                : throw new Exception("Unable to get by ID for non-ID object");
//        }

//        public async Task<List<T>> ExecuteSQLQuery(string query, List<string> parameters = null, List<object> values = null)
//        {
//            List<T> result = new List<T>();
//            using (SqlConnection connection = new SqlConnection(ConnectionString))
//            {
//                using (SqlCommand command = new SqlCommand(query, connection))
//                {
//                    AddParameters(command, parameters, values);

//                    await connection.OpenAsync().ConfigureAwait(false);
//                    using (SqlDataReader reader = await command.ExecuteReaderAsync().ConfigureAwait(false))
//                    {
//                        while (await reader.ReadAsync().ConfigureAwait(false))
//                        {
//                            object[] rowValues = new object[reader.FieldCount];
//                            reader.GetValues(rowValues);
//                            T instance = new T();
//                            LoadObject(instance, rowValues);
//                            result.Add(instance);
//                        }
//                    }
//                }
//            }
//            return result;
//        }

//        public async Task<T> ExecuteSQLQuerySingle(string query, List<string> parameters = null, List<object> values = null)
//        {
//            using (SqlConnection connection = new SqlConnection(ConnectionString))
//            {
//                using (SqlCommand command = new SqlCommand(query, connection))
//                {
//                    AddParameters(command, parameters, values);

//                    await connection.OpenAsync().ConfigureAwait(false);
//                    using (SqlDataReader reader = await command.ExecuteReaderAsync().ConfigureAwait(false))
//                    {
//                        if (await reader.ReadAsync().ConfigureAwait(false))
//                        {
//                            object[] rowValues = new object[reader.FieldCount];
//                            reader.GetValues(rowValues);
//                            T instance = new T();
//                            LoadObject(instance, rowValues);
//                            return instance;
//                        }
//                        else
//                        {
//                            return default(T);
//                        }
//                    }
//                }
//            }
//        }
//        public async Task<int> ExecuteSQLQueryCount(string query, List<string> parameters = null, List<object> values = null)
//        {
//            using (SqlConnection connection = new SqlConnection(ConnectionString))
//            {
//                using (SqlCommand command = new SqlCommand(query, connection))
//                {
//                    AddParameters(command, parameters, values);

//                    await connection.OpenAsync().ConfigureAwait(false);
//                    return (int)await command.ExecuteScalarAsync().ConfigureAwait(false);
//                }
//            }
//        }
//        public async Task<(int id, bool saveNeeded)> SaveInternal(T instance, CycleDetector cycleDetector)
//        {
//            if (instance == null)
//            {
//                throw new Exception("Unable to save null instance");
//            }

//            if (instance is IIDObject idObject)
//            {
//                if (idObject.ID > 0)
//                {
//                    if (cycleDetector.HasCycle(TableName, GetIDValues(instance), instance))
//                    {
//                        return (idObject.ID, false);
//                    }
//                    (int id, bool saveNeeded) idData = (await Update(instance).ConfigureAwait(false), true);
//                    cycleDetector.ReplaceCycle(TableName, GetIDValues(instance), instance);
//                    return idData;
//                }
//                else
//                {
//                    idObject.ID = await Insert(instance).ConfigureAwait(false);
//                    cycleDetector.AddCycle(TableName, GetIDValues(instance), instance);
//                    return (idObject.ID, true);
//                }
//            }
//            else
//            {
//                if (!await DoesJoinValueExist(instance).ConfigureAwait(false))
//                {
//                    int value = await Insert(instance).ConfigureAwait(false);
//                    cycleDetector.AddCycle(TableName, GetIDValues(instance), instance);
//                    return (value, true);
//                }
//                else
//                {
//                    return (0, false);
//                    //RSH 1/17/24 - no update functionality for join tables
//                }
//            }
//        }
//        protected virtual async Task<bool> DoesJoinValueExist(T instance)
//        {
//            StringBuilder stringBuilder = new StringBuilder();
//            stringBuilder.Append($"SELECT COUNT(*) FROM {TableName} WITH (NOLOCK) WHERE ");
//            string whereStatement = string.Join(" AND ", FieldNames.Select(fieldName => $"{fieldName}=@{fieldName}"));
//            stringBuilder.Append(whereStatement);
//            return await ExecuteSQLQueryCount(stringBuilder.ToString(), FieldNames, GetFieldValues(instance)).ConfigureAwait(false) > 0;
//        }

//        protected virtual async Task<int> Update(T instance)
//        {
//            StringBuilder stringBuilder = new StringBuilder();
//            stringBuilder.Append("SET NOCOUNT ON; ");
//            stringBuilder.Append($"UPDATE {TableName} SET ");
//            string setStatement = string.Join(",", FieldNames.Skip(1).Select(fieldName => $"{fieldName}=@{fieldName}"));
//            stringBuilder.Append(setStatement);
//            stringBuilder.Append($" WHERE ID=@ID");
//            await ExecuteNonQuery(stringBuilder.ToString(), FieldNames, GetFieldValues(instance)).ConfigureAwait(false);
//            return (instance as IIDObject).ID;
//        }

//        protected virtual async Task<int> Insert(T instance)
//        {
//            int skip = 0;
//            if (instance is IIDObject)
//            {
//                skip = 1;   //RSH 1/23/24 - skip id field
//            }
//            StringBuilder stringBuilder = new StringBuilder();
//            stringBuilder.Append("SET NOCOUNT ON; ");
//            stringBuilder.Append($"INSERT INTO {TableName} (");
//            string fieldNames = string.Join(",", FieldNames.Skip(skip));
//            stringBuilder.Append(fieldNames);
//            stringBuilder.Append(") VALUES (");
//            string fieldValues = string.Join(",", FieldNames.Skip(skip).Select(fieldName => $"@{fieldName}"));
//            stringBuilder.Append(fieldValues);
//            stringBuilder.Append(");SELECT SCOPE_IDENTITY();");
//            if (instance is IIDObject)
//            {
//                int id = await ExecuteRetrieveID(stringBuilder.ToString(), FieldNames, GetFieldValues(instance)).ConfigureAwait(false);

//                return id <= 0 ? throw new Exception($"Unable to save {TableName}") : id;
//            }
//            else
//            {
//                await ExecuteNonQuery(stringBuilder.ToString(), FieldNames, GetFieldValues(instance)).ConfigureAwait(false);
//                return 1;
//            }
//        }

//        public async Task Purge()
//        {
//            //RSH 1/31/24 - needs to be truncate but with this possibly having foreign keys, use delete instead (fix me - should drop the constraints, truncate, then re-add constraints)
//            await ExecuteNonQuery($"delete from {TableName}").ConfigureAwait(false);
//        }

//        public async Task DropTable()
//        {
//            await ExecuteNonQuery($"DROP TABLE {TableName}").ConfigureAwait(false);
//        }

//        public string GetIDValues(T instance)
//        {
//            return instance is IIDObject idObject ? idObject.ID.ToString() : string.Join(",", GetFieldValues(instance));
//        }

//        public async Task CreateTable()
//        {
//            StringBuilder stringBuilder = new StringBuilder();
//            stringBuilder.Append($"CREATE TABLE {TableName} (");
//            string fieldDefinitions = string.Empty;
//            if (DummyInstance is IIDObject)
//            {
//                stringBuilder.Append("ID int IDENTITY(1,1) PRIMARY KEY NOT NULL,");
//                fieldDefinitions = string.Join(",", FieldNames.Skip(1).Select((fieldName, index) => $"{fieldName} {FieldSQLTypes[index]}"));
//            }
//            else
//            {
//                stringBuilder.Append("PRIMARY KEY(");
//                stringBuilder.Append(string.Join(",", FieldNames));
//                stringBuilder.Append("),");
//                fieldDefinitions = string.Join(",", FieldNames.Select((fieldName, index) => $"{fieldName} {FieldSQLTypes[index]}"));
//            }
//            stringBuilder.Append(fieldDefinitions);
//            stringBuilder.Append(")");
//            await ExecuteNonQuery(stringBuilder.ToString()).ConfigureAwait(false);

//            await CreatePrimaryKeyIndex(stringBuilder).ConfigureAwait(false);
//            await PostTableCreate().ConfigureAwait(false);
//        }

//        protected virtual async Task CreatePrimaryKeyIndex(StringBuilder stringBuilder)
//        {
//            stringBuilder.Clear();
//            if (DummyInstance is IIDObject)
//            {
//                stringBuilder.Append($"CREATE INDEX IX_{TableName}_ID ON {TableName} (ID)");
//            }
//            else
//            {
//                stringBuilder.Append($"CREATE INDEX IX_{TableName}_ID ON {TableName} (");
//                stringBuilder.Append(string.Join(",", FieldNames));
//                stringBuilder.Append(")");
//            }
//            await ExecuteNonQuery(stringBuilder.ToString()).ConfigureAwait(false);
//        }

//        protected abstract Task PostTableCreate();

//        protected void LoadObject(T instance, object[] values)
//        {
//            ObjectLoader = new ObjectLoader(values);
//            LoadObjectInternal(instance);
//            ObjectLoader.ValidateEnd();
//        }

//        protected int LoadInt()
//        {
//            return ObjectLoader.LoadInt();
//        }
//        protected string LoadString()
//        {
//            return ObjectLoader.LoadString();
//        }
//        protected DateTimeOffset LoadDateTimeOffset()
//        {
//            return ObjectLoader.LoadDateTimeOffset();
//        }
//        protected bool LoadBoolean()
//        {
//            return ObjectLoader.LoadBoolean();
//        }
//        protected int? LoadNullableInt()
//        {
//            return ObjectLoader.LoadNullableInt();
//        }
//        protected DateTimeOffset? LoadNullableDateTimeOffset()
//        {
//            return ObjectLoader.LoadNullableDateTimeOffset();
//        }
//        protected bool? LoadNullableBoolean()
//        {
//            return ObjectLoader.LoadNullableBoolean();
//        }

//        protected abstract void LoadObjectInternal(T instance);
//        protected abstract string TableName { get; }
//        protected abstract List<string> FieldNames { get; }
//        protected abstract List<string> FieldSQLTypes { get; }
//        protected abstract List<object> GetFieldValues(T instance);
//    }
//}