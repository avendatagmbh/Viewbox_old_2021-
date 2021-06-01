// --------------------------------------------------------------------------------
// author: Benjamin Held / Mirko Dibbert
// since: 2011-06-24
// copyright 2011 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Xml;
using DbAccess;
using eBalanceKitBusiness.AuditCorrections;
using eBalanceKitBusiness.HyperCubes.Interfaces.Structure;
using eBalanceKitBusiness.HyperCubes.Structures;
using eBalanceKitBusiness.Interfaces;
using eBalanceKitBusiness.Localisation;
using eBalanceKitBusiness.Logs.Actions;
using eBalanceKitBusiness.Logs.DbMapping;
using eBalanceKitBusiness.Logs.Enums;
using eBalanceKitBusiness.Logs.Types;
using eBalanceKitBusiness.Manager;
using eBalanceKitBusiness.Reconciliation;
using eBalanceKitBusiness.Reconciliation.DbMapping;
using eBalanceKitBusiness.Reconciliation.Interfaces;
using eBalanceKitBusiness.Reconciliation.Interfaces.ReconciliationTypes;
using eBalanceKitBusiness.Rights;
using eBalanceKitBusiness.Structures;
using eBalanceKitBusiness.Structures.DbMapping;
using eBalanceKitBusiness.Structures.DbMapping.BalanceList;
using eBalanceKitBusiness.Structures.DbMapping.MappingTemplate;
using eBalanceKitBusiness.Structures.DbMapping.ValueMappings;

namespace eBalanceKitBusiness.Logs {
    internal class LogManager {
        #region SingletonPattern
        private static LogManager _instance;

        private LogManager() {
            MainLogsAdded = 0;
            LogBuffer = new Queue<LogEntryBase>();
            LogActionBuffer = new Queue<ActionLog>();
        }

        public static LogManager Instance { get { return _instance ?? (_instance = new LogManager()); } }
        #endregion

        #region Properties
        private int MainLogsAdded { get; set; }
        //private int UndoSteps { get { return 100; } } DEVNOTE: Constant in property...

        /// <summary>
        /// Used to supress logging events, e.g. for nessesary temporary changed during the XBRL export.
        /// </summary>
        public bool Log { get; set; }

        public bool Dispose { get; set; }
        private Queue<LogEntryBase> LogBuffer { get; set; }
        private Queue<ActionLog> LogActionBuffer { get; set; }
        #endregion

        #region Methods
        //private void CheckUndoActions() {
        //    if (MainLogsAdded < 2 * UndoSteps) return;
        //    using (IDatabase conn = AppConfig.ConnectionManager.GetConnection()) {
        //        try {
        //            conn.BeginTransaction();

        //            //Load main logs
        //            Logs.Logs logs = new Logs.Logs();
        //            LogFilter filter = new LogFilter();
        //            filter.WhereUndoPossible();
        //            filter.OrderBy("undo_possible");
        //            logs.ReadLogs(filter, conn);

        //            //Delete unnecessary logs
        //            int end = logs.LogList.Count - UndoSteps;
        //            for (int i = logs.LogList.Count - 1; i >= end; --i)
        //                logs.LogList.RemoveAt(i);

        //            //Read subclasses
        //            logs.ReadDerivedLogs(conn);
        //            foreach (var log in logs.LogList) {
        //                log.EraseUndo(conn);
        //            }

        //            conn.CommitTransaction();
        //            MainLogsAdded = 0;
        //        } catch (Exception ex) {
        //            conn.RollbackTransaction();
        //            throw ex;
        //        }
        //    }
        //}

        public void LogToDatabase(object msecs) {
            Dispose = false;
            int localMSec = 0;
            const int sleepTime = 100;
            while (!Dispose) {
                Thread.Sleep(sleepTime);
                localMSec += sleepTime;
                if (Dispose) continue;
                if (localMSec >= (int)msecs) {
                    if (LogActionBuffer.Count > 0) {
                        var actionLogs = new List<ActionLog>();
                        lock (LogActionBuffer) {
                            while (LogActionBuffer.Count > 0) {
                                //actionLogs.Add(LogActionBuffer.Dequeue());
                                ActionLog log = LogActionBuffer.Dequeue();
                                if (log != null) actionLogs.Add(log);
                            }
                            LogActionBuffer.Clear();
                        }
                        using (IDatabase conn = AppConfig.ConnectionManager.GetConnection()) {
                            foreach (ActionLog logAction in actionLogs)
                                logAction.DoAction(conn);
                        }
                    }
                    if (LogBuffer.Count > 0) {
                        var logs = new List<LogEntryBase>();
                        lock (LogBuffer) {
                            while (LogBuffer.Count > 0)
                                logs.Add(LogBuffer.Dequeue());
                            LogBuffer.Clear();
                        }

                        using (IDatabase conn = AppConfig.ConnectionManager.GetConnection()) {
                            try {
                                conn.BeginTransaction();
                                foreach (LogEntryBase log in logs)
                                    if (log != null)
                                        log.SaveToDb(conn);
                                conn.CommitTransaction();
                            }
                            catch (Exception) {
                                conn.RollbackTransaction();
                            }
                        }
                    }
                    localMSec = 0;
                }
            }
        }

        private int GetUserId() {
            if (UserManager.Instance.CurrentUser != null)
                return UserManager.Instance.CurrentUser.Id;
            if (UserManager.Instance.Users.Count > 0)
                return UserManager.Instance.Users.First().Id;
            return 0;
        }

        //public void TransferValueChanged(ReconciliationInfo ReconciliationInfoLine, decimal? newValue, bool previousYear,
        //                                 Document document) {
        //    if (Log) {
        //        int taxonomyId = ReconciliationInfoLine.Document.TaxonomyIdManager.GetId(ReconciliationInfoLine.Value.Element.Id);
        //        string taxString = Convert.ToString(taxonomyId);
        //        NewReportLog(document.Id, ReportLogContentTypes.TransferValue, ActionTypes.Change,
        //                     ReconciliationInfoLine.Id,
        //                     previousYear
        //                         ? ObjToStr(ReconciliationInfoLine.TransferValuePreviousYear)
        //                         : ObjToStr(ReconciliationInfoLine.TransferValue), ObjToStr(newValue),
        //                     previousYear
        //                         ? CreateXmlString("Type", "TVPY", "tId", taxString)
        //                         : CreateXmlString("Type", "TV", "tId", taxString)
        //            );
        //    }
        //}

        #region NewObjects
        /// <summary>
        /// Create a new log_admin entry that a new Company was added.
        /// </summary>
        /// <remarks><list type="table">
        /// <item><term>contentType</term><description>AdminLogContentTypes.Company</description></item>
        /// <item><term>actionType</term><description>ActionTypes.New</description></item>
        /// <item><term>refId</term><description>company.Id</description></item>
        /// <item><term>oldValue</term><description>null</description></item>
        /// <item><term>newValue</term><description>null</description></item>
        /// <item><term>info</term><description>null</description></item>
        /// </list></remarks>
        public void NewCompany(ILoggableObject company, bool load) {
            company.NewLog += NewLog;
            if (!load) NewAdminLog(AdminLogContentTypes.Company, ActionTypes.New, company.Id);
        }

        /// <summary>
        /// Create a new log_admin entry that a new System was added.
        /// </summary>
        /// <remarks><list type="table">
        /// <item><term>contentType</term><description>AdminLogContentTypes.System</description></item>
        /// <item><term>actionType</term><description>ActionTypes.New</description></item>
        /// <item><term>refId</term><description>system.Id</description></item>
        /// <item><term>oldValue</term><description>null</description></item>
        /// <item><term>newValue</term><description>null</description></item>
        /// <item><term>info</term><description>null</description></item>
        /// </list></remarks>
        public void NewSystem(Structures.DbMapping.System system, bool load) {
            system.NewLog += NewLog;
            if (!load) NewAdminLog(AdminLogContentTypes.System, ActionTypes.New, system.Id);
        }

        /// <summary>
        /// Create a new log_admin entry that a new Document was added.
        /// </summary>
        /// <remarks><list type="table">
        /// <item><term>contentType</term><description>AdminLogContentTypes.Document</description></item>
        /// <item><term>actionType</term><description>ActionTypes.New</description></item>
        /// <item><term> refId</term><description>document.Id</description></item>
        /// <item><term>oldValue</term><description>null</description></item>
        /// <item><term>newValue</term><description>null</description></item>
        /// <item><term>info</term><description>null</description></item>
        /// </list></remarks>
        public void NewDocument(Document document, bool load) {
            // create new document log tables for this id if not existent
            NewActionLog(new ActionAddDocument(document.Id));
            document.NewLog += NewLog;
            if (!load) NewAdminLog(AdminLogContentTypes.Document, ActionTypes.New, document.Id);
        }

        private void NewActionLog(ActionLog action) { LogActionBuffer.Enqueue(action); }

        /// <summary>
        /// Create a new log_admin entry that a new User was added and a log_admin entry for each property UserName, FullName, IsActive, IsAdmin, PasswordHash.
        /// </summary>
        /// <remarks><list type="table"> <para> 
        /// <item><term>contentType</term><description>AdminLogContentTypes.User</description></item>
        /// <item><term>actionType</term><description>ActionTypes.New</description></item>
        /// <item><term>refId</term><description>user.Id</description></item>
        /// <item><term>oldValue</term><description>null</description></item>
        /// <item><term>newValue</term><description>null</description></item>
        /// <item><term>info</term><description>null</description></item>
        /// </para><para> 
        /// <item><term>contentType</term><description>AdminLogContentTypes.User</description></item>
        /// <item><term>actionType</term><description>ActionTypes.Change</description></item>
        /// <item><term>refId</term><description>user.Id</description></item>
        /// <item><term>oldValue</term><description>null</description></item>
        /// <item><term>newValue</term><description>user.UserName</description></item>
        /// <item><term>info</term><description><root Type="UserName" /></description></item>
        ///   </para>
        /// </list></remarks>
        public void NewUser(User user, bool load) {
            user.NewLog += NewLog;
            if (load) return;

            NewAdminLog(AdminLogContentTypes.User, ActionTypes.New, user.Id);

            //This needs to be added as the add user dialog does not update the user data incrementally
            NewLog(user, new LogArgs("UserName", null, user.UserName));
            NewLog(user, new LogArgs("FullName", null, user.FullName));
            NewLog(user, new LogArgs("IsActive", null, user.IsActive));
            NewLog(user, new LogArgs("IsAdmin", null, user.IsAdmin));
            NewLog(user, new LogArgs("PasswordHash", null, user.PasswordHash));
            NewLog(user, new LogArgs("IsCompanyAdmin", null, user.IsCompanyAdmin));
            NewLog(user, new LogArgs("AssignedCompanies", null, user.AssignedCompaniesAsXml));
            NewLog(user, new LogArgs("LastLogin", null, user.LastLogin));
        }



        #region NewAdminLog
        private void NewAdminLog(AdminLogContentTypes contentType, ActionTypes actionType, long refId,
                                 string oldValue = null, string newValue = null, string info = null) {
            var adminLog = new DbAdminLog {
                ContentType = contentType,
                ActionType = actionType,
                ReferenceId = refId,
                OldValue = oldValue,
                NewValue = newValue,
                Info = info,
                TimeStamp = DateTime.Now,
                UserId = GetUserId()
            };

            try {
                LogBuffer.Enqueue(new AdminLog(adminLog));
            }
            catch (Exception ex) {
                throw new Exception(ExceptionMessages.FailedToAddDataset + ex.Message);
            }
        }
        #endregion

