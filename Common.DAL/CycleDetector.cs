
namespace Common.DAL
{
    public class CycleDetector
    {
        private Dictionary<string, Dictionary<string, object>> CycleDictionary { get; } = new Dictionary<string, Dictionary<string, object>>();

        public bool HasCycle(string tableName, string key, object value)
        {
            if (CycleDictionary.ContainsKey(tableName))
            {
                if (CycleDictionary[tableName].ContainsKey(key))
                {
                    return true;
                    //RSH 2/12/24 -the code below does not appear to be correct.  If the object has been saved already, it doesn't matter if another object has a "copy" of it - it's still the same by id
                    if (CycleDictionary[tableName][key] == value)
                    {
                        return true;
                    }
                    else
                    {
                        int a = 0;
                        //CycleDictionary[tableName][key] = value;
                        return false;
                    }
                }
                else
                {
                    CycleDictionary[tableName].Add(key, value);
                    return false;
                }
            }
            else
            {
                CycleDictionary.Add(tableName, new Dictionary<string, object>() { { key, value } });
                return false;
            }
        }

        internal void AddCycle(string tableName, string key, object value)
        {
            if (CycleDictionary.ContainsKey(tableName))
            {
                if (CycleDictionary[tableName].ContainsKey(key))
                {
                    if (CycleDictionary[tableName][key] != value)
                    {
                        int a = 0;
                        CycleDictionary[tableName][key] = value;
                    }
                }
                else
                {
                    CycleDictionary[tableName].Add(key, value);
                }
            }
            else
            {
                CycleDictionary.Add(tableName, new Dictionary<string, object>() { { key, value } });
            }
        }

        internal void ReplaceCycle(string tableName, string key, object instance)
        {
            CycleDictionary[tableName][key] = instance;
        }
    }
}