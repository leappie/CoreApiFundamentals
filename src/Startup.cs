using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using AutoMapper;
using CoreCodeCamp.Controllers;
using CoreCodeCamp.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.AspNetCore.Mvc.Versioning.Conventions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CoreCodeCamp
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<CampContext>();
            services.AddScoped<ICampRepository, CampRepository>();

            services.AddAutoMapper(Assembly.GetExecutingAssembly()); // Points at the current project and look for Profile classes that drive from Profile

            services.AddApiVersioning(opt =>
            {
                opt.AssumeDefaultVersionWhenUnspecified = true; // assumes default api to 1.1
                opt.DefaultApiVersion = new ApiVersion(1, 1); // set manually default api
                opt.ReportApiVersions = true; // returns a header with support api versions
                //opt.ApiVersionReader = new QueryStringApiVersionReader("ver"); // set the query string ver= instead of api-version=
                //opt.ApiVersionReader = new HeaderApiVersionReader("X-Version"); // set the name of the header for api version
                //opt.ApiVersionReader = ApiVersionReader.Combine( // use to support multiple api versioning
                //    new HeaderApiVersionReader("X-Version"),
                //    new QueryStringApiVersionReader("ver", "version"));
                opt.ApiVersionReader = new UrlSegmentApiVersionReader();
                //opt.Conventions.Controller<TalksController>() // set api versions per controller in a central place
                //    .HasApiVersion(new ApiVersion(1, 0));
                //opt.Conventions.Controller<TalksController>()
                //    .HasApiVersion(new ApiVersion(1, 1));
                //opt.Conventions.Controller<TalksController>()
                //    .Action(c => c.DeleteTalk(default(string), default(int)))
                //    .MapToApiVersions(1, 1);

            });

            services.AddControllers();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(cfg =>
            {
                cfg.MapControllers();
            });
        }
    }
}
