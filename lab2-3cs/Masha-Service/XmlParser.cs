using System;
using System.Xml;
using System.Xml.Schema;
using System.Reflection;
using System.Runtime.Remoting;
using System.Configuration;
using System.IO;

namespace M_Service
{
    class XmlParser : IConfigurationProvider
    {
        private bool Valid { get; set; } = false;
        private bool ValidationNeeded { get; } = false;
        public XmlParser() 
        {
            try
            {
                ValidationNeeded = Convert.ToBoolean(ConfigurationManager.AppSettings["Validate"]);
            }
            catch (Exception)
            {
                string validationLogPath = ConfigurationManager.AppSettings["ValidationLogPath"];
                if (File.Exists(validationLogPath))
                {
                    File.AppendAllText(validationLogPath, "\nНе известно, требуется ли валидация. По умолчанию не требуется, " +
                        "возможно задание некоторых полей по умолчанию.\n");
                }
            }
        }

        public T Parse<T>() where T : new()
        {
            Type typeNeeded = typeof(T);
            
            if (ValidationNeeded && !Validate())
            {
                string validationLogPath = ConfigurationManager.AppSettings["ValidationLogPath"];
                if (File.Exists(validationLogPath))
                {
                    File.AppendAllText(validationLogPath, "Ошибка валидации XML.\nВозвращён экземпляр объекта класса " +
                        $"{typeNeeded.FullName} по умолчанию.\n");
                }
                return new T();
            }
            if (!ValidationNeeded)
            {
                string validationLogPath = ConfigurationManager.AppSettings["ValidationLogPath"];
                if (File.Exists(validationLogPath))
                {
                    File.AppendAllText(validationLogPath, "Валидация XML не требуется.\nВозможен " +
                        "задание некоторых полей по умолчанию.\n");
                }
            }

            XmlDocument xDoc = new XmlDocument();
            string xmlpath = ConfigurationManager.AppSettings["PathToXml"];
            xDoc.Load(xmlpath);

            // получение корневого элемента
            XmlElement xRoot = xDoc.DocumentElement;
            if (typeNeeded.GetCustomAttribute(typeof(ParsableAttribute)) is ParsableAttribute cattr)
            {
                try
                {
                    return GetObjectRoot<T>(xRoot, cattr.AliasAlice, typeNeeded);
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
            else
            {
                try
                {
                    return GetObjectRoot<T>(xRoot, typeNeeded.Name, typeNeeded);
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
        }
        private T GetObjectRoot<T>(XmlElement xRoot, string name, Type typeNeeded)
        {
            XmlNode xnode = null;
            foreach (XmlNode node in xRoot)
            {
                if (node.Name == name)
                {
                    xnode = node;
                    break;
                }
            }
            if (xnode == null)
            {
                throw new Exception($"Element {name} not found in XML.");
            }
            
            MethodInfo nodeGenericParser = typeof(XmlParser)
                .GetMethod("ParseNode", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance)
                .MakeGenericMethod(typeNeeded);
            object[] args = { xnode };
            T obj = (T)nodeGenericParser.Invoke(this, args);
            return obj;
        }
        private T ParseNode<T>(XmlNode nodeSelected) where T : new()
        {
            Type t = typeof(T);
            string objName = t.FullName;
            ObjectHandle handle;
            try
            {
                handle = Activator.CreateInstance(null, objName);
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
            object objComplex = handle.Unwrap(); // объект типа, полученного из тега, созданный в рантайме

            if (nodeSelected.Attributes.Count > 0) 
            {
                foreach (XmlAttribute xattr in nodeSelected.Attributes)
                {
                    if (xattr != null)
                    {
                        SetPrimitiveXml<XmlAttribute>(objComplex, xattr);
                    }
                }
            }
            // обхождение узлов
            if (nodeSelected.HasChildNodes)
            {
                foreach (XmlNode xxnode in nodeSelected.ChildNodes)
                {
                    if (xxnode != null)
                    {   
                        if (xxnode.ChildNodes.Count == 1 && xxnode.FirstChild.NodeType == XmlNodeType.Text && xxnode.Attributes.Count == 0)
                        {
                            SetPrimitiveXml<XmlNode>(objComplex, xxnode);
                        }
                        else
                        {
                            
                            SetEmbeddedObject(objComplex, xxnode);
                        }
                    }
                }
            }
            return (T)objComplex; // возвращение объекта
        }
        private void SetPrimitiveXml<T>(object complexobj, T partOfTag) 
        {
            Type t = typeof(T);
            try
            {
                if (t == typeof(XmlAttribute))
                {
                    XmlAttribute xattr = partOfTag as XmlAttribute;
                    PropertyInfo[] props = complexobj.GetType()
                                .GetProperties(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
                    foreach (PropertyInfo pi in props)
                    {
                        if (pi.GetCustomAttribute(typeof(ParsableAttribute)) is ParsableAttribute cattr && cattr.AliasAlice == xattr.Name)
                        {
                            object val = Convert.ChangeType(xattr.Value, pi.PropertyType);
                            pi.SetValue(complexobj, val);
                        }
                    }
                }
                else if (t == typeof(XmlNode))
                {
                    XmlNode xnode = partOfTag as XmlNode;
                    PropertyInfo[] props = complexobj.GetType()
                                    .GetProperties(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
                    foreach (PropertyInfo pi in props)
                    {
                        if ((pi.GetCustomAttribute(typeof(ParsableAttribute)) is ParsableAttribute cattr && cattr.AliasAlice == xnode.Name) || pi.Name == xnode.Name)
                        {
                            object val = Convert.ChangeType(xnode.InnerText.Trim(), pi.PropertyType);
                            pi.SetValue(complexobj, val);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                string errorLogPath = ConfigurationManager.AppSettings["ErrorLogPath"];
                if (File.Exists(errorLogPath))
                {
                    File.AppendAllText(errorLogPath, "\n" + ex.Message + "\n" + ex.StackTrace);
                }
            }
        }
        private void SetEmbeddedObject(object complexobj, XmlNode childnode)
        {
            try
            {
                PropertyInfo[] props = complexobj.GetType()
                                    .GetProperties(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
                foreach (PropertyInfo pi in props)
                {
                    if ((pi.GetCustomAttribute(typeof(ParsableAttribute)) is ParsableAttribute cattr && cattr.AliasAlice == childnode.Name) || pi.Name == childnode.Name)
                    {
                        // извлечение нужного типа
                        Type typeNeeded = pi.PropertyType;
                        MethodInfo nodeGenericParser = typeof(XmlParser)
                            .GetMethod("ParseNode", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance)
                            .MakeGenericMethod(typeNeeded);
                        object[] args = { childnode };
                        object val = nodeGenericParser.Invoke(this, args);
                        if (val.GetType() == typeNeeded)
                        {
                            pi.SetValue(complexobj, val);
                        }
                        else
                        {
                            throw new Exception($"Не подходящий тип:\n\tЛевый операнд: {pi.PropertyType}" +
                                $"\n\tПравый операнд: {val.GetType()}\nБудет присвоено значение по умолчанию.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                string errorLogPath = ConfigurationManager.AppSettings["ErrorLogPath"];
                if (File.Exists(errorLogPath))
                {
                    File.AppendAllText(errorLogPath, "\n" + ex.Message + "\n" + ex.StackTrace);
                }
            }
        }
        private bool Validate()
        {
            try
            {
                XmlSchemaSet xsdSchema = new XmlSchemaSet();

                // Добавление схемы в коллекцию
                string targetNamespace = ConfigurationManager.AppSettings["targetNS"];
                string schemaURI = ConfigurationManager.AppSettings["XSDPath"];
                if (targetNamespace == null || schemaURI == null)
                {
                    throw new Exception("Схема валидации не найдена, либо сам файл XML.\nВыбран поставщик конфигурации по умолчанию (App.config)");
                }
                xsdSchema.Add(targetNamespace, schemaURI);

                var settings = new XmlReaderSettings
                {
                    ValidationType = ValidationType.Schema,
                    Schemas = xsdSchema
                };
                settings.ValidationEventHandler += ValidationCallBack;

                string pathToXml = ConfigurationManager.AppSettings["PathToXml"];
                XmlReader reader = XmlReader.Create(pathToXml, settings);

                while (reader.Read()) ;
                Valid = true;
            }
            catch (Exception ex)
            {
                string validationLogPath = ConfigurationManager.AppSettings["ValidationLogPath"];
                if (File.Exists(validationLogPath))
                {
                    File.AppendAllText(validationLogPath, "\n" + ex.Message + "\n" + ex.StackTrace);
                }
            }
            return Valid;
        }
        private void ValidationCallBack(object sender, ValidationEventArgs e)
        {
            if (e.Severity == XmlSeverityType.Error)
            {
                Valid = false;
                string validationLogPath = ConfigurationManager.AppSettings["ValidationLogPath"];
                if (File.Exists(validationLogPath))
                {
                    File.AppendAllText(validationLogPath, "\n" + e.Message);
                }
            }
        }
    }
}