        #region NewReportAccountRelatedLog
        /// <summary>
        /// Adds a new log entry to the report log.
        /// </summary>
        /// <param name="documentId">Id of the referenced document.</param>
        /// <param name="contentType">Content type of the logged entity.</param>
        /// <param name="actionType">Action type of the log event.</param>
        /// <param name="refId">Id of the logged entity.</param>
        /// <param name="oldValue">Old value in case of value changes.</param>
        /// <param name="newValue">Old value in case of value changes.</param>
        /// <param name="info">XML representation which contains the log entry details.</param>
        private void AddReportLogEntry(
            int documentId,
            ReportLogContentTypes contentType,
            ActionTypes actionType,
            long refId,
            string oldValue = null,
            string newValue = null,
            string info = null) {

            var reportLog = new DbReportLog {
                ReportLogContentType = contentType,
                ActionType = actionType,
                ReferenceId = refId,
                OldValue = oldValue,
                NewValue = newValue,
                Info = info,
                TimeStamp = DateTime.Now,
                UserId = GetUserId()
            };

            try {
                LogBuffer.Enqueue(new ReportLog(documentId, reportLog));
            }
            catch (Exception ex) {
                throw new Exception(ExceptionMessages.FailedToAddDataset + ex.Message);
            }
        }
        #endregion

        #region NewLog
        private static string ObjToStr(object obj) {
            //return obj == null ? null : obj.ToString();

            if (obj == null) {
                return null;
            }

            System.ComponentModel.TypeConverter tc = System.ComponentModel.TypeDescriptor.GetConverter(obj);
            
            try {
                return tc.ConvertToString(null, CultureInfo.InvariantCulture, obj);
            } catch (ArgumentException) {
                // ToDo: LOG
            } catch (NotSupportedException) {
                // ToDo: LOG
            }
            return obj.ToString();
        }

        private static string CreateTypeXmlString(string type) { return CreateXmlString("Type", type); }
        private static string CreateXmlString(string key, string value) { return "<root " + key + "=\"" + value + "\" />"; }
        private static string CreateXmlString(string key1, string value1, string key2, string value2) { return "<root " + key1 + "=\"" + value1 + "\" " + key2 + "=\"" + value2 + "\" />"; }
        private static string CreateXmlString(Dictionary<string,object> attributes) {
            StringBuilder result = new StringBuilder("<root ");
            foreach (var attribute in attributes) {
                result.Append(attribute.Key);
                result.Append(" = \"");
                result.Append(ObjToStr(attribute.Value));
                result.Append("\" ");
            }
            result.Append("/>");
            return result.ToString();
        }

        private void NewLog(ILoggableObject obj, LogArgs logArgs) {
            AdminLogContentTypes contentType;
            if (obj is Structures.DbMapping.System) contentType = AdminLogContentTypes.System;
            else if (obj is Document) contentType = AdminLogContentTypes.Document;
            else if (obj is User) contentType = AdminLogContentTypes.User;
            else if (obj is Company) contentType = AdminLogContentTypes.Company;
            else throw new Exception("LogManager: Unknown content type!");

            NewAdminLog(contentType, ActionTypes.Change, obj.Id,
                        ObjToStr(logArgs.OldValue), ObjToStr(logArgs.NewValue), CreateTypeXmlString(logArgs.Element));
        }
        #endregion NewLog

        #endregion NewObjects

        #region CopyDocument
        /// <summary>
        /// Create a new log_admin entry that a report was copied.
        /// </summary>
        /// <param name="oldDocument">The source document that was copied.</param>
        /// <param name="newDocument">The new document that was created.</param>
        /// <remarks><list type="table">
        /// <item><term>contentType</term><description>AdminLogContentTypes.Document</description></item>
        /// <item><term>actionType</term><description>ActionTypes.New</description></item>
        /// <item><term> refId</term><description>document.Id</description></item>
        /// <item><term>oldValue</term><description>null</description></item>
        /// <item><term>newValue</term><description>null</description></item>
        /// <item><term>info</term><description><root SourceDocumentId="oldDocument.Id" /></description></item>
        /// </list></remarks>
        public void CopyDocument(Document oldDocument, Document newDocument) {
            // create new document log tables for the new document if not existent
            NewActionLog(new ActionAddDocument(newDocument.Id));
            newDocument.NewLog += NewLog;
            NewAdminLog(AdminLogContentTypes.Document, ActionTypes.New, newDocument.Id,
                        info: CreateXmlString("SourceDocumentId", ObjToStr(oldDocument.Id)));
        }
        #endregion


        #region DeleteObjects
        /// <summary>
        /// Create a new log_admin entry that a Company was deleted. Create a new NewActionLog for company.Id.
        /// </summary>
        /// <remarks><list type="table">
        /// <item><term>contentType</term><description>AdminLogContentTypes.Company</description></item>
        /// <item><term>actionType</term><description>ActionTypes.Delete</description></item>
        /// <item><term>refId</term><description>company.Id</description></item>
        /// <item><term>oldValue</term><description>null</description></item>
        /// <item><term>newValue</term><description>null</description></item>
        /// <item><term>info</term><description><root Name="company.Name" /></description></item>
        /// </list></remarks>
        public void DeleteCompany(Company company) {
            NewAdminLog(AdminLogContentTypes.Company, ActionTypes.Delete, company.Id, null, null,
                        CreateXmlString("Name", company.Name));
            NewActionLog(new ActionDeleteCompany(company.Id));
        }

        /// <summary>
        /// Create a new log_admin entry that a System was deleted. Create a new NewActionLog for system.Id.
        /// </summary>
        /// <remarks><list type="table">
        /// <item><term>contentType</term><description>AdminLogContentTypes.System</description></item>
        /// <item><term>actionType</term><description>ActionTypes.Delete</description></item>
        /// <item><term>refId</term><description>system.Id</description></item>
        /// <item><term>oldValue</term><description>null</description></item>
        /// <item><term>newValue</term><description>null</description></item>
        /// <item><term>info</term><description><root Name="system.Name" /></description></item>
        /// </list></remarks>
        public void DeleteSystem(Structures.DbMapping.System system) {
            NewAdminLog(AdminLogContentTypes.System, ActionTypes.Delete, system.Id, null, null,
                        CreateXmlString("Name", system.Name));
            NewActionLog(new ActionDeleteSystem(system.Id));
        }

        /// <summary>
        /// Create a new log_admin entry that a Document was deleted. Create a new NewActionLog for document.Id.
        /// </summary>
        /// <remarks><list type="table">
        /// <item><term>contentType</term><description>AdminLogContentTypes.Document</description></item>
        /// <item><term>actionType</term><description>ActionTypes.Delete</description></item>
        /// <item><term>refId</term><description>document.Id</description></item>
        /// <item><term>oldValue</term><description>null</description></item>
        /// <item><term>newValue</term><description>null</description></item>
        /// <item><term>info</term><description><root Name="document.Name" /></description></item>
        /// </list></remarks>
        public void DeleteDocument(Document document) {
            NewAdminLog(AdminLogContentTypes.Document, ActionTypes.Delete, document.Id, null, null,
                        CreateXmlString("Name", document.Name));
            NewActionLog(new ActionDeleteDocument(document.Id));
        }

        /// <summary>
        /// Create a new log_admin entry that a User was deleted.
        /// </summary>
        /// <remarks><list type="table">
        /// <item><term>contentType</term><description>AdminLogContentTypes.User</description></item>
        /// <item><term>actionType</term><description>ActionTypes.Delete</description></item>
        /// <item><term>refId</term><description>user.Id</description></item>
        /// <item><term>oldValue</term><description>null</description></item>
        /// <item><term>newValue</term><description>null</description></item>
        /// <item><term>info</term><description><root Name="user.FullName" /></description></item>
        /// </list></remarks>
        public void DeleteUser(User user) {
            NewAdminLog(AdminLogContentTypes.User, ActionTypes.Delete, user.Id, null, null,
                        CreateXmlString("Name", user.FullName));
        }

        /// <summary>
        /// Create a new log_admin entry that a role was deleted.
        /// </summary>
        /// <remarks><list type="table">
        /// <item><term>contentType</term><description>AdminLogContentTypes.Role</description></item>
        /// <item><term>actionType</term><description>ActionTypes.Delete</description></item>
        /// <item><term>refId</term><description>DbRole.Id</description></item>
        /// <item><term>oldValue</term><description>DbRole.Name</description></item>
        /// <item><term>newValue</term><description>null</description></item>
        /// <item><term>info</term><description>null</description></item>
        /// </list></remarks>
        public void DeleteRole(Role role) {
            NewAdminLog(AdminLogContentTypes.Role, ActionTypes.Delete, role.DbRole.Id, role.DbRole.Name);
        }

        /// <summary>
        /// Delete a right.
        /// </summary>
        /// <remarks><list type="table">
        /// <item><term>contentType</term><description>AdminLogContentTypes.Right</description></item>
        /// <item><term>actionType</term><description>ActionTypes.Delete</description></item>
        /// <item><term>refId</term><description>DbRight.Id</description></item>
        /// <item><term>oldValue</term><description>DbRight.Rights</description></item>
        /// <item><term>newValue</term><description>null</description></item>
        /// <item><term>info</term><description>DbRight.RoleId=xx DbRight.ReferenceId=yy DbRight.ContentType=zz</description></item>
        /// </list></remarks>
        public void DeleteRight(Right right, Structures.DbMapping.Rights.DbRight.ContentTypes contentType) {
            // Check if the right was stored on the database or just in the program
            if (right.DbRight.Id == 0) {
                return;
            }
            NewAdminLog(AdminLogContentTypes.Right, ActionTypes.Delete, right.DbRight.Id,
                        Convert.ToString(right.DbRight.GetRightForLogging()), null,
                        CreateXmlString(new Dictionary<string, object> {
                            {"RoleId", right.DbRight.RoleId.ToString(CultureInfo.InvariantCulture)},
                            {
                                            "ReferenceId",
                                            right.DbRight.ReferenceId.ToString(CultureInfo.InvariantCulture)
                                            },
                            {"ContentType", (int)contentType}
                        }));
        }

        /// <summary>
        /// Create a new log_admin entry that a new Role was added.
        /// </summary>
        /// <param name="role">The new Role.</param>
        /// <remarks><list type="table">
        /// <item><term>contentType</term><description>AdminLogContentTypes.Role</description></item>
        /// <item><term>actionType</term><description>ActionTypes.New</description></item>
        /// <item><term>refId</term><description>DbRole.Id</description></item>
        /// <item><term>oldValue</term><description>null</description></item>
        /// <item><term>newValue</term><description>DbRole.Name</description></item>
        /// <item><term>info</term><description>null</description></item>
        /// </list></remarks>
        public void NewRole(Role role) {
            NewAdminLog(AdminLogContentTypes.Role, ActionTypes.New, role.DbRole.Id, null, role.DbRole.Name);
        }

        /// <summary>
        /// Create a new log_admin entry that a new Right was added.
        /// </summary>
        /// <param name="right">The new right.</param>
        /// <remarks><list type="table">
        /// <item><term>contentType</term><description>AdminLogContentTypes.Right</description></item>
        /// <item><term>actionType</term><description>ActionTypes.New</description></item>
        /// <item><term>refId</term><description>DbRight.Id</description></item>
        /// <item><term>oldValue</term><description>null</description></item>
        /// <item><term>newValue</term><description>DbRight.Rights</description></item>
        /// <item><term>info</term><description>DbRight.RoleId=xx DbRight.ReferenceId=yy (int)DbRight.ContentType=zz</description></item>
        /// </list></remarks>
        public void NewRight(Right right) {
            LogUpdateRight(null, right.DbRight);
        }
        #endregion

