using Dapper.FluentMap.Conventions;
using System.Text.RegularExpressions;

namespace Pomodoro.DB
{
    public class PropertyTransformConvention : Convention
    {
        /// <summary>
        /// Transforms all property names with respect to snake_case convention
        /// </summary>
        public PropertyTransformConvention()
        {
            // Based on https://github.com/henkmollema/Dapper-FluentMap#transformations
            Properties()
                .Configure( c => c.Transform(
                    s => Regex.Replace(input: s, pattern: "([A-Z])([A-Z][a-z])|([a-z0-9])([A-Z])", replacement: "$1$3_$2$4")
                    .ToLower()));
        }
    }
}
