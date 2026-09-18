using AcademiaDoZe.Application.Interfaces;
using AcademiaDoZe.Application.Services;
using Microsoft.Extensions.DependencyInjection;
// Mario Cesar Alves Júnior

namespace AcademiaDoZe.Application.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplication(
        this IServiceCollection services)
    {
        services.AddScoped<ILogradouroService, LogradouroService>();
        services.AddScoped<IAlunoService, AlunoService>();
        services.AddScoped<IColaboradorService, ColaboradorService>();
        services.AddScoped<IMatriculaService, MatriculaService>();

        return services;
    }
}