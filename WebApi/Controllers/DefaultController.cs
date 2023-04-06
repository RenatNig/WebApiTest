using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Classes.Services;

namespace WebApi.Controllers
{
    public class DefaultController : ApiController
    {
        //класс, который будет выполнять основную работу
        private static Worker worker = new Worker();

        [Authorize]
        [HttpGet]
        [Route("api/Default/GetPlacesCount/{nom_route}")]
        public async Task<HttpResponseMessage> GetPlacesCount(string nom_route)
        {
            return await worker.PlacesCountMethodAsync(nom_route);
        }

        [Authorize]
        [HttpGet]
        [Route("api/Default/GetPosNames/{nom_doc}")]
        public async Task<HttpResponseMessage> GetPosNames(string nom_doc)
        {
            return await worker.PosNameMethodAsync(nom_doc);
        }
    }
}
