using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Joe
{
    public class Environment
    {
        Dictionary<object, object> table;
        public Environment EnclosingEnvironment { get; set; }

        public Guid ScopeID { get; set; }



        public Environment()
        {
            table = new Dictionary<object, object>();
            ScopeID = Guid.NewGuid();
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
            else if (o.GetType() == typeof(PointerEntry))
            {
                var pointEntry = (PointerEntry)o;
                return GetValueByScope(pointEntry.Key, pointEntry.Value);
            }
            else
            {
                return DeValue((Entry)o);
            }
        }

        public object GetValue(String key, int index)
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
            else if (o.GetType() == typeof(PointerEntry))
            {
                var pointEntry = (PointerEntry)o;
                return GetValueByScope(pointEntry.Key, pointEntry.Value);
            }
            else
            {
                if (((Entry)o).Value.GetType() == typeof(string))
                {
                    return ((Entry)o).Value.ToString()[index].ToString();
                }
                else
                {
                    return ((object[])DeValue((Entry)o))[index];
                }
            }
        }

        private object GetValueByScope(String key, Guid scope)
        {
            if (this.ScopeID != scope)
            {
                if (EnclosingEnvironment == null)
                {
                    throw new ScopeNotFoundException();
                }
                else
                {
                    return EnclosingEnvironment.GetValueByScope(key, scope);
                }
            }
            else
            {
                var val = GetValue(key);
                if (val.GetType() == typeof(PointerEntry))
                {
                    var val2 = (PointerEntry)val;
                    return GetValueByScope(val2.Key, scope);
                }
                else
                {
                    return val;
                }
            }
        }

        public void MemDump()
        {
            Console.Out.WriteLine(String.Format("Scope ID: {0}", ScopeID));
            foreach (var key in this.table.Keys)
            {
                var val = table[key];
                if (val.GetType() == typeof(PointerEntry))
                {
                    var pointer = (PointerEntry)val;
                    Console.Out.WriteLine(String.Format("{0,10} - {1,-10}",key, pointer.ToString()));
                }
                else if (((Entry)val).Type.Equals("arr"))
                {
                    Object[] arr = (Object[])((Entry)val).Value;
                    Console.Out.WriteLine(String.Format("{0,10} - Array", key));
                    for(int i = 0; i < arr.Length; i++)
                    {
                        Console.Out.WriteLine(String.Format("     {0} - {1}", i, arr[i]));
                    }
                }
                else
                {
                    Console.Out.WriteLine(String.Format("{0,10} - {1,-10}", key, val.ToString()));
                }
            }
            if (this.EnclosingEnvironment != null)
            {
                this.EnclosingEnvironment.MemDump();
            }
            else
            {
                Console.Out.WriteLine("~~~~END MEM DUMP~~~~");
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

        private object DeValue(Entry entry)
        {
            if (entry.Value.GetType() == typeof(Entry))
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
                table.Add(key, new Entry { Type = type });
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
                if (table[key].GetType() == typeof(PointerEntry))
                {
                    var point = (PointerEntry)table[key];
                    SetValue(point.Key, value, point.Value);
                }
                else
                {
                    ((Entry)(table[key])).Value = value;
                }

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

        public void SetValue(String key, object value, int index)
        {


            if (table.ContainsKey(key))
            {
                if (table[key].GetType() == typeof(PointerEntry))
                {
                    var point = (PointerEntry)table[key];
                    SetValue(point.Key, value, point.Value);
                }
                else
                {
                    ((object[])((Entry)(table[key])).Value)[index] = value;
                }

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

        public void SetValue(String key, object value, Guid scopeID)
        {
            if (this.ScopeID == ScopeID)
            {
                SetValue(key, value);
            }
            else
            {
                if (EnclosingEnvironment != null)
                {
                    SetValue(key, value, scopeID);
                }
                else
                {
                    throw new ScopeNotFoundException();
                }
            }
        }

        public void SetValue(String key, PointerEntry pointer)
        {
            if (table.ContainsKey(key))
            {
                table[key] = pointer;

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

        public override string ToString()
        {
            if (Value != null)
            {
                return Value.ToString() + ":" + Type;
            }
            else
            {
                return "null" + ":" + Type;
            }
        }
    }

    public class PointerEntry : Entry
    {
        public new Guid Value { get; set; }

        public string Key { get; set; }

        public override string ToString()
        {
            if (Value != null)
            {
                return Key.ToString() + ":" + Value.ToString();
            }
            else
            {
                return "null" + ":" + Type;
            }
        }
    }

    public class ObjectEntry:Dictionary<string, object>{

        public ObjectEntry(string type){
            Type = type;
        }
        public string Type {get;set;}
    }
}
