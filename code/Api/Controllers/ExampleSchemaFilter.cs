using Api.QueryHandlers.Portfolio;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Text.Json.Nodes;

namespace Api.Controllers;

public class ExampleSchemaFilter : ISchemaFilter
{
    public void Apply(IOpenApiSchema schema, SchemaFilterContext context)
    {
        if (context.Type == typeof(AccountPortfolioRequest) && schema is OpenApiSchema openApiSchema)
        {
            openApiSchema.Example = new JsonObject
            {
                ["accountCodes"] = new JsonArray { JsonValue.Create("SIPP") },
                ["date"] = JsonValue.Create("2024-10-30"),
            };
        }
    }
}
