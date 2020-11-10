using CQRS.Essentials.Abstractions.CQRS;
using System;

namespace Reservations.Domain.Commands
{
    public class MakeReservation : ICommand
    {
        public Guid Id { get; private set; }
        public Guid HotelId { get; private set; }
        public string RoomType { get; private set; }
        public int TotalRoomTypeAvailable { get; set; }

        public MakeReservation(Guid id, Guid hotelId, string roomType, int totalRoomTypeAvailable)
        {
            Id = id;
            HotelId = hotelId;
            RoomType = roomType;
            TotalRoomTypeAvailable = totalRoomTypeAvailable;
        }
    }
}
