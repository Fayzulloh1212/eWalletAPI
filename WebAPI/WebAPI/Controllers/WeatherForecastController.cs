using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<WeatherForecastController> _logger;
        private ISession Session;

        public WeatherForecastController(ILogger<WeatherForecastController> logger, IHttpContextAccessor httpContextAccessor)
        {
            _logger = logger;
            Session = httpContextAccessor.HttpContext.Session;
        }

        [HttpGet("/weatherforecast")]
        public IEnumerable<WeatherForecast> Get()
        {
            List<WeatherForecast> WeatherForecasts = new List<WeatherForecast>();
            if (Session.Keys.Contains(nameof(WeatherForecasts)))
                WeatherForecasts = JsonConvert.DeserializeObject<List<WeatherForecast>>(Session.GetString(nameof(WeatherForecasts)));
            
            return WeatherForecasts;
         
            //var rng = new Random();
            //return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            //{
            //    Date = DateTime.Now.AddDays(index),
            //    TemperatureC = rng.Next(-20, 55),
            //    Summary = Summaries[rng.Next(Summaries.Length)]
            //})
            //.ToArray();
        }

        [HttpGet("/weatherforecast/{id}")]
        public WeatherForecast GetById(int Id)
        {
            List<WeatherForecast> WeatherForecasts = new List<WeatherForecast>();
            if (Session.Keys.Contains(nameof(WeatherForecasts)))
                WeatherForecasts = JsonConvert.DeserializeObject<List<WeatherForecast>>(Session.GetString(nameof(WeatherForecasts)));
            
            return WeatherForecasts.Where(wf => wf.Id == Id).FirstOrDefault();

            //var rng = new Random(); 
            //List<WeatherForecast> wfs = Enumerable.Range(1, 5).Select(index => new WeatherForecast
            //{
            //    Date = DateTime.Now.AddDays(index),
            //    TemperatureC = rng.Next(-20, 55),
            //    Summary = Summaries[rng.Next(Summaries.Length)]
            //})
            //.ToList();
              
            //return wfs[Id];
        }

        [HttpPost("/weatherforecast")]
        public WeatherForecast Save(WeatherForecast weatherForecast)
        {
            WeatherForecast res = null;

            List<WeatherForecast> WeatherForecasts = new List<WeatherForecast>();
            if (Session.Keys.Contains(nameof(WeatherForecasts)))
                WeatherForecasts = JsonConvert.DeserializeObject<List<WeatherForecast>>(Session.GetString(nameof(WeatherForecasts)));
            
            try
            {
                WeatherForecasts.Add(weatherForecast);
                Session.SetString(nameof(WeatherForecasts), JsonConvert.SerializeObject(WeatherForecasts));

                res = weatherForecast;
            }
            catch { }

            return res;
        }
    }
}
