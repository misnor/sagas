using MassTransit;
using Microsoft.Extensions.Hosting;

namespace Sample.Service
{
    public class MasTransitConsoleHostedService : IHostedService
    {
        private readonly IBusControl bus;

        public MasTransitConsoleHostedService(IBusControl bus)
        {
            this.bus = bus;
            
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await bus.StartAsync(cancellationToken).ConfigureAwait(false);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return bus.StopAsync(cancellationToken);
        }
    }
}