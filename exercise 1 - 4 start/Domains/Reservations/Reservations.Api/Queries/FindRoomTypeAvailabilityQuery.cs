using CQRS.Essentials.Abstractions.CQRS;
using Reservations.Domain.ReadModels.RoomTypeAvailability;
using System;

namespace Reservations.Api.Queries
{
    public class FindRoomTypeAvailabilityQuery : IQuery<RoomTypeAvailabilityReadModel>
    {
        public Guid HotelId { get; set; }
        public string RoomType { get; set; }
    }
}
