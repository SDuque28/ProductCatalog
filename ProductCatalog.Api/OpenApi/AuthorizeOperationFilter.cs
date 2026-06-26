using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace ProductCatalog.Api.OpenApi;

public class AuthorizeOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var methodInfo = context.MethodInfo;
        var declaringType = methodInfo.DeclaringType;

        var allowAnonymous = methodInfo.GetCustomAttributes(true).OfType<AllowAnonymousAttribute>().Any()
            || (declaringType?.GetCustomAttributes(true).OfType<AllowAnonymousAttribute>().Any() ?? false);

        if (allowAnonymous)
        {
            return;
        }

        var requiresAuthorization = methodInfo.GetCustomAttributes(true).OfType<AuthorizeAttribute>().Any()
            || (declaringType?.GetCustomAttributes(true).OfType<AuthorizeAttribute>().Any() ?? false);

        if (!requiresAuthorization)
        {
            return;
        }

        operation.Security ??= new List<OpenApiSecurityRequirement>();
        operation.Security.Add(new OpenApiSecurityRequirement
        {
            [
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = JwtBearerDefaults.AuthenticationScheme
                    }
                }
            ] = Array.Empty<string>()
        });

        operation.Responses.TryAdd("401", new OpenApiResponse { Description = "Unauthorized" });
        operation.Responses.TryAdd("403", new OpenApiResponse { Description = "Forbidden" });
    }
}
