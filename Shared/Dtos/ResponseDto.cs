using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace ODataGateway.Shared.Dtos
{
    public class Response<T>
    {
        public T Data { get; private set; }

        [JsonIgnore]
        public int StatusCode { get; private set; }

        [JsonIgnore]
        public bool IsSuccessful{ get; private set; }

        public List<string> Errors { get; set; }

        //Static Factory Methods 

        public static Response<T> Success( T data,int statuCode )
        {
            return new Response<T> { Data = data, StatusCode = statuCode, IsSuccessful = true };
        }

        public static Response<T> Success(int statuCode)
        {
            return new Response<T> { Data = default(T ), StatusCode = statuCode, IsSuccessful = true };
        }

        public static Response<T> Fail(List<string> errors, int statuCode)
        {
            return new Response<T> { Errors = errors, Data = default(T), StatusCode = statuCode, IsSuccessful = false };
        }

        public static Response<T> Fail(string errors, int statuCode)
        {
            return new Response<T> { Errors = new List<string>() { errors }, Data = default(T), StatusCode = statuCode, IsSuccessful = false };
        }

    }
}
