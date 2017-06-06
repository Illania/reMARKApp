using System.IO;
using System.Text;

namespace Mark5.ServiceReference.Utilities
{
    public class StringWriterWithEncoding : StringWriter
    {
        public override Encoding Encoding { get; }

        public StringWriterWithEncoding(Encoding encoding)
        {
            this.Encoding = encoding;
        }
    }
}