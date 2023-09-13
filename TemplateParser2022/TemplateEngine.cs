using System.Text.RegularExpressions;

namespace TemplateParser2022
{
    public class TemplateEngine : ITemplateEngine
    {
        /// <summary>
        /// Applies the specified datasource to a string template, and returns a result string
        /// with substituted values.
        /// </summary>


        public string Apply(string template, object data)
        {
            if (template == null)
            {
                throw new ArgumentNullException(nameof(template));
            }

            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            string result = template;

            // Replace spanned property name tokens [FieldA.FieldB]
            result = ReplaceSpannedTokens(result, data);

            // Replace simple field name tokens [FieldName] with optional formatting
            result = ReplaceSimpleTokens(result, data);

            // Handle scope blocks [with FieldName]...[/with]
            result = ReplaceScopeBlocks(result, data);

            return result;
        }

        public string ReplaceSpannedTokens(string template, object data)
        {
           
            var pat2 = @"\[([\w.]+)(\s+""(.*?)""\s*)?\]";
                       

            return Regex.Replace(template, pat2, match =>
            {
                string token = match.Groups[1].Value;
                var format = match.Groups[3].Value;
               
                object value = ResolveToken(token, data);

                if (!string.IsNullOrWhiteSpace(format) && value is IFormattable formattable)
                {
                    // Apply formatting if provided and the value is formattable
                    return formattable.ToString(format, null);
                }

                return value?.ToString() ?? match.Value;
            });
        }

        public string ReplaceSimpleTokens(string template, object data)
        {
            
            string pattern = @"\[([\w]+)(?: ""([^""]+)"")?\]";
            // var str = "@\"\\[([\\w]+)(?: \"\"([^\"\"]+)\"\")?\\]\"";
            return Regex.Replace(template, pattern, match =>
            {
                string token = match.Groups[1].Value;
                object value = ResolveToken(token, data);
                if (value == null)
                {
                    // Leave the token unchanged if not found
                    return match.Value;
                    //return  value?.ToString() ?? string.Empty;
                }
                // Check for optional formatting
                string format = match.Groups[2].Value;

                return string.IsNullOrEmpty(format) ? value.ToString() : string.Format($"{{0:{format}}}", value);
                
            });

        }


        public string ReplaceScopeBlocks(string template, object data)
        {
            string result = template;
            var pattern = @"\[with\s+([^\]]+)\](.*?)\[\/with\](?:(?!\[\/?with\]).*?|\[with\s+([^\]]+)\](?:(?!\[\/?with\]).*?|\[\/with\]))|\[([^\]]+)\]";

            var pattern2 = @"\[with\s+([^]]+)]((?:(?!\[\/?with]).|\[with\s+([^]]+)]|\[\/with])*)\[\/with]";

            var scopeRegex = new Regex(pattern);

            result = scopeRegex.Replace(result, match =>
            {
                if (match.Groups[1].Success)
                {
                    // Handle scope blocks
                    string scopeName = match.Groups[1].Value;
                    string scopeContent = match.Groups[2].Value;

                    var scopeProperty = data.GetType().GetProperty(scopeName);
                    if (scopeProperty != null)
                    {
                        object scopeData = scopeProperty.GetValue(data);
                        string replacedScopeContent = Apply(scopeContent, scopeData);
                        return replacedScopeContent;
                    }
                    return "";
                }
                else if (match.Groups[3].Success)
                {
                    // Handle individual placeholders
                    string propertyName = match.Groups[3].Value;

                    var property = data.GetType().GetProperty(propertyName);
                    if (property != null)
                    {
                        object propertyValue = property.GetValue(data);
                        return propertyValue.ToString();
                    }
                    return match.Value;
                    
                }

                // return match.Value;
                return string.Empty;
                
            });

            return result;

        }

        public object ResolveToken(string token, object data)
        {
            string[] properties = token.Split('.');
            object currentObject = data;


            foreach (string property in properties)
            {
                if (currentObject == null)
                {
                    return null; // Stop resolving if any property is null
                }

                Type objectType = currentObject.GetType();
                var propertyInfo = objectType.GetProperty(property);

                if (propertyInfo != null)
                {
                    currentObject = propertyInfo.GetValue(currentObject);
                }
                
                else
                {
                    return null; // Property not found
                                 //return string.Empty;
                }
            }

            return currentObject;
        }

    }
}
