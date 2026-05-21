using System.Security.Cryptography;
using System.Text;

namespace Application.UseCase.Auth.EmailVerification;

internal static class EmailVerificationCodeHasher
{
    public static string Hash(string code)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(code.Trim()));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
}