        /// <summary>
        /// Returns a Xml for the specified DbRight in the format "RoleId=xx ReferenceId=yy ContentType(right's)=zz"
        /// </summary>
        private string GetXmlForDbRight(Structures.DbMapping.Rights.DbRight right) {
            return CreateXmlString(new Dictionary<string, object>(){{"RoleId", right.RoleId.ToString(CultureInfo.InvariantCulture)},
                            {"ReferenceId", right.ReferenceId.ToString(CultureInfo.InvariantCulture)}, {"ContentType", (int)right.ContentType}});
        }

        /// <summary>
        /// Create a new log_admin entry for an update of this specific Role and for each changed right within this role.<see cref="UpdateRight"/>
        /// </summary>
        /// <param name="newRole">The Role that changed with the new values.</param>
        /// <remarks>Uses the database to get the old values. Comares every right and adds log entries for the changed rights.
        /// <list type="table">
        /// <item><term>contentType</term><description>AdminLogContentTypes.Role</description></item>
        /// <item><term>actionType</term><description>ActionTypes.Change</description></item>
        /// <item><term>refId</term><description>DbRole.Id</description></item>
        /// <item><term>oldValue</term><description><root Name="DbRole.Name" Comment="DbRole.Comment" /></description></item>
        /// <item><term>newValue</term><description><root Name="DbRole.Name" Comment="DbRole.Comment" /></description></item>
        /// <item><term>info</term><description>null</description></item>
        /// </list></remarks>
        public void UpdateRole(Role newRole) {
            Role oldData = new Role(null);
            using (IDatabase conn = AppConfig.ConnectionManager.GetConnection()) {
                try {
                    var dbRole = conn.DbMapping.Load<Structures.DbMapping.Rights.DbRole>(newRole.DbRole);
                    oldData = new Role(dbRole);
                    oldData.LoadDetails();
                }
                #pragma warning disable 0168
                catch (Exception ex) {
                    // ToDo LOG
                }
                #pragma warning restore 0168
            }

            string oldName = oldData.DbRole == null ? string.Empty : oldData.DbRole.Name;
            string oldComment = oldData.DbRole == null ? string.Empty : oldData.DbRole.Comment ?? string.Empty;
            string newName = newRole.DbRole.Name;
            string newComment = newRole.DbRole.Comment ?? string.Empty;
            NewAdminLog(AdminLogContentTypes.Role, ActionTypes.Change, newRole.DbRole.Id,
                        CreateXmlString("Name", oldName, "Comment", oldComment),
                        CreateXmlString("Name", newName, "Comment", newComment));

            for (int i = 0; i < newRole.Rights.Count; i++) {
                if (oldData.Rights.Count > i && oldData.Rights[i] != newRole.Rights[i]) {
                    LogUpdateRight(oldData.Rights[i].DbRight, newRole.Rights[i].DbRight);
                }
            }
        }

        private void LogUpdateRight(Structures.DbMapping.Rights.DbRight oldValue, Structures.DbMapping.Rights.DbRight newValue) {
            var info = GetXmlForDbRight(newValue);

            if (oldValue != null && info.Equals(GetXmlForDbRight(oldValue)) && oldValue.GetRightForLogging() == newValue.GetRightForLogging()) {
                // no change so we don't need to log anything
                return;
            }

            NewAdminLog(AdminLogContentTypes.Right, oldValue == null ? ActionTypes.New : ActionTypes.Change, newValue.Id,
                        oldValue == null ? null : oldValue.GetRightForLogging().ToString(CultureInfo.InvariantCulture),
                        newValue.GetRightForLogging().ToString(CultureInfo.InvariantCulture), info);

            //if (oldValue.Read != newValue.Read) {
            //    NewAdminLog(AdminLogContentTypes.Right, ActionTypes.Change, newValue.Id,
            //                Convert.ToString(oldValue.Read), Convert.ToString(newValue.Read), info);
            //}
            //if (oldValue.Write != newValue.Write) {
            //    NewAdminLog(AdminLogContentTypes.Right, ActionTypes.Change, newValue.Id,
            //                Convert.ToString(oldValue.Write), Convert.ToString(newValue.Write), info);
            //}
            //if (oldValue.Send != newValue.Send) {
            //    NewAdminLog(AdminLogContentTypes.Right, ActionTypes.Change, newValue.Id,
            //                Convert.ToString(oldValue.Send), Convert.ToString(newValue.Send), info);
            //}
            //if (oldValue.Export != newValue.Export) {
            //    NewAdminLog(AdminLogContentTypes.Right, ActionTypes.Change, newValue.Id,
            //                Convert.ToString(oldValue.Export), Convert.ToString(newValue.Export), info);
            //}
        }

        /// <summary>
        /// Create a new log_admin entry for an update of this specific Right.
        /// </summary>
        /// <param name="newRight">The Right that changed with the new values.</param>
        /// <remarks>Uses the database to get the old value.</remarks>
        /// <remarks><list type="table">
        /// <item><term>contentType</term><description>AdminLogContentTypes.Right</description></item>
        /// <item><term>actionType</term><description>ActionTypes.Change</description></item>
        /// <item><term>refId</term><description>DbRight.Id</description></item>
        /// <item><term>oldValue</term><description>DbRight.Rights</description></item>
        /// <item><term>newValue</term><description>DbRight.Rights</description></item>
        /// <item><term>info</term><description>RoleId="DbRight.RoleId" ReferenceId="DbRight.ReferenceId" ContentType="(int)DbRight.ContentType"</description></item>
        /// </list></remarks>
        public void UpdateRight(Right newRight) {
            Structures.DbMapping.Rights.DbRight oldData = new Structures.DbMapping.Rights.DbRight();
            using (IDatabase conn = AppConfig.ConnectionManager.GetConnection()) {
                try {
                    oldData = conn.DbMapping.Load<Structures.DbMapping.Rights.DbRight>(newRight.DbRight.Id);
                }
                #pragma warning disable 0168
                catch (Exception ex) {
                    // ToDo LOG
                }
                #pragma warning restore 0168
            }
            if (oldData != newRight.DbRight) {
                LogUpdateRight(oldData, newRight.DbRight);
            }
        }
        
        #region BalanceListChange

        /// <summary>
        /// Create a new log_report entry for an update of this specific BalanceList.
        /// </summary>
        /// <remarks><list type="table">
        /// <item><term>documentId</term><description>balanceList.Document.Id</description></item>
        /// <item><term>contentType</term><description>ReportLogContentTypes.BalanceList</description></item>
        /// <item><term>actionType</term><description>ActionTypes.New or ActionTypes.Delete (depending on parameter add)</description></item>
        /// <item><term>refId</term><description>balanceList.Id</description></item>
        /// <item><term>oldValue</term><description>null</description></item>
        /// <item><term>newValue</term><description>null</description></item>
        /// <item><term>info</term><description>balanceList.ToXml()</description></item>
        /// </list></remarks>
        public void BalanceListChange(IBalanceList balanceList, bool add) {
            if (Log) {
                AddReportLogEntry(balanceList.Document.Id, ReportLogContentTypes.BalanceList,
                             add ? ActionTypes.New : ActionTypes.Delete,
                             balanceList.Id, null, null, balanceList.ToXml());
            }
        }
        #endregion BalanceListChange

        #region Assignments
        private string AssignmentType(IBalanceListEntry account) {
            if (account is SplittedAccount)
                return "SA";
            return "A";
        }

        private string GetAssignmentInfo(IBalanceListEntry account) { return CreateTypeXmlString(AssignmentType(account)); }

        #region DeleteAssignment

        /// <summary>
        /// Create a new log_report for a removed account assignment.
        /// </summary>
        /// <remarks><list type="table">
        /// <item><term>documentId</term><description>document.Id</description></item>
        /// <item><term>contentType</term><description>ReportLogContentTypes.Account</description></item>
        /// <item><term>actionType</term><description>ActionTypes.Delete</description></item>
        /// <item><term>refId</term><description>account.Id</description></item>
        /// <item><term>oldValue</term><description>account.AssignedElementId</description></item>
        /// <item><term>newValue</term><description>0</description></item>
        /// <item><term>info</term><description>A for Account or SA for SplittedAccount</description></item>
        /// </list></remarks>
        public void DeleteAssignment(IBalanceListEntry account, Document document) {
            AddReportLogEntry(document.Id, ReportLogContentTypes.Account, ActionTypes.Delete,
                         ((BalanceListEntryBase)account).Id, ObjToStr(account.AssignedElementId), "0",
                         GetAssignmentInfo(account));
        }
        #endregion DeleteAssignment

        #region DeleteAssignments

        /// <summary>
        /// Create one new log_report for all removed account assignments.
        /// </summary>
        /// <remarks><list type="table">
        /// <item><term>documentId</term><description>document.Id</description></item>
        /// <item><term>contentType</term><description>ReportLogContentTypes.Account</description></item>
        /// <item><term>actionType</term><description>ActionTypes.Delete</description></item>
        /// <item><term>refId</term><description>0</description></item>
        /// <item><term>oldValue</term><description>null</description></item>
        /// <item><term>newValue</term><description><root><acc Id="account.Id" Old="account.AssignedElementId" Type="A for Account or SA for SplittedAccount"/></root></description></item>
        /// <item><term>info</term><description>null</description></item>
        /// </list></remarks>
        public void DeleteAssignments(IEnumerable<IBalanceListEntry> accounts, Document document) {
            if (Log && accounts.Count() > 0) {
                if (accounts.Count() == 1) {
                    DeleteAssignment(accounts.First(), document);
                    return;
                }

                var doc = new XmlDocument();
                XmlElement root = doc.CreateElement("root");
                doc.AppendChild(root);
                foreach (IBalanceListEntry account in accounts) {
                    XmlElement accountTag = doc.CreateElement("Acc");
                    root.AppendChild(accountTag);
                    accountTag.Attributes.Append(doc.CreateAttribute("Id")).InnerText =
                        (((BalanceListEntryBase)account).Id).ToString(CultureInfo.InvariantCulture);
                    accountTag.Attributes.Append(doc.CreateAttribute("Old")).InnerText =
                        ObjToStr(account.AssignedElementId);
                    accountTag.Attributes.Append(doc.CreateAttribute("Type")).InnerText = AssignmentType(account);
                }
                AddReportLogEntry(document.Id, ReportLogContentTypes.Account, ActionTypes.Delete,
                             0, null, doc.InnerXml, null);
            }
        }
        #endregion DeleteAssignments

        #region UpdateAssignment

        /// <summary>
        /// Create a new log_report for a removed account assignment.
        /// </summary>
        /// <remarks><list type="table">
        /// <item><term>documentId</term><description>document.Id</description></item>
        /// <item><term>contentType</term><description>ReportLogContentTypes.Account</description></item>
        /// <item><term>actionType</term><description>ActionTypes.Change (ActionTypes.New if oldAssignedId == 0)</description></item>
        /// <item><term>refId</term><description>account.Id</description></item>
        /// <item><term>oldValue</term><description>oldAssignedId</description></item>
        /// <item><term>newValue</term><description>assignedElementId</description></item>
        /// <item><term>info</term><description>A for Account or SA for SplittedAccount</description></item>
        /// </list></remarks>
        public void UpdateAssignment(IBalanceListEntry account, long oldAssignedId, long assignedElementId,
                                     Document document) {
            if (Log) {
                AddReportLogEntry(document.Id, ReportLogContentTypes.Account,
                             oldAssignedId == 0 ? ActionTypes.New : ActionTypes.Change,
                             ((BalanceListEntryBase)account).Id, ObjToStr(oldAssignedId), ObjToStr(assignedElementId),
                             GetAssignmentInfo(account));
            }
        }
        #endregion UpdateAssignment

        #region UpdateAssignments

