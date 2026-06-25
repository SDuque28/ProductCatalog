using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using ProductCatalog.Api.Interfaces;

namespace ProductCatalog.Tests.Controllers;

public class TestProductApiFactory : WebApplicationFactory<Program>
{
    private readonly IProductService _productService;

    public TestProductApiFactory(IProductService productService)
    {
        _productService = productService;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Production");

        builder.ConfigureLogging(logging =>
        {
            logging.ClearProviders();
        });

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<IProductService>();
            services.AddScoped(_ => _productService);
        });
    }
}
