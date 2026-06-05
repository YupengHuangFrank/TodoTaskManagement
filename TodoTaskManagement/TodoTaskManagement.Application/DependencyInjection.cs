using Microsoft.Extensions.DependencyInjection;
using TodoTaskManagement.Application.Authentication;
using TodoTaskManagement.Application.Tasks;

namespace TodoTaskManagement.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<ITaskService, TaskService>();
        return services;
    }
}
