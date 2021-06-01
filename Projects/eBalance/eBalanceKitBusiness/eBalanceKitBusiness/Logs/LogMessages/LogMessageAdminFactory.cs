using eBalanceKitBusiness.Logs.DbMapping;
using eBalanceKitBusiness.Logs.Enums;

namespace eBalanceKitBusiness.Logs.LogMessages {
    class LogMessageAdminFactory {
        public static LogMessageAdmin CreateLogMessage(DbAdminLog dbAdminLog){
            switch (dbAdminLog.ContentType) {
                case AdminLogContentTypes.System:
                    return new LogMessageAdminSystem(dbAdminLog);
                case AdminLogContentTypes.Company:
                    return new LogMessageAdminCompany(dbAdminLog);
                case AdminLogContentTypes.Document:
                    return new LogMessageAdminDocument(dbAdminLog);
                case AdminLogContentTypes.Template:
                    return new LogMessageAdminTemplate(dbAdminLog);
                case AdminLogContentTypes.User:
                    return new LogMessageAdminUser(dbAdminLog);
                case AdminLogContentTypes.UserRightDocument:
                    return new LogMessageAdminUserRight(dbAdminLog);
                case AdminLogContentTypes.Role:
                    return new LogMessageAdminRole(dbAdminLog);
                case AdminLogContentTypes.Right:
                    return new LogMessageAdminRight(dbAdminLog);
                default:
                    return null;
            }
        }
    }

}
