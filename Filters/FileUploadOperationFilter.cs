using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using Microsoft.AspNetCore.Mvc;

namespace Kadikoy.Filters;

public class FileUploadOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        // Sadece multipart/form-data consume eden endpoint'leri işle
        var consumesAttribute = context.MethodInfo.GetCustomAttributes(true)
            .OfType<ConsumesAttribute>()
            .FirstOrDefault();

        if (consumesAttribute == null || !consumesAttribute.ContentTypes.Contains("multipart/form-data"))
            return;

        var fileParameters = context.ApiDescription.ParameterDescriptions
            .Where(p => p.ModelMetadata?.ModelType == typeof(IFormFile))
            .ToList();

        // Tüm form parametrelerini al (Form ve FormFile kaynakları)
        var allFormParameters = context.ApiDescription.ParameterDescriptions
            .Where(p => p.Source != null && (p.Source.Id == "Form" || p.Source.Id == "FormFile"))
            .ToList();

        var properties = new Dictionary<string, OpenApiSchema>();
        var required = new HashSet<string>();

        // Önce form parametrelerini ekle
        foreach (var param in allFormParameters)
        {
            if (param.ModelMetadata?.ModelType == typeof(IFormFile))
            {
                properties[param.Name] = new OpenApiSchema { Type = "string", Format = "binary" };
                required.Add(param.Name);
            }
            else
            {
                var schema = new OpenApiSchema();
                var type = param.ModelMetadata?.ModelType;
                if (type == typeof(int) || type == typeof(int?))
                {
                    schema.Type = "integer"; schema.Format = "int32";
                }
                else if (type == typeof(long) || type == typeof(long?))
                {
                    schema.Type = "integer"; schema.Format = "int64";
                }
                else if (type == typeof(bool) || type == typeof(bool?))
                {
                    schema.Type = "boolean";
                }
                else
                {
                    schema.Type = "string";
                }
                properties[param.Name] = schema;
                if (param.IsRequired) required.Add(param.Name);
            }
        }

        // allFormParameters içinde görünmediyse bile dosya parametrelerini zorla ekle
        foreach (var fp in fileParameters)
        {
            if (!properties.ContainsKey(fp.Name))
            {
                properties[fp.Name] = new OpenApiSchema { Type = "string", Format = "binary" };
                required.Add(fp.Name);
            }
        }

        operation.RequestBody = new OpenApiRequestBody
        {
            Content = new Dictionary<string, OpenApiMediaType>
            {
                ["multipart/form-data"] = new OpenApiMediaType
                {
                    Schema = new OpenApiSchema
                    {
                        Type = "object",
                        Properties = properties,
                        Required = required
                    }
                }
            }
        };

        // Form/FormFile kaynaklı parametreleri query/path listesinden kaldır
        var parametersToRemove = operation.Parameters
            .Where(p => allFormParameters.Any(fp => fp.Name == p.Name) || fileParameters.Any(fp => fp.Name == p.Name))
            .ToList();

        foreach (var parameter in parametersToRemove)
        {
            operation.Parameters.Remove(parameter);
        }
    }
}

