using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using GameServer.Models;
using Microsoft.Owin.Hosting;
using Newtonsoft.Json;

namespace GameServer
{
    public class Program
    {
        static void Main()
        {
            string baseAddress = "http://localhost:9000/";

            // Start OWIN host 
            using (WebApp.Start<Startup>(url: baseAddress))
            {
                Console.WriteLine("A kliens fut az alábbi címen: http://localhost:9000/");
                Console.WriteLine("Api hívás példák:\r\napi/Position/PostPosition\r\napi/Position/GetPosition\r\napi/Score/GetScore\r\napi/Score/PostScore");
                
                //Tesztelni:
                //HttpClient client = new HttpClient();
                // var scores = new Position(){X = 2,Y=3,Id = Guid.NewGuid(),OccurenceTime = DateTime.Now,PositionType = "PlayerPosition" };
                //var mySerializedSampleData = JsonConvert.SerializeObject(scores);
                //var buffer = System.Text.Encoding.UTF8.GetBytes(mySerializedSampleData);
                //var byteContent = new ByteArrayContent(buffer);
                //byteContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                //var result = client.PostAsync(baseAddress + "api/Position/PostPosition", byteContent).Result;
                //Console.WriteLine(result);

                //var response = client.GetAsync(baseAddress + "api/Position/GetPosition").Result;
                //Console.WriteLine(response);
                //Console.WriteLine(response.Content.ReadAsStringAsync().Result);
                Console.ReadLine();
            }
        }
    }
}