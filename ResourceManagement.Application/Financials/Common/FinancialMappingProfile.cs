using AutoMapper;
using ResourceManagement.Domain.Entities;
using ResourceManagement.Contracts.Financials;

namespace ResourceManagement.Application.Financials.Common
{
    public class FinancialMappingProfile : Profile
    {
        public FinancialMappingProfile()
        {
            CreateMap<Billing, BillingDto>();
            CreateMap<Expense, ExpenseDto>();
            CreateMap<Override, OverrideDto>();
        }
    }
}
