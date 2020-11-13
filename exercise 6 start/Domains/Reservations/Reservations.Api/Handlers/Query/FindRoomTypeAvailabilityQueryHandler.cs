using System.Threading;
using System.Threading.Tasks;
using CQRS.Essentials.Abstractions.CQRS;
using Infrastructure.Storage.Abstractions;
using Reservations.Api.Queries;
using Reservations.Domain.ReadModels.RoomTypeAvailability;

namespace Reservations.Api.Handlers.Query
{
    public class FindRoomTypeAvailabilityQueryHandler : IQueryHandler<FindRoomTypeAvailabilityQuery, RoomTypeAvailabilityReadModel>
    {
        private readonly IRepositoryFactory _repositoryFactory;

        public FindRoomTypeAvailabilityQueryHandler(IRepositoryFactory repositoryFactory)
        {
            _repositoryFactory = repositoryFactory;
        }

        public async Task<RoomTypeAvailabilityReadModel> Handle(FindRoomTypeAvailabilityQuery query, CancellationToken cancellationToken)
        {
            var repo = _repositoryFactory.Create<RoomTypeAvailabilityReadModel>();
            return await repo.Read(query, cancellationToken);
        }
    }
}
