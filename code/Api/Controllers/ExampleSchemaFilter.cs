using Api.QueryHandlers.Portfolio;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Api.Controllers;

public class ExampleSchemaFilter : ISchemaFilter
{
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        if (context.Type == typeof(AccountPortfolioRequest))
        {
            schema.Example = new OpenApiObject()
            {
                ["accountCode"] = new OpenApiString("SIPP"),
                ["date"] = new OpenApiString("2024-10-30"),
            };
        }
    }
}
