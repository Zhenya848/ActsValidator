using ActsValidator.Domain.Shared;
using CSharpFunctionalExtensions;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Formatting = System.Xml.Formatting;

namespace ActsValidator.Presentation.Extensions;

public static class JsonConvertExtensions
{
    public static Result<T, Error> ConvertJsonToType<T>(this string message)
    {
        var type = typeof(T);
        
        if (type == typeof(string))
            return Error.Validation("type.is.invalid", "Type can't be string");
        
        var isCollection = typeof(System.Collections.IEnumerable).IsAssignableFrom(type);
        
        var settings = new JsonSerializerSettings
        {
            Formatting = (Newtonsoft.Json.Formatting)Formatting.None,
            NullValueHandling = NullValueHandling.Ignore,
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
            DateTimeZoneHandling = DateTimeZoneHandling.Utc,
            DateFormatString = "yyyy-MM-ddTHH:mm:ss.fffZ"
        };

        var json = GetJsonFromMessage(message, isCollection);
        
        if (json.IsFailure)
            return json.Error;
        
        var result = JsonConvert.DeserializeObject<T>(json.Value, settings);
        
        if (result is null)
            return Error.Failure("object.deserialize.failure", "Can't deserialize object");
        
        return result;
    }

    private static Result<string, Error> GetJsonFromMessage(string message, bool isCollection)
    {
        var firstBracketIndex = message.IndexOf(isCollection ? '[' : '{');
        var lastBracketIndex = message.LastIndexOf(isCollection ? ']' : '}');
        
        if (firstBracketIndex == -1 || lastBracketIndex == -1)
            return Errors.General.ValueIsInvalid("json");
        
        return message.Substring(firstBracketIndex, lastBracketIndex - firstBracketIndex + 1);
    }
}