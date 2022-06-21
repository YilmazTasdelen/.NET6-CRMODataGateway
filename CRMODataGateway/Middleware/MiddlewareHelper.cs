using CRMODataGateway.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Primitives;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace CRMODataGateway.Middleware
{
    public class MiddlewareHelper
    {
        static readonly HttpClient client = new HttpClient();
        private static  IConfiguration _configuration;


        public MiddlewareHelper(IConfiguration config)
        {
            _configuration = config;
        }

  

        public static dynamic FinishGetRequestRouter(string entity, string query, UserInfo user,string domain,string baseAdd) {
            var credentials = new NetworkCredential(user.UserName, user.Password);
            var client = getNewHttpClient(user.UserName, user.Password, domain, baseAdd);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            // var response = client.GetAsync("accounts?$top=1").Result;
            var response = client.GetAsync(entity+query).Result;
            if (response.IsSuccessStatusCode)
            {
                return response.Content.ReadAsStringAsync();
                
            }
            return "";
        }

        public static dynamic FinishPostRequestRouter(string entity, string query,string bodyJson,UserInfo user,string ADDomain, string BaseAddress)
        {
            HttpClient client = new HttpClient(new HttpClientHandler() { Credentials = new NetworkCredential(user.UserName,user.Password, ADDomain) });
            client.BaseAddress = new Uri(BaseAddress);
            client.Timeout = new TimeSpan(0, 2, 0);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, entity + query);
            request.Content = new StringContent(bodyJson, Encoding.UTF8, "application/json");
            client.DefaultRequestHeaders.Add("OData-MaxVersion", "4.0");
            client.DefaultRequestHeaders.Add("OData-Version", "4.0");
            client.DefaultRequestHeaders.Add("prefer", "return=representation"); // this not compatiable with crm 2015 release 1. It make us allow to get inserted row back 
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            var response = client.Send(request);
            if (response.IsSuccessStatusCode)
            {
                return response.Content.ReadAsStringAsync();
            }
            return "";
        }
        public static HttpClient getNewHttpClient(string userName, string password, string domainName, string webAPIBaseAddress)
        {
            HttpClient client = new HttpClient(new HttpClientHandler() { Credentials = new NetworkCredential(userName, password, domainName) });
            client.BaseAddress = new Uri(webAPIBaseAddress);
            client.Timeout = new TimeSpan(0, 2, 0);
            return client;
        }

        public static bool ValidateToken(string token,string AesSymetricKey)
        {
            if (token == null)
                return false;

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(AesSymetricKey);
            try
            {
                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = false,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    // set clockskew to zero so tokens expire exactly at token expiration time (instead of 5 minutes later)
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);

                var jwtToken = (JwtSecurityToken)validatedToken;
                var userId = jwtToken.Claims.First(x => x.Type == ClaimTypes.Name).Value;

                // return user id from JWT token if validation successful
                return true;
            }
            catch
            {
                // return null if validation fails
                return true;
            }
        }

        //todo : check expridation date 
        public static dynamic CreateContextDetailObject(HttpContext context)
        {
            return new
            {
                path = context.Request.Path.Value,
                entityName = context.Request.Path.Value.Split('/')[context.Request.Path.Value.Split('/').Count() - 1],
                query = context.Request.QueryString.Value,
                token = context.Request.Headers.FirstOrDefault(x => x.Key == "Auth").Value.FirstOrDefault()
            };

        }

        public static dynamic GetcredentialFromToken(string token,string keys)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(keys);
            try
            {
                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = false,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    // set clockskew to zero so tokens expire exactly at token expiration time (instead of 5 minutes later)
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);

                var jwtToken = (JwtSecurityToken)validatedToken;
                var userName = jwtToken.Claims.First(x => x.Type == ClaimTypes.Name).Value;
                var AccessCode = jwtToken.Claims.First(x => x.Type == "AccessCode").Value;

                // return user id from JWT token if validation successful
                return new UserInfo { UserName = userName, Password = DecryptString(AccessCode)};
            }
            catch (Exception ex)
            {
                //todo : log              
            }

            return null;
        }

        public static string EncryptString(string plainText)
        {
            byte[] iv = new byte[16];
            byte[] array;

            using (Aes aes = Aes.Create())
            {
                aes.Key = Encoding.UTF8.GetBytes("b14ca5898a4e4133bbce2ea2315a1916");
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

        public static string DecryptString(string cipherText)
        {
            byte[] iv = new byte[16];
            byte[] buffer = Convert.FromBase64String(cipherText);

            using (Aes aes = Aes.Create())
            {
                aes.Key = Encoding.UTF8.GetBytes("b14ca5898a4e4133bbce2ea2315a1916");
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
