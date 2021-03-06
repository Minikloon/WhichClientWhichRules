﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using WhichClientWhichRules.Models;

namespace WhichClientWhichRules
{
	public class Startup
	{
		public Startup(IHostingEnvironment env)
		{
			var builder = new ConfigurationBuilder()
				.SetBasePath(env.ContentRootPath)
				.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
				.AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
				.AddEnvironmentVariables();
			Configuration = builder.Build();
		}

		public IConfigurationRoot Configuration { get; }

		// This method gets called by the runtime. Use this method to add services to the container.
		public void ConfigureServices(IServiceCollection services)
		{
			// Add authentication services
			services.AddAuthentication(options =>
			{
				options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
			});

			// Add framework services.
			services.AddMvc();

			// Add functionality to inject IOptions<T>
			services.AddOptions();

			// Add the Auth0 Settings object so it can be injected
			services.Configure<Auth0Settings>(Configuration.GetSection("Auth0"));

			// Add the SiteOptions object so it can be injected
			services.Configure<SiteOptions>(Configuration.GetSection("Site"));
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory, IOptions<Auth0Settings> auth0Settings)
		{
			loggerFactory.AddConsole(Configuration.GetSection("Logging"));
			loggerFactory.AddDebug();

			if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
				app.UseBrowserLink();
			}
			else
			{
				app.UseExceptionHandler("/Home/Error");
			}

			app.UseStaticFiles();

			// Add the cookie middleware
			app.UseCookieAuthentication(new CookieAuthenticationOptions
			{
				AutomaticAuthenticate = true,
				AutomaticChallenge = true
			});

			// Add the OIDC middleware
			var options = new OpenIdConnectOptions("Auth0")
			{
				Authority = $"https://{auth0Settings.Value.Domain}",
				
				ClientId = auth0Settings.Value.ClientId,
				ClientSecret = auth0Settings.Value.ClientSecret,
				
				AutomaticAuthenticate = false,
				AutomaticChallenge = false,
				
				ResponseType = "code",

				CallbackPath = new PathString("/signin-auth0"),
				
				ClaimsIssuer = "Auth0",

				Events = new OpenIdConnectEvents
				{
					OnRemoteFailure = context =>
					{
						if (context.Failure.Message.Contains("unauthorized"))
						{
							context.Response.Redirect("/Account/AccessDenied");
							context.HandleResponse();
						}

						return Task.CompletedTask;
					},
					OnRedirectToIdentityProviderForSignOut = (context) =>
					{
						var logoutUri = $"https://{auth0Settings.Value.Domain}/v2/logout?client_id={auth0Settings.Value.ClientId}";

						var postLogoutUri = context.Properties.RedirectUri;
						if (!string.IsNullOrEmpty(postLogoutUri))
						{
							if (postLogoutUri.StartsWith("/"))
							{
								// transform to absolute
								var request = context.Request;
								postLogoutUri = request.Scheme + "://" + request.Host + request.PathBase + postLogoutUri;
							}
							logoutUri += $"&returnTo={ Uri.EscapeDataString(postLogoutUri)}";
						}

						context.Response.Redirect(logoutUri);
						context.HandleResponse();

						return Task.CompletedTask;
					}
				}
			};
			options.Scope.Clear();
			options.Scope.Add("openid");
			app.UseOpenIdConnectAuthentication(options);

			app.UseMvc(routes =>
			{
				routes.MapRoute(
					name: "default",
					template: "{controller=Home}/{action=Index}/{id?}");
			});
		}
	}
}
