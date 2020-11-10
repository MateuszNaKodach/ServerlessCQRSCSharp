using System;

namespace Reservations.Domain.Events
{
    public class ReservationNotMade
    {
        public Guid Id { get; private set; }
        public Guid HotelId { get; private set; }
        public string RoomType { get; private set; }
        public string Reason { get; private set; }

        public ReservationNotMade(Guid id, Guid hotelId, string roomType, string reason)
        {
            Id = id;
            HotelId = hotelId;
            RoomType = roomType;
            Reason = reason;
        }
    }
}
