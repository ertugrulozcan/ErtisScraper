using ErtisScraper.Abstractions;
using ErtisScraper.Extensions.AspNetCore;
using ErtisScraper.Samples.DiyanetOrg.Services;
using ErtisScraper.Samples.DiyanetOrg.Services.Interfaces;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;

namespace ErtisScraper.Samples.DiyanetOrg
{
	public class Startup
	{
		#region Properties

		public IConfiguration Configuration { get; }

		#endregion

		#region Constructors

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="configuration"></param>
		public Startup(IConfiguration configuration)
		{
			this.Configuration = configuration;
		}

		#endregion

		#region Methods

		public void ConfigureServices(IServiceCollection services)
		{
			services.AddSingleton<ITargetProvider, ConfigurationTargetProvider>();
			services.AddSingleton<ICrawlerProvider, CrawlerProvider>();
			
			services.AddControllers();
			services.AddSwaggerGen(c =>
			{
				c.SwaggerDoc("v1", new OpenApiInfo { Title = "ErtisScraper.Samples.DiyanetOrg", Version = "v1" });
			});
		}

		public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
		{
			if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
				app.UseSwagger();
				app.UseSwaggerUI(c =>
					c.SwaggerEndpoint("/swagger/v1/swagger.json", "ErtisScraper.Samples.DiyanetOrg v1"));
			}

			app.UseHttpsRedirection();
			app.UseRouting();
			app.UseAuthorization();

			app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
		}

		#endregion
	}
}