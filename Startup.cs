﻿/*
 * Copyright (c) 2018 Google Inc.
 *
 * Licensed under the Apache License, Version 2.0 (the "License"); you may not
 * use this file except in compliance with the License. You may obtain a copy of
 * the License at
 *
 * http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS, WITHOUT
 * WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the
 * License for the specific language governing permissions and limitations under
 * the License.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HelloWorld
{
  // [START sample]
  public class Startup
  {
    public Startup(IConfiguration configuration)
    {
      Configuration = configuration;
    }

    public IConfiguration Configuration { get; }
    // This method gets called by the runtime. Use this method to add services to the container.
    // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
    public void ConfigureServices(IServiceCollection services)
    {
      services.Configure<CookiePolicyOptions>(options =>
        {
          options.CheckConsentNeeded = context => true;
          options.MinimumSameSitePolicy = SameSiteMode.None;
        });

      services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

      services.AddCors(options => options.AddPolicy("CorsPolicy",
      builder =>
      {
        builder.AllowAnyMethod().AllowAnyHeader()
                     .WithOrigins("http://localhost:55830")
                     .AllowCredentials();
      }));

      services.AddSignalR();
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IHostingEnvironment env)
    {
      if (env.IsDevelopment())
      {

        app.UseDeveloperExceptionPage();
      }
      else
      {
        app.UseExceptionHandler("/Error");
        app.UseHsts();
      }

      app.UseHttpsRedirection();
      app.UseStaticFiles();
      app.UseCookiePolicy();
      app.UseCors("CorsPolicy");
      app.UseSignalR(routes =>
      {
        routes.MapHub<ChatHub>("/chatHub");
      });
      app.UseMvc();
    }
  }
  // [END sample]
}
