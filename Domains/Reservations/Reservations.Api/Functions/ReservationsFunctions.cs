using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Threading;
using System.Collections.Generic;
using Reservations.Api.Queries;
using Reservations.Domain.Commands;
using Reservations.Domain.ReadModels.Reservation;
using CQRS.Essentials.Abstractions.CQRS;
using Reservations.Domain.ReadModels.RoomTypeAvailability;

namespace HotelManagement.Reservations.Api.Functions
{
    public class ReservationsFunctions
    {
        private readonly IBus _bus;

        public ReservationsFunctions(IBus bus)
        {
            _bus = bus;
        }

        [FunctionName("MakeReservationFunction")]
        public async Task<IActionResult> MakeReservation([HttpTrigger(AuthorizationLevel.Function, "post", Route = "reservation/make")] HttpRequest req, CancellationToken cancellationToken, ILogger log)
        {
            try
            {
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                var makeReservationCommand = JsonConvert.DeserializeObject<MakeReservation>(requestBody);  //you may want to use a mapper from dto to command instead (backwards compatibility)
                //get the total room type available
                var roomTypeAvailabilityReadModel = await _bus.Send<FindRoomTypeAvailabilityQuery, RoomTypeAvailabilityReadModel>(new FindRoomTypeAvailabilityQuery { HotelId = makeReservationCommand.HotelId, RoomType = makeReservationCommand.RoomType}, cancellationToken);
                makeReservationCommand.TotalRoomTypeAvailable = roomTypeAvailabilityReadModel?.Amount ?? 0;
                //fire off command
                var events = await _bus.Send(makeReservationCommand, cancellationToken);
                return new OkObjectResult(events);
            }
            catch (Exception ex)
            {
                log.LogCritical(ex, ex.Message, new Dictionary<string, object> { ["Request"] = req.Body });
                return new BadRequestObjectResult("Error occured on MakeReservationFunction.MakeReservation.");
            }
        }

        [FunctionName("GetReservationByIdFunction")]
        public async Task<IActionResult> GetReservationById([HttpTrigger(AuthorizationLevel.Function, "get", Route = "reservation/{hotelId}/{reservationId}")] HttpRequest req, Guid hotelId, Guid reservationId, CancellationToken cancellationToken, ILogger log)
        {
            try
            {
                var query = new FindReservationQuery { HotelId = hotelId, Id = reservationId };
                var result = await _bus.Send<FindReservationQuery, ReservationsReadModel>(query, cancellationToken);
                return new OkObjectResult(result);
            }
            catch (Exception ex)
            {
                log.LogCritical(ex, ex.Message, new Dictionary<string, object> { ["Request"] = req.Query.ToString() });
                return new BadRequestObjectResult("Error occured on GetReservationByIdFunction.GetReservationById.");
            }
        }
    }
}
