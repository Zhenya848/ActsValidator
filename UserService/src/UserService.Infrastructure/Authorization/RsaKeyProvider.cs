using System.Security.Cryptography;
using UserService.Application.Abstractions;
using UserService.Presentation.Options;

namespace UserService.Infrastructure.Authorization;

public class RsaKeyProvider : IKeyProvider
{
    private readonly bool _createNewKeys;
    private readonly string _privateKeyPath;
    private readonly string _publicKeyPath;
    private readonly RSA _rsa;

    public RsaKeyProvider(AuthOptions authOptions)
    {
        _createNewKeys = authOptions.CreateNewKeys;
        _rsa = RSA.Create();
        
        _privateKeyPath = authOptions.PrivateKeyPath;
        _publicKeyPath = authOptions.PublicKeyPath;
        
        EnsureKeysExist();
    }

    private void EnsureKeysExist()
    {
        if (_createNewKeys && (File.Exists(_privateKeyPath) == false || File.Exists(_publicKeyPath) == false))
            GenerateKeys();
    }

    public RSA GetPrivateRsa()
    {
        byte[] privateKeyBytes = File.ReadAllBytes(_privateKeyPath);
        _rsa.ImportRSAPrivateKey(privateKeyBytes, out _);
        
        return _rsa;
    }

    public RSA GetPublicRsa()
    {
        byte[] publicKeyBytes = File.ReadAllBytes(_publicKeyPath);
        _rsa.ImportRSAPublicKey(publicKeyBytes, out _);
        
        return _rsa;
    }

    private void GenerateKeys()
    {
        byte[] privateKey = _rsa.ExportRSAPrivateKey();
        byte[] publicKey = _rsa.ExportRSAPublicKey();
        
        File.WriteAllBytes(_privateKeyPath, privateKey);
        File.WriteAllBytes(_publicKeyPath, publicKey);
    }
}