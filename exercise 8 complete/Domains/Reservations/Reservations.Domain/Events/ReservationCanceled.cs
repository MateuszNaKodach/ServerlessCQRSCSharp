using System;

namespace Reservations.Domain.Events
{
    public class ReservationCanceled
    {
        public Guid Id { get; private set; }
        public Guid HotelId { get; private set; }
        public string RoomType { get; private set; }

        public ReservationCanceled(Guid id, Guid hotelId, string roomType)
        {
            Id = id;
            HotelId = hotelId;
            RoomType = roomType;
        }
    }
}