        /// <summary>
        /// Create one new log_report for all changed account assignments.
        /// </summary>
        /// <remarks><list type="table">
        /// <item><term>documentId</term><description>document.Id</description></item>
        /// <item><term>contentType</term><description>ReportLogContentTypes.Account</description></item>
        /// <item><term>actionType</term><description>ActionTypes.Change</description></item>
        /// <item><term>refId</term><description>0</description></item>
        /// <item><term>oldValue</term><description>null</description></item>
        /// <item><term>newValue</term><description><root><acc Id="account.Id" Old="account.AssignedElementId" Type="A for Account or SA for SplittedAccount" New="assignment.Item3 (newAssignment.Id)"/></root></description></item>
        /// <item><term>info</term><description>null</description></item>
        /// </list></remarks>
        internal void UpdateAssignments(List<Tuple<IBalanceListEntry, int, int>> assignments, Document document) {
            if (Log && assignments.Count > 0) {
                if (assignments.Count == 1) {
                    UpdateAssignment(assignments[0].Item1, assignments[0].Item2, assignments[0].Item3, document);
                    return;
                }
                var doc = new XmlDocument();
                XmlElement root = doc.CreateElement("root");
                doc.AppendChild(root);
                foreach (var assignment in assignments) {
                    XmlElement account = doc.CreateElement("Acc");
                    root.AppendChild(account);
                    account.Attributes.Append(doc.CreateAttribute("Id")).InnerText =
                        Convert.ToString(((BalanceListEntryBase)assignment.Item1).Id);
                    account.Attributes.Append(doc.CreateAttribute("Old")).InnerText = Convert.ToString(assignment.Item2);
                    account.Attributes.Append(doc.CreateAttribute("New")).InnerText = Convert.ToString(assignment.Item3);
                    account.Attributes.Append(doc.CreateAttribute("Type")).InnerText = AssignmentType(assignment.Item1);
                }
                AddReportLogEntry(document.Id, ReportLogContentTypes.Account, ActionTypes.Change,
                             0, null, doc.InnerXml, null);
            }
        }
        #endregion UpdateAssignments

        #endregion Assignments

        #region Templates

        #region UseTemplate
        /// <summary>
        /// Create a new log_report entry for using the template.
        /// </summary>
        /// <remarks><list type="table">
        /// <item><term>documentId</term><description>document.Id</description></item>
        /// <item><term>contentType</term><description>ReportLogContentTypes.Template</description></item>
        /// <item><term>actionType</term><description>ActionTypes.Used</description></item>
        /// <item><term>refId</term><description>document.SelectedBalanceList.Id</description></item>
        /// <item><term>oldValue</term><description>null</description></item>
        /// <item><term>newValue</term><description>null</description></item>
        /// <item><term>info</term><description><root Name="selectedTemplate.Name" Comment="selectedTemplate.Comment" Id="selectedTemplate.Id" /></description></item>
        /// </list></remarks>
        public void UseTemplate(MappingTemplateHead selectedTemplate, Document document,
                                List<Tuple<IBalanceListEntry, string>> newAssignments) {
            if (Log) {
                AddReportLogEntry(document.Id, ReportLogContentTypes.Template, ActionTypes.Used,
                             document.SelectedBalanceList.Id,
                             null, null, selectedTemplate.ToXml().InnerXml);
            }
        }
        #endregion UseTemplate

        #region DeleteTemplate
        /// <summary>
        /// Create a new log_admin entry for deleting the template.
        /// </summary>
        /// <remarks><list type="table">
        /// <item><term>contentType</term><description>AdminLogContentTypes.Template</description></item>
        /// <item><term>actionType</term><description>ActionTypes.Delete</description></item>
        /// <item><term>refId</term><description>template.Id</description></item>
        /// <item><term>oldValue</term><description>MappingTemplate.TemplateManager.GetTemplateXml(template).InnerXml</description></item>
        /// <item><term>newValue</term><description>null</description></item>
        /// <item><term>info</term><description><root Name="selectedTemplate.Name" Comment="selectedTemplate.Comment" Id="selectedTemplate.Id" /></description></item>
        /// </list></remarks>
        public void DeleteTemplate(MappingTemplateHead template) {
            if (Log) {
                NewAdminLog(AdminLogContentTypes.Template, ActionTypes.Delete, template.Id,
                            MappingTemplate.TemplateManager.GetTemplateXml(template).InnerXml, null,
                            template.ToXml().InnerXml);
            }
        }
        #endregion DeleteTemplate

        #region NewTemplate
        /// <summary>
        /// Create a new log_admin entry for adding the template.
        /// </summary>
        /// <remarks><list type="table">
        /// <item><term>contentType</term><description>AdminLogContentTypes.Template</description></item>
        /// <item><term>actionType</term><description>ActionTypes.Delete</description></item>
        /// <item><term>refId</term><description>template.Id</description></item>
        /// <item><term>oldValue</term><description>null</description></item>
        /// <item><term>newValue</term><description>MappingTemplate.TemplateManager.GetTemplateXml(template).InnerXml</description></item>
        /// <item><term>info</term><description><root Name="selectedTemplate.Name" Comment="selectedTemplate.Comment" Id="selectedTemplate.Id" /></description></item>
        /// </list></remarks>
        public void NewTemplate(MappingTemplateHead template) {
            if (Log) {
                NewAdminLog(AdminLogContentTypes.Template, ActionTypes.New, template.Id, null,
                            MappingTemplate.TemplateManager.GetTemplateXml(template).InnerXml, template.ToXml().InnerXml);
            }
        }
        #endregion NewTemplate

        #region UpdateTemplate
        /// <summary>
        /// Create a new log_admin entry for changing the template.
        /// </summary>
        /// <remarks><list type="table">
        /// <item><term>contentType</term><description>AdminLogContentTypes.Template</description></item>
        /// <item><term>actionType</term><description>ActionTypes.Delete</description></item>
        /// <item><term>refId</term><description>template.Id</description></item>
        /// <item><term>oldValue</term><description>MappingTemplate.TemplateManager.GetTemplateXml(oldData).InnerXml</description></item>
        /// <item><term>newValue</term><description>MappingTemplate.TemplateManager.GetTemplateXml(template).InnerXml</description></item>
        /// <item><term>info</term><description>template.ToXml().InnerXml</description></item>
        /// </list></remarks>
        public void UpdateTemplate(MappingTemplateHead template) {
            if (Log) {
                MappingTemplateHead oldData = null;
                using (IDatabase conn = AppConfig.ConnectionManager.GetConnection()) {
                    try {
                        oldData = conn.DbMapping.Load<MappingTemplateHead>(template.Id);
                    }
                    #pragma warning disable 0168
                    catch (Exception ex) {
                        // ToDo LOG
                    }
                    #pragma warning restore 0168
                }

                NewAdminLog(AdminLogContentTypes.Template, ActionTypes.Change, template.Id,
                            MappingTemplate.TemplateManager.GetTemplateXml(oldData).InnerXml,
                            MappingTemplate.TemplateManager.GetTemplateXml(template).InnerXml, template.ToXml().InnerXml);
            }
        }
        #endregion UpdateTemplate

        #region ImportTemplate
        /// <summary>
        /// Create a new log_admin entry for importing the template.
        /// </summary>
        /// <remarks><list type="table">
        /// <item><term>contentType</term><description>AdminLogContentTypes.Template</description></item>
        /// <item><term>actionType</term><description>ActionTypes.Imported</description></item>
        /// <item><term>refId</term><description>template.Id</description></item>
        /// <item><term>oldValue</term><description>null</description></item>
        /// <item><term>newValue</term><description>sourceXml.InnerXml</description></item>
        /// <item><term>info</term><description><root Name="template.Name" Comment="template.Comment" Id="template.Id" File="fileName" /></description></item>
        /// </list></remarks>
        public void ImportTemplate(MappingTemplateHead template, XmlDocument sourceXml, string fileName) {
            var infoXml = template.ToXml();
            var child= infoXml.CreateAttribute("File");
            child.InnerText = fileName;
            if (infoXml.FirstChild.Attributes != null) infoXml.FirstChild.Attributes.Append(child);

            NewAdminLog(AdminLogContentTypes.Template, ActionTypes.Imported, template.Id,
                        null,
                        sourceXml.InnerXml, infoXml.InnerXml);
        }
        #endregion ImportTemplate

        #endregion Templates
        
        #region UpdateTaxonomy
        /// <summary>
        /// Create a new log_report entry for the change of the selected taxonomy.
        /// </summary>
        /// <param name="document">Document for which the taxonomy has been changed.</param>
        /// <param name="oldValue">Old main taxonomy name.</param>
        /// <remarks><list type="table">
        /// <item><term>documentId</term><description>document.Id</description></item>
        /// <item><term>contentType</term><description>ReportLogContentTypes.Document</description></item>
        /// <item><term>actionType</term><description>ActionTypes.Change</description></item>
        /// <item><term>refId</term><description>document.Id</description></item>
        /// <item><term>oldValue</term><description>oldValue</description></item>
        /// <item><term>newValue</term><description>document.MainTaxonomyInfo.Name</description></item>
        /// <item><term>info</term><description><root ChangedProperty="MainTaxonomy"/></description></item>
        /// </list></remarks>
        internal void UpdateTaxonomy(Document document, string oldValue) {
            if (Log) {
                const string xml = "<root ChangedProperty=\"MainTaxonomy\"/>";
                AddReportLogEntry(document.Id, ReportLogContentTypes.Document, ActionTypes.Change, document.Id, oldValue,
                                  document.MainTaxonomyInfo.Name, xml);
            }
        }
        #endregion // UpdateTaxonomy

        #region hypercube log functions

        #region NewHyperCube
        /// <summary>
        /// Create a new log_report entry for creating a new HyperCube.
        /// </summary>
        /// <remarks><list type="table">
        /// <item><term>documentId</term><description>cube.Document.Id</description></item>
        /// <item><term>contentType</term><description>ReportLogContentTypes.HyperCube</description></item>
        /// <item><term>actionType</term><description>ActionTypes.New</description></item>
        /// <item><term>refId</term><description>cube.DbEntityHyperCube.Id</description></item>
        /// <item><term>oldValue</term><description>null</description></item>
        /// <item><term>newValue</term><description>null</description></item>
        /// <item><term>info</term><description>null</description></item>
        /// </list></remarks>
        internal void NewHyperCube(IHyperCube cube) {
            if (Log) {
                AddReportLogEntry(cube.Document.Id, ReportLogContentTypes.HyperCube, ActionTypes.New,
                             ((HyperCube)cube).DbEntityHyperCube.Id);
            }
        }
        #endregion NewHyperCube

        #region DeleteHyperCube
        /// <summary>
        /// Create a new log_report entry for deleting a HyperCube.
        /// </summary>
        /// <remarks><list type="table">
        /// <item><term>documentId</term><description>cube.Document.Id</description></item>
        /// <item><term>contentType</term><description>ReportLogContentTypes.HyperCube</description></item>
        /// <item><term>actionType</term><description>ActionTypes.Delete</description></item>
        /// <item><term>refId</term><description>cube.DbEntityHyperCube.Id</description></item>
        /// <item><term>oldValue</term><description>null</description></item>
        /// <item><term>newValue</term><description>null</description></item>
        /// <item><term>info</term><description>null</description></item>
        /// </list></remarks>
        internal void DeleteHyperCube(IHyperCube cube) {
            if (Log) {
                AddReportLogEntry(cube.Document.Id, ReportLogContentTypes.HyperCube, ActionTypes.Delete,
                             ((HyperCube)cube).DbEntityHyperCube.Id);
            }
        }
        #endregion DeleteHyperCube

