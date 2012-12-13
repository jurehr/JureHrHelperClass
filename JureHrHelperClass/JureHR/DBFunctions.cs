using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Data.SqlClient;
using System.Xml;
using System.Windows.Forms;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Web;
using System.Configuration;
using System.Data.OleDb;

namespace JureHR
{
    /// <summary>
    /// Database Functions
    /// </summary>
    public class DBFunctions
    {
        /// <summary>
        /// SQL Database Functions
        /// </summary>
        public class DBSql
        {

            #region Properties

            /// <summary>
            /// Connection Name Set in Web.Config configuration , connectionStrings
            /// </summary>
            public static string ConnName { get; set; }

            /// <summary>
            /// SqlTransaction
            /// </summary>
            protected SqlTransaction transaction;
            /// <summary>
            /// SqlConnection
            /// </summary>
            protected SqlConnection connection;

            #endregion

            #region Public Support Methods

            /// <summary>
            /// SQL Database Functions Constructor
            /// </summary>
            /// <param name="ConnectionName">Connection Name Set in Web.Config configuration , connectionStrings</param>
            public DBSql(string ConnectionName)
            {
                ConnName = ConnectionName;
            }

            /// <summary>
            /// SQL Database Functions Constructor with inhereted connection
            /// </summary>
            /// <param name="ConnectionName">Connection Name Set in Web.Config configuration , connectionStrings</param>
            /// <param name="connection">SqlConnection</param>
            public DBSql(string ConnectionName, SqlConnection connection)
            {
                this.connection = connection;
                if (this.connection == null)
                {
                    startConnection();
                }
            }

            /// <summary>
            /// SQL Database Functions Constructor with inhereted transaction
            /// </summary>
            /// <param name="ConnectionName">Connection Name Set in Web.Config configuration , connectionStrings</param>
            /// <param name="transaction">SqlTransaction</param>
            public DBSql(string ConnectionName, SqlTransaction transaction)
            {
                this.transaction = transaction;
                if (this.transaction == null)
                {
                    setTransaction(transaction);
                }
                if (this.connection == null)
                {
                    this.connection = this.transaction.Connection;
                }
            }

            /// <summary>
            /// Check Database Connection
            /// </summary>
            /// <param name="connectionString"></param>
            /// <returns></returns>
            public static bool CheckDatabaseConnection(string connectionString)
            {
                SqlConnectionStringBuilder bu = new SqlConnectionStringBuilder(connectionString);
                SqlConnection con = new SqlConnection(connectionString);
                try
                {
                    con.Open();
                    con.Close();
                    return true;
                }
                catch (SqlException se)
                {
                    Mailer.ErrNotify(se, "DBFunctions DBSql CheckDatabaseConnection-False");
                    return false;
                }
            }

            /// <summary>
            /// Retrieves the connection string from the calling application .config file.
            /// The connection string must be in an application setting called 
            /// "ConnectionString".
            /// </summary>
            /// <returns></returns>
            public static SqlConnection OpenConection(string connName)
            {
                string connectionString = ConfigurationManager.ConnectionStrings[connName].ConnectionString;
                try
                {
                    SqlConnection con = new SqlConnection(connectionString);
                    con.Open();
                    return con;
                }
                catch (Exception ex)
                {
                    Mailer.ErrNotify(ex, "DBFunctions OpenConection");
                    return null;
                }
            }

            /// <summary>
            /// Gets the current SQL Server transaction
            /// </summary>
            /// <returns></returns>
            public SqlTransaction getTransaction()
            {
                return transaction;
            }

            /// <summary>
            /// Gets the current SQL Server connection
            /// </summary>
            /// <returns></returns>
            public SqlConnection getConnection()
            {
                return connection;
            }

            /// <summary>
            /// Sets a SQL Server SqlTransaction to Trans and starts it (calls startTransaction)
            /// </summary>
            /// <param name="transaction"></param>
            protected void setTransaction(SqlTransaction transaction)
            {
                this.transaction = transaction;
                startTransaction();
            }

            /// <summary>
            /// Similar to startConnection(), but it nulls an existent connection in case of any 
            /// </summary>
            public void startBrandNewConnection()
            {
                // nulling Conn
                connection = null;
                try
                {
                    connection = new SqlConnection(GetConnectionString());
                    // open brand new one
                    connection.Open();
                }
                catch (Exception e)
                {
                    throw e;
                }
            }

            /// <summary>
            /// Starts a new SQL Server connection
            /// </summary>
            public void startConnection()
            {
                try
                {
                    if (connection == null)
                    {
                        // new connection
                        connection = new SqlConnection(GetConnectionString());
                        // open connection
                        connection.Open();
                    }
                }
                catch (Exception e)
                {
                    throw e;
                }
            }

            /// <summary>
            /// Opens a SQL Server connection when there's a outstanding connection
            /// </summary>
            public void startConnectionWithOpenOption()
            {
                try
                {
                    if (connection == null)
                    {
                        // new connection
                        connection = new SqlConnection(GetConnectionString());
                        // open connection
                        connection.Open();
                    }
                    if (connection.ConnectionString == "")
                    {
                        connection.ConnectionString = GetConnectionString();
                        connection.Open();
                    }
                    if (connection.State == ConnectionState.Closed)
                        connection.Open();
                }
                catch (Exception e)
                {
                    throw e;
                }
            }

            /// <summary>
            /// Closes a SQL Server connection
            /// </summary>
            public void closeConnection()
            {
                // just close it
                connection.Close();
                connection.Dispose();
            }

            /// <summary>
            /// Begins a SQL Server transaction
            /// </summary>
            /// <returns></returns>
            public SqlTransaction startTransaction()
            {
                if (connection == null)
                {
                    startConnection();
                }
                // start new Transaction
                transaction = connection.BeginTransaction();

                return transaction;
            }

            /// <summary>
            /// Commits a SQL Server transaction
            /// </summary>
            public void commitTransaction()
            {
                // commit
                transaction.Commit();
                // close connection and transaction
                connection.Close();
            }

            /// <summary>
            /// Commits a transaction and disposes the connection
            /// </summary>
            public void commitTransactionAndDisposeConnection()
            {
                // commit
                transaction.Commit();
                // close connection and transaction
                connection.Close();
                connection.Dispose();
            }

            /// <summary>
            /// Rolls back a SQL Server transaction 
            /// </summary>
            public void rollbackTransaction()
            {
                try
                {
                    // roll back
                    transaction.Rollback();
                    // close connection and transaction
                    connection.Close();
                    connection.Dispose();
                }
                catch (Exception e)
                {
                    e.Message.ToString();
                }
            }

            /// <summary>
            /// Retrieves the connection string from the calling application .config file.
            /// The connection string must be in an application setting called 
            /// "ConnectionString".
            /// </summary>
            /// <returns></returns>
            public static string GetConnectionString()
            {
                string connectionString = ConfigurationManager.ConnectionStrings[ConnName].ConnectionString;
                if (CheckDatabaseConnection(connectionString))
                {
                    return connectionString;
                }
                else
                {
                    Mailer.ErrNotify("GetConnectionString", "DBFunctions GetConnectionString");
                    return "No Connection String Found";
                }
            }

            #endregion

            #region Private Read Methods

            /// <summary>
            /// Returns the value of the given field after running the given SQL Select statement
            /// </summary>
            /// <param name="selectStatement">An SQL Select statement</param>
            /// <param name="field">The field to look for</param>
            /// <returns></returns>
            private object ReadValue(string selectStatement, string field)
            {
                object retVal = null;

                SqlCommand command;
                SqlDataReader reader;

                try
                {
                    SqlConnection.ClearAllPools();
                    command = new SqlCommand(selectStatement, connection, transaction);

                    reader = command.ExecuteReader();

                    if (reader.Read())
                        retVal = reader[field];

                    reader.Close();
                    reader.Dispose();
                }
                catch (SqlException se)
                {
                    Mailer.ErrNotify(se, "DBFunctions ReadValue");
                }
                catch (InvalidOperationException ioe)
                {
                    if (ioe.Message.StartsWith("Timeout")) //If timeout, try again
                        return ReadValue(selectStatement, field);
                    else
                        Mailer.ErrNotify(ioe, "DBFunctions DBSql ReadValue");
                }

                return retVal;
            }

            /// <summary>
            /// running the given SQL Select statement
            /// </summary>
            /// <param name="selectStatement">An SQL Select statement</param>
            /// <param name="field">The field to look for</param>
            /// <returns>Returns an "see cref="List string"/" of the values of the given field after</returns>
            private List<string> ReadValues(string selectStatement, string field)
            {
                List<string> results = new List<string>();

                SqlCommand command;
                SqlDataReader reader;

                try
                {
                    command = new SqlCommand(selectStatement, connection, transaction);
                    reader = command.ExecuteReader(CommandBehavior.CloseConnection);

                    while (reader.Read())
                        results.Add(reader[field].ToString());

                    reader.Close();
                }
                catch (SqlException se)
                {
                    Mailer.ErrNotify(se, "DBFunctions DBSql ReadValues");
                }

                return results;

            }

            /// <summary>
            /// This method opens (if necessary) and assigns a connection, transaction, command type and parameters 
            /// to the provided command
            /// </summary>
            /// <param name="command">The SqlCommand to be prepared</param>
            /// <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
            /// <param name="commandText">The stored procedure name or T-SQL command</param>
            /// <param name="commandParameters">An array of SqlParameters to be associated with the command or 'null' if no parameters are required</param>
            private void PrepareCommand(SqlCommand command, CommandType commandType, string commandText, SqlParameter[] commandParameters)
            {
                if (command == null) Mailer.ErrNotify("command", "DBFunctions DBSql PrepareCommand");
                if (commandText == null || commandText.Length == 0) Mailer.ErrNotify("commandText", "DBFunctions DBSql PrepareCommand");

                // If the provided connection is not open, we will open it
                if (connection.State != ConnectionState.Open)
                {
                    try
                    {
                        connection.Open();
                    }
                    catch (Exception ex)
                    {
                        string msg = "Database connection Error! Maybe your Database is not runing or database connection string is mistake?";
                        Mailer.ErrNotify(ex, "DBFunctions DBSql PrepareCommand --- " + msg);
                    }
                }

                // Associate the connection with the command
                command.Connection = connection;

                // Set the command text (stored procedure name or SQL statement)
                command.CommandText = commandText;

                // If we were provided a transaction, assign it
                if (transaction != null)
                {
                    if (transaction.Connection == null) throw new ArgumentException("The transaction was rollbacked or commited, please provide an open transaction.", "transaction");
                    command.Transaction = transaction;
                }

                // Set the command type
                command.CommandType = commandType;

                // Attach the command parameters if they are provided
                if (commandParameters != null)
                {
                    AttachParameters(command, commandParameters);
                }
                return;
            }

            /// <summary>
            /// This method opens (if necessary) and assigns a connection, transaction, command type and parameters 
            /// to the provided command
            /// </summary>
            /// <param name="command">The SqlCommand to be prepared</param>
            /// <param name="connection">A valid SqlConnection, on which to execute this command</param>
            /// <param name="transaction">A valid SqlTransaction, or 'null'</param>
            /// <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
            /// <param name="commandText">The stored procedure name or T-SQL command</param>
            /// <param name="commandParameters">An array of SqlParameters to be associated with the command or 'null' if no parameters are required</param>
            private static void PrepareCommand(SqlCommand command, SqlConnection connection, SqlTransaction transaction, CommandType commandType, string commandText, SqlParameter[] commandParameters)
            {
                if (command == null) Mailer.ErrNotify("command", "DBFunctions DBSql PrepareCommand");
                if (commandText == null || commandText.Length == 0) Mailer.ErrNotify("commandText", "DBFunctions DBSql PrepareCommand");

                // If the provided connection is not open, we will open it
                if (connection.State != ConnectionState.Open)
                {
                    try
                    {
                        connection.Open();
                    }
                    catch (Exception ex)
                    {
                        string msg = "Database connection Error! Maybe your Database is not runing or database connection string is mistake?";
                        Mailer.ErrNotify(ex, "DBFunctions DBSql PrepareCommand --- " + msg);
                    }
                }

                // Associate the connection with the command
                command.Connection = connection;

                // Set the command text (stored procedure name or SQL statement)
                command.CommandText = commandText;

                // If we were provided a transaction, assign it
                if (transaction != null)
                {
                    if (transaction.Connection == null) throw new ArgumentException("The transaction was rollbacked or commited, please provide an open transaction.", "transaction");
                    command.Transaction = transaction;
                }

                // Set the command type
                command.CommandType = commandType;

                // Attach the command parameters if they are provided
                if (commandParameters != null)
                {
                    AttachParameters(command, commandParameters);
                }
                return;
            }

            /// <summary>
            /// This method assigns an array of values to an array of SqlParameters
            /// </summary>
            /// <param name="commandParameters">Array of SqlParameters to be assigned values</param>
            /// <param name="parameterValues">Array of objects holding the values to be assigned</param>
            private static void AssignParameterValues(SqlParameter[] commandParameters, object[] parameterValues)
            {
                if ((commandParameters == null) || (parameterValues == null))
                {
                    // Do nothing if we get no data
                    return;
                }

                // We must have the same number of values as we pave parameters to put them in
                if (commandParameters.Length != parameterValues.Length)
                {
                    Mailer.ErrNotify("Parameter count does not match Parameter Value count.", "DBFunctions DBSql AssignParameterValues");
                }

                // Iterate through the SqlParameters, assigning the values from the corresponding position in the 
                // value array
                for (int i = 0, j = commandParameters.Length; i < j; i++)
                {
                    // If the current array value derives from IDbDataParameter, then assign its Value property
                    if (parameterValues[i] is IDbDataParameter)
                    {
                        IDbDataParameter paramInstance = (IDbDataParameter)parameterValues[i];
                        if (paramInstance.Value == null)
                        {
                            commandParameters[i].Value = DBNull.Value;
                        }
                        else
                        {
                            commandParameters[i].Value = paramInstance.Value;
                        }
                    }
                    else if (parameterValues[i] == null)
                    {
                        commandParameters[i].Value = DBNull.Value;
                    }
                    else
                    {
                        commandParameters[i].Value = parameterValues[i];
                    }
                }
            }

