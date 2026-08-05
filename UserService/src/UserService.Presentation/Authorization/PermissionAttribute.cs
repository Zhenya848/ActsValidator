using Microsoft.AspNetCore.Authorization;

namespace UserService.Presentation.Authorization
{
    public class PermissionAttribute : AuthorizeAttribute, IAuthorizationRequirement
    {
        public string Code;

        public PermissionAttribute(string code) : base(policy: code)
        {
            Code = code;
        }
    }
}
