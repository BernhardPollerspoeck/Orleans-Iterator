/*
WARNING: This code is taken from https://github.com/dotnet/orleans/blob/main/src/AdoNet/Shared/Storage/DbExtensions.cs
and slightly modified to adhere to this projects conventions and respecting null context
 */

using System.Collections.ObjectModel;
using System.Data;

namespace Orleans.Iterator.AdoNet.MainPackageCode;

internal static class DbExtensions
{
    /// <summary>
    /// An explicit map of type CLR viz database type conversions.
    /// </summary>
    static readonly ReadOnlyDictionary<Type, DbType> _typeMap = new(new Dictionary<Type, DbType>
        {
            { typeof(object),   DbType.Object },
            { typeof(int),      DbType.Int32 },
            { typeof(int?),     DbType.Int32 },
            { typeof(uint),     DbType.UInt32 },
            { typeof(uint?),    DbType.UInt32 },
            { typeof(long),     DbType.Int64 },
            { typeof(long?),    DbType.Int64 },
            { typeof(ulong),    DbType.UInt64 },
            { typeof(ulong?),   DbType.UInt64 },
            { typeof(float),    DbType.Single },
            { typeof(float?),   DbType.Single },
            { typeof(double),   DbType.Double },
            { typeof(double?),  DbType.Double },
            { typeof(decimal),  DbType.Decimal },
            { typeof(decimal?), DbType.Decimal },
            { typeof(short),    DbType.Int16 },
            { typeof(short?),   DbType.Int16 },
            { typeof(ushort),   DbType.UInt16 },
            { typeof(ushort?),  DbType.UInt16 },
            { typeof(byte),     DbType.Byte },
            { typeof(byte?),    DbType.Byte },
            { typeof(sbyte),    DbType.SByte },
            { typeof(sbyte?),   DbType.SByte },
            { typeof(bool),     DbType.Boolean },
            { typeof(bool?),    DbType.Boolean },
            { typeof(string),   DbType.String },
            { typeof(char),     DbType.StringFixedLength },
            { typeof(char?),    DbType.StringFixedLength },
            { typeof(Guid),     DbType.Guid },
            { typeof(Guid?),    DbType.Guid },
            //Using DateTime for cross DB compatibility. The underlying DB table column type can be DateTime or DateTime2
            { typeof(DateTime),     DbType.DateTime },
            { typeof(DateTime?),    DbType.DateTime },
            { typeof(TimeSpan),     DbType.Time },
            { typeof(byte[]),       DbType.Binary },
            { typeof(TimeSpan?),        DbType.Time },
            { typeof(DateTimeOffset),   DbType.DateTimeOffset },
            { typeof(DateTimeOffset?),  DbType.DateTimeOffset },
        });

    /// <summary>
    /// Creates a new SQL parameter using the given arguments.
    /// </summary>
    /// <typeparam name="T">The type of the parameter.</typeparam>
    /// <param name="command">The command to use to create the parameter.</param>
    /// <param name="direction">The direction of the parameter.</param>
    /// <param name="parameterName">The name of the parameter.</param>
    /// <param name="value">The value of the parameter.</param>
    /// <param name="size">The size of the parameter value.</param>
    /// <param name="dbType">the <see cref="DbType"/> of the parameter.</param>
    /// <returns>A parameter created using the given arguments.</returns>
    public static IDbDataParameter CreateParameter<T>(this IDbCommand command, ParameterDirection direction, string parameterName, T value, int? size = null, DbType? dbType = null)
    {
        //There should be no boxing for value types. See at:
        //http://stackoverflow.com/questions/8823239/comparing-a-generic-against-null-that-could-be-a-value-or-reference-type
        var parameter = command.CreateParameter();
        parameter.ParameterName = parameterName;
        parameter.Value = value as object ?? DBNull.Value;
        parameter.DbType = dbType ?? _typeMap[typeof(T)];
        parameter.Direction = direction;
        if (size != null) { parameter.Size = size.Value; }

        return parameter;
    }

    /// <summary>
    /// Creates and adds a new SQL parameter to the command.
    /// </summary>
    /// <typeparam name="T">The type of the parameter.</typeparam>
    /// <param name="command">The command to use to create the parameter.</param>
    /// <param name="parameterName">The name of the parameter.</param>
    /// <param name="value">The value of the parameter.</param>
    /// <param name="direction">The direction of the parameter.</param>
    /// <param name="size">The size of the parameter value.</param>
    /// <param name="dbType">the <see cref="DbType"/> of the parameter.</param>
    public static void AddParameter<T>(this IDbCommand command, string parameterName, T value, ParameterDirection direction = ParameterDirection.Input, int? size = null)
    {
        command.Parameters.Add(command.CreateParameter(direction, parameterName, value, size));
    }


}
