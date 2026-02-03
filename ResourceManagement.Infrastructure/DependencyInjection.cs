using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using ResourceManagement.Infrastructure.Persistence;
using ResourceManagement.Infrastructure.Persistence.Repositories;
using ResourceManagement.Domain.Interfaces;

namespace ResourceManagement.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<DapperContext>();
            services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<DapperContext>());

            // Register repositories
            services.AddScoped<IRosterRepository, RosterRepository>();
            services.AddScoped<IProjectRepository, ProjectRepository>();
            services.AddScoped<IForecastRepository, ForecastRepository>();
            services.AddScoped<IBillingRepository, BillingRepository>();
            services.AddScoped<IExpenseRepository, ExpenseRepository>();
            services.AddScoped<IOverrideRepository, OverrideRepository>();
            services.AddScoped<IAuditRepository, AuditRepository>();
            services.AddScoped<IApiRequestLogRepository, ApiRequestLogRepository>();
            services.AddScoped<IGlobalRateRepository, GlobalRateRepository>();
            services.AddScoped<IExcelService, ResourceManagement.Infrastructure.Persistence.Services.ExcelService>();

            // New repositories for Part 2 & Part 3
            services.AddScoped<IProjectRateRepository, ProjectRateRepository>();
            services.AddScoped<IProjectMonthlySnapshotRepository, ProjectMonthlySnapshotRepository>();

            return services;
        }

    }
}

