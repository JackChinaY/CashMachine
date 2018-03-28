using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace CheckProject.entity
{
    public class ExportAttibute
    {
        public DataTable dataTable { get; set; }
        public string fileName { get; set; }
        public int offset { get; set; }
    }
}
