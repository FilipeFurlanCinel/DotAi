using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace CrossCutting;

public static class MediatRRegistration
{
    public static IServiceCollection AddMediatRServices(this IServiceCollection services)
    {
        services.AddMediatR(typeof(MediatRRegistration).Assembly);
        return services;
    }
}
