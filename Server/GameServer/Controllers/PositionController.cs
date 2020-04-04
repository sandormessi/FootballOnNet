using System;
using System.Collections.Generic;
using System.IO;
using System.Web.Http;
using GameServer.Models;
using Newtonsoft.Json;
using System.Configuration;

namespace GameServer.Controllers
{
    [Route("api/[controller]")]
    public class PositionController : ApiController
    {
        // GET api/values 
        [HttpGet]
        [Route("GetPosition")]
        public IHttpActionResult GetPosition()
        {
            var filePath = ConfigurationManager.AppSettings["PositionFilePath"];
            if (!File.Exists(filePath))
            {
                File.Create(filePath);
            }
            var result = JsonConvert.DeserializeObject<Position>(File.ReadAllText(filePath));
            return this.Ok(result);
        }

        /// <summary>
        /// Ezt meghívva eltárolhatjuk a jelenlegi állás (játékos / gólszám)
        /// </summary>
        /// <param name="value"></param>
        [HttpPost]
        [Route("PostPosition")]
        public IHttpActionResult PostPosition([FromBody]Position value)
        {
            if (!this.ModelState.IsValid)
            {
                return this.BadRequest(this.ModelState);
            }

            //konfig fájlból beolvasni a pozició mentési helyét
            var filePath = ConfigurationManager.AppSettings["PositionFilePath"];

            using (StreamWriter file = File.CreateText(filePath))
            {
                JsonSerializer serializer = new JsonSerializer();
                serializer.Serialize(file, value);
            }
            return this.Ok();
        }
    }
}