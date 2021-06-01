// --------------------------------------------------------------------------------
// author: Marcus Gerlach
// since: 2012-02-03
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System.Collections.Generic;
using System.Net;
using System.Net.Mail;
using System.Text;
using Config.DbStructure;
using Config.Interfaces.Mail;
using DbAccess;
using System.IO;

namespace Config.Manager {
    public static class MailManager {
        static MailManager() {
            GlobalMailConfig = CreateMailConfig();
        }

        public static bool SendMailMessage(IMailConfig config, string subject, string message, string sender, List<FileInfo> lFiles = null, Encoding encoding = null) {
            if (config.MailConnectionType == MailConnectionType.SMTP) {
                using (SmtpClient client = new SmtpClient(config.Host) {
                    Credentials = new NetworkCredential(config.User, config.Password),
                    EnableSsl = config.UseSsl
                }) {
                    using (MailMessage mailMessage = new MailMessage {
                        From = new MailAddress(sender),
                        Subject = subject,
                        Body = message
                    }) {
                        foreach (var recipient in config.Recipents) {
                            mailMessage.To.Add(recipient);
                        }
                        if (encoding == null) {
                            mailMessage.BodyEncoding = Encoding.ASCII;
                            mailMessage.SubjectEncoding = Encoding.ASCII;
                        } else {
                            mailMessage.BodyEncoding = encoding;
                            mailMessage.SubjectEncoding = encoding;
                        }

                        if (lFiles != null) {
                            foreach (var lFile in lFiles) {
                                mailMessage.Attachments.Add(new Attachment(lFile.FullName));
                            }
                        }

                        try {
                            client.Send(mailMessage);
                            return true;
                        } catch (System.Exception) {
                            return false;
                        }
                    }
                }
            }
            return false;
        }

        private static IMailConfig CreateMailConfig() {
            return new MailConfig() {
                MailConnectionType = MailConnectionType.SMTP,
                Host = "smtp.mail.yahoo.de",
                Port = 25,
                UseSsl = false,
                User = "rsadata7@yahoo.de",
                Password = "5TGyrf"
            };
        }

        public static IMailConfig GlobalMailConfig {get; set; }

        public static void Load(IDatabase conn) {
            List<MailConfig> mailConfigs = conn.DbMapping.Load<MailConfig>();
            if (mailConfigs.Count > 0)
                GlobalMailConfig = mailConfigs[mailConfigs.Count - 1];
        }
    }
}