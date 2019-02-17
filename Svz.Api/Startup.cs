using AutoMapper;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Svz.Common;

namespace Svz.Api
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
            services.AddCors();
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

            Mapper.Initialize(cfg =>
            {
                cfg.CreateMap<BookSearchView, BookView>();
                cfg.CreateMap<Book, BookView>();
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseCors(
                options => options.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin().WithExposedHeaders("X-From-Redis", "X-Node-Name")
            );
            
            if (env.IsDevelopment())
                app.UseDeveloperExceptionPage();
            else
                app.UseHsts();

            app.Use(async (context, next) =>
            {
                context.Response.Headers.Add("X-Node-Name", System.Environment.MachineName);
                await next.Invoke();
            });
            
            app.UseHttpsRedirection();
            app.UseMvc();
        }
    }
}