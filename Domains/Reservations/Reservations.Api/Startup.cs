using Reservations.Api.Handlers.Query;
using Reservations.Api.Queries;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using EventStore.ClientAPI;
using Infrastructure.EventStore;
using Infrastructure.Storage.Abstractions;
using Reservations.Domain.ReadModels.Reservation;
using Reservations.Domain.ReadModels.RoomTypeAvailability;
using Reservations.Api.Handlers.Command;
using Reservations.Domain.Commands;
using Reservations.Domain.Aggregates;
using CQRS.Essentials.Abstractions.DDD;
using CQRS.Essentials.Abstractions.CQRS;
using CQRS.Essentials.Abstractions.ES;
using CQRS.Essentials.ES;
using CQRS.Essentials.CQRS;
using CQRS.Essentials.DDD;
using Infrastructure.EventStore.Abstractions;
using Infrastructure.Sqlite.Storage;
using Microsoft.Data.Sqlite;

[assembly: FunctionsStartup(typeof(Reservations.Api.Startup))]
namespace Reservations.Api
{
    public class Startup : FunctionsStartup
    {
        private static SqliteConnection _databaseConnection = null;

        public override void Configure(IFunctionsHostBuilder hostBuilder)
        {
            var configurationBuilder = new ConfigurationBuilder();
            configurationBuilder.AddEnvironmentVariables();
            var configuration = configurationBuilder.Build();

            //set up event store
            hostBuilder.Services.AddSingleton<IEventStoreConnection>(s => new EventStoreConnectionFactory(configuration).Create(s));
            hostBuilder.Services.AddSingleton<IEventStore, Infrastructure.EventStore.EventStore>(s =>
            {
                var con = s.GetRequiredService<IEventStoreConnection>();
                var eventstore = new Infrastructure.EventStore.EventStore(con);
                return eventstore;
            });

            var configRoot = new ConfigurationBuilder().AddEnvironmentVariables().Build();
            hostBuilder.Services.AddSingleton(sp => configRoot);
            //ensure one connection for function app
            if (_databaseConnection == null)
            {
                var dbName = configRoot["ReservationsDatabaseName"];
                _databaseConnection = new SqliteConnection("Datasource=./../../../" + dbName);
                _databaseConnection.Open();
            }
            ////repo factory set up
            var repositoryFactory = new SQLRepositoryFactory(_databaseConnection);
            hostBuilder.Services.AddScoped<IRepositoryFactory>(x =>
            {
                return repositoryFactory;
            });
            //bus wiring
            var bus = new DirectBus();
            var builder = new Builder(repositoryFactory);
            //register read model denormalizers
            RegisterReadModelDenormalizers(builder);
            //register builder handler with bus
            bus.RegisterEventHandler(builder.Handle);
            //register event store wrapper client
            var serviceProvider = hostBuilder.Services.BuildServiceProvider();
            var eventStore = serviceProvider.GetRequiredService<IEventStore>();
            var eventStoreClient = new EventStoreClient(bus, eventStore);
            hostBuilder.Services.AddSingleton<IEventStoreClient>(sp => { return eventStoreClient; });
            //set up aggregate factories di
            RegisterAggregateFactories(hostBuilder);
            //set up command handlers
            RegisterCommandHandlers(hostBuilder, bus, eventStoreClient);
            //set up query handlers
            RegisterQueryHandlers(hostBuilder, bus);
            //register bus
            hostBuilder.Services.AddSingleton<IBus>(sp => { return bus; });
        }

        private void RegisterAggregateFactories(IFunctionsHostBuilder hostBuilder)
        {
            //aggregate factories registrations
            hostBuilder.Services.AddTransient<IAggregateFactory<Reservation>, AggregateFactory<Reservation>>();
        }

        private void RegisterCommandHandlers(IFunctionsHostBuilder hostBuilder, IBus bus, IEventStoreClient eventStoreClient)
        {
            var serviceProvider = hostBuilder.Services.BuildServiceProvider();
            //aggregate factories
            var reservationFactory = serviceProvider.GetRequiredService<IAggregateFactory<Reservation>>();
            //command handler registrations
            bus.RegisterCommandHandler<MakeReservation>(new MakeReservationCommandHandler(reservationFactory, eventStoreClient).Handle);
        }

        private void RegisterQueryHandlers(IFunctionsHostBuilder hostBuilder, IBus bus)
        {
            var serviceProvider = hostBuilder.Services.BuildServiceProvider();
            var repositoryFactory = serviceProvider.GetRequiredService<IRepositoryFactory>();
            //query handler registrations
            bus.RegisterQueryHandler<FindReservationQuery, ReservationsReadModel>(new FindReservationQueryHandler(repositoryFactory).Handle);
            bus.RegisterQueryHandler<FindRoomTypeAvailabilityQuery, RoomTypeAvailabilityReadModel>(new FindRoomTypeAvailabilityQueryHandler(repositoryFactory).Handle);
        }

        private void RegisterReadModelDenormalizers(IBuilder builder)
        {
            //read model denormalizers
            new ReservationsDenormalizer(builder);
            new RoomTypeAvailabilityDenormalizer(builder);
        }
    }
}
