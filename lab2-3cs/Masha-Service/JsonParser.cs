using System;
using System.Reflection;
using System.Runtime.Remoting;
using System.Configuration;
using System.Text.Json;
using System.IO;

namespace M_Service
{
    class JsonParser: IConfigurationProvider
    {
        public JsonParser() { }
        public T Parse<T>() where T : new()
        {
            T obj;
            Type typeNeed = typeof(T);
            string jsonPath = ConfigurationManager.AppSettings["PathToJson"];
            string jsonText = File.ReadAllText(jsonPath);

            using (JsonDocument jDoc = JsonDocument.Parse(jsonText))
            {
                // работа с коренем
                JsonElement root = jDoc.RootElement;
                ParsableAttribute cattr = typeNeed.GetCustomAttribute(typeof(ParsableAttribute)) as ParsableAttribute;
                JsonElement el;
                if (cattr != null)
                    el = root.GetProperty(cattr.AliasAlice);
                else
                    el = root.GetProperty(typeNeed.Name);

                if (el.ValueKind != JsonValueKind.Null && el.ValueKind != JsonValueKind.Undefined)
                {
                    MethodInfo nodeGenericParser = typeof(JsonParser)
                        .GetMethod("ParseNode", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance)
                        .MakeGenericMethod(typeNeed);
                    object[] args = { el };
                    try
                    {
                        obj = (T)nodeGenericParser.Invoke(this, args); 
                        return obj;
                    }
                    catch (Exception ex)
                    {
                        string errorLogPath = ConfigurationManager.AppSettings["ErrorLogPath"];
                        if (File.Exists(errorLogPath))
                        {
                            File.AppendAllText(errorLogPath, "\n" + ex.Message + "\n" + ex.StackTrace);
                        }
                        return new T();
                    }
                }
                return new T();
            }
        }

        private T ParseNode<T>(JsonElement nodeSelected) where T : new()
        {
            Type t = typeof(T);
            string obectNname = t.FullName;
            ObjectHandle handle;
            try
            {
                handle = Activator.CreateInstance(null, obectNname); 
            }
            catch (Exception ex)
            {
                string errorLogPath = ConfigurationManager.AppSettings["ErrorLogPath"];
                if (File.Exists(errorLogPath))
                {
                    File.AppendAllText(errorLogPath, "\n" + ex.Message + "\n" + ex.StackTrace);
                }
                return new T();
            }
            object obectComplex = handle.Unwrap(); 
            foreach (JsonProperty el in nodeSelected.EnumerateObject())
            {
                if (el.Value.ValueKind != JsonValueKind.Null && el.Value.ValueKind != JsonValueKind.Undefined)
                {
                    if (el.Value.ValueKind == JsonValueKind.Object)
                    {
                        PropertyInfo[] props = obectComplex.GetType()
                            .GetProperties(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
                        foreach (PropertyInfo pi in props)
                        {
                            ParsableAttribute cattr = pi.GetCustomAttribute(typeof(ParsableAttribute)) as ParsableAttribute;
                            if ((cattr != null && cattr.AliasAlice == el.Name) || pi.Name == el.Name)
                            {
                                // извлечение нужного типа
                                Type typeNeeded = pi.PropertyType;
                                MethodInfo nodeGenericParser = typeof(JsonParser)
                                    .GetMethod("ParseNode", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance)
                                    .MakeGenericMethod(typeNeeded);
                                object[] args = { el.Value };
                                object val = nodeGenericParser.Invoke(this, args);
                                if (val.GetType() == typeNeeded)
                                    pi.SetValue(obectComplex, val);
                            }
                        }
                    }
                    else
                    {
                        PropertyInfo[] props = obectComplex.GetType()
                            .GetProperties(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
                        foreach (PropertyInfo pi in props)
                        {
                            ParsableAttribute cattr = pi.GetCustomAttribute(typeof(ParsableAttribute)) as ParsableAttribute;
                            if ((cattr != null && cattr.AliasAlice == el.Name) || pi.Name == el.Name)
                            {
                                object val = Convert.ChangeType(el.Value.GetString(), pi.PropertyType);
                                pi.SetValue(obectComplex, val);
                            }
                        }
                    }
                }
            }
            return (T)obectComplex;
        }
    }
}
