using CRMODataGateway.Models;
using Microsoft.AspNetCore.Mvc.ActionConstraints;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Patterns;
using Newtonsoft.Json;
using System.Text;

namespace CRMODataGateway.Middleware
{
    public class HttpMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IConfiguration _configuration;
        public HttpMiddleware(RequestDelegate next, IConfiguration configuration)
        {
            _next = next;
            _configuration = configuration;
        }
        public async Task Invoke(HttpContext context) // todo log -- todo: put, patch, delete methods 
        {

            try
            {
                Stream originalBody = null;
                using (MemoryStream modifiedBody = new MemoryStream())
                {
                    using (StreamReader streamReader = new StreamReader(modifiedBody))
                    {
                        var requestProperties = Middleware.MiddlewareHelper.CreateContextDetailObject(context);
                        // todo check if there is a token 
                        if (context.Request.Method == "GET" && MiddlewareHelper.ValidateToken(requestProperties.token,_configuration["AesSymetricKey"])) // check token for get methods
                        {
                            UserInfo user = MiddlewareHelper.GetcredentialFromToken(requestProperties.token,, _configuration["AesSymetricKey"]); // get username and decrypted password
                            var result = Middleware.MiddlewareHelper.FinishGetRequestRouter(requestProperties.entityName, requestProperties.query, user,_configuration["ADDomain"],_configuration["BaseAddress"]);
                            string resultBody = result.Result.ToString();
                            //write response body
                            originalBody = context.Response.Body;
                            context.Response.Body = modifiedBody;
                            modifiedBody.Seek(0, SeekOrigin.Begin);
                            string originalContent = streamReader.ReadToEnd();
                            context.Response.Body = originalBody;
                            string newContent;
                            newContent = resultBody;
                            context.Response.ContentLength = Encoding.UTF8.GetBytes(newContent).Length;
                            await context.Response.WriteAsync(newContent);
                        }

                        if (context.Request.Method == "POST" && MiddlewareHelper.ValidateToken(requestProperties.token, _configuration["AesSymetricKey"])) // check token for get methods
                        {
                            UserInfo user = MiddlewareHelper.GetcredentialFromToken(requestProperties.token, _configuration["AesSymetricKey"]); // get username and decrypted password
                            context.Request.EnableBuffering();
                            var buffer = new byte[Convert.ToInt32(context.Request.ContentLength)];
                            await context.Request.Body.ReadAsync(buffer, 0, buffer.Length);
                            string requestBodyString = Encoding.UTF8.GetString(buffer);
                            string bodyContent = new StreamReader(context.Request.Body).ReadToEnd();
                            var result = Middleware.MiddlewareHelper.FinishPostRequestRouter(requestProperties.entityName, requestProperties.query, requestBodyString, user, _configuration["ADDomain"], _configuration["BaseAddress"]);
                            string resultBody = result.Result.ToString();
                            //write response body
                            originalBody = context.Response.Body;
                            context.Response.Body = modifiedBody;
                            modifiedBody.Seek(0, SeekOrigin.Begin);
                            string originalContent = streamReader.ReadToEnd();
                            context.Response.Body = originalBody;
                            string newContent;
                            newContent = resultBody;
                            context.Response.ContentLength = Encoding.UTF8.GetBytes(newContent).Length;
                            await context.Response.WriteAsync(newContent);
                        }
                        else
                        {
                            await _next.Invoke(context);
                        }
                       
                    }
                }
            }
            catch (Exception e)
            {

                throw;
            }






        }












        private async Task HandleFriendlyAreaExceptionAsync(HttpContext context)
        {
            await context.Response.WriteAsync(JsonConvert.SerializeObject(new
            {
                Datum = "",
                IsSuccessful = false,
                Message = ""
            }));
        }

        private Stream HandleRequestBody(HttpContext context, string newBody)
        {
            var request = context.Request;
            var stream = request.Body;// currently holds the original stream                    
            var originalReader = new StreamReader(stream);
            var originalContent = originalReader.ReadToEndAsync();
            var notModified = true;
            try
            {
                var str = newBody;
                //convert string to jsontype
                var json = JsonConvert.SerializeObject(str);
                //modified stream
                var requestData = Encoding.UTF8.GetBytes(json);
                stream = new MemoryStream(requestData);
                notModified = false;

            }
            catch (Exception e)
            {

            }
            request.Body = stream;
            return stream;
        }
    }
}
