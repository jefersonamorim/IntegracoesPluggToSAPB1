using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntegracoesPluggto.Entity
{
    public class OrderFiltered
    {
        public int total { get; set; }
        public int showing { get; set; }
        public int limit { get; set; }
        public Result[] result { get; set; }
    }

    public class Result
    {
        public Order order { get; set; }
    }

}
