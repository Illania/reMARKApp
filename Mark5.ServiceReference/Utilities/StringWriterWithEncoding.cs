using System.IO;
using System.Text;

namespace Mark5.ServiceReference.Utilities
{
    public class StringWriterWithEncoding : StringWriter
    {
        public override Encoding Encoding => encoding;

        readonly Encoding encoding;

        public StringWriterWithEncoding(Encoding encoding)
        {
            this.encoding = encoding;
        }
    }
}