            /// <summary>
            /// This method is used to attach array of SqlParameters to a SqlCommand.
            /// 
            /// This method will assign a value of DbNull to any parameter with a direction of
            /// InputOutput and a value of null.  
            /// 
            /// This behavior will prevent default values from being used, but
            /// this will be the less common case than an intended pure output parameter (derived as InputOutput)
            /// where the user provided no input value.
            /// </summary>
            /// <param name="command">The command to which the parameters will be added</param>
            /// <param name="commandParameters">An array of SqlParameters to be added to command</param>
            private static void AttachParameters(SqlCommand command, SqlParameter[] commandParameters)
            {
                if (command == null) throw new ArgumentNullException("command");
                if (commandParameters != null)
                {
                    foreach (SqlParameter p in commandParameters)
                    {
                        if (p != null)
                        {
                            // Check for derived output value with no value assigned
                            if ((p.Direction == ParameterDirection.InputOutput ||
                                p.Direction == ParameterDirection.Input) &&
                                (p.Value == null))
                            {
                                p.Value = DBNull.Value;
                            }
                            command.Parameters.Add(p);
                        }
                    }
                }
            }

            /// <summary>
            /// This method assigns dataRow column values to an array of SqlParameters
            /// </summary>
            /// <param name="commandParameters">Array of SqlParameters to be assigned values</param>
            /// <param name="dataRow">The dataRow used to hold the stored procedure's parameter values</param>
            private static void AssignParameterValues(SqlParameter[] commandParameters, DataRow dataRow)
            {
                if ((commandParameters == null) || (dataRow == null))
                {
                    // Do nothing if we get no data
                    return;
                }

                int i = 0;
                // Set the parameters values
                foreach (SqlParameter commandParameter in commandParameters)
                {
                    // Check the parameter name
                    if (commandParameter.ParameterName == null ||
                        commandParameter.ParameterName.Length <= 1)
                        throw new Exception(
                            string.Format(
                                "Please provide a valid parameter name on the parameter #{0}, the ParameterName property has the following value: '{1}'.",
                                i, commandParameter.ParameterName));
                    if (dataRow.Table.Columns.IndexOf(commandParameter.ParameterName.Substring(1)) != -1)
                        commandParameter.Value = dataRow[commandParameter.ParameterName.Substring(1)];
                    i++;
                }
            }

            #endregion

            #region Public Methods

            #region Lookup Functions

            #region DLookup

            /// <summary>
            /// Looks up the value of a field into a given table according to the
            /// given criteria
            /// </summary>
            /// <param name="field">The name of the field to lookup</param>
            /// <param name="table">The name of the table</param>
            /// <param name="criteria">Criteria expression</param>
            /// <returns>The value of the field</returns>
            public object DLookup(string field, string table, string criteria)
            {
                if (criteria == null)
                    return null;

                string selectCommand = "SELECT [" + field + "] FROM [" + table
                    + "] WHERE (" + criteria + ")";

                return ReadValue(selectCommand, field);
            }

            /// <summary>
            /// Returns all values of a field from the given table
            /// </summary>
            /// <param name="field">The name of the field to lookup</param>
            /// <param name="table">The name of the table</param>
            /// <returns>The values of the field</returns>
            public List<string> DLookupMult(string field, string table)
            {
                string selectCommand = "SELECT [" + field + "] FROM [" + table
                    + "]";

                return ReadValues(selectCommand, field);
            }

            /// <summary>
            /// Returns all values of a field from the given table according to the
            /// given criteria
            /// </summary>
            /// <param name="field">The name of the field to lookup</param>
            /// <param name="table">The name of the table</param>
            /// <param name="criteria">Criteria expression</param>
            /// <param name="sortField">Sort Field</param>
            /// <returns>The values of the field matching the criteria</returns>
            public List<string> DLookupMult(string field, string table, string criteria, string sortField)
            {
                if (criteria == null)
                    return null;

                string selectCommand = "SELECT [" + field + "] FROM [" + table
                    + "] WHERE (" + criteria + ") ORDER BY " + sortField;

                return ReadValues(selectCommand, field);
            }

            /// <summary>
            /// Looks up the value of a field into a given table at the
            /// specified row
            /// </summary>
            /// <param name="field">The name of the field to lookup</param>
            /// <param name="table">The name of the table</param>
            /// <param name="primaryKey">The primary key of the row</param>
            /// <returns>The value of the field</returns>
            public object DLookup(string field, string table, Guid primaryKey)
            {
                if (primaryKey.Equals(null))
                    return null;

                string selectCommand = "SELECT [" + field + "] FROM [" + table
                    + "] WHERE ([" + GetKeyField(table) + "] = '" +
                    primaryKey.ToString() + "')";

                return ReadValue(selectCommand, field);
            }

            #endregion

            #region DMax

            /// <summary>
            /// Returns the maximum value of the field on the given table
            /// </summary>
            /// <param name="field">The field to check for</param>
            /// <param name="table">The table to query</param>
            /// <returns></returns>
            public double DMax(string field, string table)
            {
                return DMax(field, table, null);
            }

            /// <summary>
            /// Returns the maximum value of the field on the given table according to the
            /// given criteria
            /// </summary>
            /// <param name="field">The field to check for</param>
            /// <param name="table">The table to query</param>
            /// <param name="criteria">Expression to follow a WHERE clause</param>
            /// <returns></returns>
            public double DMax(string field, string table, string criteria)
            {
                double retVal = -1;

                List<string> maxes = DMax(field, table, criteria, 1, "[" + field +
                    "]");

                if (maxes.Count > 0)
                    double.TryParse(maxes[0].ToString(), out retVal);

                return retVal;
            }

            /// <summary>
            /// Returns the maximum values of the field on the given table according to the
            /// given criteria, in a given order
            /// </summary>
            /// <param name="field">The field to check for</param>
            /// <param name="table">The table to query</param>
            /// <param name="criteria">Expression to follow a WHERE clause</param>
            /// <param name="howMany">The number of top records to retrieve</param>
            /// <param name="orderField">The field to order by</param>
            /// <returns></returns>
            public List<string> DMax(string field, string table, string criteria, int howMany, string orderField)
            {

                string selectCommand = "SELECT TOP " + howMany.ToString() + " [" + field
                    + "] FROM [" + table + "]";

                if (criteria != null)
                    selectCommand += " WHERE " + criteria;

                selectCommand += " ORDER BY " + orderField + " DESC";

                return ReadValues(selectCommand, field);
            }

            #endregion

            #region DMin

            /// <summary>
            /// Returns the minimum value of the field on the given table
            /// </summary>
            /// <param name="field">The field to check for</param>
            /// <param name="table">The table to query</param>
            /// <returns></returns>
            public double DMin(string field, string table)
            {
                return DMin(field, table, null);
            }

            /// <summary>
            /// Returns the minimum value of the field on the given table according to the
            /// given criteria
            /// </summary>
            /// <param name="field">The field to check for</param>
            /// <param name="table">The table to query</param>
            /// <param name="criteria">Expression to follow a WHERE clause</param>
            /// <returns></returns>
            public double DMin(string field, string table, string criteria)
            {
                double retVal = -1;

                List<string> mins = DMin(field, table, criteria, 1, "[" + field +
                    "]");

                if (mins.Count > 0)
                    double.TryParse(mins[0].ToString(), out retVal);

                return retVal;
            }

            /// <summary>
            /// Returns the minimum values of the field on the given table according to the
            /// given criteria, in a given order
            /// </summary>
            /// <param name="field">The field to check for</param>
            /// <param name="table">The table to query</param>
            /// <param name="criteria">Expression to follow a WHERE clause</param>
            /// <param name="howMany">The number of bottom records to retrieve</param>
            /// <param name="orderField">The field to order by</param>
            /// <returns></returns>
            public List<string> DMin(string field, string table, string criteria, int howMany, string orderField)
            {
                string selectCommand = "SELECT TOP " + howMany + " " + field +
                    " FROM " + table;

                if (criteria != null)
                    selectCommand += " WHERE " + criteria;

                selectCommand += " ORDER BY " + orderField;

                return ReadValues(selectCommand, field);
            }

            #endregion

            #region DFirst

            /// <summary>
            /// Returns the <see cref="Guid"/> of the first record in the table
            /// </summary>
            /// <param name="table">The table to query</param>
            /// <returns></returns>
            public Guid DFirst(string table)
            {
                return new Guid(DFirst(null, table, null).ToString());
            }

            /// <summary>
            /// Returns the value of the field at the first record in the table
            /// </summary>
            /// <param name="field">The field to look for</param>
            /// <param name="table">The table to query</param>
            /// <returns></returns>
            public object DFirst(string field, string table)
            {
                return DFirst(field, table, null);
            }

            /// <summary>
            /// Returns the value of the field at the first record in the table according to the
            /// given criteria
            /// </summary>
            /// <param name="field">The field to look for</param>
            /// <param name="table">The table to query</param>
            /// <param name="criteria">SQL criteria</param>
            /// <returns></returns>
            public object DFirst(string field, string table, string criteria)
            {
                return DFirst(field, table, criteria, 1)[0];
            }

            /// <summary>
            /// Returns the values of the field at the first records in the table according to the
            /// given criteria
            /// </summary>
            /// <param name="field">The field to look for</param>
            /// <param name="table">The table to query</param>
            /// <param name="criteria">SQL criteria</param>
            /// <param name="howMany">The number of records to retrieve</param>
            /// <returns></returns>
            public List<string> DFirst(string field, string table, string criteria, int howMany)
            {
                if (field == null)
                    field = GetKeyField(table);

                string selectCommand = "SELECT TOP " + howMany.ToString() +
                    " [" + field + "] FROM [" + table + "]";

                if (criteria != null)
                    selectCommand += " WHERE " + criteria;

                selectCommand += " ORDER BY [" + field + "] ASC";

                return ReadValues(selectCommand, field);
            }

            #endregion

            #region DLast

            /// <summary>
            /// Returns the <see cref="Guid"/> of the last record in the table
            /// </summary>
            /// <param name="table">The table to query</param>
            /// <returns></returns>
            public Guid DLast(string table)
            {
                return new Guid(DLast(null, table, null).ToString());
            }

            /// <summary>
            /// Returns the value of the field at the last record in the table
            /// </summary>
            /// <param name="field">The field to look for</param>
            /// <param name="table">The table to query</param>
            /// <returns></returns>
            public object DLast(string field, string table)
            {
                return DLast(field, table, null);
            }

            /// <summary>
            /// Returns the value of the field at the first record in the table according to the
            /// given criteria
            /// </summary>
            /// <param name="field">The field to look for</param>
            /// <param name="table">The table to query</param>
            /// <param name="criteria">SQL criteria</param>
            /// <returns></returns>
            public object DLast(string field, string table, string criteria)
            {
                return DLast(field, table, criteria, 1)[0];
            }

            /// <summary>
            /// Returns the values of the field at the last records in the table according to the
            /// given criteria
            /// </summary>
            /// <param name="field">The field to look for</param>
            /// <param name="table">The table to query</param>
            /// <param name="criteria">SQL criteria</param>
            /// <param name="howMany">The number of records to retrieve</param>
            /// <returns></returns>
            public List<string> DLast(string field, string table, string criteria, int howMany)
            {
                if (field == null)
                    field = GetKeyField(table);

                string selectCommand = "SELECT TOP " + howMany.ToString() + " [" + field +
                    "] FROM [" + table + "]";

                if (criteria != null)
                    selectCommand += " WHERE " + criteria;

                selectCommand += " ORDER BY [" + field + "] DESC";

                return ReadValues(selectCommand, field);
            }

            #endregion

            #region DSum

            /// <summary>
            /// Sums the values of all the records at the given field of the table
            /// </summary>
            /// <param name="field">The field to sum</param>
            /// <param name="table">The table to query</param>
            /// <returns></returns>
            public double DSum(string field, string table)
            {
                return DSum(field, table, null);
            }

            /// <summary>
            /// Sums the values of the records at the given field of the table according to 
            /// the given criteria
            /// </summary>
            /// <param name="field">The field to sum</param>
            /// <param name="table">The table to query</param>
            /// <param name="criteria">SQL criteria</param>
            /// <returns></returns>
            public double DSum(string field, string table, string criteria)
            {
                string selectCommand = "SELECT ISNULL(SUM([" + field + "]),0) AS SumField FROM ["
                    + table + "]";

                if (criteria != null)
                    selectCommand += " WHERE " + criteria;

                return Double.Parse(ReadValue(selectCommand, "SumField").ToString());
            }

            #endregion

            #region DAverage

            /// <summary>
            /// Returns the average of the values of all the records at the given field of the table
            /// </summary>
            /// <param name="field">The field to sum</param>
            /// <param name="table">The table to query</param>
            /// <returns></returns>
            public double DAverage(string field, string table)
            {
                return DAverage(field, table, null);
            }

            /// <summary>
            /// Returns the average of the values of the records at the given field of the table according to 
            /// the given criteria
            /// </summary>
            /// <param name="field">The field to summarise</param>
            /// <param name="table">The table to query</param>
            /// <param name="criteria">SQL criteria</param>
            /// <returns></returns>
            public double DAverage(string field, string table, string criteria)
            {
                string selectCommand = "SELECT isnull(AVG([" + field + "]),0) AS AvgField FROM ["
                    + table + "]";

                if (criteria != null)
                    selectCommand += " WHERE " + criteria;

                return Double.Parse(ReadValue(selectCommand, "AvgField").ToString());
            }

            #endregion

            #region DCount

            /// <summary>
            /// Counts all the records of the table
            /// </summary>
            /// <param name="table">The table to query</param>
            /// <returns></returns>
            public int DCount(string table)
            {
                return DCount("*", table, null);
            }

            /// <summary>
            /// Counts all the records of the table that match the given criteria
            /// </summary>
            /// <param name="table">The table to query</param>
            /// <param name="field">SQL criteria</param>
            /// <returns></returns>
            public int DCount(string field, string table)
            {
                return DCount(field, table, null);
            }

            /// <summary>
            /// Counts all the records of the table that match the given criteria
            /// </summary>
            /// <param name="field">Table Fiels</param>
            /// <param name="table">The table to query</param>
            /// <param name="criteria">SQL criteria</param>
            /// <returns></returns>
            public int DCount(string field, string table, string criteria)
            {
                if (!field.Equals("*"))
                    field = "[" + field + "]";

                string selectCommand = "SELECT isnull(COUNT(" + field + "),0) AS CountField FROM ["
                    + table + "]";

                if (criteria != null)
                    selectCommand += " WHERE " + criteria;

                return Int32.Parse(ReadValue(selectCommand, "CountField").ToString());
            }

