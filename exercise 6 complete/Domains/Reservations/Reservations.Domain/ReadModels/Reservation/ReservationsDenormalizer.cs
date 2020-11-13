using CQRS.Essentials.Abstractions.CQRS;
using Reservations.Domain.Events;
using System.Threading;
using System.Threading.Tasks;

namespace Reservations.Domain.ReadModels.Reservation
{
    public class ReservationsDenormalizer
    {
        public ReservationsDenormalizer(IBuilder builder)
        {
            builder.RegisterDenormalizer(new DenormalizerDesc(typeof(ReservationsReadModel)));
            builder.RegisterEventHandler<ReservationsReadModel, ReservationMade>(OnReservationMade); //virtual workshop ex-8 hint
        }

        //virtual workshop ex-8 hint
        private async Task OnReservationMade(IDenormalizerContext<ReservationsReadModel> ctx, ReservationMade @event)
        {
            var reservation = new ReservationsReadModel
            {
                Id = @event.Id,
                HotelId = @event.HotelId,
                RoomType = @event.RoomType,
                IsReserved = true
            };
            await ctx.Repository.Save(reservation, new CancellationToken());
        }
    }
}