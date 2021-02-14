using Dapper;
using Pomodoro.DB;
using System;
using System.CodeDom;
using System.Configuration;
using System.Data;
using System.Data.SQLite;
using System.Linq;

namespace Pomodoro
{
    public static class DBAccess
    {
        public static string LoadConnectionString() => ConfigurationManager.ConnectionStrings["Default"].ConnectionString;
        
        public static T LoadSetting<T>(string property)
        {
            using (IDbConnection dbc = new SQLiteConnection(LoadConnectionString()))
            {
                var cmd = $"SELECT * FROM AppSetting WHERE property = '{property}';";

                try
                {
                    return dbc
                        .Query<PropertyValuePair>(cmd)
                        .Select(pvp => CustomConverter<T>(pvp.Value))
                        .FirstOrDefault();
                }
                catch (System.Exception e)
                {
                    System.Diagnostics.Debug.Print(e.ToString());
                }
            }

            return default;
        }

        public static void SaveSetting<T>(string property, T value)
        {
            using (IDbConnection dbc = new SQLiteConnection(LoadConnectionString()))
            {
                try
                {
                    dbc.Execute($@"
                            INSERT INTO AppSetting (property, value) 
                            VALUES ('{property}', {value}) 
                            ON CONFLICT(property) DO UPDATE SET value={value};
                            ");
                }
                catch (System.Exception e)
                {
                    System.Diagnostics.Debug.Print(e.ToString());
                }
            }
        }

        public static void SavePeriodEntry(PeriodEntry entry)
        {
            using (IDbConnection dbc = new SQLiteConnection(LoadConnectionString()))
            {
                try
                {
                    dbc.Execute($@"
                            INSERT INTO PeriodEntry (start_time, duration, studying, paused) 
                            VALUES (@StartTime, @Duration, @IsStudying, @IsPaused); 
                            ", entry);
                }
                catch (System.Exception e)
                {
                    System.Diagnostics.Debug.Print(e.ToString());
                }
            }
        }

        private static T CustomConverter<T>(string str)
        {
            bool success;

            // String conversion
            if (typeof(T).Equals(typeof(string)))
                return (T)Convert.ChangeType(str, TypeCode.String);

            // Double conversion
            else if (typeof(T).Equals(typeof(double)))
            {
                success = double.TryParse(str, out var v);
                return success ? (T)Convert.ChangeType(v, TypeCode.Double) : default;
            }

            // Integer conversion
            else if (typeof(T).Equals(typeof(int)))
            {
                success = int.TryParse(str, out var v);
                return success ? (T)Convert.ChangeType(v, TypeCode.Int32) : default;
            }

            return default;
        }
    }
}