            #endregion

            #endregion

            #region Get DataTable

            /// <summary>
            /// Get Data Table
            /// <para>connects to SQL server and executes SQL statement and return DataTable.</para>
            /// </summary>
            /// <param name="getTable">Get Table Named</param>
            /// <returns>populated DataTable</returns>
            public DataTable GetDataTable(string getTable)
            {
                if (connection != null)
                    return GetDataTable(connection, null, getTable, (string)null);
                if (transaction != null)
                    return GetDataTable(transaction.Connection, transaction, getTable, (string)null);
                return null;
            }

            /// <summary>
            /// Get Data Table
            /// <para>connects to SQL server and executes SQL statement and return DataTable.</para>
            /// </summary>
            /// <param name="connection">Opend or unopend SqlConnection</param>
            /// <param name="getTable">Get Table Named</param>
            /// <returns>populated DataTable</returns>
            public static DataTable GetDataTable(SqlConnection connection, string getTable)
            {
                return GetDataTable(connection, null, getTable, (string)null);
            }

            /// <summary>
            /// Get Data Table
            /// <para>connects to SQL server and executes SQL statement and return DataTable.</para>
            /// </summary>
            /// <param name="transaction">Opend or unopend SqlTransaction</param>
            /// <param name="getTable">Get Table Named</param>
            /// <returns>populated DataTable</returns>
            public static DataTable GetDataTable(SqlTransaction transaction, string getTable)
            {
                return GetDataTable(transaction.Connection, transaction, getTable, (string)null);
            }

            /// <summary>
            /// Get Data Table
            /// <para>connects to SQL server and executes SQL statement and return DataTable.</para>
            /// </summary>
            /// <param name="getTable">Get Table Named</param>
            /// <param name="tableName">Name Of Table in DataTable</param>
            /// <returns>populated DataTable</returns>
            public DataTable GetDataTable(string getTable, string tableName)
            {
                return GetDataTable(connection, transaction, getTable, tableName);
            }

            /// <summary>
            /// Get Data Table
            /// <para>connects to SQL server and executes SQL statement and return DataTable.</para>
            /// </summary>
            /// <param name="connection">Opend or unopend SqlConnection</param>
            /// <param name="getTable">Get Table Named</param>
            /// <param name="tableName">Name Of Table in DataTable</param>
            /// <returns>populated DataTable</returns>
            public static DataTable GetDataTable(SqlConnection connection, string getTable, string tableName)
            {
                return GetDataTable(connection, null, getTable, tableName);
            }

            /// <summary>
            /// Get Data Table
            /// <para>connects to SQL server and executes SQL statement and return DataTable.</para>
            /// </summary>
            /// <param name="transaction">Opend or unopend SqlTransaction</param>
            /// <param name="getTable">Get Table Named</param>
            /// <param name="tableName">Name Of Table in DataTable</param>
            /// <returns>populated DataTable</returns>
            public static DataTable GetDataTable(SqlTransaction transaction, string getTable, string tableName)
            {
                return GetDataTable(transaction.Connection, transaction, getTable, tableName);
            }

            /// <summary>
            /// Get Data Table
            /// <para>connects to SQL server and executes SQL statement and return DataTable.</para>
            /// </summary>
            /// <param name="connection">Opend or unopend SqlConnection</param>
            /// <param name="transaction">Opend or unopend SqlTransaction</param>
            /// <param name="getTable">Get Table Named</param>
            /// <param name="tableName">Name Of Table in DataTable</param>
            /// <returns>populated DataTable</returns>
            private static DataTable GetDataTable(SqlConnection connection, SqlTransaction transaction, string getTable, string tableName)
            {
                // Pass the connection to a command object
                string SQL = "SELECT * FROM " + getTable;
                SqlCommand SqlCommand = new SqlCommand(SQL, connection);
                SqlDataAdapter SqlDataAdapter = new SqlDataAdapter();
                SqlDataAdapter.SelectCommand = SqlCommand;
                DataTable DataTable = new DataTable(tableName);
                DataTable.Locale = System.Globalization.CultureInfo.InvariantCulture;

                // Adds or refreshes rows in the DataSet to match those in the data source
                try
                {
                    if (connection.State != ConnectionState.Open) connection.Open();
                    SqlDataAdapter.Fill(DataTable);
                    connection.Close();
                }
                catch (Exception Exception)
                {
                    Mailer.ErrNotify(Exception, "DBFunctions DBSql GetDataTable");

                    return null;
                }

                return DataTable;
            }

            /// <summary>
            /// Get Data Table
            /// <para>connects to SQL server and executes SQL statement and return DataTable.</para>
            /// </summary>
            /// <param name="sqlStatement">SQL query</param>
            /// <param name="arrParam"> new SqlParameter("@Param1", "Param1")</param>
            /// <returns>populated DataTable</returns>
            public DataTable GetDataTable(string sqlStatement, params SqlParameter[] arrParam)
            {
                DataTable dt = new DataTable();

                // Define the command 
                SqlCommand cmd = new SqlCommand(sqlStatement, connection, transaction);

                // Handle the parameters 
                if (arrParam != null)
                {
                    foreach (SqlParameter param in arrParam)
                        cmd.Parameters.Add(param);
                }

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                try
                {
                    da.Fill(dt);
                }
                catch (Exception Exception)
                {
                    Mailer.ErrNotify(Exception, "DBFunctions DBSql ExecuteDataTable");
                }

                return dt;
            }

            /// <summary>
            /// Get Data Table
            /// <para>connects to SQL server and executes SQL statement and return DataTable.</para>
            /// </summary>
            /// <param name="connection">Opend or unopend SqlConnection</param>
            /// <param name="sqlStatement">SQL query</param>
            /// <param name="arrParam"> new SqlParameter("@Param1", "Param1")</param>
            /// <returns>populated DataTable</returns>
            public static DataTable GetDataTable(SqlConnection connection, string sqlStatement, params SqlParameter[] arrParam)
            {
                return GetDataTable(connection, null, sqlStatement, arrParam); ;
            }

            /// <summary>
            /// Get Data Table
            /// <para>connects to SQL server and executes SQL statement and return DataTable.</para>
            /// </summary>
            /// <param name="transaction">Opend or unopend SqlTransaction</param>
            /// <param name="sqlStatement">SQL query</param>
            /// <param name="arrParam"> new SqlParameter("@Param1", "Param1")</param>
            /// <returns>populated DataTable</returns>
            public static DataTable GetDataTable(SqlTransaction transaction, string sqlStatement, params SqlParameter[] arrParam)
            {
                return GetDataTable(transaction.Connection, transaction, sqlStatement, arrParam);
            }

            /// <summary>
            /// Get Data Table
            /// <para>connects to SQL server and executes SQL statement and return DataTable.</para>
            /// </summary>
            /// <param name="connection">Opend or unopend SqlConnection</param>
            /// <param name="transaction">Opend or unopend SqlTransaction</param>
            /// <param name="sqlStatement">SQL query</param>
            /// <param name="arrParam"> new SqlParameter("@Param1", "Param1")</param>
            /// <returns>populated DataTable</returns>
            private static DataTable GetDataTable(SqlConnection connection, SqlTransaction transaction, string sqlStatement, params SqlParameter[] arrParam)
            {
                DataTable dt = new DataTable();

                // Open the connection 
                using (connection)
                {
                    if (connection.State != ConnectionState.Open) connection.Open();

                    // Define the command 
                    using (SqlCommand cmd = new SqlCommand())
                    {
                        cmd.Connection = connection;
                        cmd.Transaction = transaction;
                        cmd.CommandType = CommandType.Text;
                        cmd.CommandText = sqlStatement;

                        // Handle the parameters 
                        if (arrParam != null)
                        {
                            foreach (SqlParameter param in arrParam)
                                cmd.Parameters.Add(param);
                        }

                        // Define the data adapter and fill the dataset 
                        using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                        {
                            try
                            {
                                da.Fill(dt);
                            }
                            catch (Exception Exception)
                            {
                                Mailer.ErrNotify(Exception, "DBFunctions DBSql ExecuteDataTable");
                            }

                        }
                    }
                }
                return dt;
            }


            #endregion

            #region FillDataset

            /// <summary>
            /// Get Data Set
            /// <para>connects to SQL server and executes SQL statement and return DataTable.</para>
            /// </summary>
            /// <param name="getTable"></param>
            /// <param name="dataSet"></param>
            /// <returns></returns>
            public void FillDataset(string getTable, DataSet dataSet)
            {
                string SQL = "SELECT * FROM " + getTable;
                SqlCommand SqlCommand = new SqlCommand(SQL, connection, transaction);
                SqlDataAdapter SqlDataAdapter = new SqlDataAdapter();
                SqlDataAdapter.SelectCommand = SqlCommand;
                try
                {
                    SqlDataAdapter.Fill(dataSet);
                }
                catch (Exception Exception)
                {
                    Mailer.ErrNotify(Exception, "DBFunctions DBSql GetDataSet");
                }
            }

            /// <summary>
            /// Execute a SqlCommand (that returns a resultset and takes no parameters) against the database specified in 
            /// the connection string. 
            /// </summary>
            /// <remarks>
            /// e.g.:  
            ///  FillDataset(connString, CommandType.StoredProcedure, "GetOrders", ds, new string[] {"orders"});
            /// </remarks>
            /// <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
            /// <param name="commandText">The stored procedure name or T-SQL command</param>
            /// <param name="dataSet">A dataset wich will contain the resultset generated by the command</param>
            /// <param name="tableNames">This array will be used to create table mappings allowing the DataTables to be referenced
            /// by a user defined name (probably the actual table name)</param>
            public void FillDataset(CommandType commandType, string commandText, DataSet dataSet, string[] tableNames)
            {
                if (connection != null)
                    FillDataset(connection, commandType, commandText, dataSet, tableNames);
                if (transaction != null)
                    FillDataset(transaction, commandType, commandText, dataSet, tableNames);
            }

            /// <summary>
            /// Execute a SqlCommand (that returns a resultset) against the database specified in the connection string 
            /// using the provided parameters.
            /// </summary>
            /// <remarks>
            /// e.g.:  
            ///  FillDataset(connString, CommandType.StoredProcedure, "GetOrders", ds, new string[] {"orders"}, new SqlParameter("@prodid", 24));
            /// </remarks>
            /// <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
            /// <param name="commandText">The stored procedure name or T-SQL command</param>
            /// <param name="commandParameters">An array of SqlParamters used to execute the command</param>
            /// <param name="dataSet">A dataset wich will contain the resultset generated by the command</param>
            /// <param name="tableNames">This array will be used to create table mappings allowing the DataTables to be referenced
            /// by a user defined name (probably the actual table name)
            /// </param>
            public void FillDataset(CommandType commandType, string commandText, DataSet dataSet, string[] tableNames, params SqlParameter[] commandParameters)
            {
                if (connection != null)
                    FillDataset(connection, commandType, commandText, dataSet, tableNames, commandParameters);
                if (transaction != null)
                    FillDataset(transaction, commandType, commandText, dataSet, tableNames, commandParameters);
            }

            /// <summary>
            /// Execute a stored procedure via a SqlCommand (that returns a resultset) against the database specified in 
            /// the connection string using the provided parameter values.  This method will query the database to discover the parameters for the 
            /// stored procedure (the first time each stored procedure is called), and assign the values based on parameter order.
            /// </summary>
            /// <remarks>
            /// This method provides no access to output parameters or the stored procedure's return value parameter.
            /// 
            /// e.g.:  
            ///  FillDataset(connString, CommandType.StoredProcedure, "GetOrders", ds, new string[] {"orders"}, 24);
            /// </remarks>
            /// <param name="spName">The name of the stored procedure</param>
            /// <param name="dataSet">A dataset wich will contain the resultset generated by the command</param>
            /// <param name="tableNames">This array will be used to create table mappings allowing the DataTables to be referenced
            /// by a user defined name (probably the actual table name)
            /// </param>    
            /// <param name="parameterValues">An array of objects to be assigned as the input values of the stored procedure</param>
            public void FillDataset(string spName, DataSet dataSet, string[] tableNames, params object[] parameterValues)
            {
                if (connection != null)
                    FillDataset(connection, spName, dataSet, tableNames, parameterValues);
                if (transaction != null)
                    FillDataset(transaction, spName, dataSet, tableNames, parameterValues);
            }

            /// <summary>
            /// Execute a SqlCommand (that returns a resultset and takes no parameters) against the provided SqlConnection. 
            /// </summary>
            /// <remarks>
            /// e.g.:  
            ///  FillDataset(conn, CommandType.StoredProcedure, "GetOrders", ds, new string[] {"orders"});
            /// </remarks>
            /// <param name="connection">A valid SqlConnection</param>
            /// <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
            /// <param name="commandText">The stored procedure name or T-SQL command</param>
            /// <param name="dataSet">A dataset wich will contain the resultset generated by the command</param>
            /// <param name="tableNames">This array will be used to create table mappings allowing the DataTables to be referenced
            /// by a user defined name (probably the actual table name)
            /// </param>    
            public static void FillDataset(SqlConnection connection, CommandType commandType, string commandText, DataSet dataSet, string[] tableNames)
            {
                FillDataset(connection, commandType, commandText, dataSet, tableNames, null);
            }

            /// <summary>
            /// Execute a SqlCommand (that returns a resultset) against the specified SqlConnection 
            /// using the provided parameters.
            /// </summary>
            /// <remarks>
            /// e.g.:  
            ///  FillDataset(conn, CommandType.StoredProcedure, "GetOrders", ds, new string[] {"orders"}, new SqlParameter("@prodid", 24));
            /// </remarks>
            /// <param name="connection">A valid SqlConnection</param>
            /// <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
            /// <param name="commandText">The stored procedure name or T-SQL command</param>
            /// <param name="dataSet">A dataset wich will contain the resultset generated by the command</param>
            /// <param name="tableNames">This array will be used to create table mappings allowing the DataTables to be referenced
            /// by a user defined name (probably the actual table name)
            /// </param>
            /// <param name="commandParameters">An array of SqlParamters used to execute the command</param>
            public static void FillDataset(SqlConnection connection, CommandType commandType, string commandText, DataSet dataSet, string[] tableNames, params SqlParameter[] commandParameters)
            {
                FillDataset(connection, null, commandType, commandText, dataSet, tableNames, commandParameters);
            }

