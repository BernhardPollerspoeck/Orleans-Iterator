﻿/*
WARNING: This code is taken from https://github.com/dotnet/orleans/blob/main/src/AdoNet/Shared/Storage/DbConnectionFactory.cs
and slightly modified to adhere to this projects conventions and respecting null context
 */

using System.Data.Common;
using System.Reflection;

namespace Orleans.Iterator.AdoNet.MainPackageCode;

internal static class DbConnectionFactory
{
    private static readonly Dictionary<string, List<Tuple<string, string>>> _providerFactoryTypeMap =
        new()
        {
                { AdoNetInvariants.InvariantNameSqlServer, new List<Tuple<string, string>>{ new("System.Data.SqlClient", "System.Data.SqlClient.SqlClientFactory") } },
                { AdoNetInvariants.InvariantNameMySql, new List<Tuple<string, string>>{ new("MySql.Data", "MySql.Data.MySqlClient.MySqlClientFactory") } },
                { AdoNetInvariants.InvariantNameOracleDatabase, new List<Tuple<string, string>>{ new("Oracle.ManagedDataAccess", "Oracle.ManagedDataAccess.Client.OracleClientFactory") } },
                { AdoNetInvariants.InvariantNamePostgreSql, new List<Tuple<string, string>>{ new("Npgsql", "Npgsql.NpgsqlFactory") } },
                { AdoNetInvariants.InvariantNameSqlLite, new List<Tuple<string, string>>{ new("Microsoft.Data.Sqlite", "Microsoft.Data.Sqlite.SqliteFactory") } },
                { AdoNetInvariants.InvariantNameSqlServerDotnetCore,new List<Tuple<string, string>>{ new("Microsoft.Data.SqlClient", "Microsoft.Data.SqlClient.SqlClientFactory") } },
                { AdoNetInvariants.InvariantNameMySqlConnector, new List<Tuple<string, string>>{ new("MySqlConnector", "MySqlConnector.MySqlConnectorFactory") , new("MySqlConnector", "MySql.Data.MySqlClient.MySqlClientFactory") } },
        };

    private static CachedFactory GetFactory(string invariantName)
    {
        if (string.IsNullOrWhiteSpace(invariantName))
        {
            throw new ArgumentNullException(nameof(invariantName));
        }

        if (!_providerFactoryTypeMap.TryGetValue(invariantName, out var providerFactoryDefinitions) || providerFactoryDefinitions.Count == 0)
            throw new InvalidOperationException($"Database provider factory with '{invariantName}' invariant name not supported.");

        var exceptions = new List<Exception>();
        foreach (var providerFactoryDefinition in providerFactoryDefinitions)
        {
            Assembly? asm = null;
            try
            {
                var asmName = new AssemblyName(providerFactoryDefinition.Item1);
                asm = Assembly.Load(asmName);
            }
            catch (Exception exc)
            {
                AddException(new InvalidOperationException($"Unable to find and/or load a candidate assembly '{providerFactoryDefinition.Item1}' for '{invariantName}' invariant name.", exc));
                continue;
            }

            if (asm == null)
            {
                AddException(new InvalidOperationException($"Can't find database provider factory with '{invariantName}' invariant name. Please make sure that your ADO.Net provider package library is deployed with your application."));
                continue;
            }

            var providerFactoryType = asm.GetType(providerFactoryDefinition.Item2);
            if (providerFactoryType == null)
            {
                AddException(new InvalidOperationException($"Unable to load type '{providerFactoryDefinition.Item2}' for '{invariantName}' invariant name."));
                continue;
            }

            var prop = providerFactoryType.GetFields().SingleOrDefault(p => string.Equals(p.Name, "Instance", StringComparison.OrdinalIgnoreCase) && p.IsStatic);
            if (prop == null)
            {
                AddException(new InvalidOperationException($"Invalid provider type '{providerFactoryDefinition.Item2}' for '{invariantName}' invariant name."));
                continue;
            }

            if (prop.GetValue(null) is not DbProviderFactory factory)
            {
                AddException(new Exception(nameof(factory)));
                continue;
            }
            if (providerFactoryType.AssemblyQualifiedName == null)
            {
                AddException(new Exception(nameof(providerFactoryType.AssemblyQualifiedName)));
                continue;
            }
            return new CachedFactory(factory, providerFactoryType.Name, "", providerFactoryType.AssemblyQualifiedName);
        }

        throw new AggregateException(exceptions);
        void AddException(Exception ex)
        {
            exceptions.Add(ex);
        }
    }

    public static DbConnection CreateConnection(string invariantName, string connectionString)
    {
        if (string.IsNullOrWhiteSpace(invariantName))
        {
            throw new ArgumentNullException(nameof(invariantName));
        }

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new ArgumentNullException(nameof(connectionString));
        }

        var factory = GetFactory(invariantName).Factory;
        var connection = factory.CreateConnection()
            ?? throw new InvalidOperationException($"Database provider factory: '{invariantName}' did not return a connection object.");
        connection.ConnectionString = connectionString;
        return connection;
    }

    private class CachedFactory(DbProviderFactory factory, string factoryName, string factoryDescription, string factoryAssemblyQualifiedNameKey)
    {

        /// <summary>
        /// The factory to provide vendor specific functionality.
        /// </summary>
        /// <remarks>For more about <see href="http://florianreischl.blogspot.fi/2011/08/adonet-connection-pooling-internals-and.html">ConnectionPool</see>
        /// and issues with using this factory. Take these notes into account when considering robustness of Orleans!</remarks>
        public readonly DbProviderFactory Factory = factory;

        /// <summary>
        /// The name of the loaded factory, set by a database connector vendor.
        /// </summary>
        public readonly string FactoryName = factoryName;

        /// <summary>
        /// The description of the loaded factory, set by a database connector vendor.
        /// </summary>
        public readonly string FactoryDescription = factoryDescription;

        /// <summary>
        /// The description of the loaded factory, set by a database connector vendor.
        /// </summary>
        public readonly string FactoryAssemblyQualifiedNameKey = factoryAssemblyQualifiedNameKey;
    }
}