        #region UpdateHyperCubeValue
        /// <summary>
        /// Create a new log_report entry for changing a value in the HyperCube.
        /// </summary>
        /// <remarks><list type="table">
        /// <item><term>documentId</term><description>cube.Document.Id</description></item>
        /// <item><term>contentType</term><description>ReportLogContentTypes.HyperCube</description></item>
        /// <item><term>actionType</term><description>ActionTypes.Change</description></item>
        /// <item><term>refId</term><description>cube.DbEntityHyperCube.Id</description></item>
        /// <item><term>oldValue</term><description>oldValue</description></item>
        /// <item><term>newValue</term><description>newValue</description></item>
        /// <item><term>info</term><description><root DimensionId="DbEntityHyperCubeItem.DimensionId (parameter dimensionId)"/></description></item>
        /// </list></remarks>
        internal void UpdateHyperCubeValue(IHyperCube cube, long dimensionId, string oldValue, string newValue) {
            string xml = "<root DimensionId=\"" + dimensionId + "\"/>";

            if (Log) {
                AddReportLogEntry(cube.Document.Id, ReportLogContentTypes.HyperCube, ActionTypes.Change,
                             ((HyperCube)cube).DbEntityHyperCube.Id, oldValue, newValue, xml);
            }
        }
        #endregion UpdateHyperCubeValue

        #region ImportHyperCube
        /// <summary>
        /// Create a new log_report entry for importing the HyperCube.
        /// </summary>
        /// <remarks><list type="table">
        /// <item><term>documentId</term><description>cube.Document.Id</description></item>
        /// <item><term>contentType</term><description>ReportLogContentTypes.HyperCube</description></item>
        /// <item><term>actionType</term><description>ActionTypes.Imported</description></item>
        /// <item><term>refId</term><description>cube.DbEntityHyperCube.Id</description></item>
        /// <item><term>oldValue</term><description>null</description></item>
        /// <item><term>newValue</term><description>null</description></item>
        /// <item><term>info</term><description>"" (=ToDo)</description></item>
        /// </list></remarks>
        internal void ImportHyperCube(IHyperCube cube) {

            // TODO: write list of (oldValue, newValue, dimensionId) tuples of all changed fields to xml
            string xml = string.Empty;

            if (Log) {
                AddReportLogEntry(cube.Document.Id, ReportLogContentTypes.HyperCube, ActionTypes.Imported,
                             ((HyperCube)cube).DbEntityHyperCube.Id, null, null, xml);
            }
        }
        #endregion ImportHyperCube

        #endregion hypercube log functions

        #region reconciliation log functions

        #region NewReconciliation
        /// <summary>
        /// Logs the creation of a new reconciliation.
        /// </summary>
        /// <param name="reconciliation">Reconciliation which has been added.</param>
        /// <remarks><list type="table">
        /// <item><term>documentId</term><description>reconciliation.Document.Id</description></item>
        /// <item><term>contentType</term><description>ReportLogContentTypes.Reconciliation</description></item>
        /// <item><term>actionType</term><description>ActionTypes.New</description></item>
        /// <item><term>refId</term><description>reconciliation.DbEntity.Id</description></item>
        /// <item><term>oldValue</term><description>null</description></item>
        /// <item><term>newValue</term><description>null</description></item>
        /// <item><term>info</term><description><root Type="(int)reconciliation.ReconciliationType"/></description></item>
        /// </list></remarks>
        internal void NewReconciliation(IReconciliation reconciliation) {
            string xml = "<root Type=\"" + (int)reconciliation.ReconciliationType + "\"/>";
            if (Log) {
                AddReportLogEntry(reconciliation.Document.Id, ReportLogContentTypes.Reconciliation, ActionTypes.New,
                ((Reconciliation.ReconciliationTypes.Reconciliation)reconciliation).DbEntity.Id, info: xml);
            }
        }
        #endregion // NewReconciliation

        #region DeleteReconciliation
        /// <summary>
        /// Logs the deletion of a reconciliation.
        /// </summary>
        /// <param name="reconciliation">Reconciliation which has been deleted.</param>
        /// <remarks><list type="table">
        /// <item><term>documentId</term><description>reconciliation.Document.Id</description></item>
        /// <item><term>contentType</term><description>ReportLogContentTypes.Reconciliation</description></item>
        /// <item><term>actionType</term><description>ActionTypes.Delete</description></item>
        /// <item><term>refId</term><description>reconciliation.DbEntity.Id</description></item>
        /// <item><term>oldValue</term><description>null</description></item>
        /// <item><term>newValue</term><description>null</description></item>
        /// <item><term>info</term><description>null</description></item>
        /// </list></remarks>
        internal void DeleteReconciliation(IReconciliation reconciliation) {
            if (Log) {
                AddReportLogEntry(reconciliation.Document.Id, ReportLogContentTypes.Reconciliation, ActionTypes.Delete,
                ((Reconciliation.ReconciliationTypes.Reconciliation)reconciliation).DbEntity.Id);
            }
        }
        #endregion // DeleteReconciliation

        #region UpdateReconciliationName
        /// <summary>
        /// Logs the change of the name of a reconciliation.
        /// </summary>
        /// <param name="reconciliation">Reconciliation which has been changed.</param>
        /// <param name="oldValue">Old name.</param>
        /// <remarks><list type="table">
        /// <item><term>documentId</term><description>reconciliation.Document.Id</description></item>
        /// <item><term>contentType</term><description>ReportLogContentTypes.Reconciliation</description></item>
        /// <item><term>actionType</term><description>ActionTypes.Change</description></item>
        /// <item><term>refId</term><description>reconciliation.DbEntity.Id</description></item>
        /// <item><term>oldValue</term><description>oldValue</description></item>
        /// <item><term>newValue</term><description>reconciliation.Name</description></item>
        /// <item><term>info</term><description><root ChangedProperty="Name"/></description></item>
        /// </list></remarks>
        internal void UpdateReconciliationName(IReconciliation reconciliation, string oldValue) {
            if (Log) {
                const string xml = "<root ChangedProperty=\"Name\"/>";
                AddReportLogEntry(reconciliation.Document.Id, ReportLogContentTypes.Reconciliation, ActionTypes.Change,
                ((Reconciliation.ReconciliationTypes.Reconciliation)reconciliation).DbEntity.Id, oldValue, reconciliation.Name, xml);
            }
        }
        #endregion // UpdateReconciliationName

        #region UpdateReconciliationComment
        /// <summary>
        /// Logs the change of the comment of a reconciliation.
        /// </summary>
        /// <param name="reconciliation">Reconciliation which has been changed.</param>
        /// <param name="oldValue">Old comment.</param>
        /// <remarks><list type="table">
        /// <item><term>documentId</term><description>reconciliation.Document.Id</description></item>
        /// <item><term>contentType</term><description>ReportLogContentTypes.Reconciliation</description></item>
        /// <item><term>actionType</term><description>ActionTypes.Change</description></item>
        /// <item><term>refId</term><description>reconciliation.DbEntity.Id</description></item>
        /// <item><term>oldValue</term><description>oldValue</description></item>
        /// <item><term>newValue</term><description>reconciliation.Comment</description></item>
        /// <item><term>info</term><description><root ChangedProperty="Comment"/></description></item>
        /// </list></remarks>
        internal void UpdateReconciliationComment(IReconciliation reconciliation, string oldValue) {
            if (Log) {
                const string xml = "<root ChangedProperty=\"Comment\"/>";
                AddReportLogEntry(reconciliation.Document.Id, ReportLogContentTypes.Reconciliation, ActionTypes.Change,
                ((Reconciliation.ReconciliationTypes.Reconciliation)reconciliation).DbEntity.Id, oldValue, reconciliation.Comment, xml);
            }
        }
        #endregion // UpdateReconciliationComment

        #region UpdateReconciliationTransferKind
        /// <summary>
        /// Logs the change of the transfer kind of a reconciliation.
        /// </summary>
        /// <param name="reconciliation">Reconciliation which has been changed.</param>
        /// <param name="oldValue">Old transfer kind.</param>
        /// <remarks><list type="table">
        /// <item><term>documentId</term><description>reconciliation.Document.Id</description></item>
        /// <item><term>contentType</term><description>ReportLogContentTypes.Reconciliation</description></item>
        /// <item><term>actionType</term><description>ActionTypes.Change</description></item>
        /// <item><term>refId</term><description>reconciliation.DbEntity.Id</description></item>
        /// <item><term>oldValue</term><description>oldValue</description></item>
        /// <item><term>newValue</term><description>(int)reconciliation.TransferrKind</description></item>
        /// <item><term>info</term><description><root ChangedProperty="TransferrKind"/></description></item>
        /// </list></remarks>
        internal void UpdateReconciliationTransferKind(IReconciliation reconciliation, string oldValue) {
            if (Log) {
                const string xml = "<root ChangedProperty=\"TransferrKind\"/>";
                AddReportLogEntry(reconciliation.Document.Id, ReportLogContentTypes.Reconciliation, ActionTypes.Change,
                ((Reconciliation.ReconciliationTypes.Reconciliation)reconciliation).DbEntity.Id, oldValue, ((int)reconciliation.TransferKind).ToString(CultureInfo.InvariantCulture), xml);
            }
        }
        #endregion // UpdateReconciliationTransferKind

        #region NewReconciliationTransaction
        /// <summary>
        /// Logs the creation of a new reconciliation transaction.
        /// </summary>
        /// <param name="transaction">Transaction which has been added.</param>
        /// <remarks><list type="table">
        /// <item><term>documentId</term><description>transaction.Document.Id</description></item>
        /// <item><term>contentType</term><description>ReportLogContentTypes.ReconciliationTransaction</description></item>
        /// <item><term>actionType</term><description>ActionTypes.New</description></item>
        /// <item><term>refId</term><description>transaction.DbEntity.Id</description></item>
        /// <item><term>oldValue</term><description>null</description></item>
        /// <item><term>newValue</term><description>null</description></item>
        /// <item><term>info</term><description><root Type="(int)transaction.TransactionType" Reconciliation="transaction.Reconciliation.DbEntity.Id"/></description></item>
        /// </list></remarks>
        internal void NewReconciliationTransaction(IReconciliationTransaction transaction) {
            if (Log) {
                string xml = "<root Type=\"" + ((int)transaction.TransactionType).ToString(CultureInfo.InvariantCulture) + "\" " +
                    "Reconciliation=\"" + ((Reconciliation.ReconciliationTypes.Reconciliation)transaction.Reconciliation).DbEntity.Id.ToString(CultureInfo.InvariantCulture) + "\"";
                AddReportLogEntry(transaction.Document.Id, ReportLogContentTypes.ReconciliationTransaction, ActionTypes.New,
                ((ReconciliationTransaction)transaction).DbEntity.Id, info: xml);
            }
        }
        #endregion // NewReconciliationTransaction

        #region DeleteReconciliationTransaction
        /// <summary>
        /// Logs the deletion of a reconciliation transaction.
        /// </summary>
        /// <param name="transaction">Transaction which has been deleted.</param>
        /// <remarks><list type="table">
        /// <item><term>documentId</term><description>transaction.Document.Id</description></item>
        /// <item><term>contentType</term><description>ReportLogContentTypes.ReconciliationTransaction</description></item>
        /// <item><term>actionType</term><description>ActionTypes.Delete</description></item>
        /// <item><term>refId</term><description>transaction.DbEntity.Id</description></item>
        /// <item><term>oldValue</term><description>null</description></item>
        /// <item><term>newValue</term><description>null</description></item>
        /// <item><term>info</term><description><root Reconciliation="transaction.Reconciliation.DbEntity.Id"/></description></item>
        /// </list></remarks>
        internal void DeleteReconciliationTransaction(IReconciliationTransaction transaction) {
            if (Log) {
                string xml = "<root " +
                    "Reconciliation=\"" + ((Reconciliation.ReconciliationTypes.Reconciliation)transaction.Reconciliation).DbEntity.Id.ToString(CultureInfo.InvariantCulture) + "\"";
                AddReportLogEntry(transaction.Document.Id, ReportLogContentTypes.ReconciliationTransaction, ActionTypes.Delete,
                ((ReconciliationTransaction)transaction).DbEntity.Id, info: xml);
            }
        }
        #endregion // DeleteReconciliationTransaction

