using Reservations.Domain.Events;
using Reservations.Domain.Commands;
using System;
using System.Collections.Generic;
using CQRS.Essentials.DDD;

namespace Reservations.Domain.Aggregates
{
    public class Reservation : AggregateBase<Reservation.State>
    {
        public Reservation()
        {
            RegisterTransition<ReservationMade>(Apply);
            RegisterTransition<ReservationNotMade>(Apply);
        }

        public struct State
        {
            public Guid Id { get; set; }
            public Guid HotelId { get; set; }
            public string RoomType { get; set; }
            public bool IsReserved { get; set; }
            public string Reason { get; set; }
        }

        public IEnumerable<object> MakeReservation(MakeReservation command)
        {
            if(command.TotalRoomTypeAvailable > 0)
            {
                RaiseEvent(new ReservationMade(command.Id, command.HotelId, command.RoomType));
            }else
            {
                RaiseEvent(new ReservationNotMade(command.Id, command.HotelId, command.RoomType, $"The room type {command.RoomType} is currently not available"));
            }
            return base.UnCommitedEvents;
        }

        private State Apply(State state, ReservationMade @event)
        {
            state.Id = @event.Id;
            state.HotelId = @event.HotelId;
            state.RoomType = @event.RoomType;
            state.IsReserved = true;
            return state;
        }

        private State Apply(State state, ReservationNotMade @event)
        {
            state.Id = @event.Id;
            state.HotelId = @event.HotelId;
            state.RoomType = @event.RoomType;
            state.IsReserved = false;
            state.Reason = @event.Reason;
            return state;
        }
    }
}
