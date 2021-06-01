using System.IO;
using Ude.Core;

namespace Ude
{
    /// <summary>
    ///   Default implementation of charset detection interface. The detector can be fed by a System.IO.Stream:
    ///   <example>
    ///     <code>using (FileStream fs = File.OpenRead(filename)) {
    ///       CharsetDetector cdet = new CharsetDetector();
    ///       cdet.Feed(fs);
    ///       cdet.DataEnd();
    ///       Console.WriteLine("{0}, {1}", cdet.Charset, cdet.Confidence);</code>
    ///   </example>
    ///   or by a byte a array:
    ///   <example>
    ///     <code>byte[] buff = new byte[1024];
    ///       int read;
    ///       while ((read = stream.Read(buff, 0, buff.Length)) > 0 && !done)
    ///       Feed(buff, 0, read);
    ///       cdet.DataEnd();
    ///       Console.WriteLine("{0}, {1}", cdet.Charset, cdet.Confidence);</code>
    ///   </example>
    /// </summary>
    public class CharsetDetector : UniversalDetector, ICharsetDetector
    {
        private string charset;

        private float confidence;

        //public event DetectorFinished Finished;

        public CharsetDetector() : base(FILTER_ALL)
        {
        }

        #region ICharsetDetector Members

        public void Feed(Stream stream)
        {
            var buff = new byte[1024];
            int read;
            while ((read = stream.Read(buff, 0, buff.Length)) > 0 && !done)
            {
                Feed(buff, 0, read);
            }
        }

        public void Feed(Stream stream, int maxLength)
        {
            var buff = new byte[1024];
            int read;
            int currRead = 0;
            while ((read = stream.Read(buff, 0, buff.Length)) > 0 && !done && currRead < maxLength)
            {
                Feed(buff, 0, read);
                currRead += read;
            }
        }

        public bool IsDone()
        {
            return done;
        }

        public override void Reset()
        {
            charset = null;
            confidence = 0.0f;
            base.Reset();
        }

        public string Charset
        {
            get { return charset; }
        }

        public float Confidence
        {
            get { return confidence; }
        }

        #endregion

        protected override void Report(string charset, float confidence)
        {
            this.charset = charset;
            this.confidence = confidence;
//            if (Finished != null) {
//                Finished(charset, confidence);
//            }
        }
    }

    //public delegate void DetectorFinished(string charset, float confidence);
}