        #region UpdateReconciliationTransactionValue
        /// <summary>
        /// Logs the change of the value of a reconciliation transaction.
        /// </summary>
        /// <param name="transaction">Transaction which has been changed.</param>
        /// <param name="oldValue">Old value.</param>
        /// <remarks><list type="table">
        /// <item><term>documentId</term><description>transaction.Document.Id</description></item>
        /// <item><term>contentType</term><description>ReportLogContentTypes.ReconciliationTransaction</description></item>
        /// <item><term>actionType</term><description>ActionTypes.Change</description></item>
        /// <item><term>refId</term><description>transaction.DbEntity.Id</description></item>
        /// <item><term>oldValue</term><description>oldValue</description></item>
        /// <item><term>newValue</term><description>transaction.Value</description></item>
        /// <item><term>info</term><description><root Reconciliation="transaction.Reconciliation.DbEntity.Id" ChangedProperty="Value"/></description></item>
        /// </list></remarks>
        internal void UpdateReconciliationTransactionValue(IReconciliationTransaction transaction, string oldValue) {
            if (Log) {
                string xml = "<root " +
                    "Reconciliation=\"" + ((Reconciliation.ReconciliationTypes.Reconciliation)transaction.Reconciliation).DbEntity.Id.ToString(CultureInfo.InvariantCulture) + "\" " +
                    "ChangedProperty=\"Value\"/>";
                AddReportLogEntry(transaction.Document.Id, ReportLogContentTypes.ReconciliationTransaction, ActionTypes.Change,
                ((ReconciliationTransaction)transaction).DbEntity.Id, oldValue, transaction.Value.ToString(), xml);
            }
        }
        #endregion // UpdateReconciliationTransactionValue

        #region UpdateReconciliationTransactionPosition
        /// <summary>
        /// Logs the change of the assigned position of a reconciliation transaction.
        /// </summary>
        /// <param name="transaction">Transaction which has been changed.</param>
        /// <param name="oldValue">Id of the old position.</param>
        /// <remarks><list type="table">
        /// <item><term>documentId</term><description>transaction.Document.Id</description></item>
        /// <item><term>contentType</term><description>ReportLogContentTypes.ReconciliationTransaction</description></item>
        /// <item><term>actionType</term><description>ActionTypes.Change</description></item>
        /// <item><term>refId</term><description>transaction.DbEntity.Id</description></item>
        /// <item><term>oldValue</term><description>oldValue</description></item>
        /// <item><term>newValue</term><description>transaction.Position.Id (null if transaction.Position == null)</description></item>
        /// <item><term>info</term><description><root Reconciliation="transaction.Reconciliation.DbEntity.Id" ChangedProperty="Position"/></description></item>
        /// </list></remarks>
        internal void UpdateReconciliationTransactionPosition(IReconciliationTransaction transaction, string oldValue) {
            if (Log) {
                string xml = "<root " +
                    "Reconciliation=\"" + ((Reconciliation.ReconciliationTypes.Reconciliation)transaction.Reconciliation).DbEntity.Id.ToString(CultureInfo.InvariantCulture) + "\" " +
                    "ChangedProperty=\"Position\"/>";
                AddReportLogEntry(transaction.Document.Id, ReportLogContentTypes.ReconciliationTransaction, ActionTypes.Change,
                    ((ReconciliationTransaction)transaction).DbEntity.Id, oldValue, transaction.Position == null ? null : transaction.Position.Id, xml);
            }
        }
        #endregion // UpdateReconciliationTransactionPosition

        #region ImportReconciliationPreviousYearValues
        /// <summary>
        /// Logs the import of previous year values.
        /// </summary>
        /// <param name="reconciliation">Reconciliation which has been imported.</param>
        /// <remarks><list type="table">
        /// <item><term>documentId</term><description>reconciliation.Document.Id</description></item>
        /// <item><term>contentType</term><description>ReportLogContentTypes.Reconciliation</description></item>
        /// <item><term>actionType</term><description>ActionTypes.Imported</description></item>
        /// <item><term>refId</term><description>reconciliation.DbEntity.Id</description></item>
        /// <item><term>oldValue</term><description>null</description></item>
        /// <item><term>newValue</term><description>null</description></item>
        /// <item><term>info</term><description><root Type="(int)reconciliation.ReconciliationType" /></description></item>
        /// </list></remarks>
        internal void ImportReconciliationPreviousYearValues(IReconciliation reconciliation) {
            string xml = "<root Type=\"" + (int)reconciliation.ReconciliationType + "\"/>";
            if (Log) {
                AddReportLogEntry(reconciliation.Document.Id, ReportLogContentTypes.Reconciliation, ActionTypes.Imported,
                ((Reconciliation.ReconciliationTypes.Reconciliation)reconciliation).DbEntity.Id, info: xml);
            }
        }
        #endregion // ImportReconciliationPreviousYearValues

        #region LogPreviousYearValuesDeleted
        /// <summary>
        /// Logs the deletion of all transactions.
        /// </summary>
        /// <param name="reconciliation">Reconciliation for which all transactions has been deleted.</param>
        /// <remarks><list type="table">
        /// <item><term>documentId</term><description>reconciliation.Document.Id</description></item>
        /// <item><term>contentType</term><description>ReportLogContentTypes.Reconciliation</description></item>
        /// <item><term>actionType</term><description>ActionTypes.Delete</description></item>
        /// <item><term>refId</term><description>reconciliation.DbEntity.Id</description></item>
        /// <item><term>oldValue</term><description>null</description></item>
        /// <item><term>newValue</term><description>null</description></item>
        /// <item><term>info</term><description><root Type="(int)reconciliation.ReconciliationType" Remark="all transactions deleted"/></description></item>
        /// </list></remarks>
        internal void DeleteAllTransactions(IReconciliation reconciliation) {
            string xml = "<root Type=\"" + (int)reconciliation.ReconciliationType + "\" Remark=\"all transactions deleted\"/>";
            if (Log) {
                AddReportLogEntry(reconciliation.Document.Id, ReportLogContentTypes.Reconciliation, ActionTypes.Delete,
                ((Reconciliation.ReconciliationTypes.Reconciliation)reconciliation).DbEntity.Id, info: xml);
            }
        }
        #endregion // LogPreviousYearValuesDeleted


        #region CopyReconciliation
        /// <summary>
        /// Logs the creation of a new reconciliation.
        /// </summary>
        /// <param name="reconciliation">Reconciliation which has been added.</param>
        /// <param name="sourceDocument">The document where the reconciliation correction has been imported from.</param>
        /// <remarks><list type="table">
        /// <item><term>documentId</term><description>reconciliation.Document.Id</description></item>
        /// <item><term>contentType</term><description>ReportLogContentTypes.Reconciliation</description></item>
        /// <item><term>actionType</term><description>ActionTypes.New</description></item>
        /// <item><term>refId</term><description>reconciliation.DbEntity.Id</description></item>
        /// <item><term>oldValue</term><description>null</description></item>
        /// <item><term>newValue</term><description>null</description></item>
        /// <item><term>info</term><description><root Type="(int)reconciliation.ReconciliationType"/></description></item>
        /// </list></remarks>
        internal void CopyReconciliation(IReconciliation reconciliation, Document sourceDocument) {
            string xml = CreateXmlString("Type", ObjToStr((int)reconciliation.ReconciliationType), "SourceDoc", ObjToStr(sourceDocument.Id));
            if (Log) {
                AddReportLogEntry(reconciliation.Document.Id, ReportLogContentTypes.Reconciliation, ActionTypes.New,
                ((Reconciliation.ReconciliationTypes.Reconciliation)reconciliation).DbEntity.Id, info: xml);
            }
        }
        #endregion // CopyReconciliation

        #region CopyReconciliationTransaction
        /// <summary>
        /// Logs the creation of a new reconciliation transaction.
        /// </summary>
        /// <param name="transaction">Transaction which has been added.</param>
        /// <param name="sourceDocument">The document where the reconciliation correction has been imported from.</param>
        /// <remarks><list type="table">
        /// <item><term>documentId</term><description>transaction.Document.Id</description></item>
        /// <item><term>contentType</term><description>ReportLogContentTypes.ReconciliationTransaction</description></item>
        /// <item><term>actionType</term><description>ActionTypes.New</description></item>
        /// <item><term>refId</term><description>transaction.DbEntity.Id</description></item>
        /// <item><term>oldValue</term><description>null</description></item>
        /// <item><term>newValue</term><description>null</description></item>
        /// <item><term>info</term><description><root Type="(int)transaction.TransactionType" Reconciliation="transaction.Reconciliation.DbEntity.Id" SourceDoc="sourceDocument.Id"/></description></item>
        /// </list></remarks>
        internal void CopyReconciliationTransaction(IReconciliationTransaction transaction, Document sourceDocument) {
            if (Log) {
                
                string xml = CreateXmlString(new Dictionary<string, object>() {
                    {"Type", (int) transaction.TransactionType},
                    {"Reconciliation",
                        ((Reconciliation.ReconciliationTypes.Reconciliation)transaction.Reconciliation).DbEntity.Id},
                    {"SourceDoc", sourceDocument.Id}
                });
                AddReportLogEntry(transaction.Document.Id, ReportLogContentTypes.ReconciliationTransaction,
                                  ActionTypes.New,
                                  ((ReconciliationTransaction) transaction).DbEntity.Id, info: xml);

            }
        }

        /// <summary>
        /// Logs the creation of a new reconciliation transaction.
        /// </summary>
        /// <param name="transaction">Transaction which has been added.</param>
        /// <param name="sourceDocument">The document where the reconciliation correction has been imported from.</param>
        /// <remarks><list type="table">
        /// <item><term>documentId</term><description>transaction.Document.Id</description></item>
        /// <item><term>contentType</term><description>ReportLogContentTypes.ReconciliationTransaction</description></item>
        /// <item><term>actionType</term><description>ActionTypes.New</description></item>
        /// <item><term>refId</term><description>transaction.DbEntity.Id</description></item>
        /// <item><term>oldValue</term><description>null</description></item>
        /// <item><term>newValue</term><description>null</description></item>
        /// <item><term>info</term><description><root Type="(int)transaction.TransactionType" Reconciliation="transaction.Reconciliation.DbEntity.Id" SourceDoc="sourceDocument.Id"/></description></item>
        /// </list></remarks>
        internal void CopyReconciliationTransaction(DbEntityReconciliationTransaction transaction, Document sourceDocument) {
            if (Log) {
                
                string xml = CreateXmlString(new Dictionary<string, object>() {
                    {"Type", (int) transaction.TransactionType},
                    {"Reconciliation", transaction.DbEntityReconciliation.Id},
                    {"SourceDoc", sourceDocument.Id}
                });
                AddReportLogEntry(transaction.Document.Id, ReportLogContentTypes.ReconciliationTransaction,
                                  ActionTypes.New,
                                  transaction.Id, info: xml);

            }
        }
        #endregion // CopyReconciliationTransaction


        #endregion // reconciliation log functions

