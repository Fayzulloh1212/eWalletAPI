using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebAPI.Models
{
    public class Wallet
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public string CustomerName { get; set; }
        public double Balance { get; set; }
        public int OperCount { get; set; }
        public bool IsIdent { get; set; } = true;
    }
}
