using System;
using System.Data;
using Dapper;

namespace AnimalShelter.Database;

public class EnumTypeHandler<T> : SqlMapper.TypeHandler<T> where T : struct, Enum
{
    public override void SetValue(IDbDataParameter parameter, T value)
    {
        parameter.DbType = DbType.String;
        parameter.Value = value.ToString();
    }

    public override T Parse(object value)
    {
        return (T)Enum.Parse(typeof(T), value.ToString()!);
    }
}