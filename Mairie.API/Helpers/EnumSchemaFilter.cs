using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Mairie.API.Helpers
{
    public class EnumSchemaFilter : ISchemaFilter
    {
        /// <summary>
        /// Permet de redéfinir l'énumération pour swagger et ajouter les valeurs de l'enum
        /// </summary> 
        public void Apply(OpenApiSchema schema, SchemaFilterContext context)
        {
            if (context.Type.IsEnum)
            {
                schema.Enum.Clear();
                Enum.GetNames(context.Type)
                    .ToList()
                    .ForEach(name => schema.Enum.Add(new OpenApiString($"{name}")));
            }
        }
    }
}