            /// <summary>
            /// Execute a stored procedure via a SqlCommand (that returns a resultset) against the specified SqlConnection 
            /// using the provided parameter values.  This method will query the database to discover the parameters for the 
            /// stored procedure (the first time each stored procedure is called), and assign the values based on parameter order.
            /// </summary>
            /// <remarks>
            /// This method provides no access to output parameters or the stored procedure's return value parameter.
            /// 
            /// e.g.:  
            ///  FillDataset(conn, "GetOrders", ds, new string[] {"orders"}, 24, 36);
            /// </remarks>
            /// <param name="connection">A valid SqlConnection</param>
            /// <param name="spName">The name of the stored procedure</param>
            /// <param name="dataSet">A dataset wich will contain the resultset generated by the command</param>
            /// <param name="tableNames">This array will be used to create table mappings allowing the DataTables to be referenced
            /// by a user defined name (probably the actual table name)
            /// </param>
            /// <param name="parameterValues">An array of objects to be assigned as the input values of the stored procedure</param>
            public static void FillDataset(SqlConnection connection, string spName, DataSet dataSet, string[] tableNames, params object[] parameterValues)
            {
                if (connection == null) throw new ArgumentNullException("connection");
                if (dataSet == null) throw new ArgumentNullException("dataSet");
                if (spName == null || spName.Length == 0) throw new ArgumentNullException("spName");

                // If we receive parameter values, we need to figure out where they go
                if ((parameterValues != null) && (parameterValues.Length > 0))
                {
                    // Pull the parameters for this stored procedure from the parameter cache (or discover them & populate the cache)
                    SqlParameter[] commandParameters = SqlHelperParameterCache.GetSpParameterSet(connection, spName);

                    // Assign the provided values to these parameters based on parameter order
                    AssignParameterValues(commandParameters, parameterValues);

                    // Call the overload that takes an array of SqlParameters
                    FillDataset(connection, CommandType.StoredProcedure, spName, dataSet, tableNames, commandParameters);
                }
                else
                {
                    // Otherwise we can just call the SP without params
                    FillDataset(connection, CommandType.StoredProcedure, spName, dataSet, tableNames);
                }
            }

            /// <summary>
            /// Execute a SqlCommand (that returns a resultset and takes no parameters) against the provided SqlTransaction. 
            /// </summary>
            /// <remarks>
            /// e.g.:  
            ///  FillDataset(trans, CommandType.StoredProcedure, "GetOrders", ds, new string[] {"orders"});
            /// </remarks>
            /// <param name="transaction">A valid SqlTransaction</param>
            /// <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
            /// <param name="commandText">The stored procedure name or T-SQL command</param>
            /// <param name="dataSet">A dataset wich will contain the resultset generated by the command</param>
            /// <param name="tableNames">This array will be used to create table mappings allowing the DataTables to be referenced
            /// by a user defined name (probably the actual table name)
            /// </param>
            public static void FillDataset(SqlTransaction transaction, CommandType commandType, string commandText, DataSet dataSet, string[] tableNames)
            {
                FillDataset(transaction.Connection, transaction, commandType, commandText, dataSet, tableNames, null);
            }

            /// <summary>
            /// Execute a SqlCommand (that returns a resultset) against the specified SqlTransaction
            /// using the provided parameters.
            /// </summary>
            /// <remarks>
            /// e.g.:  
            ///  FillDataset(trans, CommandType.StoredProcedure, "GetOrders", ds, new string[] {"orders"}, new SqlParameter("@prodid", 24));
            /// </remarks>
            /// <param name="transaction">A valid SqlTransaction</param>
            /// <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
            /// <param name="commandText">The stored procedure name or T-SQL command</param>
            /// <param name="dataSet">A dataset wich will contain the resultset generated by the command</param>
            /// <param name="tableNames">This array will be used to create table mappings allowing the DataTables to be referenced
            /// by a user defined name (probably the actual table name)
            /// </param>
            /// <param name="commandParameters">An array of SqlParamters used to execute the command</param>
            public static void FillDataset(SqlTransaction transaction, CommandType commandType, string commandText, DataSet dataSet, string[] tableNames, params SqlParameter[] commandParameters)
            {
                FillDataset(transaction.Connection, transaction, commandType, commandText, dataSet, tableNames, commandParameters);
            }

            /// <summary>
            /// Execute a stored procedure via a SqlCommand (that returns a resultset) against the specified 
            /// SqlTransaction using the provided parameter values.  This method will query the database to discover the parameters for the 
            /// stored procedure (the first time each stored procedure is called), and assign the values based on parameter order.
            /// </summary>
            /// <remarks>
            /// This method provides no access to output parameters or the stored procedure's return value parameter.
            /// 
            /// e.g.:  
            ///  FillDataset(trans, "GetOrders", ds, new string[]{"orders"}, 24, 36);
            /// </remarks>
            /// <param name="transaction">A valid SqlTransaction</param>
            /// <param name="spName">The name of the stored procedure</param>
            /// <param name="dataSet">A dataset wich will contain the resultset generated by the command</param>
            /// <param name="tableNames">This array will be used to create table mappings allowing the DataTables to be referenced
            /// by a user defined name (probably the actual table name)
            /// </param>
            /// <param name="parameterValues">An array of objects to be assigned as the input values of the stored procedure</param>
            public static void FillDataset(SqlTransaction transaction, string spName, DataSet dataSet, string[] tableNames, params object[] parameterValues)
            {
                if (transaction == null) throw new ArgumentNullException("transaction");
                if (transaction != null && transaction.Connection == null) throw new ArgumentException("The transaction was rollbacked or commited, please provide an open transaction.", "transaction");
                if (dataSet == null) throw new ArgumentNullException("dataSet");
                if (spName == null || spName.Length == 0) throw new ArgumentNullException("spName");

                // If we receive parameter values, we need to figure out where they go
                if ((parameterValues != null) && (parameterValues.Length > 0))
                {
                    // Pull the parameters for this stored procedure from the parameter cache (or discover them & populate the cache)
                    SqlParameter[] commandParameters = SqlHelperParameterCache.GetSpParameterSet(transaction.Connection, spName);

                    // Assign the provided values to these parameters based on parameter order
                    AssignParameterValues(commandParameters, parameterValues);

                    // Call the overload that takes an array of SqlParameters
                    FillDataset(transaction, CommandType.StoredProcedure, spName, dataSet, tableNames, commandParameters);
                }
                else
                {
                    // Otherwise we can just call the SP without params
                    FillDataset(transaction, CommandType.StoredProcedure, spName, dataSet, tableNames);
                }
            }

            /// <summary>
            /// Private helper method that execute a SqlCommand (that returns a resultset) against the specified SqlTransaction and SqlConnection
            /// using the provided parameters.
            /// </summary>
            /// <remarks>
            /// e.g.:  
            ///  FillDataset(conn, trans, CommandType.StoredProcedure, "GetOrders", ds, new string[] {"orders"}, new SqlParameter("@prodid", 24));
            /// </remarks>
            /// <param name="connection">A valid SqlConnection</param>
            /// <param name="transaction">A valid SqlTransaction</param>
            /// <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
            /// <param name="commandText">The stored procedure name or T-SQL command</param>
            /// <param name="dataSet">A dataset wich will contain the resultset generated by the command</param>
            /// <param name="tableNames">This array will be used to create table mappings allowing the DataTables to be referenced
            /// by a user defined name (probably the actual table name)
            /// </param>
            /// <param name="commandParameters">An array of SqlParamters used to execute the command</param>
            private static void FillDataset(SqlConnection connection, SqlTransaction transaction, CommandType commandType, string commandText, DataSet dataSet, string[] tableNames, params SqlParameter[] commandParameters)
            {
                if (connection == null) Mailer.ErrNotify("connection", "DBFunctions DBSql FillDataset");
                if (dataSet == null) Mailer.ErrNotify("dataSet", "DBFunctions DBSql FillDataset");

                // Create a command and prepare it for execution
                SqlCommand command = new SqlCommand();
                PrepareCommand(command, connection, transaction, commandType, commandText, commandParameters);

                // Create the DataAdapter & DataSet
                using (SqlDataAdapter dataAdapter = new SqlDataAdapter(command))
                {

                    // Add the table mappings specified by the user
                    if (tableNames != null && tableNames.Length > 0)
                    {
                        string tableName = "Table";
                        for (int index = 0; index < tableNames.Length; index++)
                        {
                            if (tableNames[index] == null || tableNames[index].Length == 0) Mailer.ErrNotify("The tableNames parameter must contain a list of tables, a value was provided as null or empty string.", "DBFunctions DBSql FillDataset");
                            dataAdapter.TableMappings.Add(tableName, tableNames[index]);
                            tableName += (index + 1).ToString();
                        }
                    }

                    // Fill the DataSet using default values for DataTable names, etc
                    dataAdapter.Fill(dataSet);

                    // Detach the SqlParameters from the command object, so they can be used again
                    command.Parameters.Clear();
                }
            }

            #endregion

            #region ExecuteDataset

            /// <summary>
            /// Execute a SqlCommand (that returns a resultset and takes no parameters) against the database specified in 
            /// the connection string. 
            /// </summary>
            /// <remarks>
            /// e.g.:  
            ///  DataSet ds = ExecuteDataset(connString, CommandType.StoredProcedure, "GetOrders");
            /// </remarks>
            /// <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
            /// <param name="commandText">The stored procedure name or T-SQL command</param>
            /// <returns>A dataset containing the resultset generated by the command</returns>
            public DataSet ExecuteDataset(CommandType commandType, string commandText)
            {
                if (connection != null)
                    return ExecuteDataset(connection, commandType, commandText, (SqlParameter[])null);
                if (transaction != null)
                    return ExecuteDataset(transaction, commandType, commandText, (SqlParameter[])null);
                return null;
            }

            /// <summary>
            /// Execute a SqlCommand (that returns a resultset) against the database specified in the connection string 
            /// using the provided parameters.
            /// </summary>
            /// <remarks>
            /// e.g.:  
            ///  DataSet ds = ExecuteDataset(connString, CommandType.StoredProcedure, "GetOrders", new SqlParameter("@prodid", 24));
            /// </remarks>
            /// <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
            /// <param name="commandText">The stored procedure name or T-SQL command</param>
            /// <param name="commandParameters">An array of SqlParamters used to execute the command</param>
            /// <returns>A dataset containing the resultset generated by the command</returns>
            public DataSet ExecuteDataset(CommandType commandType, string commandText, params SqlParameter[] commandParameters)
            {
                if (connection != null)
                    return ExecuteDataset(connection, commandType, commandText, commandParameters);
                if (transaction != null)
                    return ExecuteDataset(transaction, commandType, commandText, commandParameters);
                return null;
            }

            /// <summary>
            /// Execute a stored procedure via a SqlCommand (that returns a resultset) against the database specified in 
            /// the connection string using the provided parameter values.  This method will query the database to discover the parameters for the 
            /// stored procedure (the first time each stored procedure is called), and assign the values based on parameter order.
            /// </summary>
            /// <remarks>
            /// This method provides no access to output parameters or the stored procedure's return value parameter.
            /// 
            /// e.g.:  
            ///  DataSet ds = ExecuteDataset(connString, "GetOrders", 24, 36);
            /// </remarks>
            /// <param name="spName">The name of the stored procedure</param>
            /// <param name="parameterValues">An array of objects to be assigned as the input values of the stored procedure</param>
            /// <returns>A dataset containing the resultset generated by the command</returns>
            public DataSet ExecuteDataset(string spName, params object[] parameterValues)
            {
                if (spName == null || spName.Length == 0) Mailer.ErrNotify("spName", "DBFunctions DBSql ExecuteDataset");

                // If we receive parameter values, we need to figure out where they go
                if ((parameterValues != null) && (parameterValues.Length > 0))
                {
                    // Pull the parameters for this stored procedure from the parameter cache (or discover them & populate the cache)
                    SqlParameter[] commandParameters = SqlHelperParameterCache.GetSpParameterSet(GetConnectionString(), spName);

                    // Assign the provided values to these parameters based on parameter order
                    AssignParameterValues(commandParameters, parameterValues);

                    // Call the overload that takes an array of SqlParameters
                    if (connection != null)
                        return ExecuteDataset(connection, CommandType.StoredProcedure, spName, commandParameters);
                    if (transaction != null)
                        return ExecuteDataset(transaction, CommandType.StoredProcedure, spName, commandParameters);
                    return null;
                }
                else
                {
                    // Otherwise we can just call the SP without params
                    if (connection != null)
                        return ExecuteDataset(connection, CommandType.StoredProcedure, spName, (SqlParameter[])null);
                    if (transaction != null)
                        return ExecuteDataset(transaction, CommandType.StoredProcedure, spName, (SqlParameter[])null);
                    return null;
                }
            }

            /// <summary>
            /// Execute a SqlCommand (that returns a resultset and takes no parameters) against the provided SqlConnection. 
            /// </summary>
            /// <remarks>
            /// e.g.:  
            ///  DataSet ds = ExecuteDataset(conn, CommandType.StoredProcedure, "GetOrders");
            /// </remarks>
            /// <param name="connection">A valid SqlConnection</param>
            /// <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
            /// <param name="commandText">The stored procedure name or T-SQL command</param>
            /// <returns>A dataset containing the resultset generated by the command</returns>
            public static DataSet ExecuteDataset(SqlConnection connection, CommandType commandType, string commandText)
            {
                // Pass through the call providing null for the set of SqlParameters
                return ExecuteDataset(connection, commandType, commandText, (SqlParameter[])null);
            }

