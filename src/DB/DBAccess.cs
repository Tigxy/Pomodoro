using Dapper;
using Dapper.FluentMap;
using Pomodoro.DB;
using Pomodoro.DB.Handlers;
using Pomodoro.Models;
using System;
using System.CodeDom;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Windows.Forms;

namespace Pomodoro
{
    /// <summary>
    /// Provides functions to store and retrieve data from a database
    /// Note: This most likely needs some more love by more experienced SQL users
    /// </summary>
    public static class DBAccess
    {
        static DBAccess()
        {
            ConfigureDapper();
        }

        /// <summary>
        /// Loads the connections string to our database
        /// </summary>
        /// <returns>the connection string to our database</returns>
        public static string LoadConnectionString() => ConfigurationManager.ConnectionStrings["Default"].ConnectionString;

        /// <summary>
        /// Configers Dapper to use your conventions when handling the database 
        /// </summary>
        private static void ConfigureDapper()
        {
            FluentMapper.Initialize(config =>
            {
                config
                    .AddConvention<PropertyTransformConvention>()
                    .ForEntity<PeriodEntry>()
                    .ForEntity<PropertyValuePair>()
                    .ForEntity<Profile>();
            });

            SqlMapper.RemoveTypeMap(typeof(DateTime));
            SqlMapper.RemoveTypeMap(typeof(TimeSpan));
            SqlMapper.AddTypeHandler(new DateTimeHandler());
            SqlMapper.AddTypeHandler(new TimeSpanHandler());
        }

        /// <summary>
        /// Loads a single setting from our database
        /// </summary>
        /// <typeparam name="T">The type of the expected value</typeparam>
        /// <param name="property">The parameter name of the wanted value</param>
        /// <returns>The value of the parameter or its default value if not found</returns>
        public static T LoadParameter<T>(string property)
        {
            using (IDbConnection dbc = new SQLiteConnection(LoadConnectionString()))
            {
                var cmd = $"SELECT * FROM AppSetting WHERE property = '{property}';";

                try
                {
                    PropertyValuePair pair = dbc.QuerySingle<PropertyValuePair>(cmd);
                    if (pair == null || pair.Value == null)
                        return default;

                    return pair.Value.CustomConverter<T>();
                }
                catch (Exception e)
                {
                    System.Diagnostics.Debug.Print(e.ToString());
                }
            }

            return default;
        }

