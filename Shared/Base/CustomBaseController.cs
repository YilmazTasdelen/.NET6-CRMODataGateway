using ODataGateway.Shared.Dtos;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ODataGateway.Shared.Base
{
    public class CustomBaseController:ControllerBase
    {
        public IActionResult CreateActionResultInstance<T>(Response<T> response) 
        {
            return new ObjectResult(response)
            {
                StatusCode = response.StatusCode

            };
        }


    }
}
