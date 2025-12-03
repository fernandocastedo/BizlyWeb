using Microsoft.AspNetCore.Mvc.Filters;

namespace BizlyWeb.Attributes
{
    /// <summary>
    /// Atributo para deshabilitar el binding automático de form values grandes
    /// Útil para archivos grandes que pueden causar problemas de memoria
    /// </summary>
    public class DisableFormValueModelBindingAttribute : Attribute, IResourceFilter
    {
        public void OnResourceExecuting(ResourceExecutingContext context)
        {
            var formValueProviderFactory = context.ValueProviderFactories
                .OfType<Microsoft.AspNetCore.Mvc.ModelBinding.FormValueProviderFactory>()
                .FirstOrDefault();
            if (formValueProviderFactory != null)
            {
                context.ValueProviderFactories.Remove(formValueProviderFactory);
            }

            var jqueryFormValueProviderFactory = context.ValueProviderFactories
                .OfType<Microsoft.AspNetCore.Mvc.ModelBinding.JQueryFormValueProviderFactory>()
                .FirstOrDefault();
            if (jqueryFormValueProviderFactory != null)
            {
                context.ValueProviderFactories.Remove(jqueryFormValueProviderFactory);
            }
        }

        public void OnResourceExecuted(ResourceExecutedContext context)
        {
            // No hacer nada después de la ejecución
        }
    }
}

