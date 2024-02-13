using System.IO;
using System.Text;

namespace reMark.ServiceReference.Utilities
{
    public class StringWriterWithEncoding : StringWriter
    {
        public override Encoding Encoding { get; }

        public StringWriterWithEncoding(Encoding encoding)
        {
            Encoding = encoding;
        }
    }
}