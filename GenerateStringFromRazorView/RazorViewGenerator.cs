using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Internal;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.PlatformAbstractions;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace HashCode.Common.AspNet
{
    public class RazorViewGenerator
    {
        private readonly IRazorViewEngine viewEngine;
        private readonly ITempDataProvider tempDataProvider;
        private readonly ActionContext actionContext;

        public static RazorViewGenerator Create(string viewsBasePath = null)
        {
            var services = new ServiceCollection();
            var applicationEnvironment = PlatformServices.Default.Application;
            var diagnosticSource = new DiagnosticListener("Microsoft.AspNetCore");
            var basePath = viewsBasePath ?? applicationEnvironment.ApplicationBasePath;

            services
                .AddSingleton<IHostingEnvironment>(new HostingEnvironment() { })
                .AddSingleton(applicationEnvironment)
                .AddSingleton<DiagnosticSource>(diagnosticSource)
                .AddSingleton<FakeHttpContext>()
                .AddSingleton<RazorViewGenerator>();

            services.Configure<RazorViewEngineOptions>(options =>
            {
                options.FileProviders.Clear();
                options.FileProviders.Add(new PhysicalFileProvider(basePath));
            });

            services.AddLogging().AddMvc();

            var servicesProvider = services.BuildServiceProvider();
            var instance = servicesProvider.GetRequiredService<RazorViewGenerator>();
            return instance;
        }

        public RazorViewGenerator(
            IRazorViewEngine viewEngine,
            ITempDataProvider tempDataProvider,
            FakeHttpContext httpContext)
        {
            this.viewEngine = viewEngine;
            this.tempDataProvider = tempDataProvider;
            this.actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor());
        }

        public async Task<string> RenderViewToString(string name, object model)
        {
            var viewEngineResult = viewEngine.FindView(this.actionContext, name, false);

            if (viewEngineResult.Success == false)
            {
                throw new InvalidOperationException($"Couldn't find view '{name}'. Searched locations: " + string.Join(", ", viewEngineResult.SearchedLocations));
            }

            var view = viewEngineResult.View;

            var viewDataDictionnary = new ViewDataDictionary(
                metadataProvider: new EmptyModelMetadataProvider(),
                modelState: new ModelStateDictionary())
            {
                Model = model
            };

            using (var output = new StringWriter())
            {
                var viewContext = new ViewContext(
                    actionContext,
                    view,
                    viewDataDictionnary,
                    new TempDataDictionary(actionContext.HttpContext, tempDataProvider),
                    output,
                    new HtmlHelperOptions());

                await view.RenderAsync(viewContext);
                return output.ToString();
            }
        }

        public class FakeHttpContext : DefaultHttpContext
        {
            public FakeHttpContext(IServiceProvider serviceProvider)
            {
                this.RequestServices = serviceProvider;
            }
        }
    }
}
