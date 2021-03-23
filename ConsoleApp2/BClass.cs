using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp2
{
    public class BClass
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public List<BClass> BClasses { get; set; }
    }
}
