using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XmlXsdGenerator
{
    public interface IXmlGenerator
    {
        string GenerateXml(DataSet ds);
        Task<string> GenerateXmlAsync(DataSet ds);
    }
}
