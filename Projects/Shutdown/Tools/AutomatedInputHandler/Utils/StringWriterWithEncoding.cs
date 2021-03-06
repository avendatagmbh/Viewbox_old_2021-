// --------------------------------------------------------------------------------
// author: Sebastian Vetter
// since: 2013-01-30
// copyright 2013 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System;
using System.IO;
using System.Text;

namespace Utils {
    public class StringWriterWithEncoding : StringWriter {
        private readonly Encoding _encoding;

        public StringWriterWithEncoding() {
        }

        public StringWriterWithEncoding(IFormatProvider formatProvider)
            : base(formatProvider) {
        }

        public StringWriterWithEncoding(StringBuilder sb)
            : base(sb) {
        }

        public StringWriterWithEncoding(StringBuilder sb, IFormatProvider formatProvider)
            : base(sb, formatProvider) {
        }


        public StringWriterWithEncoding(Encoding encoding) {
            _encoding = encoding;
        }

        public StringWriterWithEncoding(IFormatProvider formatProvider, Encoding encoding)
            : base(formatProvider) {
            _encoding = encoding;
        }

        public StringWriterWithEncoding(StringBuilder sb, Encoding encoding)
            : base(sb) {
            _encoding = encoding;
        }

        public StringWriterWithEncoding(StringBuilder sb, IFormatProvider formatProvider, Encoding encoding)
            : base(sb, formatProvider) {
            _encoding = encoding;
        }

        public override Encoding Encoding {
            get { return _encoding ?? base.Encoding; }
        }
    }
}