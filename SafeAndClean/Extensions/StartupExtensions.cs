using Data.DbContext;
using Data.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Hubs;
using Services.Core;

namespace SafeAndClean.Extensions
{
    public static class StartupExtensions
    {
        public static void ConfigIdentityDbContext(this IServiceCollection services, string dbConnection)
        {
            services.AddDbContext<AppDbContext>(options => options.UseLazyLoadingProxies().UseSqlServer(dbConnection));
            //services.AddDbContext<AppDbContext>(options => options.UseSqlServer(dbConnection));
            
            services.AddIdentity<User, IdentityRole>(options =>
            {
                options.User.RequireUniqueEmail = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireDigit = false;
                options.Password.RequiredLength = 6;
                options.Password.RequireLowercase = false;
            })
                    .AddEntityFrameworkStores<AppDbContext>()
                    .AddDefaultTokenProviders();
        }

        public static void BusinessServices(this IServiceCollection services)
        {
            services.AddSingleton<IBookingHub, BookingHub>();
            services.AddSingleton<IChatHub, ChatHub>();
            services.AddSingleton<INotificationHub, NotificationHub>();

            services.AddScoped<IAccountService, AccountService>();
            services.AddScoped<IBookingService, BookingService>();
            services.AddScoped<IBookingImageService, BookingImageService>();
            services.AddScoped<IBookingLogService, BookingLogService>();
            services.AddScoped<IBookingStatusService, BookingStatusService>();
            services.AddScoped<IChartService, ChartService>();
            services.AddScoped<ICleaningToolService, CleaningToolService>();
            services.AddScoped<IFeedbackService, FeedbackService>();
            services.AddScoped<IIntervalService, IntervalService>();
            services.AddScoped<IMessageService, MessageService>();
            services.AddScoped<INotificationService, NotificationService>();
            services.AddScoped<IRequestCleaningToolService, RequestCleaningToolService>();
            services.AddScoped<IScheduleService, ScheduleService>();
            services.AddScoped<IServiceBookingService, ServiceBookingService>();
            services.AddScoped<IServiceService, ServiceService>();
            services.AddScoped<IServiceGroupService, ServiceGroupService>();
            services.AddScoped<ISendMailService, SendMailService>();
            services.AddScoped<ISubPackService, SubPackService>();
            services.AddScoped<ISettingService, SettingService>();
            services.AddScoped<ITransactionService, TransactionService>();

            services.AddScoped<IAlgorithmService, AlgorithmService>();
            services.AddScoped<IGoogleService, GoogleService>();
        }

        public static void ConfigCors(this IServiceCollection services)
        {
            services.AddCors(options => options.AddPolicy("AllowAll", builder => builder.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin()));
        }
    }
}
