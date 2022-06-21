using CRMODataGateway.Interfaces;
using CRMODataGateway.Models;
using Dapper;
using ODataGateway.Shared.Dtos;
using System.Data.SqlClient;
using System.DirectoryServices.AccountManagement;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace CRMODataGateway.Services
{
    public class TokenService : ITokenService
    {
        public IConfiguration _configuration;
        SqlConnection _connection;
        private string getSystemUserQuery =
        #region getUserQuery 
            @"select
                                   SUBSTRING(s.DomainName, 9, 100) as userName,
	                            	s.DomainName,
	                            	s.SystemUserId,
	                            	s.new_bayi as Dealer,
	                            	s.TerritoryId,
	                            	s.TerritoryIdName,
	                            	s.OrganizationId,
	                            	OrganizationIdName,
	                            	s.BusinessUnitId,
	                            	s.BusinessUnitIdName,
	                            	s.Title,
	                            	s.IsDisabled,
	                            	s.AccessMode,
                                    '' as AccessCode
                                   from SystemUser s where s.DomainName like";
            #endregion

        public TokenService(IConfiguration configuration)
        {
            _configuration = configuration;
            _connection = new SqlConnection(_configuration["ConnectionStrings:Default"]);
        }

        public async Task<User> GetUserInfo(UserInfo user)
        {
            if (checkUserFromActiveDirectory(user))
            {
                //var parameters = new { DomainName = user.UserName};
                return await _connection.QueryFirstOrDefaultAsync<User>(getSystemUserQuery + "'%" + user.UserName + "%'");
            }
            return null;
        }

        public async Task<bool> ValidateUser(User user)
        {
            return String.IsNullOrEmpty(user.userName) == true?  false : true;

        }

        public async Task<dynamic> CreateTokenClaims(User user)
        {
            var claims = new[]
               {
                new Claim(ClaimTypes.Name, user.userName??"empty"),
                new Claim("DomainName", user.DomainName??"empty"),
                new Claim("UserId", user.SystemUserId.ToString()??"empty"),
                new Claim("Dealer", user.Dealer.ToString()??"empty"),
                new Claim("TerritoryId", user.TerritoryId??"empty"),
                new Claim("TerritoryIdName", user.TerritoryIdName??"empty"),
                new Claim("OrganizationId", user.OrganizationId.ToString()??"empty"),
                new Claim("OrganizationIdName", user.OrganizationIdName??"empty"),
                new Claim("BusinessUnitId", user.BusinessUnitId.ToString()??"empty"),
                new Claim("BusinessUnitIdName", user.BusinessUnitIdName??"empty"),
                new Claim("Title", user.Title??"empty"),
                new Claim("IsDisabled", user.IsDisabled.ToString()??"empty"),
                new Claim("AccessMode", user.AccessMode.ToString()??"empty"),
                new Claim("AccessCode", user.AccessCode.ToString()??"empty")
                };
            return claims;
        }
        private bool checkUserFromActiveDirectory(UserInfo userInfo)
        {
            bool isValid = false;
            try
            {
                using (var adContext = new PrincipalContext(ContextType.Domain, _configuration["Domain"]))
                {
                    isValid = adContext.ValidateCredentials(userInfo.UserName, userInfo.Password);
                }
            }
            catch (Exception ex)
            {
                // log request
            }
            return isValid;
        }

         public string EncryptString( string plainText)
        {
            byte[] iv = new byte[16];
            byte[] array;

            using (Aes aes = Aes.Create())
            {
                aes.Key = Encoding.UTF8.GetBytes(_configuration["AesSymetricKey"]);
                aes.IV = iv;

                ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

                using (MemoryStream memoryStream = new MemoryStream())
                {
                    using (CryptoStream cryptoStream = new CryptoStream((Stream)memoryStream, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter streamWriter = new StreamWriter((Stream)cryptoStream))
                        {
                            streamWriter.Write(plainText);
                        }

                        array = memoryStream.ToArray();
                    }
                }
            }

            return Convert.ToBase64String(array);
        }

        public string DecryptString( string cipherText)
        {
            byte[] iv = new byte[16];
            byte[] buffer = Convert.FromBase64String(cipherText);

            using (Aes aes = Aes.Create())
            {
                aes.Key = Encoding.UTF8.GetBytes(_configuration["AesSymetricKey"]);
                aes.IV = iv;
                ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

                using (MemoryStream memoryStream = new MemoryStream(buffer))
                {
                    using (CryptoStream cryptoStream = new CryptoStream((Stream)memoryStream, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader streamReader = new StreamReader((Stream)cryptoStream))
                        {
                            return streamReader.ReadToEnd();
                        }
                    }
                }
            }
        }
    }
}
