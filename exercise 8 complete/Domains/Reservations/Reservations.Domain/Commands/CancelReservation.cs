using CQRS.Essentials.Abstractions.CQRS;
using System;

namespace Reservations.Domain.Commands
{
    public class CancelReservation : ICommand
    {
        public Guid Id { get; private set; }
        public Guid HotelId { get; private set; }
        public string RoomType { get; private set; }

        public CancelReservation(Guid id, Guid hotelId, string roomType)
        {
            Id = id;
            HotelId = hotelId;
            RoomType = roomType;
        }
    }
}
