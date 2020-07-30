using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nulah.VSIX.TaskTool.StandardLib.Models
{
    public class ReflectedValueInfo
    {
        public string Name { get; set; }
        public object Value { get; set; }
        public Type ValueType { get; set; }
        public bool IsNullableType { get; set; }
        public bool IsNull { get; set; }
        public bool IsPrivate { get; set; }
    }
}
