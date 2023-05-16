using FlightPlanner.Models;
using FlightPlanner.Storage;
using Microsoft.AspNetCore.Mvc;

namespace FlightPlanner.Controllers
{
    [Route("api")]
    [ApiController]
    public class CustomerApiController : ControllerBase
    {
        [HttpGet]
        [Route("airports")]
        public IActionResult GetAirport(string search)
        {
            var result = FlightStorage.GetAirport(search);

            return Ok(result);
        }

        [HttpPost]
        [Route("flights/search")]
        public IActionResult SearchFlight(SearchFlightRequest searchFlight)
        {
            if (FlightStorage.IsInvalidFlightSearch(searchFlight))
            {
                return BadRequest();
            }

            var result = FlightStorage.SearchFlight(searchFlight);

            return Ok(result);
        }

        [HttpGet]
        [Route("flights/{id}")]
        public IActionResult GetFlight(int id)
        {
            var flight = FlightStorage.GetFlight(id);

            return flight == null ? NotFound() : Ok(flight);
        }
    }
}
