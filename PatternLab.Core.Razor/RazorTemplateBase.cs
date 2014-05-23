using System;
using System.Collections;
using System.Collections.Generic;
using PatternLab.Core.Providers;
using RazorEngine.Templating;

namespace PatternLab.Core.Razor
{
    public class RazorTemplateBase<T> : TemplateBase<T>
    {
        public override TemplateWriter Include(string cacheName, object model = null)
        {
            var s = model as string;
            return s != null
                ? Include(cacheName, s, new RazorDynamicDictionary())
                : Include(cacheName, string.Empty, model);
        }

        public TemplateWriter Include(string cacheName, string styleModifier, object parameters)
        {
            /*var data = Model as RazorDynamicDictionary;
            if (data != null)
            {
                if (!string.IsNullOrEmpty(styleModifier))
                {
                    data.Add(PatternProvider.KeywordModifier,
                        styleModifier.Replace(PatternProvider.IdentifierModifierSeparator, ' ').Trim());
                }

                if (model is bool && !(bool) model)
                {
                    return null;
                }
                
                if (model is RazorDynamicDictionary)
                {
                    var parameters = model as RazorDynamicDictionary;

                    foreach (var parameter in parameters.Dictionary)
                    {
                        data.Add(parameter.Key, parameter.Value);
                    }
                }
                else if (model is object[])
                {
                    foreach (var dictionary in model as object[])
                    {
                        var test = new RazorDynamicDictionary(dictionary as IDictionary<string, object>);
                        foreach (var parameter in test.Dictionary)
                        {
                            data.Add(parameter.Key, parameter.Value);
                        }
                    }
                }

                model = data;
            }
            else
            {
                model = Model;
            }*/

            return base.Include(cacheName, Model);
        }
    }
}