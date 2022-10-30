using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebAPI.Models;
using Npgsql;
using System.Data;

namespace WebAPI
{
    public static class DbContext
    {
        public static List<Wallet> GetWallets()
        {
            List<Wallet> wallets = new List<Wallet>();

            NpgsqlConnection connection = null;
            NpgsqlCommand command = null;

            try
            {
                connection = new NpgsqlConnection(Program.ConnectionString);
                connection.Open();

                string q = @"SELECT w.id, w.customer_id, 
(SELECT c.name FROM main.customer c where c.id = w.customer_id) As customer_name, 
(SELECT coalesce(sum(wh.sum), 0) FROM main.wallet_history wh where wh.wallet_id = w.id) As balance, 
w.is_ident FROM main.wallet w";
                command = new NpgsqlCommand(q, connection);

                DataTable dt = new DataTable();

                NpgsqlDataAdapter da = new NpgsqlDataAdapter(command);
                da.Fill(dt);

                foreach (DataRow row in dt.Rows)
                {
                    Wallet wallet = new Wallet()
                    {
                        Id = int.Parse("" + row["id"]),
                        CustomerId = int.Parse("" + row["customer_id"]),
                        CustomerName = "" + row["customer_name"],
                        Balance = double.Parse("" + row["balance"]),
                        IsIdent = bool.Parse("" + row["is_ident"])
                    };
                    wallets.Add(wallet);
                }
            }
            catch(Exception ex)
            {

            }
            finally
            {
                if (connection.State == ConnectionState.Open)
                    connection.Close();
                if (command != null)
                    command.Dispose();
            }

            return wallets;
        }

        public static Wallet GetWallet(int Id)
        {
            Wallet wallet = null;

            NpgsqlConnection connection = null;
            NpgsqlCommand command = null;

            try
            {
                connection = new NpgsqlConnection(Program.ConnectionString);
                connection.Open();

                string q = $@"SELECT w.id, w.customer_id, 
(SELECT c.name FROM main.customer c where c.id = w.customer_id) As customer_name, 
(SELECT coalesce(sum(wh.sum), 0) FROM main.wallet_history wh where wh.wallet_id = w.id) As balance, 
w.is_ident FROM main.wallet w 
WHERE w.id = {Id}";
                command = new NpgsqlCommand(q, connection);

                DataTable dt = new DataTable();

                NpgsqlDataAdapter da = new NpgsqlDataAdapter(command);
                da.Fill(dt);

                wallet = new Wallet()
                {
                    Id = int.Parse("" + dt.Rows[0]["id"]),
                    CustomerId = int.Parse("" + dt.Rows[0]["customer_id"]),
                    CustomerName = "" + dt.Rows[0]["customer_name"],
                    Balance = double.Parse("" + dt.Rows[0]["balance"]),
                    IsIdent = bool.Parse("" + dt.Rows[0]["is_ident"])
                };
            }
            catch (Exception ex)
            {

            }
            finally
            {
                if (connection.State == ConnectionState.Open)
                    connection.Close();
                if (command != null)
                    command.Dispose();
            }

            return wallet;
        }

        public static bool IsExistsWallet(int Id)
        {
            bool result = false;
            NpgsqlConnection connection = null;
            NpgsqlCommand command = null;

            try
            {
                connection = new NpgsqlConnection(Program.ConnectionString);
                connection.Open();

                string q = $@"SELECT EXISTS(SELECT 1 FROM main.wallet w WHERE w.id = {Id});";
                command = new NpgsqlCommand(q, connection);

                DataTable dt = new DataTable();

                NpgsqlDataAdapter da = new NpgsqlDataAdapter(command);
                da.Fill(dt);

                result = bool.Parse("" + dt.Rows[0][0]);
            }
            catch (Exception ex)
            {

            }
            finally
            {
                if (connection.State == ConnectionState.Open)
                    connection.Close();
                if (command != null)
                    command.Dispose();
            }

            return result;
        }