            /// <summary>
            /// Execute a SqlCommand (that returns a resultset) against the specified SqlConnection 
            /// using the provided parameters.
            /// </summary>
            /// <remarks>
            /// e.g.:  
            ///  DataSet ds = ExecuteDataset(conn, CommandType.StoredProcedure, "GetOrders", new SqlParameter("@prodid", 24));
            /// </remarks>
            /// <param name="connection">A valid SqlConnection</param>
            /// <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
            /// <param name="commandText">The stored procedure name or T-SQL command</param>
            /// <param name="commandParameters">An array of SqlParamters used to execute the command</param>
            /// <returns>A dataset containing the resultset generated by the command</returns>
            public static DataSet ExecuteDataset(SqlConnection connection, CommandType commandType, string commandText, params SqlParameter[] commandParameters)
            {
                if (connection == null) Mailer.ErrNotify("connection", "DBFunctions DBSql FillDataset");

                // Create a command and prepare it for execution
                SqlCommand cmd = new SqlCommand();
                PrepareCommand(cmd, connection, (SqlTransaction)null, commandType, commandText, commandParameters);

                // Create the DataAdapter & DataSet
                using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                {
                    DataSet ds = new DataSet();

                    // Fill the DataSet using default values for DataTable names, etc
                    da.Fill(ds);

                    // Detach the SqlParameters from the command object, so they can be used again
                    cmd.Parameters.Clear();

                    // Return the dataset
                    return ds;
                }
            }

            /// <summary>
            /// Execute a stored procedure via a SqlCommand (that returns a resultset) against the specified SqlConnection 
            /// using the provided parameter values.  This method will query the database to discover the parameters for the 
            /// stored procedure (the first time each stored procedure is called), and assign the values based on parameter order.
            /// </summary>
            /// <remarks>
            /// This method provides no access to output parameters or the stored procedure's return value parameter.
            /// 
            /// e.g.:  
            ///  DataSet ds = ExecuteDataset(conn, "GetOrders", 24, 36);
            /// </remarks>
            /// <param name="connection">A valid SqlConnection</param>
            /// <param name="spName">The name of the stored procedure</param>
            /// <param name="parameterValues">An array of objects to be assigned as the input values of the stored procedure</param>
            /// <returns>A dataset containing the resultset generated by the command</returns>
            public static DataSet ExecuteDataset(SqlConnection connection, string spName, params object[] parameterValues)
            {
                if (connection == null) Mailer.ErrNotify("connection", "DBFunctions DBSql FillDataset");
                if (spName == null || spName.Length == 0) Mailer.ErrNotify("spName", "DBFunctions DBSql FillDataset");

                // If we receive parameter values, we need to figure out where they go
                if ((parameterValues != null) && (parameterValues.Length > 0))
                {
                    // Pull the parameters for this stored procedure from the parameter cache (or discover them & populate the cache)
                    SqlParameter[] commandParameters = SqlHelperParameterCache.GetSpParameterSet(connection, spName);

                    // Assign the provided values to these parameters based on parameter order
                    AssignParameterValues(commandParameters, parameterValues);

                    // Call the overload that takes an array of SqlParameters
                    return ExecuteDataset(connection, CommandType.StoredProcedure, spName, commandParameters);
                }
                else
                {
                    // Otherwise we can just call the SP without params
                    return ExecuteDataset(connection, CommandType.StoredProcedure, spName);
                }
            }

            /// <summary>
            /// Execute a SqlCommand (that returns a resultset and takes no parameters) against the provided SqlTransaction. 
            /// </summary>
            /// <remarks>
            /// e.g.:  
            ///  DataSet ds = ExecuteDataset(trans, CommandType.StoredProcedure, "GetOrders");
            /// </remarks>
            /// <param name="transaction">A valid SqlTransaction</param>
            /// <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
            /// <param name="commandText">The stored procedure name or T-SQL command</param>
            /// <returns>A dataset containing the resultset generated by the command</returns>
            public static DataSet ExecuteDataset(SqlTransaction transaction, CommandType commandType, string commandText)
            {
                // Pass through the call providing null for the set of SqlParameters
                return ExecuteDataset(transaction, commandType, commandText, (SqlParameter[])null);
            }

            /// <summary>
            /// Execute a SqlCommand (that returns a resultset) against the specified SqlTransaction
            /// using the provided parameters.
            /// </summary>
            /// <remarks>
            /// e.g.:  
            ///  DataSet ds = ExecuteDataset(trans, CommandType.StoredProcedure, "GetOrders", new SqlParameter("@prodid", 24));
            /// </remarks>
            /// <param name="transaction">A valid SqlTransaction</param>
            /// <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
            /// <param name="commandText">The stored procedure name or T-SQL command</param>
            /// <param name="commandParameters">An array of SqlParamters used to execute the command</param>
            /// <returns>A dataset containing the resultset generated by the command</returns>
            public static DataSet ExecuteDataset(SqlTransaction transaction, CommandType commandType, string commandText, params SqlParameter[] commandParameters)
            {
                if (transaction == null) Mailer.ErrNotify("transaction", "DBFunctions DBSql FillDataset");
                if (transaction != null && transaction.Connection == null) Mailer.ErrNotify("The transaction was rollbacked or commited, please provide an open transaction.", "DBFunctions DBSql FillDataset");

                // Create a command and prepare it for execution
                SqlCommand cmd = new SqlCommand();
                PrepareCommand(cmd, transaction.Connection, transaction, commandType, commandText, commandParameters);

                // Create the DataAdapter & DataSet
                using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                {
                    DataSet ds = new DataSet();

                    // Fill the DataSet using default values for DataTable names, etc
                    da.Fill(ds);

                    // Detach the SqlParameters from the command object, so they can be used again
                    cmd.Parameters.Clear();

                    // Return the dataset
                    return ds;
                }
            }

            /// <summary>
            /// Execute a stored procedure via a SqlCommand (that returns a resultset) against the specified 
            /// SqlTransaction using the provided parameter values.  This method will query the database to discover the parameters for the 
            /// stored procedure (the first time each stored procedure is called), and assign the values based on parameter order.
            /// </summary>
            /// <remarks>
            /// This method provides no access to output parameters or the stored procedure's return value parameter.
            /// 
            /// e.g.:  
            ///  DataSet ds = ExecuteDataset(trans, "GetOrders", 24, 36);
            /// </remarks>
            /// <param name="transaction">A valid SqlTransaction</param>
            /// <param name="spName">The name of the stored procedure</param>
            /// <param name="parameterValues">An array of objects to be assigned as the input values of the stored procedure</param>
            /// <returns>A dataset containing the resultset generated by the command</returns>
            public static DataSet ExecuteDataset(SqlTransaction transaction, string spName, params object[] parameterValues)
            {
                if (transaction == null) Mailer.ErrNotify("transaction", "DBFunctions DBSql FillDataset");
                if (transaction != null && transaction.Connection == null) Mailer.ErrNotify("The transaction was rollbacked or commited, please provide an open transaction.", "DBFunctions DBSql FillDataset");
                if (spName == null || spName.Length == 0) Mailer.ErrNotify("spName", "DBFunctions DBSql FillDataset");

                // If we receive parameter values, we need to figure out where they go
                if ((parameterValues != null) && (parameterValues.Length > 0))
                {
                    // Pull the parameters for this stored procedure from the parameter cache (or discover them & populate the cache)
                    SqlParameter[] commandParameters = SqlHelperParameterCache.GetSpParameterSet(transaction.Connection, spName);

                    // Assign the provided values to these parameters based on parameter order
                    AssignParameterValues(commandParameters, parameterValues);

                    // Call the overload that takes an array of SqlParameters
                    return ExecuteDataset(transaction, CommandType.StoredProcedure, spName, commandParameters);
                }
                else
                {
                    // Otherwise we can just call the SP without params
                    return ExecuteDataset(transaction, CommandType.StoredProcedure, spName);
                }
            }

            #endregion ExecuteDataset

            #region UpdateDataset

            /// <summary>
            /// Executes the respective command for each inserted, updated, or deleted row in the DataSet.
            /// </summary>
            /// <remarks>
            /// e.g.:  
            ///  UpdateDataset(conn, insertCommand, deleteCommand, updateCommand, dataSet, "Order");
            /// </remarks>
            /// <param name="insertCommand">A valid transact-SQL statement or stored procedure to insert new records into the data source</param>
            /// <param name="deleteCommand">A valid transact-SQL statement or stored procedure to delete records from the data source</param>
            /// <param name="updateCommand">A valid transact-SQL statement or stored procedure used to update records in the data source</param>
            /// <param name="dataSet">The DataSet used to update the data source</param>
            /// <param name="tableName">The DataTable used to update the data source.</param>
            public static void UpdateDataset(SqlCommand insertCommand, SqlCommand deleteCommand, SqlCommand updateCommand, DataSet dataSet, string tableName)
            {
                if (insertCommand == null) Mailer.ErrNotify("insertCommand", "DBFunctions DBSql FillDataset");
                if (deleteCommand == null) Mailer.ErrNotify("deleteCommand", "DBFunctions DBSql FillDataset");
                if (updateCommand == null) Mailer.ErrNotify("updateCommand", "DBFunctions DBSql FillDataset");
                if (tableName == null || tableName.Length == 0) Mailer.ErrNotify("tableName", "DBFunctions DBSql FillDataset");

                // Create a SqlDataAdapter, and dispose of it after we are done
                using (SqlDataAdapter dataAdapter = new SqlDataAdapter())
                {
                    // Set the data adapter commands
                    dataAdapter.UpdateCommand = updateCommand;
                    dataAdapter.InsertCommand = insertCommand;
                    dataAdapter.DeleteCommand = deleteCommand;

                    // Update the dataset changes in the data source
                    dataAdapter.Update(dataSet, tableName);

                    // Commit all the changes made to the DataSet
                    dataSet.AcceptChanges();
                }
            }

            #endregion

            #region Does Record Existes

            /// <summary>
            /// Record Exists
            /// <para>connects to SQL server and executes SQL statement to determine whether the given record exists in the database.</para>
            /// </summary>
            /// <param name="SQL"></param>
            /// <returns></returns>
            public bool RecordExists(string SQL)
            {
                if (connection != null)
                    return RecordExists(connection, null, SQL);
                if (transaction != null)
                    return RecordExists(transaction.Connection, transaction, SQL);
                return false;
            }

            /// <summary>
            /// Record Exists
            /// <para>connects to SQL server and executes SQL statement to determine whether the given record exists in the database.</para>
            /// </summary>
            /// <param name="connection"></param>
            /// <param name="SQL"></param>
            /// <returns></returns>
            public static bool RecordExists(SqlConnection connection, string SQL)
            {
                return RecordExists(connection, null, SQL);
            }

            /// <summary>
            /// Record Exists
            /// <para>connects to SQL server and executes SQL statement to determine whether the given record exists in the database.</para>
            /// </summary>
            /// <param name="transaction"></param>
            /// <param name="SQL"></param>
            /// <returns></returns>
            public static bool RecordExists(SqlTransaction transaction, string SQL)
            {
                return RecordExists(transaction.Connection, transaction, SQL);
            }

            /// <summary>
            /// Record Exists
            /// <para>connects to SQL server and executes SQL statement to determine whether the given record exists in the database.</para>
            /// </summary>
            /// <param name="connection"></param>
            /// <param name="transaction"></param>
            /// <param name="SQL"></param>
            /// <returns></returns>
            private static bool RecordExists(SqlConnection connection, SqlTransaction transaction, string SQL)
            {
                SqlDataReader SqlDataReader = null;

                try
                {
                    SqlCommand SqlCommand = new SqlCommand(SQL, connection, transaction);
                    SqlDataReader = SqlCommand.ExecuteReader();

                }
                catch (Exception Exception)
                {
                    Mailer.ErrNotify(Exception, "DBFunctions DBSql RecordExists");
                    return false;
                }

                if (SqlDataReader != null && SqlDataReader.Read())
                {
                    // close sql reader before exit
                    if (SqlDataReader != null)
                    {
                        SqlDataReader.Close();
                        SqlDataReader.Dispose();
                    }

                    // record found
                    return true;
                }
                else
                {
                    // close sql reader before exit
                    if (SqlDataReader != null)
                    {
                        SqlDataReader.Close();
                        SqlDataReader.Dispose();
                    }

                    // record not found
                    return false;
                }
            }


            #endregion

            #region Update Functions

            /// <summary>
            /// Update a field of the given table for records that match the given criteria
            /// to a new value
            /// </summary>
            /// <param name="field">The field to update</param>
            /// <param name="table">The table</param>
            /// <param name="criteria">SQL Criteria</param>
            /// <param name="newValue">New value</param>
            /// <returns></returns>
            public int DUpdate(string field, string table, string criteria, string newValue)
            {
                if (connection != null)
                    return DUpdate(connection, field, table, criteria, newValue);
                if (transaction != null)
                    return DUpdate(transaction, field, table, criteria, newValue);
                return -1;
            }

            /// <summary>
            /// Update a field of the given table for records that match the given criteria
            /// to a new value
            /// </summary>
            /// <param name="connection">A valid SqlConnection</param>
            /// <param name="field">The field to update</param>
            /// <param name="table">The table</param>
            /// <param name="criteria">SQL Criteria</param>
            /// <param name="newValue">New value</param>
            /// <returns></returns>
            public static int DUpdate(SqlConnection connection, string field, string table, string criteria, string newValue)
            {
                return DUpdate(connection, null, field, table, criteria, newValue);
            }


            /// <summary>
            /// Update a field of the given table for records that match the given criteria
            /// to a new value
            /// </summary>
            /// <param name="transaction">A valid SqlTransaction</param>
            /// <param name="field">The field to update</param>
            /// <param name="table">The table</param>
            /// <param name="criteria">SQL Criteria</param>
            /// <param name="newValue">New value</param>
            /// <returns></returns>
            public static int DUpdate(SqlTransaction transaction, string field, string table, string criteria, string newValue)
            {
                return DUpdate(transaction.Connection, transaction, field, table, criteria, newValue);
            }


            /// <summary>
            /// Update a field of the given table for records that match the given criteria
            /// to a new value
            /// </summary>
            /// <param name="connection">A valid SqlConnection</param>
            /// <param name="transaction">A valid SqlTransaction</param>
            /// <param name="field">The field to update</param>
            /// <param name="table">The table</param>
            /// <param name="criteria">SQL Criteria</param>
            /// <param name="newValue">New value</param>
            /// <returns></returns>
            public static int DUpdate(SqlConnection connection, SqlTransaction transaction, string field, string table, string criteria, string newValue)
            {
                int retVal = -1;

                SqlCommand command;

                string updateCommand = "UPDATE [" + table + "] SET [" + field + "] = " + newValue;

                if (criteria != null)
                    updateCommand += " WHERE (" + criteria + ")";

                try
                {
                    command = new SqlCommand(updateCommand, connection, transaction);
                    retVal = command.ExecuteNonQuery();
                }
                catch (SqlException e)
                {
                    Mailer.ErrNotify(e, "DBFunctions DBSql DUpdate");
                }

                return retVal;
            }

