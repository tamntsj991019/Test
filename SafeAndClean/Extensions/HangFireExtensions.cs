using Microsoft.Extensions.DependencyInjection;
using Hangfire;
using Hangfire.Dashboard;

namespace SafeAndClean.Extensions
{
    public static class HangFireExtensions
    {
        public static void ConfigHangFire(this IServiceCollection services, string dbConnection)
        {
            services.AddHangfire(x => x.UseSqlServerStorage(dbConnection));
            services.AddHangfireServer();
        }
    }

    public class MyAuthorizationFilter : IDashboardAuthorizationFilter
    {
        public bool Authorize(DashboardContext context)
        {
            return true;
        }
    }
}
