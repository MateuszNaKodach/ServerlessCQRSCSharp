using CQRS.Essentials.Abstractions.CQRS;
using Reservations.Domain.Events;
using System.Threading;
using System.Threading.Tasks;

namespace Reservations.Domain.ReadModels.RoomTypeAvailability
{
    public class RoomTypeAvailabilityDenormalizer
    {
        public RoomTypeAvailabilityDenormalizer(IBuilder builder)
        {
            builder.RegisterDenormalizer(new DenormalizerDesc(typeof(RoomTypeAvailabilityReadModel)));
            builder.RegisterEventHandler<RoomTypeAvailabilityReadModel, ReservationMade>(OnReservationMade); //virtual workshop ex-8 hint
        }

        //virtual workshop ex-8 hint
        private async Task OnReservationMade(IDenormalizerContext<RoomTypeAvailabilityReadModel> ctx, ReservationMade @event)
        {
            var roomTypeAvailability = await ctx.Repository.Read(new { @event.HotelId, @event.RoomType }, new CancellationToken());
            if (roomTypeAvailability != null)
            {
                roomTypeAvailability.Amount -= 1;
                await ctx.Repository.Save(roomTypeAvailability, new CancellationToken());
            }
        }
    }
}