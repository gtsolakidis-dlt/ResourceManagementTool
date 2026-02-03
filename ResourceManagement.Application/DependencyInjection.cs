using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using FluentValidation;
using MediatR;
using AutoMapper;
using ResourceManagement.Domain.Interfaces;
using ResourceManagement.Domain.Services;

namespace ResourceManagement.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            var assembly = typeof(DependencyInjection).Assembly;

            services.AddMediatR(cfg => {
                cfg.RegisterServicesFromAssembly(assembly);
                cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ResourceManagement.Application.Common.Behaviors.AuditBehavior<,>));
            });

            services.AddValidatorsFromAssembly(assembly);
            services.AddAutoMapper(assembly);

            // Domain Services
            services.AddScoped<IFinancialCalculationService, FinancialCalculationService>();

            // Part 3: Snapshot Recalculation Service
            services.AddScoped<ISnapshotRecalculationService, SnapshotRecalculationService>();

            return services;
        }
    }
}