        #region AddSendLog
        /// <summary>
        /// Create a new log_send entry.
        /// </summary>
        /// <remarks><list type="table">
        /// <item><term>documentId</term><description>documentId</description></item>
        /// <item><term>UserId</term><description>UserManager.Instance.CurrentUser.Id</description></item>
        /// <item><term>ReportInfo</term><description>""</description></item>
        /// <item><term>ResultMessage</term><description>resultMessage</description></item>
        /// <item><term>SendError</term><description>error</description></item>
        /// <item><term>SendData</term><description>sendData</description></item>
        /// </list></remarks>
        public void AddSendLog(string sendData, DbSendLog.SendErrorType error, string resultMessage, int documentId) {
            if (Log) {
                var sendLog = new DbSendLog {
                    SendData = sendData,
                    SendError = error,
                    ResultMessage = resultMessage == null ? "" : resultMessage,
                    ReportInfo = "",
                    TimeStamp = DateTime.Now,
                    UserId = GetUserId(),
                    DocumentId = documentId
                };

                try {
                    LogBuffer.Enqueue(new SendLog(sendLog));
                }
                catch (Exception ex) {
                    throw new Exception(ExceptionMessages.FailedToAddDataset + ex.Message);
                }
            }
        }
        #endregion

        #region UpdateValue
        private DbReportValueChangeLog.ValueTypes StringToValueType(string type) {
            switch (type) {
                case "ValueChanged":
                    return DbReportValueChangeLog.ValueTypes.Value;
                case "ValueOtherChanged":
                    return DbReportValueChangeLog.ValueTypes.OtherValue;
                case "ManualValueChanged":
                    return DbReportValueChangeLog.ValueTypes.ManualValue;
                case "IsManualValueChanged":
                    return DbReportValueChangeLog.ValueTypes.IsManualValue;
                case "SendAccountBalancesChanged":
                    return DbReportValueChangeLog.ValueTypes.SendAccountBalances;
                case "AddToList":
                    return DbReportValueChangeLog.ValueTypes.AddToList;
                case "DeleteFromList":
                    return DbReportValueChangeLog.ValueTypes.DeleteFromList;
                default:
                    throw new Exception("Unknown value type: " + type);
            }
        }

        /// <summary>
        /// Create a new log_admin entry.
        /// </summary>
        /// <remarks><list type="table">
        /// <item><term>contentType</term><description>AdminLogContentTypes.Company</description></item>
        /// <item><term>actionType</term><description>ActionTypes.Change</description></item>
        /// <item><term>refId</term><description>Company.Id</description></item>
        /// <item><term>oldValue</term><description>addValueInfo.OldValue</description></item>
        /// <item><term>newValue</term><description>addValueInfo.NewValue.Value</description></item>
        /// <item><term>info</term><description><Root Id="valueMapping.Id" Type="(int)DbReportValueChangeLog.ValueTypes" TId="addValueInfo.Document.TaxonomyIdManager.GetId(addValueInfo.TaxonomyString)" /></description></item>
        /// </list></remarks>
        private void UpdateCompanyValue(AddValueInfo addValueInfo) {
            if (!Log) return;

            int taxonomyId = addValueInfo.Document != null
                                 ? (addValueInfo.Document.TaxonomyIdManager.GetId(addValueInfo.TaxonomyString))
                                 : (addValueInfo.Company.TaxonomyIdManager.GetId(addValueInfo.TaxonomyString));

            string oldValue = addValueInfo.OldValue;
            string newValue = addValueInfo.NewValue.Value;
            if (addValueInfo.Change == "ValueOtherChanged")
                newValue = addValueInfo.NewValue.ValueOther;

            //Xml Info
            var valueMapping = addValueInfo.NewValue as ValueMappingBase;
            if (valueMapping == null) throw new Exception("Konnte den Wert nicht ins Log eintragen.");
            var doc = new XmlDocument();
            XmlElement root = doc.CreateElement("Root");
            doc.AppendChild(root);
            root.Attributes.Append(doc.CreateAttribute("Id")).InnerText = valueMapping.Id.ToString(CultureInfo.InvariantCulture);
            root.Attributes.Append(doc.CreateAttribute("Type")).InnerText =
                Convert.ToInt16(StringToValueType(addValueInfo.Change)).ToString(CultureInfo.InvariantCulture);
            if ((ValueMappingBaseCompany)addValueInfo.NewValue.Parent == null) return;
            int companyId = ((ValueMappingBaseCompany)addValueInfo.NewValue.Parent).Company.Id;
            root.Attributes.Append(doc.CreateAttribute("TId")).InnerText = taxonomyId.ToString(CultureInfo.InvariantCulture);

            NewAdminLog(AdminLogContentTypes.Company, ActionTypes.Change, companyId,
                        oldValue, newValue, doc.InnerXml);
        }

        /// <summary>
        /// Create a new log_report_X_value_change entry.
        /// </summary>
        /// <remarks><list type="table">
        /// <item><term>NewValue</term><description>addValueInfo.NewValue.Value (addValueInfo.NewValue.ValueOther if addValueInfo.Change == "ValueOtherChanged")</description></item>
        /// <item><term>OldValue</term><description>addValueInfo.OldValue</description></item>
        /// TaxonomyId:	taxonomyId
        /// <item><term>ReferenceId</term><description>valueMapping.Id</description></item>
        /// <item><term>UserId</term><description>UserManager.Instance.CurrentUser.Id</description></item>
        /// <item><term>ValueType</term><description>addValueInfo.Change converted to DbReportValueChangeLog.ValueTypes</description></item>
        /// </list></remarks>
        internal void UpdateValue(AddValueInfo addValueInfo) {
            if (!Log) return;
            if (addValueInfo == null) return;
            if (
                !(addValueInfo.Change == "AddToList" || addValueInfo.Change == "DeleteFromList" ||
                  addValueInfo.Change == "ValueChanged" ||
                  addValueInfo.Change == "IsManualValueChanged" || addValueInfo.Change == "ManualValueChanged" ||
                  addValueInfo.Change == "ValueOtherChanged"))
                return;
            if (addValueInfo.NewValue as ValuesGCD_Company != null) {
                UpdateCompanyValue(addValueInfo);
            }
            else {
                int taxonomyId = addValueInfo.Document != null
                                     ? addValueInfo.Document.TaxonomyIdManager.GetId(addValueInfo.TaxonomyString)
                                     : addValueInfo.Company.TaxonomyIdManager.GetId(addValueInfo.TaxonomyString);

                var valueMapping = addValueInfo.NewValue as ValueMappingBase;
                if (valueMapping == null) throw new Exception("Konnte den Wert nicht ins Log eintragen.");
                var valueChangeLog = new DbReportValueChangeLog {
                    NewValue = addValueInfo.NewValue.Value,
                    OldValue = addValueInfo.OldValue,
                    TaxonomyId = taxonomyId,
                    ReferenceId = valueMapping.Id,
                    UserId = GetUserId(),
                    TimeStamp = DateTime.Now
                };

                try {
                    valueChangeLog.ValueType = StringToValueType(addValueInfo.Change);
                }
                catch (Exception) {
                    return;
                }
                if (addValueInfo.Change == "ValueOtherChanged")
                    valueChangeLog.NewValue = addValueInfo.NewValue.ValueOther;

                try {
                    int docId = -1;
                    if (addValueInfo.NewValue.Parent == null) {
                        var document = addValueInfo.NewValue as ValueMappingBaseDocument;
                        if (document != null) docId = document.Document.Id;
                    }
                    else {
                        var document = addValueInfo.NewValue.Parent as ValueMappingBaseDocument;
                        if (document != null) docId = document.Document.Id;
                    }
                    if (docId == -1) return;
                    LogBuffer.Enqueue(new ReportValueChangeLog(docId, valueChangeLog));
                }
                catch (Exception ex) {
                    throw new Exception(ExceptionMessages.FailedToAddDataset + ex.Message);
                }
            }
        }

        internal class AddValueInfo {
            internal AddValueInfo(object idObject) {
                Debug.Assert(idObject is Document || idObject is Company);
                if (idObject is Document) Document = idObject as Document;
                else Company = idObject as Company;
            }

            public Document Document { get; private set; }
            public Company Company { get; private set; }
            public string TaxonomyString { get; set; }
            public string OldValue { get; set; }
            public IValueMapping NewValue { get; set; }
            public string Change { get; set; }
        }
        #endregion

        #region audit correction log functions

        #region NewAuditCorrection
        /// <summary>
        /// Logs the creation of a new audit correction.
        /// </summary>
        /// <param name="auditCorrection">Audit correction which has been added.</param>
        /// <remarks><list type="table">
        /// <item><term>documentId</term><description>auditCorrection.Document.Id</description></item>
        /// <item><term>contentType</term><description>ReportLogContentTypes.AuditCorrection</description></item>
        /// <item><term>actionType</term><description>ActionTypes.New</description></item>
        /// <item><term>refId</term><description>auditCorrection.DbEntity.Id</description></item>
        /// <item><term>oldValue</term><description>null</description></item>
        /// <item><term>newValue</term><description>null</description></item>
        /// <item><term>info</term><description>-</description></item>
        /// </list></remarks>
        internal void NewAuditCorrection(AuditCorrection auditCorrection) {
            if (Log) {
                AddReportLogEntry(auditCorrection.Document.Id, ReportLogContentTypes.AuditCorrection, ActionTypes.New,
                auditCorrection.DbEntity.Id);
            }
        }
        #endregion // NewAuditCorrection

        #region DeleteAuditCorrection
        /// <summary>
        /// Logs the deletion of a audit correction.
        /// </summary>
        /// <param name="auditCorrection">Audit correction which has been deleted.</param>
        /// <remarks><list type="table">
        /// <item><term>documentId</term><description>reconciliation.Document.Id</description></item>
        /// <item><term>contentType</term><description>ReportLogContentTypes.AuditCorrection</description></item>
        /// <item><term>actionType</term><description>ActionTypes.Delete</description></item>
        /// <item><term>refId</term><description>reconciliation.DbEntity.Id</description></item>
        /// <item><term>oldValue</term><description>null</description></item>
        /// <item><term>newValue</term><description>null</description></item>
        /// <item><term>info</term><description>null</description></item>
        /// </list></remarks>
        internal void DeleteAuditCorrection(AuditCorrection auditCorrection) {
            if (Log) {
                AddReportLogEntry(auditCorrection.Document.Id, ReportLogContentTypes.AuditCorrection, ActionTypes.Delete,
                                  (auditCorrection.DbEntity.Id));
            }
        }
        #endregion // DeleteAuditCorrection

        #region DeleteAllAuditCorrections
        /// <summary>
        /// Logs the deletion of all audit corrections.
        /// </summary>
        /// <param name="document">Document for which all audit corrections has been deleted.</param>
        /// <remarks><list type="table">
        /// <item><term>documentId</term><description>document.Id</description></item>
        /// <item><term>contentType</term><description>ReportLogContentTypes.AuditCorrection</description></item>
        /// <item><term>actionType</term><description>ActionTypes.AllValuesDeleted</description></item>
        /// <item><term>refId</term><description>0</description></item>
        /// <item><term>oldValue</term><description>null</description></item>
        /// <item><term>newValue</term><description>null</description></item>
        /// <item><term>info</term><description>null</description></item>
        /// </list></remarks>
        internal void DeleteAllAuditCorrections(Document document) {
            if (Log) {
                AddReportLogEntry(document.Id, ReportLogContentTypes.AuditCorrection, ActionTypes.AllValuesDeleted, 0);
            }
        }
        #endregion // DeleteAuditCorrection

