using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebAPI.Models
{
    public class WalletHistory
    {
        public int Id { get; set; }
        public int WalletId { get; set; }
        public double Sum { get; set; }
        public DateTime OperDate { get; set; }
        public int UserId { get; set; }
    }
}