        /// <summary>
        /// Stores a single parameter in our database
        /// </summary>
        /// <typeparam name="T">The type the parameters value is expected to have</typeparam>
        /// <param name="property">The property name</param>
        /// <param name="value">The value of the property</param>
        public static void SaveParameter<T>(string property, T value)
        {
            using (IDbConnection dbc = new SQLiteConnection(LoadConnectionString()))
            {
                try
                {
                    dbc.Execute($@"
                            INSERT INTO AppSetting (property, value) 
                            VALUES ('{property}', {value?.ToString() ?? "''"}) 
                            ON CONFLICT(property) DO UPDATE SET value={value?.ToString() ?? "''"};");
                }
                catch (Exception e)
                {
                    System.Diagnostics.Debug.Print(e.ToString());
                }
            }
        }

        /// <summary>
        /// Stores (logs) the specified <see cref="PeriodEntry"/>
        /// </summary>
        /// <param name="entry">The entry to store</param>
        public static void SavePeriodEntry(PeriodEntry entry)
        {
            using (IDbConnection dbc = new SQLiteConnection(LoadConnectionString()))
            {
                try
                {
                    dbc.Execute($@"
                            INSERT INTO PeriodEntry (start_time, duration, is_studying, is_paused) 
                            VALUES (@StartTime, @Duration, @IsStudying, @IsPaused); 
                            ", entry);
                }
                catch (Exception e)
                {
                    System.Diagnostics.Debug.Print(e.ToString());
                }
            }
        }

        /// <summary>
        /// Stores (logs) the specified collection of <see cref="PeriodEntry"/>/>
        /// </summary>
        /// <param name="entries">The entries to store</param>
        public static void SavePeriodEntry(IEnumerable<PeriodEntry> entries)
        {
            using (IDbConnection dbc = new SQLiteConnection(LoadConnectionString()))
            {
                try
                {
                    dbc.Execute($@"
                            INSERT INTO PeriodEntry (start_time, duration, is_studying, is_paused) 
                            VALUES (@StartTime, @Duration, @IsStudying, @IsPaused); 
                            ", entries);
                }
                catch (Exception e)
                {
                    System.Diagnostics.Debug.Print(e.ToString());
                }
            }
        }

        /// <summary>
        /// Retrieves all <see cref="PeriodEntry"/> up until the specified point in time
        /// </summary>
        /// <param name="earliest">The earliest date to retrieve the <see cref="PeriodEntry"/> for</param>
        /// <returns>All found and matching <see cref="PeriodEntry"/></returns>
        public static IEnumerable<PeriodEntry> LoadPeriodEntries(DateTime earliest)
        {
            return LoadPeriodEntries(earliest, DateTime.UtcNow);
        }

        /// <summary>
        /// Retrieves all <see cref="PeriodEntry"/> between the specified points in time
        /// </summary>
        /// <param name="earliest">The earliest date to retrieve the <see cref="PeriodEntry"/> for</param>
        /// <param name="latest">The latest date to retrieve the <see cref="PeriodEntry"/> for</param>
        /// <returns>All found and matching <see cref="PeriodEntry"/></returns>
        public static IEnumerable<PeriodEntry> LoadPeriodEntries(DateTime earliest, DateTime latest)
        {
            using (IDbConnection dbc = new SQLiteConnection(LoadConnectionString()))
            {
                var cmd = $"SELECT * FROM PeriodEntry " +
                    $"WHERE start_time >= '{GetIsoTimeString(earliest)}' AND start_time < '{GetIsoTimeString(latest)}';";

                try
                {
                    return dbc.Query<PeriodEntry>(cmd);
                }
                catch (Exception e)
                {
                    System.Diagnostics.Debug.Print(e.ToString());
                    return new List<PeriodEntry>();
                }
            }
        }

        /// <summary>
        /// Gets the specified profile from the database
        /// </summary>
        /// <param name="name">The name of the profile</param>
        /// <returns>The profile or its default in case of error</returns>
        public static Profile GetProfile(string name)
        {
            using (IDbConnection dbc = new SQLiteConnection(LoadConnectionString()))
            {
                var cmd = $"SELECT * FROM Profile WHERE name='{name}';";

                try
                {
                    return dbc.QuerySingle<Profile>(cmd);
                }
                catch (Exception e)
                {
                    System.Diagnostics.Debug.Print(e.ToString());
                    return default;
                }
            }
        }

        /// <summary>
        /// Gets the names of all the profile stored in the database
        /// </summary>
        /// <returns>The anmes</returns>
        public static IEnumerable<string> GetProfileNames()
        {
            using (IDbConnection dbc = new SQLiteConnection(LoadConnectionString()))
            {
                var cmd = $"SELECT name FROM Profile;";

                try
                {
                    return dbc.Query<string>(cmd);
                }
                catch (Exception e)
                {
                    System.Diagnostics.Debug.Print(e.ToString());
                    return default;
                }
            }
        }

        /// <summary>
        /// Stores the specified <see cref="Profile"/> in the database
        /// </summary>
        /// <param name="profile">The profile to store</param>
        /// <returns>Whether operation was successful</returns>
        public static bool SaveProfile(Profile profile)
        {
            using (IDbConnection dbc = new SQLiteConnection(LoadConnectionString()))
            {
                try
                {
                    dbc.Execute($@"
                            INSERT INTO Profile (name, duration_short_break, duration_long_break, duration_studying, cycles_until_long_break, auto_switch_mode_after_end) 
                            VALUES (@Name, @DurationShortBreak, @DurationLongBreak, @DurationStudying, @CyclesUntilLongBreak, @AutoSwitchModeAfterEnd)
                            ON CONFLICT(name) DO UPDATE SET 
                                duration_short_break={profile.DurationShortBreak}, 
                                duration_long_break={profile.DurationLongBreak}, 
                                duration_studying={profile.DurationStudying}, 
                                cycles_until_long_break={profile.CyclesUntilLongBreak}, 
                                auto_switch_mode_after_end={profile.AutoSwitchModeAfterEnd}; 
                            ", profile);
                }
                catch (Exception e)
                {
                    System.Diagnostics.Debug.Print(e.ToString());
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Converts a string value to the specified type. In case of error, 
        /// the default value of <typeparamref name="T"/> will be returned
        /// </summary>
        /// <typeparam name="T">The type to convert the string value to</typeparam>
        /// <param name="s">The string representation of <typeparamref name="T"/></param>
        /// <returns>The actual value of <typeparamref name="T"/></returns>
        private static T CustomConverter<T>(this string s)
        {
            bool success;

            // String conversion
            if (typeof(T).Equals(typeof(string)))
                return (T)Convert.ChangeType(s, TypeCode.String);

            // Double conversion
            else if (typeof(T).Equals(typeof(double)))
            {
                success = double.TryParse(s, out var v);
                return success ? (T)Convert.ChangeType(v, TypeCode.Double) : default;
            }

            // Integer conversion
            else if (typeof(T).Equals(typeof(int)))
            {
                success = int.TryParse(s, out var v);
                return success ? (T)Convert.ChangeType(v, TypeCode.Int32) : default;
            }

            return default;
        }

        /// <summary>
        /// Converts the datetime to ISO string representation (which is used in DB)
        /// </summary>
        /// <param name="dt">The <see cref="DateTime"/> value to convert</param>
        /// <returns>The ISO representation of the specified <see cref="DateTime"/> value</returns>
        private static string GetIsoTimeString(DateTime dt)
        {
            return dt
                .ToUniversalTime()
                .ToString("s", System.Globalization.CultureInfo.InvariantCulture);
        }
    }
}