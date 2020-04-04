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
    public class ScoreController : ApiController
    {
        // GET api/values 
        [HttpGet]
        [Route("GetScore")]
        public IHttpActionResult GetScore()
        {
            var filePath = ConfigurationManager.AppSettings["GameScoreFilePath"];
            if (!File.Exists(filePath))
            {
                File.Create(filePath);
            }
            var result = JsonConvert.DeserializeObject<Scores>(File.ReadAllText(filePath));
            return this.Ok(result);
        }

        /// <summary>
        /// Ezt meghívva eltárolhatjuk a jelenlegi állás (játékos / gólszám)
        /// </summary>
        /// <param name="value"></param>
        [HttpPost]
        [Route("PostScore")]
        public IHttpActionResult PostScore([FromBody]Scores value)
        {
            if (!this.ModelState.IsValid)
            {
                return this.BadRequest(this.ModelState);
            }

            //konfig fájlból beolvasni az állás mentési helyét
            var filePath = ConfigurationManager.AppSettings["GameScoreFilePath"];

            using (StreamWriter file = File.CreateText(filePath))
            {
                JsonSerializer serializer = new JsonSerializer();
                serializer.Serialize(file, value);
            }
            return this.Ok();
        }
    }
}