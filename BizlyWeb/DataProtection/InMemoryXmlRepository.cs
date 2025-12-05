using Microsoft.AspNetCore.DataProtection.Repositories;
using System.Xml.Linq;

namespace BizlyWeb.DataProtection
{
    /// <summary>
    /// Repositorio en memoria para Data Protection Keys
    /// Usado en contenedores donde no hay persistencia de archivos
    /// </summary>
    public class InMemoryXmlRepository : IXmlRepository
    {
        private readonly List<XElement> _elements = new();

        public IReadOnlyCollection<XElement> GetAllElements()
        {
            return _elements.AsReadOnly();
        }

        public void StoreElement(XElement element, string friendlyName)
        {
            _elements.Add(element);
        }
    }
}

