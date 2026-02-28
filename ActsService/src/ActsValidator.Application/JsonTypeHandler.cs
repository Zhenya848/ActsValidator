using System.Data;
using System.Text.Json;
using Dapper;

namespace ActsValidator.Application;

public class JsonTypeHandler<T> : SqlMapper.TypeHandler<T>
{
    public override void SetValue(IDbDataParameter parameter, T? value)
    {
        parameter.Value = value is null ? DBNull.Value : JsonSerializer.Serialize(value);
    }

    public override T? Parse(object value)
    {
        if (value is null || value is DBNull) return default;
        
        var json = value.ToString();
        return string.IsNullOrEmpty(json) 
            ? default 
            : JsonSerializer.Deserialize<T>(json, JsonSerializerOptions.Default);
    }
}