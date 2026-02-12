using System.Security.Cryptography;

namespace UserService.Application.Abstractions;

public interface IKeyProvider
{
    public RSA GetPrivateRsa();
    public RSA GetPublicRsa();
}