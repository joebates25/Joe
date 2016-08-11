using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Joe
{
    public class Environment
    {
        Dictionary<String, object> table;
        public Environment EnclosingEnvironment { get; set; }

        

        public Environment()
        {
            table = new Dictionary<string, object>();
        }

        public object GetValue(String key)
        {
            Object o = null;
            var value = table.TryGetValue(key, out o);
            if (o == null)
            {
                if (EnclosingEnvironment != null)
                {
                    return EnclosingEnvironment.GetValue(key);
                }
                else
                {
                    throw new VariableNotFoundException("Variable " + key + " could not be found.");
                }
            }
            else
            {
                return DeValue((Entry)o);
            }
        }

        public Entry GetEntry(String key)
        {
            Object o = null;
            var value = table.TryGetValue(key, out o);
            if (o == null)
            {
                if (EnclosingEnvironment != null)
                {
                    return EnclosingEnvironment.GetEntry(key);
                }
                else
                {
                    throw new VariableNotFoundException("Variable " + key + " could not be found.");
                }
            }
            else
            {
                return (Entry)o;
            }
        }

        public object DeValue(Entry entry)
        {
            if( entry.Value.GetType() == typeof(Entry))
            {
                return DeValue((Entry)entry.Value);
            }
            else
            {
                return entry.Value;
            }
        }

        public void AddValue(String key)
        {
            if (!table.ContainsKey(key))
            {
                table.Add(key, null);
            }
            else
            {
                throw new VariableAlreadyExistsInScopeException("Variable " + key + " already exists.");
            }
        }

        public void AddValueWithType(String key, String type)
        {
            if (!table.ContainsKey(key))
            {
                table.Add(key, new Entry { Type = type});
            }
            else
            {
                throw new VariableAlreadyExistsInScopeException("Variable " + key + " already exists.");
            }
        }

        public void AddValue(String key, String type)
        {
            if (!table.ContainsKey(key))
            {
                table.Add(key, new Entry { Type = type, Value = null });
            }
            else
            {
                throw new VariableAlreadyExistsInScopeException("Variable " + key + " already exists.");
            }
        }

        public string GetType(String key)
        {
            Entry entry = (Entry)this.GetValue(key);
            return entry.Type;
        }

        public void SetValue(String key, object value)
        {
            if (table.ContainsKey(key))
            {  
                    ((Entry)(table[key])).Value = value;
                
            }
            else
            {
                if (EnclosingEnvironment != null)
                {
                    EnclosingEnvironment.SetValue(key, value);
                }
                else
                {
                    throw new VariableNotFoundException();
                }
            }
        }
    }

    public class Entry
    {
        public Entry()
        {

        }

        public string Type { get; set; }

        public object Value { get; set; }
    }
}
