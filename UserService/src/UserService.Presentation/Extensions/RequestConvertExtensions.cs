using CSharpFunctionalExtensions;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using UserService.Domain.Shared;
using Formatting = System.Xml.Formatting;

namespace UserService.Presentation.Extensions;

public static class RequestConvertExtensions
{
    public static Result<T, Error> ConvertObjectToType<T>(this object request)
    {
        var settings = new JsonSerializerSettings
        {
            Formatting = (Newtonsoft.Json.Formatting)Formatting.None,
            NullValueHandling = NullValueHandling.Ignore,
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            ContractResolver = new DefaultContractResolver
            {
                NamingStrategy = new SnakeCaseNamingStrategy()
            },
            DateTimeZoneHandling = DateTimeZoneHandling.Utc,
            DateFormatString = "yyyy-MM-ddTHH:mm:ss.fffZ"
        };
        
        var json = request.ToString() ?? string.Empty;
        
        if (string.IsNullOrEmpty(json))
            return Error.Failure("object.serialize.failure", "Can't serialize object");
        
        var result = JsonConvert.DeserializeObject<T>(json, settings);
        
        if (result is null)
            return Error.Failure("object.deserialize.failure", "Can't deserialize object");
        
        return result;
    }
}