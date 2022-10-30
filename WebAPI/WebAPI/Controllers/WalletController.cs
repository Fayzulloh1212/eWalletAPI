using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebAPI.Models;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WalletController : Controller
    {
        private ISession Session;

        public WalletController(IHttpContextAccessor httpContextAccessor)
        {
            Session = httpContextAccessor.HttpContext.Session;
        }
          
        [HttpPost("/wallet")]
        public IEnumerable<Wallet> Get()
        {
            List<Wallet> wallets = new List<Wallet>();

            try
            {
                wallets = DbContext.GetWallets();
            }
            catch { }
             
            return wallets; 
        }

        //1. Проверить существует ли аккаунт электронного кошелька.
        [HttpPost("/wallet/IsExists/{Id}")]
        public bool IsExists(int Id)
        {
            bool isExists = false;

            try
            {
                //isExists = DbContext.IsExistsWallet(Id);
                isExists = DbContext.GetWallet(Id) != null;
            }
            catch { }

            return isExists;
        }

        //2. Пополнение электронного кошелька.
        [HttpPost("/wallet/AddSum")]
        public Wallet AddSum(WalletHistory walletHistory)
        {
            Wallet wallet = null;

            if (walletHistory.Sum > 0)
            {
                try
                {
                    wallet = DbContext.SetWalletHistory(walletHistory);
                }
                catch { }
            }

            return wallet;
        }
        
        [HttpPost("/wallet/SubtSum")]
        public Wallet SubtractSum(WalletHistory walletHistory)
        {
            Wallet wallet = null;

            if (walletHistory.Sum >= 0)
            {
                try
                {
                    walletHistory.Sum = (-1) * walletHistory.Sum;
                    wallet = DbContext.SetWalletHistory(walletHistory);
                }
                catch { }
            }

            return wallet;
        }


        //3. Получить общее количество и суммы операций пополнения за текущий месяц.
        [HttpPost("/wallet/GetRechargesCurrMonth/{walletId}")]
        public Wallet GetRechargesCurrMonth(int walletId)
        {
            Wallet wallet = null;
            
            try
            {
                DateTime datefrom = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                DateTime dateTo = datefrom.AddMonths(1).AddDays(-1);

                wallet = DbContext.GetWalletHistory(walletId, datefrom, dateTo, isRecharges: 1);
            }
            catch { } 

            return wallet;
        }

        //4. Получить баланс электронного кошелька.
        [HttpPost("/wallet/{Id}")]
        public Wallet GetBalance(int Id)
        {
            Wallet wallet = null;

            try
            {
                wallet = DbContext.GetWallet(Id);
            }
            catch { }

            return wallet;
        }
    }
}
