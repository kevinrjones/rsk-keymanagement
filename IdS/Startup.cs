﻿// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using IdentityModel;
using IdentityServer4;
using IdentityServer4.KeyManagement.EntityFramework;
using IdentityServer4.Quickstart.UI;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;

namespace IdS
{
    public class Startup
    {
        public IWebHostEnvironment Environment { get; }
        public IConfiguration Configuration { get; }

        public Startup(IWebHostEnvironment environment, IConfiguration configuration)
        {
            Environment = environment;
            Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews();

            // configures IIS out-of-proc settings (see https://github.com/aspnet/AspNetCore/issues/14882)
            services.Configure<IISOptions>(iis =>
            {
                iis.AuthenticationDisplayName = "Windows";
                iis.AutomaticAuthentication = false;
            });

            // configures IIS in-proc settings
            services.Configure<IISServerOptions>(iis =>
            {
                iis.AuthenticationDisplayName = "Windows";
                iis.AutomaticAuthentication = false;
            });

            var connectionString = Configuration.GetConnectionString("DefaultConnection");

            var builder = services.AddIdentityServer(options =>
                {
                    options.Events.RaiseErrorEvents = true;
                    options.Events.RaiseInformationEvents = true;
                    options.Events.RaiseFailureEvents = true;
                    options.Events.RaiseSuccessEvents = true;
                })
                .AddTestUsers(TestUsers.Users)
                // this adds the config data from DB (clients, resources, CORS)
                .AddConfigurationStore(options =>
                {
                    options.ConfigureDbContext = builder => builder.UseSqlite(connectionString);
                })
                // this adds the operational data from DB (codes, tokens, consents)
                .AddOperationalStore(options =>
                {
                    options.ConfigureDbContext = builder => builder.UseSqlite(connectionString);

                    // this enables automatic token cleanup. this is optional.
                    options.EnableTokenCleanup = true;
                }).AddSigningKeyManagement(options =>
                {
                    options.Licensee = "DEMO";
                    options.License = "eyJTb2xkRm9yIjowLjAsIktleVByZXNldCI6NiwiU2F2ZUtleSI6ZmFsc2UsIkxlZ2FjeUtleSI6ZmFsc2UsIlJlbmV3YWxTZW50VGltZSI6IjAwMDEtMDEtMDFUMDA6MDA6MDAiLCJhdXRoIjoiREVNTyIsImV4cCI6IjIwMjAtMDUtMTZUMDA6MDA6MDAiLCJpYXQiOiIyMDIwLTA0LTE2VDA4OjEwOjA3Iiwib3JnIjoiREVNTyIsImF1ZCI6NX0=.FNuHlEm+muSRcR5z0wTKFV2tXJ2xn5QmK7HddlKLkO3SaG+1ykzlLviY+XRKAL5o745PSQ9qV3BiJMWV0CV9b5qGnxXQjVi1DKQHA8MdCH02wKul7W5ZGPPDyZp6ppB7IXG5cQeLLNeKNTo4QnYTSqHTUur7rHbAVNSXIsuEahb3KtQSGnsCX+H+n9Mqruvf/N6zHFE9QqYrVZ7zQVWOjrO5s7qDc3Kf0xFKayW1BJzNe6uz/Fe2yFgiCVEihU/OHpM1tSAyDVUdBehGz7qKhd6jRtsB8ZLEITqz3Z56GfW26h1ACnQQz500PNBxUkLYiBIb5pZisnppRaPIVdMJSWLozEp87fmafS2sfokh48SPqILTP9kh3m2AzaDozqKXS3ll3EKMNyBorJPdWwQpQJKa1D52wLO/O0VSpxN781tR7qb4VSwJTP8dehGHcrlkPOTdR2g8irknK2+upsojAaCwPIUPfdTuRnKZtVIZMkAh5HLB+I8c8vwNhXOM+tg6TY9XVqqz32RomX9TJti7LdfL8ABFqPnyoB9lK3A89jUV2gpy70IriaVLsHl2piNGAus5+sJzY+x5x+rZIYvn5GThDkbyWGo2cor8aUzlQ2SILaSsV5bwzG3q5mLbLQFTmxnjpqfY27d9DDzLGo5KDU4j+pKMlBKL7sfpZfBg4gc=";
                    options.KeyActivationDelay = TimeSpan.FromSeconds(20);
                    options.KeyExpiration = TimeSpan.FromSeconds(40);
                    options.KeyRetirement = TimeSpan.FromSeconds(80);
                }).PersistKeysToDatabase(new DatabaseKeyManagementOptions {
                    ConfigureDbContext = 
                        efOptions => efOptions.UseSqlite(connectionString)
                }).ProtectKeysWithDataProtection()
                .EnableInMemoryCaching();

            var certName = "CN=IdsDemo";
            var cert = X509.CurrentUser
                .My.SubjectDistinguishedName
                .Find(certName, false)
                .FirstOrDefault();
            
            services.AddDataProtection()
                .PersistKeysToDatabase(new DatabaseKeyManagementOptions
                {
                    ConfigureDbContext =
                        efOptions => efOptions.UseSqlite(connectionString)
                }).ProtectKeysWithCertificate(cert);
            
            // not recommended for production - you need to store your key material somewhere secure
            // builder.AddDeveloperSigningCredential();

            services.AddAuthentication()
                .AddGoogle(options =>
                {
                    options.SignInScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme;

                    // register your IdentityServer with Google at https://console.developers.google.com
                    // enable the Google+ API
                    // set the redirect URI to http://localhost:5000/signin-google
                    options.ClientId = "copy client ID from Google here";
                    options.ClientSecret = "copy client secret from Google here";
                });
        }

        public void Configure(IApplicationBuilder app)
        {
            if (Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
            }

            app.UseStaticFiles();

            app.UseRouting();
            app.UseIdentityServer();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapDefaultControllerRoute();
            });
        }
    }
}
