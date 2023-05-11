using FlightPlanner.Models;

namespace FlightPlanner.Storage
{
    public static class FlightStorage
    {
        private static List<Flight> _flights = new();
        private static int _id = 1;
        private static readonly object _flightLock = new();

        public static void Clear()
        {
            _flights.Clear();
            _id = 1;
        }

        public static Flight GetFlight(int id)
        {
            lock (_flightLock)
            {
                return _flights.SingleOrDefault(f => f.Id == id);
            }
        }

        public static bool FlightExists(Flight flight)
        {
            lock (_flightLock)
            {
                return _flights.Any(f => f.ArrivalTime == flight.ArrivalTime &&
                                         f.DepartureTime == flight.DepartureTime &&
                                         f.From.AirportCode.ToLower() == flight.From.AirportCode.ToLower() &&
                                         f.To.AirportCode.ToLower() == flight.To.AirportCode.ToLower());
            }
        }

        public static Flight AddFlight(Flight input)
        {
            lock (_flightLock)
            {
                var flight = new Flight
                {
                    Id = _id++,
                    ArrivalTime = input.ArrivalTime,
                    Carrier = input.Carrier,
                    DepartureTime = input.DepartureTime,
                    From = input.From,
                    To = input.To
                };

                _flights.Add(flight);
                return flight;
            }
        }

        public static void DeleteFlight(int id)
        {
            lock (_flightLock)
            {
                _flights.RemoveAll(f => f.Id == id);
            }
        }

        public static List<Airport> FindAirport(string search)
        {
            var trimmedSearch = search.Trim();

            var result = _flights
                .SelectMany(f => new[] { f.From, f.To })
                .Distinct()
                .Where(airport => IsMatch(airport, trimmedSearch))
                .ToList();

            return result;
        }

        private static bool IsMatch(Airport airport, string search)
        {
            return airport.City.Contains(search, StringComparison.CurrentCultureIgnoreCase) ||
                   airport.Country.Contains(search, StringComparison.CurrentCultureIgnoreCase) ||
                   airport.AirportCode.Contains(search, StringComparison.CurrentCultureIgnoreCase);
        }

        public static PageResult SearchFlight(SearchFlightRequest flight)
        {
            var matches = _flights.Where(f =>
                f.From.AirportCode.StartsWith(flight.From, StringComparison.CurrentCultureIgnoreCase) &&
                f.To.AirportCode.StartsWith(flight.To, StringComparison.CurrentCultureIgnoreCase) && 
                f.DepartureTime.Contains(flight.DepartureDate)).ToList();

            var result = new PageResult
            {
                Items = matches,
                TotalItems = matches.Count,
                Page = 0
            };

            return result;
        }

        public static bool IsInvalidFlight(Flight flight)
        {
            var departure = DateTime.Parse(flight.DepartureTime);
            var arrival = DateTime.Parse(flight.ArrivalTime);

            if (flight == null) return true;

            var isInvalidDetail = string.IsNullOrWhiteSpace(flight.From.AirportCode) ||
                                  string.IsNullOrWhiteSpace(flight.To.AirportCode) ||
                                  string.IsNullOrWhiteSpace(flight.From.Country) ||
                                  string.IsNullOrWhiteSpace(flight.To.Country) ||
                                  string.IsNullOrWhiteSpace(flight.From.City) ||
                                  string.IsNullOrWhiteSpace(flight.To.City) ||
                                  string.IsNullOrWhiteSpace(flight.Carrier) ||
                                  string.IsNullOrWhiteSpace(flight.DepartureTime) ||
                                  string.IsNullOrWhiteSpace(flight.ArrivalTime);

            var isInvalidAirport = flight.From.AirportCode.Trim().ToUpper() == flight.To.AirportCode.Trim().ToUpper();

            return isInvalidDetail || isInvalidAirport || arrival <= departure;
        }

        public static bool IsInvalidFlightSearch(SearchFlightRequest searchFlight)
        {
            return searchFlight.From == null ||
                   searchFlight.To == null ||
                   searchFlight.DepartureDate == null ||
                   searchFlight.From == searchFlight.To;
        }
    }
}