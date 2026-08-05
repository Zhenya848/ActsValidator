namespace UserService.Contracts;

public interface IAccountsContract
{
    Task<IReadOnlyList<string>> GetUserPermissionCodes(Guid userId);
}