            #endregion

            #region Insert Functions

            #region DBInsert

            /// <summary>
            /// Insert the given values at the specified fields name of the table
            /// </summary>
            /// <param name="fieldNames">String array of field names</param>
            /// <param name="table">The table</param>
            /// <param name="values">Object array of values (must be same length as 
            /// fieldNames)</param>
            /// <param name="userID">Guid</param>
            /// <returns>The see cref="Guid"/ of the record inserted</returns>
            public Guid DBInsert(string[] fieldNames, string table, object[] values, Guid userID)
            {
                int retVal = -1;

                if (fieldNames.Length != values.Length)
                    return new Guid();

                SqlCommand command;

                string fieldString = "";
                string valuesString = "";

                //Formulate value string
                for (int i = 0; i < fieldNames.Length; i++)
                {
                    if (fieldNames[i] != "[User]" && fieldNames[i] != "RegDate")
                    {
                        fieldString += "[" + fieldNames[i] + "], ";

                        if (values[i].GetType() == typeof(System.String))
                            valuesString += "N'" + values[i].ToString() + "', ";
                        else if (values[i].GetType() == typeof(System.Guid))
                            valuesString += "'" + values[i].ToString() + "', ";
                        else
                            valuesString += values[i].ToString() + ", ";
                    }
                }

                //Add user identification
                fieldString = fieldString + "[User]";
                valuesString = valuesString + "'" + userID.ToString() + "'";

                string insertCommand = "INSERT INTO [" + table + "] (" + fieldString + ") VALUES (" + valuesString + ")";

                try
                {
                    command = new SqlCommand(insertCommand, connection, transaction);
                    retVal = command.ExecuteNonQuery();
                }
                catch (SqlException se)
                {
                    Mailer.ErrNotify(se, "DBFunction DBSql DBInsert");
                }

                if (retVal == 1)
                    return DLast(table);
                else
                    return new Guid();

            }

            /// <summary>
            /// Returns the latest identity used within the scope of that user statement
            /// </summary>
            /// <param name="SQL"></param>
            /// <returns></returns>
            public long DBExecuteScalar(string SQL)
            {
                long ReturnID = 0;
                object ReturnObject = null;

                try
                {
                    // Execute Query, and return last insert ID
                    SQL += ";SELECT @@IDENTITY AS 'Identity'";
                    SqlCommand SqlCommand = new SqlCommand(SQL, connection, transaction);

                    ReturnObject = SqlCommand.ExecuteScalar();

                    SqlCommand.Dispose();
                    SqlCommand = null;
                }
                catch (SqlException se)
                {
                    // Error occurred while trying to execute reader
                    Mailer.ErrNotify(se, "DBFunction DBInsertExecuteScalar - SqlException");

                    return 0;
                }

                // convert return object to long
                try
                {
                    ReturnID = long.Parse(ReturnObject.ToString());
                }
                catch (Exception se)
                {
                    // Error occurred while trying to convert return id to long
                    Mailer.ErrNotify(se, "DBFunction DBSql DBInsertExecuteScalar");

                    return 0;
                }
                return ReturnID;
            }

            #endregion

            #endregion

            #region Delete Functions

            #region DBDelete

            /// <summary>
            /// Removes the record with the specified key from the table
            /// </summary>
            /// <param name="primaryKey">The primary key as <see cref="Guid"/></param>
            /// <param name="table">The table</param>
            /// <returns></returns>
            public int DBDelete(Guid primaryKey, string table)
            {
                int retVal = -1;

                if (primaryKey == null || String.IsNullOrEmpty(table))
                    return retVal;

                SqlCommand command;

                string deleteCommand = "DELETE FROM [" + table + "] WHERE [" +
                    GetKeyField(table) + "] = '" + primaryKey.ToString() + "'";

                try
                {
                    command = new SqlCommand(deleteCommand, connection, transaction);
                    retVal = command.ExecuteNonQuery();
                }
                catch (SqlException se)
                {
                    Mailer.ErrNotify(se, "DBFunction DBSql DBDelete");
                }

                return retVal;

            }

            /// <summary>
            /// Removes the records with the specified keys from the table
            /// </summary>
            /// <param name="primaryKeys">The primary keys as see cref="Guid"/ array</param>
            /// <param name="table">The table</param>
            /// <returns></returns>
            public int DBDelete(Guid[] primaryKeys, string table)
            {
                int retVal = -1;

                if (primaryKeys.Length < 1 || String.IsNullOrEmpty(table))
                    return retVal;

                SqlCommand command;

                string keyField = GetKeyField(table);
                string deleteCommand = "DELETE FROM " + table + " WHERE";

                foreach (Guid curKey in primaryKeys)
                    deleteCommand += " (" + keyField + " = '" + curKey.ToString() + "') OR";

                deleteCommand = deleteCommand.Substring(0, deleteCommand.Length - 3);

                try
                {
                    command = new SqlCommand(deleteCommand, connection, transaction);
                    retVal = command.ExecuteNonQuery();
                }
                catch (SqlException se)
                {
                    Mailer.ErrNotify(se, "DBFunction DBSql DBDelete");
                }

                return retVal;
            }

            /// <summary>
            /// Removes the records that match the given from the table
            /// </summary>
            /// <param name="criteria">SQL Criteria</param>
            /// <param name="table">The table</param>
            /// <returns></returns>
            public int DBDelete(string criteria, string table)
            {
                int retVal = -1;

                if (String.IsNullOrEmpty(criteria) || String.IsNullOrEmpty(table))
                    return retVal;

                SqlCommand command;

                string deleteCommand = "DELETE FROM " + table + " WHERE " +
                    criteria;

                try
                {
                    command = new SqlCommand(deleteCommand, connection, transaction);
                    retVal = command.ExecuteNonQuery();
                }
                catch (SqlException se)
                {
                    Mailer.ErrNotify(se, "DBFunction DBSql DBDelete");
                }

                return retVal;

            }

            #endregion

            #endregion

            #region DatabaseInfo Functions

            /// <summary>
            /// Retrieves the name of the primary key field for the given table
            /// </summary>
            /// <param name="tableName">The table</param>
            /// <returns></returns>
            public string GetKeyField(string tableName)
            {
                return ReadValue("(SELECT c.name AS COLUMN_NAME " +
                                "FROM sys.key_constraints AS k INNER JOIN " +
                                "sys.tables AS t ON t.object_id = k.parent_object_id INNER JOIN " +
                                "sys.index_columns AS ic ON ic.object_id = t.object_id AND " +
                                "ic.index_id = k.unique_index_id INNER JOIN " +
                                "sys.columns AS c ON c.object_id = t.object_id AND " +
                                "c.column_id = ic.column_id " +
                                "WHERE (t.name = N'" + tableName + "'))", "COLUMN_NAME").ToString();
            }

            /// <summary>
            /// Get All Column Names
            /// </summary>
            /// <param name="table">The table to query</param>
            /// <returns></returns>
            public List<string> DGetColumnNames(string table)
            {
                List<string> columnNames = new List<string>();
                using (connection)
                {
                    if (connection.State != ConnectionState.Open) connection.Open();

                    // Define the command 
                    using (SqlCommand cmd = new SqlCommand())
                    {
                        cmd.Connection = connection;
                        cmd.CommandType = CommandType.Text;
                        cmd.CommandText = "SELECT TOP 0 [" + table + "].* FROM [" + table + "] WHERE 1 = 2";

                        using (var reader = cmd.ExecuteReader())
                        {
                            // This will return false - we don't care, we just want to make sure the schema table is there.
                            reader.Read();

                            var schemaTable = reader.GetSchemaTable();
                            foreach (DataRow column in schemaTable.Rows)
                            {
                                columnNames.Add(column["ColumnName"].ToString());
                            }
                        }
                    }
                }
                return columnNames;
            }

            #endregion

            #region Direct Access Functions

            /// <summary>
            /// Execute a SELECT query directly on the server
            /// </summary>
            /// <param name="commandText">The SELECT statement text</param>
            /// <returns></returns>
            public List<string> ExecuteReaderQuery(string commandText)
            {
                List<string> results = new List<string>();
                SqlCommand command;
                SqlDataReader reader;

                try
                {
                    command = new SqlCommand(commandText, connection, transaction);
                    reader = command.ExecuteReader(CommandBehavior.CloseConnection);

                    while (reader.Read())
                    {
                        Dictionary<string, string> resLine =
                            new Dictionary<string, string>(reader.FieldCount);

                        for (int i = 0; i < reader.FieldCount; i++)
                            resLine.Add(reader.GetName(i), reader[i].ToString());

                        results = new List<string>(resLine.Values);
                    }


                    reader.Close();
                }
                catch (SqlException se)
                {
                    Mailer.ErrNotify(se, "DBFunction DBSql ExecuteReaderQuery");
                }

                return results;

            }

            /// <summary>
            /// Execute a non-reader (update, insert, delete) query directly on the server
            /// </summary>
            /// <param name="commandText">The SQL statement text</param>
            /// <returns></returns>
            public int ExecuteNonReader(string commandText)
            {
                int retVal = -1;

                if (String.IsNullOrEmpty(commandText))
                    return retVal;

                SqlCommand command;

                try
                {
                    command = new SqlCommand(commandText, connection, transaction);
                    retVal = command.ExecuteNonQuery();
                }
                catch (SqlException se)
                {
                    Mailer.ErrNotify(se, "DBFunction DBSql ExecuteNonReader");
                }
                finally
                {
                    connection.Dispose();
                }

                return retVal;

            }

            #endregion

            #region Image Database Manipulation

            /// <summary>
            /// Insert Update Image
            /// <para>connects to SQL server and executes SQL statement and update/insert binary image data in database table</para>
            /// </summary>
            /// <param name="SQL"></param>
            /// <param name="Image"></param>
            /// <param name="ImageFieldName"></param>
            /// <param name="ImageFormat"></param>
            /// <returns>returns row id</returns>
            public int InsertUpdateImage(string SQL, Image Image, string ImageFieldName, ImageFormat ImageFormat)
            {
                int SqlRetVal = 0;

                try
                {
                    SqlCommand command = new SqlCommand(SQL, connection, transaction);

                    // Convert image to memory stream
                    System.IO.MemoryStream MemoryStream = new System.IO.MemoryStream();
                    Image.Save(MemoryStream, ImageFormat);

                    // Add image as SQL parameter
                    SqlParameter parameter = new SqlParameter("@" + ImageFieldName, SqlDbType.Image);

                    parameter.Value = MemoryStream.ToArray();
                    command.Parameters.Add(parameter);

                    // Executes a Transact-SQL statement against the connection and returns the number of rows affected.
                    SqlRetVal = command.ExecuteNonQuery();

                    // Dispose command
                    command.Dispose();
                    command = null;
                }
                catch (Exception Exception)
                {
                    Mailer.ErrNotify(Exception, "DBFunction DBSql InsertUpdateImage");
                    return 0;
                }

                return SqlRetVal;
            }

            /// <summary>
            /// Scalar To Image
            /// <para>Connects to SQL server and executes SQL statement and returns the binary image data from database</para>
            /// </summary>
            /// <param name="SQL"></param>
            /// <returns></returns>
            public Image GetImageFromDB(string SQL)
            {
                object SqlRetVal = null;
                Image Image = null;

                try
                {
                    SqlCommand command = new SqlCommand(SQL, connection, transaction);

                    SqlRetVal = command.ExecuteScalar();

                    // Dispose command
                    command.Dispose();
                    command = null;
                }
                catch (Exception Exception)
                {
                    Mailer.ErrNotify(Exception, "DBFunction DBSql GetImageFromDB");

                    return null;
                }

                // convert object to image
                try
                {
                    // get image from object
                    byte[] ImageData = new byte[0];
                    ImageData = (byte[])SqlRetVal;
                    System.IO.MemoryStream MemoryStream = new System.IO.MemoryStream(ImageData);
                    Image = Image.FromStream(MemoryStream);
                }
                catch (Exception Exception)
                {
                    Mailer.ErrNotify(Exception, "DBFunction DBSql GetImageFromDB");

                    return null;
                }

                return Image;
            }

            #endregion

            #endregion

            /// <summary>
            /// 
            /// </summary>
            public class TryParseValue
            {

                private static bool CellNameDoesNotExists(DataRow dr, string ColumnName)
                {
                    if (ColumnName.Trim().Length > 0 && dr.Table.Columns.Contains(ColumnName) && dr.Table.Columns[ColumnName] != null)
                    {
                        return true;
                    }
                    else
                    {
                        string MessageBody = "The Column Name '<b>" + ColumnName + "</b>' Does Not Exists in table name '<b>" + dr.Table.TableName + "</b>'.";
                           // + CommonLib.ServiceModelStatic.New().CurrentExecutingMethodName_GetStringOfLastFrames(15);
                        string Subject = "The Column Name '" + ColumnName + "' is missing";
                        Mailer.SendNotifation(MessageBody, Subject);
                        return false;
                    }
                }
                private static bool CellNameDoesNotExists(XmlNode Node, string CellName, out string Value)
                {
                    Value = "";
                    if (CellName.Trim().Length > 0 && Node.Attributes[CellName] != null && !string.IsNullOrEmpty(Node.Attributes[CellName].Value))
                    {
                        Value = Node.Attributes[CellName].Value;
                        return true;
                    }
                    else
                    {
                        string MessageBody = "The Attribute Name '<b>" + CellName + "</b>' Does Not Exists in XmlNode.";
                          //  + CommonLib.ServiceModelStatic.New().CurrentExecutingMethodName_GetStringOfLastFrames(15);
                        string Subject = "The Attribute Name '" + CellName + "' is missing";
                        string MessageErrorHeader = "The Attribute Name '" + CellName + "' is missing";
                        Mailer.SendNotifation(MessageBody, Subject);
                        return false;
                    }
                }

