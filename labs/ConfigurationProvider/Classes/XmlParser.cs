using System;
using System.Xml;
using System.Xml.Schema;
using System.Reflection;
using System.Configuration;
using System.IO;

using LoggerToTXT;
namespace Config_Provider
{
    class XmlParser : IConfigurationProvider
    {
        private bool Valid { get; set; } = false;
        private bool ValidationNeeded { get; } = false;
        readonly string pathToXml;
        readonly string targetNameSpace;
        readonly string pathToXSD;
        LoggerToTxt logger = new LoggerToTxt();

        public XmlParser(string pathxml, string pathxsd = null, string targetNS = "urn:conf-schema")
        {
            try
            {
                ValidationNeeded = Convert.ToBoolean(ConfigurationManager.AppSettings["ConfigValidate"]);
            }
            catch (Exception)
            {
                string validationLogPath = ConfigurationManager.AppSettings["ConfigValidationLogPath"];
                logger.LogToAsync(validationLogPath, "\nНе ясно, требуется ли валидация. По умолчанию валидация не требуется, " +
                            "возможен неполный парсинг (некоторые поля будут заданы по умолчанию).\n");
                
            }
            pathToXml = pathxml;
            targetNameSpace = targetNS;
            pathToXSD = pathxsd;
            if (pathToXml != null && File.Exists(Path.GetFileNameWithoutExtension(pathToXml) + ".xsd"))
                pathToXSD = Path.GetFileNameWithoutExtension(pathToXml) + ".xsd";
        }
        public T Parse<T>() where T : new()
        {
            Type typeNeeded = typeof(T);

            if (ValidationNeeded && !Validate())
            {
                string validationLogPath = ConfigurationManager.AppSettings["ConfigValidationLogPath"]; 
                logger.LogToAsync(validationLogPath, "\nОшибка валидации XML.\nВозвращён экземпляр объекта класса " +
                            $"{typeNeeded.FullName} по умолчанию.\n");
                return new T();
            }

            if (!ValidationNeeded)
            {

                string validationLogPath = ConfigurationManager.AppSettings["ConfigValidationLogPath"];
                logger.LogToAsync(validationLogPath, "Валидация XML не требуется.\nВозможен " +
                            "неполный парсинг (некоторые поля будут заданы по умолчанию).\n");
            }

            ParsableAttribute cattr = typeNeeded.GetCustomAttribute(typeof(ParsableAttribute)) as ParsableAttribute;
            XmlDocument xDoc = new XmlDocument();
            xDoc.Load(pathToXml);
            // получим корневой элемент
            XmlElement xRoot = xDoc.DocumentElement;
            if (cattr != null)
            {
                try
                {
                    return GetObjectFromRoot<T>(xRoot, cattr.Alias, typeNeeded);
                }
                catch (Exception ex)
                {

                    string errorLogPath = ConfigurationManager.AppSettings["ConfigErrorLogPath"];
                    logger.LogToAsync(errorLogPath, "\n" + ex.Message + "\n" + ex.StackTrace);

                    return new T();
                }
            }
            else
            {
                try
                {
                    return GetObjectFromRoot<T>(xRoot, typeNeeded.Name, typeNeeded);
                }
                catch (Exception ex)
                {

                    string errorLogPath = ConfigurationManager.AppSettings["ConfigErrorLogPath"];
                    logger.LogToAsync(errorLogPath, "\n" + ex.Message + "\n" + ex.StackTrace);

                    return new T();
                }
            }
        }
        private T GetObjectFromRoot<T>(XmlElement xRoot, string name, Type typeNeeded)
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
            // вызов парсера узла
            MethodInfo nodeGenericParser = typeof(XmlParser)
                .GetMethod("ParseNode", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance)
                .MakeGenericMethod(typeNeeded);
            object[] args = { xnode };
            T obj = (T)nodeGenericParser.Invoke(this, args); // call T ParseNode<T>(selected node)
            return obj;
        }
        private T ParseNode<T>(XmlNode nodeSelected) where T : new()
        {
            Type t = typeof(T);
            object complexobj;
            try
            {
                complexobj = Activator.CreateInstance<T>(); // из текущей сборки
            }
            catch (Exception ex)
            {

                string errorLogPath = ConfigurationManager.AppSettings["ConfigErrorLogPath"];
                logger.LogToAsync(errorLogPath, "\n" + ex.Message + "\n" + ex.StackTrace);

                return new T();
            }

            if (nodeSelected.Attributes.Count > 0) // получаем атрибуты тега
            {
                foreach (XmlAttribute xattr in nodeSelected.Attributes)
                {
                    if (xattr != null)
                    {
                        SetPrimitiveXml<XmlAttribute>(complexobj, xattr);
                    }
                }
            }
            // обходим все дочерние узлы
            if (nodeSelected.HasChildNodes)
            {
                foreach (XmlNode xxnode in nodeSelected.ChildNodes)
                {
                    if (xxnode != null)
                    {   // просто текст посреди тега
                        if (xxnode.ChildNodes.Count == 1 && xxnode.FirstChild.NodeType == XmlNodeType.Text && xxnode.Attributes.Count == 0)
                        {
                            SetPrimitiveXml<XmlNode>(complexobj, xxnode);
                        }
                        else
                        {
                            // another complex property INSIDE a complex object
                            SetEmbeddedObject(complexobj, xxnode);
                        }
                    }
                }
            }
            return (T)complexobj; // вернуть объект после парсинга
        }
        private void SetPrimitiveXml<T>(object complexobj, T partOfTag) // T может быть XmlAttribute и XmlNode
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
                        ParsableAttribute cattr = pi.GetCustomAttribute(typeof(ParsableAttribute)) as ParsableAttribute;
                        if (cattr != null && cattr.Alias == xattr.Name)
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
                        ParsableAttribute cattr = pi.GetCustomAttribute(typeof(ParsableAttribute)) as ParsableAttribute;
                        if ((cattr != null && cattr.Alias == xnode.Name) || pi.Name == xnode.Name)
                        {
                            object val = Convert.ChangeType(xnode.InnerText.Trim(), pi.PropertyType);
                            pi.SetValue(complexobj, val);
                        }
                    }
                }
            }
            catch (Exception ex)
            {

                string errorLogPath = ConfigurationManager.AppSettings["ConfigErrorLogPath"];
                logger.LogToAsync(errorLogPath, "\n" + ex.Message + "\n" + ex.StackTrace);
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
                    ParsableAttribute cattr = pi.GetCustomAttribute(typeof(ParsableAttribute)) as ParsableAttribute;
                    if ((cattr != null && cattr.Alias == childnode.Name) || pi.Name == childnode.Name)
                    {
                        // извлекаем нужный нам тип
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
                            throw new Exception($"Неподходящий тип:\n\tЛевый операнд: {pi.PropertyType}" +
                                $"\n\tПравый операнд: {val.GetType()}\nБудет присвоено значение по умолчанию.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {

                string errorLogPath = ConfigurationManager.AppSettings["ConfigErrorLogPath"];
                logger.LogToAsync(errorLogPath, "\n" + ex.Message + "\n" + ex.StackTrace);
            }
        }
        private bool Validate()
        {
            // Create the XmlSchemaSet class.
            try
            {
                XmlSchemaSet xsdSchema = new XmlSchemaSet();
                // Add the schema to the collection.
                string schemaURI = pathToXSD;
                if (targetNameSpace == null || schemaURI == null)
                {
                    throw new Exception("Не найдена схема валидации либо сам файл XML.\nВыбран поставщик конфигурации по" +
                        " умолчанию (App.config)");
                }
                xsdSchema.Add(targetNameSpace, schemaURI);

                // Set the validation settings.
                var settings = new XmlReaderSettings();
                settings.ValidationType = ValidationType.Schema;
                settings.Schemas = xsdSchema;
                settings.ValidationEventHandler += ValidationCallBack;

                // Create the XmlReader object.
                XmlReader reader = XmlReader.Create(pathToXml, settings);

                // Parse the file.
                while (reader.Read()) ;
                Valid = true;
            }
            catch (Exception ex)
            {

                string validationLogPath = ConfigurationManager.AppSettings["ConfigValidationLogPath"];
                logger.LogToAsync(validationLogPath, "\n" + ex.Message + "\n" + ex.StackTrace);

            }
            return Valid;
        }
        private void ValidationCallBack(object sender, ValidationEventArgs ex)
        {
            if (ex.Severity == XmlSeverityType.Error)
            {
                Valid = false;
                string validationLogPath = ConfigurationManager.AppSettings["ConfigValidationLogPath"];
                logger.LogToAsync(validationLogPath, "\n" + ex.Message);
            }
        }
    }
}
