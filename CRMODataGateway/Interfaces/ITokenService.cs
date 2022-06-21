using CRMODataGateway.Models;
using ODataGateway.Shared.Dtos;

namespace CRMODataGateway.Interfaces
{
    public interface ITokenService
    {
      Task<User> GetUserInfo(UserInfo user);
      Task<bool> ValidateUser(User user);
      Task<dynamic> CreateTokenClaims(User user);
      string EncryptString(string plainText);
      string DecryptString(string cipherText);
    }
}
