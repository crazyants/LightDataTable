using Generic.LightDataTable.Transaction;

namespace Generic.LightDataTable.Abstract
{
    public class Repository : TransactionData
    {
        public Repository(string appSettingsOrSqlConnectionString= "Dbconnection") : base(appSettingsOrSqlConnectionString)
        {
        }

    }
}