        #region UpdateAuditCorrectionName
        /// <summary>
        /// Logs the change of the name of a reconciliation.
        /// </summary>
        /// <param name="auditCorrection">Audit correction which has been changed.</param>
        /// <param name="oldValue">Old name.</param>
        /// <remarks><list type="table">
        /// <item><term>documentId</term><description>auditCorrection.Document.Id</description></item>
        /// <item><term>contentType</term><description>ReportLogContentTypes.AuditCorrection</description></item>
        /// <item><term>actionType</term><description>ActionTypes.Change</description></item>
        /// <item><term>refId</term><description>reconciliation.DbEntity.Id</description></item>
        /// <item><term>oldValue</term><description>oldValue</description></item>
        /// <item><term>newValue</term><description>auditCorrection.Name</description></item>
        /// <item><term>info</term><description><root ChangedProperty="Name"/></description></item>
        /// </list></remarks>
        internal void UpdateAuditCorrectionName(AuditCorrection auditCorrection, string oldValue) {
            if (Log) {
                const string xml = "<root ChangedProperty=\"Name\"/>";
                AddReportLogEntry(auditCorrection.Document.Id, ReportLogContentTypes.AuditCorrection, ActionTypes.Change,
                auditCorrection.DbEntity.Id, oldValue, auditCorrection.Name, xml);
            }
        }
        #endregion // UpdateAuditCorrectionName

        #region UpdateAuditCorrectionComment
        /// <summary>
        /// Logs the change of the comment of a reconciliation.
        /// </summary>
        /// <param name="auditCorrection">Audit correction which has been changed.</param>
        /// <param name="oldValue">Old comment.</param>
        /// <remarks><list type="table">
        /// <item><term>documentId</term><description>auditCorrection.Document.Id</description></item>
        /// <item><term>contentType</term><description>ReportLogContentTypes.AuditCorrection</description></item>
        /// <item><term>actionType</term><description>ActionTypes.Change</description></item>
        /// <item><term>refId</term><description>reconciliation.DbEntity.Id</description></item>
        /// <item><term>oldValue</term><description>oldValue</description></item>
        /// <item><term>newValue</term><description>auditCorrection.Comment</description></item>
        /// <item><term>info</term><description><root ChangedProperty="Comment"/></description></item>
        /// </list></remarks>
        internal void UpdateAuditCorrectionComment(AuditCorrection auditCorrection, string oldValue) {
            if (Log) {
                const string xml = "<root ChangedProperty=\"Comment\"/>";
                AddReportLogEntry(auditCorrection.Document.Id, ReportLogContentTypes.AuditCorrection, ActionTypes.Change,
                auditCorrection.DbEntity.Id, oldValue, auditCorrection.Comment, xml);
            }
        }
        #endregion // UpdateAuditCorrectionComment

        #region NewAuditCorrectionTransaction
        /// <summary>
        /// Logs the creation of a new reconciliation transaction.
        /// </summary>
        /// <param name="transaction">Transaction which has been added.</param>
        /// <remarks><list type="table">
        /// <item><term>documentId</term><description>transaction.Document.Id</description></item>
        /// <item><term>contentType</term><description>ReportLogContentTypes.AuditCorrectionTransaction</description></item>
        /// <item><term>actionType</term><description>ActionTypes.New</description></item>
        /// <item><term>refId</term><description>transaction.DbEntity.Id</description></item>
        /// <item><term>oldValue</term><description>null</description></item>
        /// <item><term>newValue</term><description>null</description></item>
        /// <item><term>info</term><description><root AuditCorrection="transaction.Parent.DbEntity.Id"/></description></item>
        /// </list></remarks>
        internal void NewAuditCorrectionTransaction(AuditCorrectionTransaction transaction) {
            if (Log) {
                string xml = "<root AuditCorrection=\"" + ((AuditCorrection)transaction.Parent).DbEntity.Id.ToString(CultureInfo.InvariantCulture) + "\"";
                AddReportLogEntry(transaction.Document.Id, ReportLogContentTypes.AuditCorrectionTransaction,
                                  ActionTypes.New, transaction.DbEntity.Id, info: xml);
            }
        }
        #endregion // NewAuditCorrectionTransaction

        #region DeleteAuditCorrectionTransaction
        /// <summary>
        /// Logs the deletion of a reconciliation transaction.
        /// </summary>
        /// <param name="transaction">Transaction which has been deleted.</param>
        /// <remarks><list type="table">
        /// <item><term>documentId</term><description>transaction.Document.Id</description></item>
        /// <item><term>contentType</term><description>ReportLogContentTypes.AuditCorrectionTransaction</description></item>
        /// <item><term>actionType</term><description>ActionTypes.Delete</description></item>
        /// <item><term>refId</term><description>transaction.DbEntity.Id</description></item>
        /// <item><term>oldValue</term><description>null</description></item>
        /// <item><term>newValue</term><description>null</description></item>
        /// <item><term>info</term><description><root AuditCorrection="transaction.Parent.DbEntity.Id"/></description></item>
        /// </list></remarks>
        internal void DeleteAuditCorrectionTransaction(AuditCorrectionTransaction transaction) {
            if (Log) {
                string xml = "<root AuditCorrection=\"" + ((AuditCorrection)transaction.Parent).DbEntity.Id.ToString(CultureInfo.InvariantCulture) + "\"";
                AddReportLogEntry(transaction.Document.Id, ReportLogContentTypes.AuditCorrectionTransaction,
                                  ActionTypes.Delete, transaction.DbEntity.Id, info: xml);
            }
        }
        #endregion // DeleteAuditCorrectionTransaction

        #region UpdateAuditCorrectionTransactionValue
        /// <summary>
        /// Logs the change of the value of a reconciliation transaction.
        /// </summary>
        /// <param name="transaction">Transaction which has been changed.</param>
        /// <param name="oldValue">Old value.</param>
        /// <remarks><list type="table">
        /// <item><term>documentId</term><description>transaction.Document.Id</description></item>
        /// <item><term>contentType</term><description>ReportLogContentTypes.AuditCorrectionTransaction</description></item>
        /// <item><term>actionType</term><description>ActionTypes.Change</description></item>
        /// <item><term>refId</term><description>transaction.DbEntity.Id</description></item>
        /// <item><term>oldValue</term><description>oldValue</description></item>
        /// <item><term>newValue</term><description>transaction.Value</description></item>
        /// <item><term>info</term><description><root Reconciliation="transaction.Reconciliation.DbEntity.Id" ChangedProperty="Value"/></description></item>
        /// </list></remarks>
        internal void UpdateAuditCorrectionTransactionValue(AuditCorrectionTransaction transaction, string oldValue) {
            if (Log) {
                string xml = "<root " + "AuditCorrection=\"" +
                             ((AuditCorrection) transaction.Parent).DbEntity.Id.ToString(CultureInfo.InvariantCulture) +
                             "\" " +
                             "ChangedProperty=\"Value\"/>";

                AddReportLogEntry(transaction.Document.Id, ReportLogContentTypes.AuditCorrectionTransaction,
                                  ActionTypes.Change,
                                  transaction.DbEntity.Id, oldValue,
                                  transaction.Value.HasValue
                                      ? transaction.Value.Value.ToString(CultureInfo.InvariantCulture)
                                      : null, xml);
            }
        }
        #endregion // UpdateAuditCorrectionTransactionValue


        #region CopyAuditCorrection
        /// <summary>
        /// Logs the creation of a new audit correction by transfering a previous year correction.
        /// </summary>
        /// <param name="auditCorrection">Audit correction which has been added.</param>
        /// <param name="sourceDocument">The document where the audit correction has been imported from.</param>
        /// <param name="saveTransactions">Flag that decides if an automatic log for auditCorrection.transacions has to be created. (Default = false)</param>
        /// <remarks><list type="table">
        /// <item><term>documentId</term><description>auditCorrection.Document.Id</description></item>
        /// <item><term>contentType</term><description>ReportLogContentTypes.AuditCorrection</description></item>
        /// <item><term>actionType</term><description>ActionTypes.New</description></item>
        /// <item><term>refId</term><description>auditCorrection.DbEntity.Id</description></item>
        /// <item><term>oldValue</term><description>null</description></item>
        /// <item><term>newValue</term><description>null</description></item>
        /// <item><term>info</term><description><root SourceDoc="sourceDocument.Id" /></description></item>
        /// </list></remarks>
        internal void CopyAuditCorrection(AuditCorrections.DbMapping.DbEntityAuditCorrection auditCorrection, Document sourceDocument, bool saveTransactions = false) {
            if (Log) {
                AddReportLogEntry(auditCorrection.Document.Id, ReportLogContentTypes.AuditCorrection, ActionTypes.New,
                auditCorrection.Id, info: CreateXmlString("SourceDoc", ObjToStr(sourceDocument.Id)));

                if (saveTransactions) {
                    CopyAuditCorrectionTransaction(auditCorrection.Transactions, sourceDocument);
                }

            }
        }
        #endregion // CopyAuditCorrection


        #region CopyAuditCorrectionTransaction
        /// <summary>
        /// Logs the creation of a new audit transaction by transfering a previous year correction transaction.
        /// </summary>
        /// <param name="transaction">Transaction which has been added.</param>
        /// <param name="sourceDocument">The document where the audit correction has been imported from.</param>
        /// <param name="info">The information that should be stored as Info. If null (default) see remarks.</param>
        /// <remarks><list type="table">
        /// <item><term>documentId</term><description>transaction.Document.Id</description></item>
        /// <item><term>contentType</term><description>ReportLogContentTypes.AuditCorrectionTransaction</description></item>
        /// <item><term>actionType</term><description>ActionTypes.New</description></item>
        /// <item><term>refId</term><description>transaction.DbEntity.Id</description></item>
        /// <item><term>oldValue</term><description>null</description></item>
        /// <item><term>newValue</term><description>null</description></item>
        /// <item><term>info</term><description><root AuditCorrection="transaction.DbEntityAuditCorrection.Id" SourceDoc="sourceDocument.Id" /></description></item>
        /// </list></remarks>
        internal void CopyAuditCorrectionTransaction(AuditCorrections.DbMapping.DbEntityAuditCorrectionTransaction transaction, Document sourceDocument, string info = null) {
            if (Log) {
                string xml = info ?? CreateXmlString("AuditCorrection", ObjToStr(transaction.DbEntityAuditCorrection.Id), "SourceDoc", ObjToStr(sourceDocument.Id));
                AddReportLogEntry(transaction.Document.Id, ReportLogContentTypes.AuditCorrectionTransaction,
                                  ActionTypes.New, transaction.Id, info: xml);
            }
        }

        /// <summary>
        /// Calls <see cref="CopyAuditCorrectionTransaction(eBalanceKitBusiness.AuditCorrections.DbMapping.DbEntityAuditCorrectionTransaction,eBalanceKitBusiness.Structures.DbMapping.Document,string)"/> for each entry with improved performance.
        /// </summary>
        /// <param name="transactions">Transactions which has been added.</param>
        /// <param name="sourceDocument">The document where the audit correction has been imported from.</param>
        internal void CopyAuditCorrectionTransaction(List<AuditCorrections.DbMapping.DbEntityAuditCorrectionTransaction> transactions, Document sourceDocument) {
            if (Log) {
                if (transactions == null || transactions.Count <= 0) {
                    return;
                }

                string xml =
                    CreateXmlString("AuditCorrection", ObjToStr(transactions.First().DbEntityAuditCorrection.Id),
                                    "SourceDoc", ObjToStr(sourceDocument.Id));
                foreach (var transaction in transactions) {
                    CopyAuditCorrectionTransaction(transaction, sourceDocument, xml);
                }
            }
        }
        #endregion // CopyAuditCorrectionTransaction
        
        #endregion // audit correction log functions


        // HINT: Please use ObjToStr(xx) instead ToString(..)


        #endregion //Methods
    }
}