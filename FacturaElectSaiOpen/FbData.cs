using System;
using System.Configuration;
using FirebirdSql.Data.FirebirdClient;

namespace FacturaElectSaiOpen
{
    class FbData
    {
        public FbConnection db = new FbConnection();

        private bool conectar()
        {
            //var appsetting = System.Configuration.ConfigurationManager.AppSettings;

            try
            {
                var cnstring = ConfigurationManager.AppSettings.Get("connectionstring");

                db.ConnectionString = cnstring;
                
                db.Open();

                return true;
            }
            catch {
                return false;
            }

        }

        public FbDataAdapter DataReader(string sql)
        {
            conectar();

            //var transaction = db.BeginTransaction();

            //db.BeginTransaction();

            var da = new FbDataAdapter(sql, db);

            return da;
        }

        public FbDataReader ExecuteReader(string sql)
        {
            conectar();

            using (var transaction = db.BeginTransaction())
            {
                using (var command = new FbCommand(sql, db, transaction))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        //while (reader.Read())
                        //{
                        //    var values = new object[reader.FieldCount];
                        //    reader.GetValues(values);
                        //    Console.WriteLine(string.Join("|", values));
                        //}

                        return reader;
                    }
                }
            }
            
        }


       

        public int ExecuteNonQuery(string sql)
        {
            conectar();

            using (var transaction = db.BeginTransaction())
            {
                using (var command = new FbCommand(sql, db, transaction))
                {
                    var records = command.ExecuteNonQuery();

                    transaction.Commit();

                    return records;
                }
            }

        }

    }




}