                #region int
                /// <summary>
                /// Holds 32-bit signed integers. 
                /// The smallest possible value of an int variable is -2,147,483,648; 
                /// the largest possible value is 2,147,483,647. 
                /// </summary>
                /// <value>int: (int)1.1</value>
                public static int TryParse(object Value_TryParse, int Value_ReturnOnError)
                {
                    int result = Value_ReturnOnError;
                    try
                    {
                        int.TryParse(Value_TryParse.ToString(), out result);

                    }
                    catch
                    {
                        result = Value_ReturnOnError;
                    }
                    return result;
                }
                /// <summary>
                /// Holds 32-bit signed integers. 
                /// The smallest possible value of an int variable is -2,147,483,648; 
                /// the largest possible value is 2,147,483,647. 
                /// </summary>
                /// <value>int: (int)1.1</value>
                public static int TryParse(DataRow dr, string Name, int OnErorrReturnValue)
                {
                    if (CellNameDoesNotExists(dr, Name))
                    {
                        foreach (DataColumn dc in dr.Table.Columns)
                        {
                            if (dc.ColumnName.ToLower() == Name.ToLower())
                            {
                                return TryParse(dr[Name], OnErorrReturnValue);
                            }
                        }
                    }
                    return OnErorrReturnValue;
                }
                public static int TryParse(XmlNode Node, string Name, int OnErorrReturnValue)
                {
                    string Value = "";
                    if (CellNameDoesNotExists(Node, Name, out Value))
                    {

                        return TryParse(Value, OnErorrReturnValue);

                    }
                    return OnErorrReturnValue;
                }
                #endregion

                #region float
                /// <summary>
                ///Holds a 32-bit signed floating-point value. 
                ///The smallest possible value of a float type is approximately 1.5 times 10 to the 45th power; 
                ///the largest possible value is approximately 3.4 times 10 to the 38th power. 
                /// </summary>
                /// <value>float: (float)3.5 or 3.5F or 3.5f</value>
                public static float TryParse(object Value_TryParse, float Value_ReturnOnError)
                {
                    float result = Value_ReturnOnError;
                    try
                    {
                        float.TryParse(Value_TryParse.ToString(), out result);
                    }
                    catch
                    {
                        result = Value_ReturnOnError;
                    }
                    return result;
                }
                /// <summary>
                ///Holds a 32-bit signed floating-point value. 
                ///The smallest possible value of a float type is approximately 1.5 times 10 to the 45th power; 
                ///the largest possible value is approximately 3.4 times 10 to the 38th power. 
                /// </summary>
                /// <value>float: (float)3.5 or 3.5F or 3.5f</value>
                public static float TryParse(DataRow dr, string Name, float OnErorrReturnValue)
                {
                    if (CellNameDoesNotExists(dr, Name))
                    {
                        foreach (DataColumn dc in dr.Table.Columns)
                        {
                            if (dc.ColumnName.ToLower() == Name.ToLower())
                            {
                                return TryParse(dr[Name], OnErorrReturnValue);
                            }
                        }
                    }
                    return OnErorrReturnValue;
                }
                public static float TryParse(XmlNode Node, string Name, float OnErorrReturnValue)
                {
                    string Value = "";
                    if (CellNameDoesNotExists(Node, Name, out Value))
                    {

                        return TryParse(Value, OnErorrReturnValue);

                    }
                    return OnErorrReturnValue;
                }
                #endregion

                #region double
                ///<summary>
                ///Holds a 64-bit signed floating-point value. 
                ///The smallest possible value of a double is approximately 5 times 10 to the 324th; 
                ///the largest possible value is approximately 1.7 times 10 to the 308th. 
                ///</summary>
                /// <value>double: (double)3.5 or 3.5D or 3.5E+3</value>
                public static double TryParse(object Value_TryParse, double Value_ReturnOnError)
                {
                    double result = Value_ReturnOnError;
                    try
                    {
                        double.TryParse(Value_TryParse.ToString(), out result);
                    }
                    catch
                    {
                        result = Value_ReturnOnError;
                    }
                    return result;
                }
                ///<summary>
                ///Holds a 64-bit signed floating-point value. 
                ///The smallest possible value of a double is approximately 5 times 10 to the 324th; 
                ///the largest possible value is approximately 1.7 times 10 to the 308th. 
                ///</summary>
                /// <value>double: (double)3.5 or 3.5D or 3.5E+3</value>
                public static double TryParse(DataRow dr, string Name, double OnErorrReturnValue)
                {
                    if (CellNameDoesNotExists(dr, Name))
                    {
                        foreach (DataColumn dc in dr.Table.Columns)
                        {
                            if (dc.ColumnName.ToLower() == Name.ToLower())
                            {
                                return TryParse(dr[Name], OnErorrReturnValue);
                            }
                        }
                    }
                    return OnErorrReturnValue;
                }
                public static double TryParse(XmlNode Node, string Name, double OnErorrReturnValue)
                {
                    string Value = "";
                    if (CellNameDoesNotExists(Node, Name, out Value))
                    {

                        return TryParse(Value, OnErorrReturnValue);

                    }
                    return OnErorrReturnValue;
                }
                #endregion

                #region decimal
                /// <summary>
                ///Holds a 128-bit signed floating-point value. 
                ///Variables of type decimal are good for financial calculations. 
                ///The smallest possible value of a decimal type is approximately 1 times 10 to the 28th power; 
                ///the largest possible value is approximately 7.9 times 10 to the 28th power. 
                /// </summary>
                /// <value>decimal : (decimal)3.5 or 3.5m</value>
                public static decimal TryParse(object Value_TryParse, decimal Value_ReturnOnError)
                {
                    decimal result = Value_ReturnOnError;
                    try
                    {
                        decimal.TryParse(Value_TryParse.ToString(), out result);
                    }
                    catch
                    {
                        result = Value_ReturnOnError;
                    }
                    return result;
                }
                /// <summary>
                ///Holds a 128-bit signed floating-point value. 
                ///Variables of type decimal are good for financial calculations. 
                ///The smallest possible value of a decimal type is approximately 1 times 10 to the 28th power; 
                ///the largest possible value is approximately 7.9 times 10 to the 28th power. 
                /// </summary>
                /// <value>decimal : (decimal)3.5 or 3.5m</value>
                public static decimal TryParse(DataRow dr, string Name, decimal OnErorrReturnValue)
                {
                    if (CellNameDoesNotExists(dr, Name))
                    {
                        foreach (DataColumn dc in dr.Table.Columns)
                        {
                            if (dc.ColumnName.ToLower() == Name.ToLower())
                            {
                                return TryParse(dr[Name], OnErorrReturnValue);
                            }
                        }
                    }
                    return OnErorrReturnValue;
                }
                public static decimal TryParse(XmlNode Node, string Name, decimal OnErorrReturnValue)
                {
                    string Value = "";
                    if (CellNameDoesNotExists(Node, Name, out Value))
                    {

                        return TryParse(Value, OnErorrReturnValue);

                    }
                    return OnErorrReturnValue;
                }
                #endregion

                #region bool
                /// <summary>
                ///Holds one of two possible values, true or false
                /// </summary>
                /// <value>int: 0</value>
                public static bool TryParse(object Value_TryParse, bool Value_ReturnOnError)
                {
                    bool result = Value_ReturnOnError;
                    try
                    {
                        bool.TryParse(Value_TryParse.ToString(), out result);
                    }
                    catch
                    {
                        result = Value_ReturnOnError;
                    }
                    return result;
                }
                /// <summary>
                ///Holds one of two possible values, true or false
                /// </summary>
                /// <value>int: 0</value>
                public static bool TryParse(DataRow dr, string Name, bool OnErorrReturnValue)
                {
                    if (CellNameDoesNotExists(dr, Name))
                    {
                        //foreach (DataColumn dc in dr.Table.Columns)
                        //{
                        //    if (dc.ColumnName.ToLower() == Name.ToLower())
                        //    {
                        return TryParse(dr[Name], OnErorrReturnValue);
                        //    }
                        //}
                    }
                    return OnErorrReturnValue;
                }
                public static bool TryParse(XmlNode Node, string Name, bool OnErorrReturnValue)
                {
                    string Value = "";
                    if (CellNameDoesNotExists(Node, Name, out Value))
                    {

                        return TryParse(Value, OnErorrReturnValue);

                    }
                    return OnErorrReturnValue;
                }
                #endregion

                #region Guid
                public static System.Guid TryParse(object Value_TryParse, System.Guid Value_ReturnOnError)
                {
                    System.Guid result = Value_ReturnOnError;
                    try
                    {
                        result = new System.Guid(Value_TryParse.ToString());
                    }
                    catch
                    {
                        result = Value_ReturnOnError;
                    }
                    return result;
                }
                public static System.Guid TryParse(DataRow dr, string Name, System.Guid OnErorrReturnValue)
                {
                    if (CellNameDoesNotExists(dr, Name))
                    {
                        //foreach (DataColumn dc in dr.Table.Columns)
                        //{
                        //    if (dc.ColumnName.ToLower() == Name.ToLower())
                        //    {
                        return TryParse(dr[Name], OnErorrReturnValue);
                        //    }
                        //}
                    }
                    return OnErorrReturnValue;
                }
                public static System.Guid TryParse(XmlNode Node, string Name, System.Guid OnErorrReturnValue)
                {
                    string Value = "";
                    if (CellNameDoesNotExists(Node, Name, out Value))
                    {

                        return TryParse(Value, OnErorrReturnValue);

                    }
                    return OnErorrReturnValue;
                }
                #endregion

                #region string
                public static string TryParse(object Value_TryParse, string Value_ReturnOnError)
                {
                    string result = Value_ReturnOnError;
                    try
                    {
                        result = Value_TryParse.ToString();
                    }
                    catch
                    {
                        result = Value_ReturnOnError;
                    }
                    return result;
                }
                public static string TryParse(DataRow dr, string Name, string OnErorrReturnValue)
                {
                    if (CellNameDoesNotExists(dr, Name))
                    {
                        //foreach (DataColumn dc in dr.Table.Columns)
                        //{
                        //    if (dc.ColumnName.ToLower() == Name.ToLower())
                        //    {
                        return TryParse(dr[Name], OnErorrReturnValue);
                        //    }
                        //}
                    }
                    return OnErorrReturnValue;
                }
                public static string TryParse(XmlNode Node, string Name, string OnErorrReturnValue)
                {
                    string Value = "";
                    if (CellNameDoesNotExists(Node, Name, out Value))
                    {

                        return TryParse(Value, OnErorrReturnValue);

                    }
                    return OnErorrReturnValue;
                }
                #endregion

                #region System.DateTime
                public static System.DateTime TryParse(object Value_TryParse, System.DateTime Value_ReturnOnError)
                {
                    System.DateTime result = Value_ReturnOnError;
                    try
                    {
                        System.DateTime.TryParse(Value_TryParse.ToString(), out result);
                    }
                    catch
                    {
                        result = Value_ReturnOnError;
                    }
                    return result;
                }
                public static System.DateTime TryParse(DataRow dr, string Name, System.DateTime OnErorrReturnValue)
                {
                    if (CellNameDoesNotExists(dr, Name))
                    {
                        //foreach (DataColumn dc in dr.Table.Columns)
                        //{
                        //    if (dc.ColumnName.ToLower() == Name.ToLower())
                        //    {
                        return TryParse(dr[Name], OnErorrReturnValue);
                        //    }
                        //}
                    }
                    return OnErorrReturnValue;
                }
                public static System.DateTime TryParse(XmlNode Node, string Name, System.DateTime OnErorrReturnValue)
                {
                    string Value = "";
                    if (CellNameDoesNotExists(Node, Name, out Value))
                    {

                        return TryParse(Value, OnErorrReturnValue);

                    }
                    return OnErorrReturnValue;
                }
                #endregion

                #region XmlDocument
                public static XmlDocument TryParse(object Value_TryParse, XmlDocument Value_ReturnOnError)
                {
                    try
                    {
                        XmlDocument xmlDoc = new XmlDocument();
                        xmlDoc.LoadXml(Value_TryParse.ToString());
                        return xmlDoc;
                    }
                    catch
                    {
                        return Value_ReturnOnError;
                    }
                }
                public static XmlDocument TryParse(DataRow dr, string Name, XmlDocument OnErorrReturnValue)
                {
                    if (CellNameDoesNotExists(dr, Name))
                    {
                        //foreach (DataColumn dc in dr.Table.Columns)
                        //{
                        //    if (dc.ColumnName.ToLower() == Name.ToLower())
                        //    {
                        return TryParse(dr[Name], OnErorrReturnValue);
                        //    }
                        //}
                    }
                    return OnErorrReturnValue;
                }
                public static XmlDocument TryParse(XmlNode Node, string Name, XmlDocument OnErorrReturnValue)
                {
                    string Value = "";
                    if (CellNameDoesNotExists(Node, Name, out Value))
                    {

                        return TryParse(Value, OnErorrReturnValue);

                    }
                    return OnErorrReturnValue;
                }
                #endregion

            }

        }

        /// <summary>
        /// Access Database Functions
        /// </summary>
        public class DBOleDb
        {
            #region Public Properties

            /// <summary>
            /// Connection Name Set in Web.Config
            /// </summary>
            public string ConnName { get; set; }

            #endregion

            #region Public Support Methods

            /// <summary>
            /// OleDb Database Functions Constructor
            /// </summary>
            /// <param name="ConnectionName"></param>
            public DBOleDb(string ConnectionName)
            {
                ConnName = ConnectionName;
            }

            #endregion

            #region Private Read Methods

            /// <summary>
            /// Retrieves the connection string from the calling application .config file.
            /// The connection string must be in an application setting called 
            /// "ConnectionString".
            /// </summary>
            /// <returns></returns>
            private string GetConnectionString()
            {
                string connectionString = ConfigurationManager.ConnectionStrings[ConnName].ConnectionString;
                return connectionString;
            }

            #endregion

            #region Public Methods

            /// <summary>
            /// Delete a Row from Table
            /// </summary>
            /// <param name="targetTable"></param>
            /// <param name="idColumn"></param>
            /// <param name="value"></param>
            /// <returns></returns>
            public bool DeleteRow(string targetTable, string idColumn, string value)
            {
                try
                {
                    OleDbConnection con = new OleDbConnection(GetConnectionString());
                    string query = "SELECT * FROM " + targetTable + " WHERE " + idColumn + " = '" + value + "'";

                    OleDbDataAdapter da = GetDataAdapter(con, query);

                    //command builder required for delete.
                    OleDbCommandBuilder cbCommandBuilder = new OleDbCommandBuilder(da);

                    DataTable dtable = GetDataTable(da);
                    dtable.Rows[0].Delete();
                    da.Update(dtable);
                    con.Close();
                    return true;
                }
                catch (Exception ex)
                {
                    Mailer.ErrNotify(ex, "DBFunctions DBOleDb DeleteRow");
                    return false;
                }
            }

