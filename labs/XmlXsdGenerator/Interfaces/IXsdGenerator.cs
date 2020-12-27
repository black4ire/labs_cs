using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XmlXsdGenerator
{
    public interface IXsdGenerator
    {
        string GenerateXsd(DataSet ds);
        Task<string> GenerateXsdAsync(DataSet ds);
    }
}
