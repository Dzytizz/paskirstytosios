using System.Xml;
using System.Xml.Serialization;

namespace lib
{
    public static class XMLParser
    {
        public static string SerializeToString<T>(T value) where T : class
        {
            var emptyNamespaces = new XmlSerializerNamespaces(new[] { XmlQualifiedName.Empty });
            var serializer = new XmlSerializer(value.GetType());
            var settings = new XmlWriterSettings();
            settings.Indent = true;
            settings.OmitXmlDeclaration = true;

            using (var stream = new StringWriter())
            {
                using (var writer = XmlWriter.Create(stream, settings))
                {
                    serializer.Serialize(writer, value, emptyNamespaces);
                    return stream.ToString();
                }
            }
        }

        public static T? DeserializeFromString<T>(string xmlText) where T : class
        {
            XmlSerializer serializer = new XmlSerializer(typeof(T));
            T? request;
            using (StringReader stringReader = new StringReader(xmlText))
            {
                request = serializer.Deserialize(stringReader) as T;
            }

            return request;
        }
    }
}
