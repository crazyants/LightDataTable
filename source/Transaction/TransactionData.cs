using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Reflection;
using Generic.LightDataTable.Interface;
using Generic.LightDataTable.InterFace;

namespace Generic.LightDataTable.Transaction
{
    public class TransactionData : ICustomRepository
    {
        protected readonly string SqlConnectionStringString;
        protected SqlConnection SqlConnection { get; private set; }
        internal SqlTransaction Trans { get; private set; }
        private static bool _assLoaded;

        private static void LoadPropertyChangedAss()
        {
            if (_assLoaded)
                return;
            const string assemblyName = "ProcessedByFody";
            foreach (var a in Assembly.GetEntryAssembly().DefinedTypes)
            {
                if (a.Name.Contains(assemblyName))
                {

                    _assLoaded = true;
                    return;
                }
            }
            throw new Exception("PropertyChanged.dll could not be found please install PropertyChanged.Fody. FodyWeavers.XML should look like <?xml version=\"1.0\" encoding=\"utf - 8\" ?>" +
                Environment.NewLine + "<Weavers>" +
                Environment.NewLine + "<PropertyChanged />" +
                Environment.NewLine + "</Weavers> ");
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="appSettingsOrSqlConnectionString">
        /// AppSettingsName that containe the connectionstringName,
        /// OR ConnectionStringName,
        /// OR Full ConnectionString
        /// Default is Dbconnection
        /// </param>
        public TransactionData(string appSettingsOrSqlConnectionString = "Dbconnection")
        {
            LoadPropertyChangedAss();
            if (string.IsNullOrEmpty(appSettingsOrSqlConnectionString))
                if (string.IsNullOrEmpty(SqlConnectionStringString))
                    throw new Exception("appSettingsOrSqlConnectionString cant be empty");

            if (string.IsNullOrEmpty(appSettingsOrSqlConnectionString)) return;

            if (appSettingsOrSqlConnectionString.Contains(";"))
                SqlConnectionStringString = appSettingsOrSqlConnectionString; // its full connectionString

            // set connectionString by appsettings
            if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings[appSettingsOrSqlConnectionString]))
            {
                SqlConnectionStringString = ConfigurationManager
                    .ConnectionStrings[ConfigurationManager.AppSettings[appSettingsOrSqlConnectionString]]
                    .ConnectionString;
            }
            else
            {
                SqlConnectionStringString = ConfigurationManager.ConnectionStrings[appSettingsOrSqlConnectionString].ConnectionString;
            }
        }

        private void ValidateConnection()
        {
            if (SqlConnection == null)
                SqlConnection = new SqlConnection(SqlConnectionStringString);
            if (SqlConnection.State == ConnectionState.Broken || SqlConnection.State == ConnectionState.Closed)
                SqlConnection.Open();
        }

        public SqlTransaction CreateTransaction()
        {
            ValidateConnection();
            if (Trans?.Connection == null)
            {
                Trans = SqlConnection.BeginTransaction();
            }
            return Trans;
        }


        public IDataReader ExecuteReader(SqlCommand cmd)
        {
            ValidateConnection();
            return cmd.ExecuteReader();
        }

        public object ExecuteScalar(SqlCommand cmd)
        {
            ValidateConnection();
            return cmd.ExecuteScalar();
        }

        public int ExecuteNonQuery(SqlCommand cmd)
        {
            ValidateConnection();
            return cmd.ExecuteNonQuery();
        }

        public void Rollback()
        {
            Trans?.Rollback();
            Dispose();

        }

        public void Commit()
        {
            Trans?.Commit();
            Dispose();
        }

        /// <summary>
        /// return a list of LightDataTable e.g. DataSet
        /// </summary>
        /// <param name="cmd">sqlCommand that are create from GetSqlCommand</param>
        /// <param name="primaryKeyId"> Table primaryKeyId, so LightDataTable.FindByPrimaryKey could be used </param>
        /// <returns></returns>
        protected List<ILightDataTable> GetLightDataTableList(SqlCommand cmd, string primaryKeyId = null)
        {
            var returnList = new List<ILightDataTable>();
            var reader = ExecuteReader(cmd);
            returnList.Add(new LightDataTable().ReadData(reader, primaryKeyId));

            while (reader.NextResult())
                returnList.Add(new LightDataTable().ReadData(reader, primaryKeyId));
            reader.Close();
            return returnList;
        }

        /// <summary>
        /// return LightDataTable e.g. DataTable
        /// </summary>
        /// <param name="cmd">sqlCommand that are create from GetSqlCommand</param>
        /// <param name="primaryKey">Table primaryKeyId, so LightDataTable.FindByPrimaryKey could be used </param>
        /// <returns></returns>
        public ILightDataTable GetLightDataTable(SqlCommand cmd, string primaryKey = null)
        {
            ValidateConnection();
            var reader = cmd.ExecuteReader();
            return new LightDataTable().ReadData(reader, primaryKey);
        }

        /// <summary>
        /// SqlDbType by system.Type
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public SqlDbType GetSqlType(Type type)
        {
            if (type == typeof(string))
                return SqlDbType.NVarChar;

            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
                type = Nullable.GetUnderlyingType(type);

            var param = new SqlParameter("", Activator.CreateInstance(type));
            return param.SqlDbType;
        }


        /// <summary>
        /// Add parameters to SqlCommand
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="attrName"></param>
        /// <param name="value"></param>
        /// <param name="dbType"></param>
        public void AddInnerParameter(SqlCommand cmd, string attrName, object value, SqlDbType dbType = SqlDbType.NVarChar)
        {
            if (attrName != null && attrName[0] != '@')
                attrName = "@" + attrName;

            var sqlDbTypeValue = value ?? DBNull.Value;
            var param = cmd.CreateParameter();
            param.SqlDbType = dbType;
            param.Value = sqlDbTypeValue;
            param.ParameterName = attrName;
            cmd.Parameters.Add(param);
        }

        /// <summary>
        /// Return SqlCommand that already containe SQLConnection
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        public SqlCommand GetSqlCommand(string sql)
        {
            ValidateConnection();
            return Trans == null ? new SqlCommand(sql, SqlConnection) : new SqlCommand(sql, SqlConnection, Trans);
        }

        public virtual void Dispose()
        {
            Trans?.Dispose();
            SqlConnection?.Dispose();
            Trans = null;
            SqlConnection = null;
        }
    }
}