        public static Wallet SetWalletHistory(WalletHistory walletHistory)
        {
            Wallet wallet = null;

            NpgsqlConnection connection = null;
            NpgsqlCommand command = null;

            try
            {
                connection = new NpgsqlConnection(Program.ConnectionString);
                connection.Open();

                string q = $@"INSERT INTO main.wallet_history(wallet_id, sum, oper_user_id, oper_date)
VALUES(@wallet_id, @sum, @oper_user_id, @oper_date);";
                command = new NpgsqlCommand(q, connection);

                command.Parameters.Add("@wallet_id", NpgsqlTypes.NpgsqlDbType.Integer).Value = walletHistory.WalletId;
                command.Parameters.Add("@sum", NpgsqlTypes.NpgsqlDbType.Numeric).Value = walletHistory.Sum;
                command.Parameters.Add("@oper_user_id", NpgsqlTypes.NpgsqlDbType.Integer).Value = walletHistory.UserId;
                command.Parameters.Add("@oper_date", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTime.Now;

                int res = int.TryParse("" + command.ExecuteNonQuery(), out int i) ? i : 0;
                
                if (res > 0)
                    wallet = GetWallet(walletHistory.WalletId);

            }
            catch (Exception ex)
            {

            }
            finally
            {
                if (connection.State == ConnectionState.Open)
                    connection.Close();
                if (command != null)
                    command.Dispose();
            }

            return wallet;
        }
    
        public static Wallet GetWalletHistory(int walletId, DateTime dateFrom, DateTime dateTo, int isRecharges = 2)
        {
            //isRecharges:
            // 0 -> not recharges
            // 1 -> only recharges
            // 2 -> all
           
            Wallet wallet = null;

            NpgsqlConnection connection = null;
            NpgsqlCommand command = null;

            try
            {
                connection = new NpgsqlConnection(Program.ConnectionString);
                connection.Open();

                string q = $@"SELECT coalesce(count(wh.id), 0) AS oper_count, coalesce(sum(wh.sum), 0) AS balance, w.is_ident, w.customer_id, 
(SELECT c.name FROM main.customer c where c.id = w.customer_id) As customer_name
FROM main.wallet_history wh INNER JOIN main.wallet w ON (w.id = wh.wallet_id)
WHERE wh.wallet_id = @wallet_id
    AND wh.oper_date::date >= @oper_date_from::date
    AND wh.oper_date::date <= @oper_date_to::date";

                q += isRecharges switch
                { 
                    0 => " AND wh.sum <= 0",
                    1 => " AND wh.sum > 0",
                    _ => ""
                };
                q += " GROUP BY w.is_ident, w.customer_id;";

                command = new NpgsqlCommand(q, connection);

                command.Parameters.Add("@wallet_id", NpgsqlTypes.NpgsqlDbType.Integer).Value = walletId;
                command.Parameters.Add("@oper_date_from", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = dateFrom;
                command.Parameters.Add("@oper_date_to", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = dateTo;

                DataTable dt = new DataTable();

                NpgsqlDataAdapter da = new NpgsqlDataAdapter(command);
                da.Fill(dt);

                wallet = new Wallet()
                {
                    Id = walletId,
                    CustomerId = int.Parse("" + dt.Rows[0]["customer_id"]),
                    CustomerName = "" + dt.Rows[0]["customer_name"],
                    Balance = double.Parse("" + dt.Rows[0]["balance"]),
                    OperCount = int.Parse("" + dt.Rows[0]["oper_count"]),
                    IsIdent = bool.Parse("" + dt.Rows[0]["is_ident"])
                };

            }
            catch (Exception ex)
            {

            }
            finally
            {
                if (connection.State == ConnectionState.Open)
                    connection.Close();
                if (command != null)
                    command.Dispose();
            }

            return wallet;
        }

    }
}
