using System.Text.RegularExpressions;

namespace UserService.Domain.Shared;

public static class EmailValidator
{
    public static bool IsEmailValid(this string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;
        
        string pattern = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";
        return Regex.IsMatch(email, pattern);
    }
}