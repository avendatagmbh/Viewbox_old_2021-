using System.Collections.Generic;
using System.Linq;

namespace Utils
{
    public static class MimeToExtension
    {
        #region MimeExts

        private static readonly List<KeyValuePair<string, string>> _mimeExts = new List<KeyValuePair<string, string>>
                                                                                   {
                                                                                       new KeyValuePair<string, string>(
                                                                                           "x-world/x-3dmf", ".3dm "),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "x-world/x-3dmf", ".3dmf"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/octet-stream",
                                                                                           ".a"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/x-authorware-bin",
                                                                                           ".aab"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/x-authorware-map",
                                                                                           ".aam"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/x-authorware-seg",
                                                                                           ".aas"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "text/vnd.abc", ".abc"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "text/html", ".acgi"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "video/animaflex", ".afl"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/postscript",
                                                                                           ".ai"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "audio/aiff", ".aif"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "audio/x-aiff", ".aif"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/x-aim", ".aim"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "text/x-audiosoft-intra",
                                                                                           ".aip"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/x-navi-animation",
                                                                                           ".ani"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/x-nokia-9000-communicator-add-on-software",
                                                                                           ".aos"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/mime", ".aps"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/octet-stream",
                                                                                           ".arc"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/arj", ".arj"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/octet-stream",
                                                                                           ".arj"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "image/x-jg", ".art"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "video/x-ms-asf", ".asf"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "text/x-asm", ".asm"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "text/asp", ".asp"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/x-mplayer2",
                                                                                           ".asx"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "video/x-ms-asf", ".asx"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "video/x-ms-asf-plugin",
                                                                                           ".asx"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "audio/basic", ".au"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "audio/x-au", ".au"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/x-troff-msvideo",
                                                                                           ".avi"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "video/avi", ".avi"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "video/msvideo", ".avi"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "video/x-msvideo", ".avi"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "video/avs-video", ".avs"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/x-bcpio",
                                                                                           ".bcpio"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/mac-binary",
                                                                                           ".bin"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/macbinary",
                                                                                           ".bin"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/octet-stream",
                                                                                           ".bin"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/x-binary",
                                                                                           ".bin"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/x-macbinary",
                                                                                           ".bin"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "image/bmp", ".bmp"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "image/x-windows-bmp", ".bmp"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/book", ".book"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/x-bsh", ".bsh"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/x-bzip", ".bz"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/x-bzip2", ".bz2"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "text/plain", ".txt"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "text/x-c", ".c"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/vnd.ms-pki.seccat",
                                                                                           ".cat"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/clariscad",
                                                                                           ".ccad"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/x-cocoa", ".cco"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/cdf", ".cdf"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/x-cdf", ".cdf"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/x-netcdf",
                                                                                           ".cdf"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/pkix-cert",
                                                                                           ".cer"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/x-x509-ca-cert",
                                                                                           ".cer"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/x-chat", ".cha"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/x-chat", ".chat"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/java", ".class"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/java-byte-code",
                                                                                           ".class"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/x-java-class",
                                                                                           ".class"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/octet-stream",
                                                                                           ".com"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "text/plain", ".com"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "text/plain", ".conf"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/x-cpio", ".cpio"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/mac-compactpro",
                                                                                           ".cpt"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/x-compactpro",
                                                                                           ".cpt"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/x-cpt", ".cpt"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/pkcs-crl",
                                                                                           ".crl"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/pkix-crl",
                                                                                           ".crl"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/pkix-cert",
                                                                                           ".crt"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/x-x509-ca-cert",
                                                                                           ".crt"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/x-x509-user-cert",
                                                                                           ".crt"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/x-csh", ".csh"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "text/x-script.csh", ".csh"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/x-pointplus",
                                                                                           ".css"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "text/css", ".css"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "text/plain", ".cxx"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/x-director",
                                                                                           ".dcr"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/x-deepv",
                                                                                           ".deepv"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "text/plain", ".def"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/x-x509-ca-cert",
                                                                                           ".der"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "video/x-dv", ".dif"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/x-director",
                                                                                           ".dir"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "video/dl", ".dl"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "video/x-dl", ".dl"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/msword", ".doc"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/msword", ".dot"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/commonground",
                                                                                           ".dp"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/drafting",
                                                                                           ".drw"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/octet-stream",
                                                                                           ".dump"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "video/x-dv", ".dv"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/x-dvi", ".dvi"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "drawing/x-dwf (old)", ".dwf"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "model/vnd.dwf", ".dwf"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/acad", ".dwg"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "image/vnd.dwg", ".dwg"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "image/x-dwg", ".dwg"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/dxf", ".dxf"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "image/vnd.dwg", ".dxf"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "image/x-dwg", ".dxf"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/x-director",
                                                                                           ".dxr"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "text/x-script.elisp", ".el"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/x-bytecode.elisp (compiled elisp)",
                                                                                           ".elc"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/x-elc", ".elc"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/x-envoy", ".env"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/postscript",
                                                                                           ".eps"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/x-esrehber",
                                                                                           ".es"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "text/x-setext", ".etx"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/envoy", ".evy"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/x-envoy", ".evy"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/octet-stream",
                                                                                           ".exe"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "text/plain", ".f"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "text/x-fortran", ".f"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "text/x-fortran", ".f77"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "text/plain", ".f90"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "text/x-fortran", ".f90"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/vnd.fdf", ".fdf"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/fractals",
                                                                                           ".fif"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "image/fif", ".fif"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "video/fli", ".fli"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "video/x-fli", ".fli"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "image/florian", ".flo"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "text/vnd.fmi.flexstor",
                                                                                           ".flx"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "video/x-atomic3d-feature",
                                                                                           ".fmf"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "text/plain", ".for"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "text/x-fortran", ".for"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "image/vnd.fpx", ".fpx"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "image/vnd.net-fpx", ".fpx"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/freeloader",
                                                                                           ".frl"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "audio/make", ".funk"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "text/plain", ".g"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "image/g3fax", ".g3"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "image/gif", ".gif"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "video/gl", ".gl"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "video/x-gl", ".gl"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "audio/x-gsm", ".gsd"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "audio/x-gsm", ".gsm"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/x-gsp", ".gsp"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/x-gss", ".gss"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/x-gtar", ".gtar"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/x-compressed",
                                                                                           ".gz"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/x-gzip", ".gz"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/x-gzip", ".gzip"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "multipart/x-gzip", ".gzip"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "text/plain", ".h"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "text/x-h", ".h"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/x-hdf", ".hdf"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/x-helpfile",
                                                                                           ".help"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/vnd.hp-hpgl",
                                                                                           ".hgl"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "text/plain", ".hh"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "text/x-h", ".hh"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "text/x-script", ".hlb"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/hlp", ".hlp"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/x-helpfile",
                                                                                           ".hlp"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/x-winhelp",
                                                                                           ".hlp"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/vnd.hp-hpgl",
                                                                                           ".hpg"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/vnd.hp-hpgl",
                                                                                           ".hpgl"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/binhex", ".hqx"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/binhex4", ".hqx"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/mac-binhex",
                                                                                           ".hqx"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/mac-binhex40",
                                                                                           ".hqx"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/x-binhex40",
                                                                                           ".hqx"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/x-mac-binhex40",
                                                                                           ".hqx"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/hta", ".hta"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "text/x-component", ".htc"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "text/html", ".htm"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "text/html", ".html"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "text/html", ".htmls"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "text/webviewhtml", ".htt"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "text/html", ".htx"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "x-conference/x-cooltalk",
                                                                                           ".ice"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "image/x-icon", ".ico"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "text/plain", ".idc"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "image/ief", ".ief"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "image/ief", ".iefs"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/iges", ".iges"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "model/iges", ".iges"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/iges", ".igs"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "model/iges", ".igs"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/x-ima", ".ima"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/x-httpd-imap",
                                                                                           ".imap"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/inf", ".inf"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/x-internett-signup",
                                                                                           ".ins"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/x-ip2", ".ip"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "video/x-isvideo", ".isu"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "audio/it", ".it"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/x-inventor",
                                                                                           ".iv"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "i-world/i-vrml", ".ivr"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/x-livescreen",
                                                                                           ".ivy"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "audio/x-jam", ".jam"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "text/plain", ".jav"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "text/x-java-source", ".jav"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "text/plain", ".java"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "text/x-java-source", ".java"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/x-java-commerce",
                                                                                           ".jcm"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "image/jpeg", ".jfif"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "image/pjpeg", ".jfif"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "image/jpeg", ".jfif-tbnl"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "image/jpeg", ".jpe"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "image/pjpeg", ".jpe"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "image/jpeg", ".jpeg"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "image/pjpeg", ".jpeg"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "image/jpeg", ".jpg"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "image/pjpeg", ".jpg"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "image/x-jps", ".jps"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/x-javascript",
                                                                                           ".js"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "image/jutvision", ".jut"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "audio/midi", ".kar"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "music/x-karaoke", ".kar"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/x-ksh", ".ksh"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "text/x-script.ksh", ".ksh"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "audio/nspaudio", ".la"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "audio/x-nspaudio", ".la"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "audio/x-liveaudio", ".lam"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/x-latex",
                                                                                           ".latex"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/lha", ".lha"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/octet-stream",
                                                                                           ".lha"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/x-lha", ".lha"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/octet-stream",
                                                                                           ".lhx"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "text/plain", ".list"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "audio/nspaudio", ".lma"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "audio/x-nspaudio", ".lma"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "text/plain", ".log"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/x-lisp", ".lsp"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "text/x-script.lisp", ".lsp"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "text/plain", ".lst"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "text/x-la-asf", ".lsx"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/x-latex", ".ltx"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/octet-stream",
                                                                                           ".lzh"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/x-lzh", ".lzh"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/lzx", ".lzx"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/octet-stream",
                                                                                           ".lzx"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/x-lzx", ".lzx"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "text/plain", ".m"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "text/x-m", ".m"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "video/mpeg", ".m1v"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "audio/mpeg", ".m2a"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "video/mpeg", ".m2v"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "audio/x-mpequrl", ".m3u"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/x-troff-man",
                                                                                           ".man"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/x-navimap",
                                                                                           ".map"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "text/plain", ".mar"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/mbedlet", ".mbd"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/x-magic-cap-package-1.0",
                                                                                           ".mc$"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/mcad", ".mcd"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/x-mathcad",
                                                                                           ".mcd"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "image/vasa", ".mcf"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "text/mcf", ".mcf"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/netmc", ".mcp"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/x-troff-me",
                                                                                           ".me"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "message/rfc822", ".mht"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "message/rfc822", ".mhtml"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/x-midi", ".mid"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "audio/midi", ".mid"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "audio/x-mid", ".mid"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "audio/x-midi", ".mid"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "music/crescendo", ".mid"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "x-music/x-midi", ".mid"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/x-midi", ".midi"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "audio/midi", ".midi"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "audio/x-mid", ".midi"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "audio/x-midi", ".midi"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "music/crescendo", ".midi"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "x-music/x-midi", ".midi"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/x-frame", ".mif"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/x-mif", ".mif"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "message/rfc822", ".mime"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "www/mime", ".mime"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "audio/x-vnd.audioexplosion.mjuicemediafile",
                                                                                           ".mjf"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "video/x-motion-jpeg",
                                                                                           ".mjpg"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/base64", ".mm"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/x-meme", ".mm"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/base64", ".mme"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "audio/mod", ".mod"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "audio/x-mod", ".mod"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "video/quicktime", ".moov"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "video/quicktime", ".mov"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "video/x-sgi-movie", ".movie"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "audio/mpeg", ".mp2"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "audio/x-mpeg", ".mp2"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "video/mpeg", ".mp2"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "video/x-mpeg", ".mp2"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "video/x-mpeq2a", ".mp2"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "audio/mpeg3", ".mp3"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "audio/x-mpeg-3", ".mp3"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "video/mpeg", ".mp3"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "video/x-mpeg", ".mp3"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "audio/mpeg", ".mpa"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "video/mpeg", ".mpa"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/x-project",
                                                                                           ".mpc"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "video/mpeg", ".mpe"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "video/mpeg", ".mpeg"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "audio/mpeg", ".mpg"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "video/mpeg", ".mpg"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "audio/mpeg", ".mpga"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/vnd.ms-project",
                                                                                           ".mpp"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/x-project",
                                                                                           ".mpt"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/x-project",
                                                                                           ".mpv"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/x-project",
                                                                                           ".mpx"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/marc", ".mrc"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/x-troff-ms",
                                                                                           ".ms"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "video/x-sgi-movie", ".mv"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "audio/make", ".my"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/x-vnd.audioexplosion.mzz",
                                                                                           ".mzz"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "image/naplps", ".nap"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "image/naplps", ".naplps"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/x-netcdf", ".nc"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/vnd.nokia.configuration-message",
                                                                                           ".ncm"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "image/x-niff", ".nif"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "image/x-niff", ".niff"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/x-mix-transfer",
                                                                                           ".nix"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/x-conference",
                                                                                           ".nsc"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/x-navidoc",
                                                                                           ".nvd"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/octet-stream",
                                                                                           ".o"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/oda", ".oda"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/x-omc", ".omc"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/x-omcdatamaker",
                                                                                           ".omcd"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/x-omcregerator",
                                                                                           ".omcr"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "text/x-pascal", ".p"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/pkcs10", ".p10"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/x-pkcs10",
                                                                                           ".p10"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/pkcs-12", ".p12"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/x-pkcs12",
                                                                                           ".p12"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/x-pkcs7-signature",
                                                                                           ".p7a"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/pkcs7-mime",
                                                                                           ".p7c"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/x-pkcs7-mime",
                                                                                           ".p7c"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/pkcs7-mime",
                                                                                           ".p7m"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/x-pkcs7-mime",
                                                                                           ".p7m"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/x-pkcs7-certreqresp",
                                                                                           ".p7r"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/pkcs7-signature",
                                                                                           ".p7s"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/pro_eng",
                                                                                           ".part"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "text/pascal", ".pas"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "image/x-portable-bitmap",
                                                                                           ".pbm"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/vnd.hp-pcl",
                                                                                           ".pcl"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/x-pcl", ".pcl"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "image/x-pict", ".pct"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "image/x-pcx", ".pcx"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "chemical/x-pdb", ".pdb"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/pdf", ".pdf"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "audio/make", ".pfunk"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "audio/make.my.funk",
                                                                                           ".pfunk"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "image/x-portable-graymap",
                                                                                           ".pgm"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "image/x-portable-greymap",
                                                                                           ".pgm"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "image/pict", ".pic"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "image/pict", ".pict"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/x-newton-compatible-pkg",
                                                                                           ".pkg"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/vnd.ms-pki.pko",
                                                                                           ".pko"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "text/plain", ".pl"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "text/x-script.perl", ".pl"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/x-pixclscript",
                                                                                           ".plx"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "image/x-xpixmap", ".pm"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "text/x-script.perl-module",
                                                                                           ".pm"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/x-pagemaker",
                                                                                           ".pm4"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/x-pagemaker",
                                                                                           ".pm5"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "image/png", ".png"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/x-portable-anymap",
                                                                                           ".pnm"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "image/x-portable-anymap",
                                                                                           ".pnm"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/mspowerpoint",
                                                                                           ".pot"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/vnd.ms-powerpoint",
                                                                                           ".pot"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "model/x-pov", ".pov"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/vnd.ms-powerpoint",
                                                                                           ".ppa"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "image/x-portable-pixmap",
                                                                                           ".ppm"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/mspowerpoint",
                                                                                           ".pps"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/vnd.ms-powerpoint",
                                                                                           ".pps"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/mspowerpoint",
                                                                                           ".ppt"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/powerpoint",
                                                                                           ".ppt"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/vnd.ms-powerpoint",
                                                                                           ".ppt"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/x-mspowerpoint",
                                                                                           ".ppt"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/mspowerpoint",
                                                                                           ".ppz"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/x-freelance",
                                                                                           ".pre"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/pro_eng", ".prt"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/postscript",
                                                                                           ".ps"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/octet-stream",
                                                                                           ".psd"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "paleovu/x-pv", ".pvu"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/vnd.ms-powerpoint",
                                                                                           ".pwz"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "text/x-script.phyton", ".py"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "applicaiton/x-bytecode.python",
                                                                                           ".pyc"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "audio/vnd.qcelp", ".qcp"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "x-world/x-3dmf", ".qd3"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "x-world/x-3dmf", ".qd3d"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "image/x-quicktime", ".qif"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "video/quicktime", ".qt"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "video/x-qtc", ".qtc"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "image/x-quicktime", ".qti"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "image/x-quicktime", ".qtif"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "audio/x-pn-realaudio", ".ra"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "audio/x-pn-realaudio-plugin",
                                                                                           ".ra"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "audio/x-realaudio", ".ra"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "audio/x-pn-realaudio",
                                                                                           ".ram"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/x-cmu-raster",
                                                                                           ".ras"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "image/cmu-raster", ".ras"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "image/x-cmu-raster", ".ras"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "image/cmu-raster", ".rast"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "text/x-script.rexx", ".rexx"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "image/vnd.rn-realflash",
                                                                                           ".rf"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "image/x-rgb", ".rgb"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/vnd.rn-realmedia",
                                                                                           ".rm"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "audio/x-pn-realaudio", ".rm"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "audio/mid", ".rmi"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "audio/x-pn-realaudio",
                                                                                           ".rmm"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "audio/x-pn-realaudio",
                                                                                           ".rmp"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "audio/x-pn-realaudio-plugin",
                                                                                           ".rmp"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/ringing-tones",
                                                                                           ".rng"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/vnd.nokia.ringing-tone",
                                                                                           ".rng"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/vnd.rn-realplayer",
                                                                                           ".rnx"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/x-troff",
                                                                                           ".roff"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "image/vnd.rn-realpix", ".rp"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "audio/x-pn-realaudio-plugin",
                                                                                           ".rpm"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "text/richtext", ".rt"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "text/vnd.rn-realtext", ".rt"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/rtf", ".rtf"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/x-rtf", ".rtf"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "text/richtext", ".rtf"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/rtf", ".rtx"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "text/richtext", ".rtx"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "video/vnd.rn-realvideo",
                                                                                           ".rv"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "text/x-asm", ".s"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "audio/s3m", ".s3m"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/octet-stream",
                                                                                           ".saveme"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/x-tbook", ".sbk"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/x-lotusscreencam",
                                                                                           ".scm"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "text/x-script.guile", ".scm"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "text/x-script.scheme",
                                                                                           ".scm"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "video/x-scm", ".scm"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "text/plain", ".sdml"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/sdp", ".sdp"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/x-sdp", ".sdp"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/sounder", ".sdr"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/sea", ".sea"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/x-sea", ".sea"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/set", ".set"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "text/sgml", ".sgm"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "text/x-sgml", ".sgm"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "text/sgml", ".sgml"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "text/x-sgml", ".sgml"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/x-bsh", ".sh"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/x-sh", ".sh"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/x-shar", ".sh"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "text/x-script.sh", ".sh"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/x-bsh", ".shar"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/x-shar", ".shar"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "text/html", ".shtml"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "text/x-server-parsed-html",
                                                                                           ".shtml"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "audio/x-psid", ".sid"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/x-sit", ".sit"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/x-stuffit",
                                                                                           ".sit"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/x-koan", ".skd"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/x-koan", ".skm"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/x-koan", ".skp"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/x-koan", ".skt"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/x-seelogo",
                                                                                           ".sl"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/smil", ".smi"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/smil", ".smil"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "audio/basic", ".snd"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "audio/x-adpcm", ".snd"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/solids", ".sol"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/x-pkcs7-certificates",
                                                                                           ".spc"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "text/x-speech", ".spc"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/futuresplash",
                                                                                           ".spl"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/x-sprite",
                                                                                           ".spr"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/x-sprite",
                                                                                           ".sprite"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/x-wais-source",
                                                                                           ".src"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "text/x-server-parsed-html",
                                                                                           ".ssi"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/streamingmedia",
                                                                                           ".ssm"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/vnd.ms-pki.certstore",
                                                                                           ".sst"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/step", ".step"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/sla", ".stl"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/vnd.ms-pki.stl",
                                                                                           ".stl"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/x-navistyle",
                                                                                           ".stl"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/step", ".stp"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/x-sv4cpio",
                                                                                           ".sv4cpio"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/x-sv4crc",
                                                                                           ".sv4crc"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "image/vnd.dwg", ".svf"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "image/x-dwg", ".svf"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/x-world", ".svr"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "x-world/x-svr", ".svr"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/x-shockwave-flash",
                                                                                           ".swf"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/x-troff", ".t"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "text/x-speech", ".talk"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/x-tar", ".tar"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/toolbook",
                                                                                           ".tbk"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/x-tbook", ".tbk"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/x-tcl", ".tcl"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "text/x-script.tcl", ".tcl"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "text/x-script.tcsh", ".tcsh"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/x-tex", ".tex"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/x-texinfo",
                                                                                           ".texi"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/x-texinfo",
                                                                                           ".texinfo"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/plain", ".text"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "text/plain", ".text"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/gnutar", ".tgz"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/x-compressed",
                                                                                           ".tgz"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "image/tiff", ".tif"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "image/x-tiff", ".tif"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "image/tiff", ".tiff"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "image/x-tiff", ".tiff"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/x-troff", ".tr"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "audio/tsp-audio", ".tsi"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/dsptype", ".tsp"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "audio/tsplayer", ".tsp"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "text/tab-separated-values",
                                                                                           ".tsv"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "image/florian", ".turbot"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "text/plain", ".txt"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "text/x-uil", ".uil"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "text/uri-list", ".uni"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "text/uri-list", ".unis"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/i-deas", ".unv"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "text/uri-list", ".uri"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "text/uri-list", ".uris"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/x-ustar",
                                                                                           ".ustar"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "multipart/x-ustar", ".ustar"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/octet-stream",
                                                                                           ".uu"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "text/x-uuencode", ".uu"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "text/x-uuencode", ".uue"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/x-cdlink",
                                                                                           ".vcd"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "text/x-vcalendar", ".vcs"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/vda", ".vda"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "video/vdo", ".vdo"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/groupwise",
                                                                                           ".vew"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "video/vivo", ".viv"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "video/vnd.vivo", ".viv"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "video/vivo", ".vivo"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "video/vnd.vivo", ".vivo"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/vocaltec-media-desc",
                                                                                           ".vmd"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/vocaltec-media-file",
                                                                                           ".vmf"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "audio/voc", ".voc"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "audio/x-voc", ".voc"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "video/vosaic", ".vos"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "audio/voxware", ".vox"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "audio/x-twinvq-plugin",
                                                                                           ".vqe"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "audio/x-twinvq", ".vqf"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "audio/x-twinvq-plugin",
                                                                                           ".vql"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/x-vrml", ".vrml"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "model/vrml", ".vrml"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "x-world/x-vrml", ".vrml"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "x-world/x-vrt", ".vrt"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/x-visio", ".vsd"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/x-visio", ".vst"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/x-visio", ".vsw"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/wordperfect6.0",
                                                                                           ".w60"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/wordperfect6.1",
                                                                                           ".w61"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/msword", ".w6w"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "audio/wav", ".wav"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "audio/x-wav", ".wav"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/x-qpro", ".wb1"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "image/vnd.wap.wbmp", ".wbmp"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/vnd.xara",
                                                                                           ".web"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/msword", ".wiz"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/x-123", ".wk1"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "windows/metafile", ".wmf"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "text/vnd.wap.wml", ".wml"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/vnd.wap.wmlc",
                                                                                           ".wmlc"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "text/vnd.wap.wmlscript",
                                                                                           ".wmls"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/vnd.wap.wmlscriptc",
                                                                                           ".wmlsc"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/msword", ".word"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/wordperfect",
                                                                                           ".wp"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/wordperfect",
                                                                                           ".wp5"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/wordperfect6.0",
                                                                                           ".wp5"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/wordperfect",
                                                                                           ".wp6"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/wordperfect",
                                                                                           ".wpd"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/x-wpwin", ".wpd"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/x-lotus", ".wq1"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/mswrite", ".wri"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/x-wri", ".wri"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/x-world", ".wrl"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "model/vrml", ".wrl"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "x-world/x-vrml", ".wrl"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "model/vrml", ".wrz"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "x-world/x-vrml", ".wrz"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "text/scriplet", ".wsc"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/x-wais-source",
                                                                                           ".wsrc"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/x-wintalk",
                                                                                           ".wtk"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "image/x-xbitmap", ".xbm"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "image/x-xbm", ".xbm"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "image/xbm", ".xbm"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "video/x-amt-demorun", ".xdr"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "xgl/drawing", ".xgz"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "image/vnd.xiff", ".xif"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/excel", ".xl"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/excel", ".xla"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/x-excel", ".xla"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/x-msexcel",
                                                                                           ".xla"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/excel", ".xlb"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/vnd.ms-excel",
                                                                                           ".xlb"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/x-excel", ".xlb"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/excel", ".xlc"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/vnd.ms-excel",
                                                                                           ".xlc"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/x-excel", ".xlc"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/excel", ".xld"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/x-excel", ".xld"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/excel", ".xlk"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/x-excel", ".xlk"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/excel", ".xll"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/vnd.ms-excel",
                                                                                           ".xll"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/x-excel", ".xll"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/excel", ".xlm"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/vnd.ms-excel",
                                                                                           ".xlm"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/x-excel", ".xlm"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/excel", ".xls"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/vnd.ms-excel",
                                                                                           ".xls"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/x-excel", ".xls"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/x-msexcel",
                                                                                           ".xls"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/excel", ".xlt"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/x-excel", ".xlt"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/excel", ".xlv"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/x-excel", ".xlv"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/excel", ".xlw"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/vnd.ms-excel",
                                                                                           ".xlw"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/x-excel", ".xlw"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/x-msexcel",
                                                                                           ".xlw"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "audio/xm", ".xm"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/xml", ".xml"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "text/xml", ".xml"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "xgl/movie", ".xmz"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/x-vnd.ls-xpix",
                                                                                           ".xpix"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "image/x-xpixmap", ".xpm"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "image/xpm", ".xpm"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "image/png", ".x-png"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "video/x-amt-showrun", ".xsr"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "image/x-xwd", ".xwd"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "image/x-xwindowdump", ".xwd"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "chemical/x-pdb", ".xyz"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/x-compress",
                                                                                           ".z"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/x-compressed",
                                                                                           ".z"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/x-compressed",
                                                                                           ".zip"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/x-zip-compressed",
                                                                                           ".zip"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/zip", ".zip"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "multipart/x-zip", ".zip"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "application/octet-stream",
                                                                                           ".zoo"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "text/x-script.zsh", ".zsh "),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "fax", ".fax"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "pdf", ".pdf"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "reo", ".reo"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "doc", ".doc"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "otf", ".otf"),
                                                                                       new KeyValuePair<string, string>(
                                                                                           "tif", ".tif")
                                                                                   };

        #endregion MimeExts                                                           };

        public static string ConvertMimeToExt(string mime)
        {
            return _mimeExts.FirstOrDefault(w => w.Key.ToLower() == mime.ToLower()).Value;
        }
    }
}