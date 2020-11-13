using System.Threading.Tasks;
using System.Threading;
using Reservations.Domain.Commands;
using Reservations.Domain.Aggregates;
using CQRS.Essentials.Abstractions.CQRS;
using CQRS.Essentials.Abstractions.DDD;
using CQRS.Essentials.Abstractions.ES;

namespace Reservations.Api.Handlers.Command
{
    public class CancelReservationCommandHandler : ICommandHandler<CancelReservation>
    {
        private readonly IAggregateFactory<Reservation> _reservationFactory;
        private readonly IEventStoreClient _eventStoreClient;

        public CancelReservationCommandHandler(IAggregateFactory<Reservation> reservationFactory, IEventStoreClient eventStoreClient)
        {
            _reservationFactory = reservationFactory;
            _eventStoreClient = eventStoreClient;
        }

        public async Task<object[]> Handle(CancelReservation command, CancellationToken cancellationToken)
        {
            var reservationId = command.Id;
            //use factory to get entity info
            var reservation = await _reservationFactory.Get(reservationId, cancellationToken);
            //persist and publish
            await _eventStoreClient.Save(reservation, reservationId);
            //return events
            return new object[] { };
        }
    }
}