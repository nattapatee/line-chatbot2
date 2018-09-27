using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using centralloggerbot.Models;
using centralloggerbot.Middleware;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.EntityFrameworkCore;
using CentralLogger;

namespace centralloggerbot
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            var connectionString = Configuration.GetValue("ConnectionString", "");
            var envConnectionString = Environment.GetEnvironmentVariable("CENTRAL_LOGGER_CS");

            if (!string.IsNullOrEmpty(envConnectionString))
            {
                connectionString = envConnectionString;
            }

            Console.WriteLine($"ConnectionString = {connectionString}");
            services.Configure<AppSettings>(Configuration);
            services.AddDbContext<CentralLoggerContext>(options => options.UseNpgsql(connectionString));
            services.AddMvc();

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            app.UseLineValidationMiddleware(Configuration.GetSection("LineSettings")["ChannelSecret"]);
            app.UseMvc();
        }
    }
}