            /// <summary>
            /// populate data table
            /// </summary>
            /// <param name="query"></param>
            /// <returns></returns>
            public DataTable ExecQuery(string query)
            {
                DataTable dt = new DataTable();
                try
                {
                    OleDbConnection con = new OleDbConnection(GetConnectionString());
                    OleDbDataAdapter da = new OleDbDataAdapter(query, con);
                    OleDbCommandBuilder cb = new OleDbCommandBuilder(da);
                    da.Fill(dt);
                    return dt;
                }
                catch (Exception ex)
                {
                    Mailer.ErrNotify(ex, "DBFunctions DBOleDb ExecQuery");
                    return null;
                }
            }

            /// <summary>
            /// used internally
            /// </summary>
            /// <param name="myConnection"></param>
            /// <param name="query"></param>
            /// <returns></returns>
            private OleDbDataAdapter GetDataAdapter(OleDbConnection myConnection, string query)
            {
                try
                {
                    OleDbDataAdapter da = new OleDbDataAdapter(query, myConnection);
                    return da;
                }
                catch (Exception ex)
                {
                    Mailer.ErrNotify(ex, "DBFunctions DBOleDb GetDataAdapter");
                    return null;
                }
            }

            /// <summary>
            /// used internally
            /// </summary>
            /// <param name="daDataAdapter"></param>
            /// <returns></returns>
            private DataTable GetDataTable(OleDbDataAdapter daDataAdapter)
            {
                try
                {
                    DataTable myDataTable = new DataTable();
                    daDataAdapter.Fill(myDataTable);
                    return myDataTable;
                }
                catch (Exception ex)
                {
                    Mailer.ErrNotify(ex, "DBFunctions DBOleDb GetDataTable");
                    return null;
                }
            }

            /// <summary>
            /// updates database with data from dt
            /// </summary>
            /// <param name="daDataAdapter"></param>
            /// <param name="myDataTable"></param>
            /// <returns></returns>
            public bool UpdateTable(OleDbDataAdapter daDataAdapter, DataTable myDataTable)
            {
                try
                {
                    OleDbCommandBuilder cbCommandBuilder = new OleDbCommandBuilder(daDataAdapter);
                    daDataAdapter.Update(myDataTable);
                    return true;
                }
                catch (Exception ex)
                {
                    Mailer.ErrNotify(ex, "DBFunctions DBOleDb UpdateTable");
                    return false;
                }
            }

            #endregion
        }
    }

    /// <summary>
    /// Database ParameterType
    /// </summary>
    public static class DBParameterType
    {
        #region Public Methods

        /// <summary>
        /// Type to Database Type
        /// </summary>
        /// <param name="type"></param>
        /// <returns>returns DbType</returns>
        public static DbType ToDbType(Type type)
        {
            if (type == typeof(string)) return DbType.String;
            if (type == typeof(UInt64)) return DbType.UInt64;
            if (type == typeof(Int64)) return DbType.Int64;
            if (type == typeof(Int32)) return DbType.Int32;
            if (type == typeof(UInt32)) return DbType.UInt32;
            if (type == typeof(DateTime)) return DbType.DateTime;
            if (type == typeof(UInt16)) return DbType.UInt16;
            if (type == typeof(Int16)) return DbType.Int16;
            if (type == typeof(SByte)) return DbType.SByte;
            if (type == typeof(Object)) return DbType.Object;
            if (type == typeof(double)) return DbType.Double;
            if (type == typeof(byte[])) return DbType.Binary;
            if (type == typeof(decimal)) return DbType.Decimal;
            if (type == typeof(Guid)) return DbType.Guid;
            if (type == typeof(Boolean)) return DbType.Boolean;

            Mailer.ErrNotify(string.Format("Unknown conversion for type '{0}'", type.FullName), "DBParameterType");

            return new DbType();
        }

        /// <summary>
        /// Get the native type based on the database type
        /// </summary>
        /// <param name="dbType">The database type to convert</param>
        /// <returns>The equivalent managed type, otherwise the DBNull type</returns>
        public static Type ToManagedType(DbType dbType)
        {
            Type result = typeof(DBNull);

            switch (dbType)
            {
                case DbType.String:
                    result = typeof(string);
                    break;

                case DbType.UInt64:
                    result = typeof(UInt64);
                    break;

                case DbType.Int64:
                    result = typeof(Int64);
                    break;

                case DbType.Int32:
                    result = typeof(Int32);
                    break;

                case DbType.UInt32:
                    result = typeof(UInt32);
                    break;

                case DbType.Single:
                    result = typeof(float);
                    break;

                case DbType.Date:
                    result = typeof(DateTime);
                    break;

                case DbType.DateTime:
                    result = typeof(DateTime);
                    break;

                case DbType.Time:
                    result = typeof(DateTime);
                    break;

                case DbType.StringFixedLength:
                    result = typeof(string);
                    break;

                case DbType.UInt16:
                    result = typeof(UInt16);
                    break;

                case DbType.Int16:
                    result = typeof(Int16);
                    break;

                case DbType.SByte:
                    result = typeof(byte);
                    break;

                case DbType.Object:
                    result = typeof(object);
                    break;

                case DbType.AnsiString:
                    result = typeof(string);
                    break;

                case DbType.AnsiStringFixedLength:
                    result = typeof(string);
                    break;

                case DbType.VarNumeric:
                    result = typeof(decimal);
                    break;

                case DbType.Currency:
                    result = typeof(double);
                    break;

                case DbType.Binary:
                    result = typeof(byte[]);
                    break;

                case DbType.Decimal:
                    result = typeof(decimal);
                    break;

                case DbType.Double:
                    result = typeof(Double);
                    break;

                case DbType.Guid:
                    result = typeof(Guid);
                    break;

                case DbType.Boolean:
                    result = typeof(bool);
                    break;
            }

            return result;
        }

        #endregion
    }

    /// <summary>
    /// SqlHelperParameterCache provides functions to leverage a static cache of procedure parameters, and the
    /// ability to discover parameters for stored procedures at run-time.
    /// </summary>
    public sealed class SqlHelperParameterCache
    {
        #region private methods, variables, and constructors

        //Since this class provides only static methods, make the default constructor private to prevent 
        //instances from being created with "new SqlHelperParameterCache()"
        private SqlHelperParameterCache() { }

        private static Hashtable paramCache = Hashtable.Synchronized(new Hashtable());

        /// <summary>
        /// Resolve at run time the appropriate set of SqlParameters for a stored procedure
        /// </summary>
        /// <param name="connection">A valid SqlConnection object</param>
        /// <param name="spName">The name of the stored procedure</param>
        /// <param name="includeReturnValueParameter">Whether or not to include their return value parameter</param>
        /// <returns>The parameter array discovered.</returns>
        private static SqlParameter[] DiscoverSpParameterSet(SqlConnection connection, string spName, bool includeReturnValueParameter)
        {
            if (connection == null) throw new ArgumentNullException("connection");
            if (spName == null || spName.Length == 0) throw new ArgumentNullException("spName");

            SqlCommand cmd = new SqlCommand(spName, connection);
            cmd.CommandType = CommandType.StoredProcedure;

            connection.Open();
            SqlCommandBuilder.DeriveParameters(cmd);
            connection.Close();

            if (!includeReturnValueParameter)
            {
                cmd.Parameters.RemoveAt(0);
            }

            SqlParameter[] discoveredParameters = new SqlParameter[cmd.Parameters.Count];

            cmd.Parameters.CopyTo(discoveredParameters, 0);

            // Init the parameters with a DBNull value
            foreach (SqlParameter discoveredParameter in discoveredParameters)
            {
                discoveredParameter.Value = DBNull.Value;
            }
            return discoveredParameters;
        }

        /// <summary>
        /// Deep copy of cached SqlParameter array
        /// </summary>
        /// <param name="originalParameters"></param>
        /// <returns></returns>
        private static SqlParameter[] CloneParameters(SqlParameter[] originalParameters)
        {
            SqlParameter[] clonedParameters = new SqlParameter[originalParameters.Length];

            for (int i = 0, j = originalParameters.Length; i < j; i++)
            {
                clonedParameters[i] = (SqlParameter)((ICloneable)originalParameters[i]).Clone();
            }

            return clonedParameters;
        }

        #endregion private methods, variables, and constructors

        #region caching functions

        /// <summary>
        /// Add parameter array to the cache
        /// </summary>
        /// <param name="connectionString">A valid connection string for a SqlConnection</param>
        /// <param name="commandText">The stored procedure name or T-SQL command</param>
        /// <param name="commandParameters">An array of SqlParamters to be cached</param>
        public static void CacheParameterSet(string connectionString, string commandText, params SqlParameter[] commandParameters)
        {
            if (connectionString == null || connectionString.Length == 0) throw new ArgumentNullException("connectionString");
            if (commandText == null || commandText.Length == 0) throw new ArgumentNullException("commandText");

            string hashKey = connectionString + ":" + commandText;

            paramCache[hashKey] = commandParameters;
        }

        /// <summary>
        /// Retrieve a parameter array from the cache
        /// </summary>
        /// <param name="connectionString">A valid connection string for a SqlConnection</param>
        /// <param name="commandText">The stored procedure name or T-SQL command</param>
        /// <returns>An array of SqlParamters</returns>
        public static SqlParameter[] GetCachedParameterSet(string connectionString, string commandText)
        {
            if (connectionString == null || connectionString.Length == 0) throw new ArgumentNullException("connectionString");
            if (commandText == null || commandText.Length == 0) throw new ArgumentNullException("commandText");

            string hashKey = connectionString + ":" + commandText;

            SqlParameter[] cachedParameters = paramCache[hashKey] as SqlParameter[];
            if (cachedParameters == null)
            {
                return null;
            }
            else
            {
                return CloneParameters(cachedParameters);
            }
        }

        #endregion caching functions

        #region Parameter Discovery Functions

        /// <summary>
        /// Retrieves the set of SqlParameters appropriate for the stored procedure
        /// </summary>
        /// <remarks>
        /// This method will query the database for this information, and then store it in a cache for future requests.
        /// </remarks>
        /// <param name="connectionString">A valid connection string for a SqlConnection</param>
        /// <param name="spName">The name of the stored procedure</param>
        /// <returns>An array of SqlParameters</returns>
        public static SqlParameter[] GetSpParameterSet(string connectionString, string spName)
        {
            return GetSpParameterSet(connectionString, spName, false);
        }

        /// <summary>
        /// Retrieves the set of SqlParameters appropriate for the stored procedure
        /// </summary>
        /// <remarks>
        /// This method will query the database for this information, and then store it in a cache for future requests.
        /// </remarks>
        /// <param name="connectionString">A valid connection string for a SqlConnection</param>
        /// <param name="spName">The name of the stored procedure</param>
        /// <param name="includeReturnValueParameter">A bool value indicating whether the return value parameter should be included in the results</param>
        /// <returns>An array of SqlParameters</returns>
        public static SqlParameter[] GetSpParameterSet(string connectionString, string spName, bool includeReturnValueParameter)
        {
            if (connectionString == null || connectionString.Length == 0) throw new ArgumentNullException("connectionString");
            if (spName == null || spName.Length == 0) throw new ArgumentNullException("spName");

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                return GetSpParameterSetInternal(connection, spName, includeReturnValueParameter);
            }
        }

        /// <summary>
        /// Retrieves the set of SqlParameters appropriate for the stored procedure
        /// </summary>
        /// <remarks>
        /// This method will query the database for this information, and then store it in a cache for future requests.
        /// </remarks>
        /// <param name="connection">A valid SqlConnection object</param>
        /// <param name="spName">The name of the stored procedure</param>
        /// <returns>An array of SqlParameters</returns>
        internal static SqlParameter[] GetSpParameterSet(SqlConnection connection, string spName)
        {
            return GetSpParameterSet(connection, spName, false);
        }

        /// <summary>
        /// Retrieves the set of SqlParameters appropriate for the stored procedure
        /// </summary>
        /// <remarks>
        /// This method will query the database for this information, and then store it in a cache for future requests.
        /// </remarks>
        /// <param name="connection">A valid SqlConnection object</param>
        /// <param name="spName">The name of the stored procedure</param>
        /// <param name="includeReturnValueParameter">A bool value indicating whether the return value parameter should be included in the results</param>
        /// <returns>An array of SqlParameters</returns>
        internal static SqlParameter[] GetSpParameterSet(SqlConnection connection, string spName, bool includeReturnValueParameter)
        {
            if (connection == null) throw new ArgumentNullException("connection");
            using (SqlConnection clonedConnection = (SqlConnection)((ICloneable)connection).Clone())
            {
                return GetSpParameterSetInternal(clonedConnection, spName, includeReturnValueParameter);
            }
        }

        /// <summary>
        /// Retrieves the set of SqlParameters appropriate for the stored procedure
        /// </summary>
        /// <param name="connection">A valid SqlConnection object</param>
        /// <param name="spName">The name of the stored procedure</param>
        /// <param name="includeReturnValueParameter">A bool value indicating whether the return value parameter should be included in the results</param>
        /// <returns>An array of SqlParameters</returns>
        private static SqlParameter[] GetSpParameterSetInternal(SqlConnection connection, string spName, bool includeReturnValueParameter)
        {
            if (connection == null) throw new ArgumentNullException("connection");
            if (spName == null || spName.Length == 0) throw new ArgumentNullException("spName");

            string hashKey = connection.ConnectionString + ":" + spName + (includeReturnValueParameter ? ":include ReturnValue Parameter" : "");

            SqlParameter[] cachedParameters;

            cachedParameters = paramCache[hashKey] as SqlParameter[];
            if (cachedParameters == null)
            {
                SqlParameter[] spParameters = DiscoverSpParameterSet(connection, spName, includeReturnValueParameter);
                paramCache[hashKey] = spParameters;
                cachedParameters = spParameters;
            }

            return CloneParameters(cachedParameters);
        }

        #endregion Parameter Discovery Functions

    }
}
