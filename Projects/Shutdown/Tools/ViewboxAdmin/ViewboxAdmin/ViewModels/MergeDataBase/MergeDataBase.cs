using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SystemDb;
using SystemDb.Internal;
using AV.Log;
using DbAccess;
using log4net;

namespace ViewboxAdmin.ViewModels.MergeDataBase
{
    internal class MergeDataBase : StatusReporterBase, IMergeDataBase
    {
        private ILog _log = LogHelper.GetLogger();
        private IDatabase _dbFrom;
        private IDatabase _dbTo;

        public MergeDataBase(IMergeMetaDatabaseFactory businessObjectFactory) {
            this.MergerFactory = businessObjectFactory;
        }

        public IMergeMetaDatabaseFactory MergerFactory { get; private set; }

        public void MergeDataBases(ISystemDb mergeTo, ISystemDb mergeFrom)
        {
            _dbTo = ConnectionManager.CreateConnection(mergeTo.ConnectionManager.DbConfig);
            _dbFrom = ConnectionManager.CreateConnection(mergeFrom.ConnectionManager.DbConfig);

            try {
                OnStart();
                var sw = Stopwatch.StartNew();
                
                IMergeMetaDatabases dw = MergerFactory.Create(mergeTo);
                dw.PropertyChanged+= PropertyChanged;

                OnDebug(sw.Elapsed.ToString());

                try
                {
                    HighestLevelElements(mergeTo, mergeFrom, dw);
                }
                catch (Exception ex)
                {
                    throw;
                }
                OnDebug(sw.Elapsed.ToString());

                try
                {
                    TablesAndColumns(mergeTo, mergeFrom, dw);
                }
                catch (Exception ex)
                {
                    throw;
                }
                OnDebug(sw.Elapsed.ToString());

                try
                {
                    Order_Optim_IssueExt(mergeTo, mergeFrom, dw);
                }
                catch (Exception ex)
                {
                    throw;
                }
                OnDebug(sw.Elapsed.ToString());

                try
                {
                    UserRoleElements(mergeTo, mergeFrom, dw);
                }
                catch (Exception ex)
                {
                    throw;
                }
                OnDebug(sw.Elapsed.ToString());

                try
                {
                    SomeDirtyMerge(mergeTo, mergeFrom, dw);
                }
                catch (Exception ex)
                {
                    throw;
                }
                OnDebug(sw.Elapsed.ToString());

                try
                {
                    TextMerge(mergeFrom, dw);
                }
                catch (Exception ex)
                {
                    throw;
                }
                OnDebug(sw.Elapsed.ToString());
            }
            catch(Exception ex) {
                string message = "Error during merge process: " + ex.Message;
                _log.Error(message, ex);
                OnDebug(message);
                OnCrashed();
            }
            OnCompleted();
        }

        private void SomeDirtyMerge(ISystemDb mergeTo, ISystemDb mergeFrom, IMergeMetaDatabases dw) {
           //dirty hacks
            ILoaderHack loadFrom = MergerFactory.CreateLoader(mergeFrom);
            ILoaderHack loadTo = MergerFactory.CreateLoader(mergeTo);
            loadFrom.Languages = mergeFrom.Languages;
            loadTo.Languages = mergeTo.Languages;
            
            OnDebug("relations...");
            dw.Relation(loadFrom.LoadRelationFromTable(), loadTo.LoadRelationFromTable());


            OnDebug("(prepare) parameter values");
            var pvalulist = loadFrom.LoadParameterValue();
            OnDebug("parameter values...");
            dw.ParameterValues(pvalulist, loadTo.LoadParameterValue());

            OnDebug("parameter values text");
            dw.ParameterValueText(pvalulist, mergeFrom.Languages);

            OnDebug("parameter collection mapping...");
            dw.ParameterCollectionMapping(loadFrom.LoadParameterCollectionFromTable());

            OnDebug("info");
            dw.Info(loadFrom.Info(), loadTo.Info());

            OnDebug("notes");
            dw.Note(loadFrom.Note());

            OnDebug("about");
            dw.About(loadFrom.About());
        }

        private void Order_Optim_IssueExt(ISystemDb mergeTo, ISystemDb mergeFrom, IMergeMetaDatabases dw){
            ILoaderHack loadFrom = MergerFactory.CreateLoader(mergeFrom);
            OnDebug("new log action");

            if (TableExists<NewLogActionMerge>(_dbFrom))
            {
                CreateTable<NewLogActionMerge>(_dbTo);
                dw.NewLogActionMerge(loadFrom.LoadNewLogAction());
            }

            OnDebug("Order area");
            dw.OrderArea(mergeFrom.Objects, mergeTo.Objects);
            OnDebug("Issue extensions");
//            dw.IssueExtensions(mergeFrom.Issues);
//            dw.IssueExtensions(LoadEntities<IssueExtension>(_dbFrom));
            dw.IssueExtensions(LoadEntities<IssueExtension>(_dbFrom), LoadEntities<IssueExtension>(_dbTo));
            OnDebug("Parameter");
            dw.Parameters(mergeFrom.Issues, mergeTo.Issues);
            OnDebug("user issue parameter history");
            dw.UserIssueParameterHistory(loadFrom.UserIssueParameterHistory());

            OnDebug("optimizations...");
            dw.OptimizationAggregate(LoadEntities<Optimization>(_dbFrom), LoadEntities<Optimization>(_dbTo));
            
            OnDebug("user optimization settings");
            dw.UserOptimizationSettings(loadFrom.UserOptimizationSettings());
        }

        private void CreateTable<T1>(IDatabase db)
        {
            try
            {
                db.Open();
                db.DbMapping.CreateTableIfNotExists<T1>();
            }
            finally
            {
                db.Close();

            }
        }
        private List<T> LoadEntities<T>(IDatabase db) where T: new()
        {
            try
            {
                db.Open();
                return db.DbMapping.Load<T>();
            }
            finally
            {
                db.Close();

            }
        }

        private bool TableExists<T>(IDatabase db)
        {
            try
            {
                db.Open();
                return db.TableExists(db.DbMapping.GetTableName<T>());
            }
            finally
            {
                db.Close();
            }
        }

        private void TablesAndColumns(ISystemDb mergeTo, ISystemDb mergeFrom, IMergeMetaDatabases dw) {
            ILoaderHack loadFrom = MergerFactory.CreateLoader(mergeFrom);

            OnDebug("Tables...");
            dw.Table(mergeFrom.Objects, mergeTo.Objects);

            OnDebug("table scheme mapping...");
            List<TableObjectSchemeMapping> tableObjectSchemeMapping = loadFrom.LoadTableObjectSchemeMapping();

            dw.TableObjectSchemeMapping_(tableObjectSchemeMapping);

            OnDebug("table original names");
            List<TableOriginalName> tableOriginalName = loadFrom.TableOriginalName();
            dw.TableOriginalName(tableOriginalName);

            OnDebug("table archive information");
            List<TableArchiveInformation> tableArchiveInformation = loadFrom.TableArchiveInformation();
            dw.TableArchiveInformation(tableArchiveInformation);
            
            OnDebug("Columns...");
            dw.Columns(mergeFrom.Columns);

            OnDebug("Objectypes ...");
            if (TableExists<ObjectTypes>(_dbFrom))
            {
                CreateTable<ObjectTypes>(_dbTo);

                dw.ObjectTypes(LoadEntities<ObjectTypes>(_dbFrom), LoadEntities<ObjectTypes>(_dbTo), dw);

                if (TableExists<ObjectTypeText>(_dbFrom))
                {
                    CreateTable<ObjectTypeText>(_dbTo);
                    dw.ObjectTypeText(LoadEntities<ObjectTypeText>(_dbFrom), LoadEntities<ObjectTypeText>(_dbTo), dw);
                }
            }
        }

        private void HighestLevelElements(ISystemDb mergeTo, ISystemDb mergeFrom, IMergeMetaDatabases dw) {

            OnDebug("Start with the highest level elements in hierarchy...");
            OnDebug("langugaes...");
            dw.Languages(mergeFrom.Languages, mergeTo.Languages);
            OnDebug("Categories...");
            dw.CategoryMapping(mergeFrom.Categories);
            OnDebug("Users...");
            dw.Users(mergeFrom.Users, new UserCollection(LoadEntities<User>(_dbTo).OfType<IUser>()));
            OnDebug("Roles...");
            dw.Roles(mergeFrom.Roles, mergeTo.Roles);
            OnDebug("OptimizationGroups...");
            dw.OptimizationGroup(LoadEntities<OptimizationGroup>(_dbFrom), LoadEntities<OptimizationGroup>(_dbTo));
//            dw.OptimizationGroup(mergeFrom.OptimizationGroups);
            OnDebug("Scheme");
            dw.Scheme(mergeFrom.Schemes, mergeTo.Schemes, dw);

            if (TableExists<SchemeText>(_dbFrom))
            {
                CreateTable<SchemeText>(_dbTo);
                dw.SchemeTexts(LoadEntities<SchemeText>(_dbFrom), LoadEntities<SchemeText>(_dbTo), dw);
            }


            OnDebug("properties");
            dw.Property(mergeFrom.Properties, mergeTo.Properties);
            OnDebug("user property settings");
            dw.UserPropertySettings(mergeFrom.UserPropertySettings, mergeTo.UserPropertySettings);
        }

        private void UserRoleElements(ISystemDb mergeTo, ISystemDb mergeFrom, IMergeMetaDatabases dw) {
            OnDebug("column roles");
            dw.ColumnRole(mergeFrom.RoleColumnRights, mergeTo.RoleColumnRights);
            OnDebug("column users");
            dw.ColumnUser(mergeFrom.UserColumnRights, mergeTo.UserColumnRights);
            OnDebug("table roles");
            dw.TableRole(mergeFrom.RoleTableObjectRights, mergeTo.RoleTableObjectRights);
            OnDebug("table users");
            dw.TableUser(mergeFrom.UserTableObjectRights, mergeTo.UserTableObjectRights);
            OnDebug("category roles");
            dw.CategoryRole(mergeFrom.RoleCategoryRights, mergeTo.RoleCategoryRights);
            OnDebug("category users");
            dw.CategoryUser(mergeFrom.UserCategoryRights, mergeTo.UserCategoryRights);
            OnDebug("optimization roles");
            dw.OptimizationRole(mergeFrom.RoleOptimizationRights, mergeTo.RoleOptimizationRights);

            OnDebug("optimization users");
            
            //dw.OptimizationUser(mergeFrom.UserOptimizationRights, mergeTo.UserOptimizationRights);
            dw.OptimizationUser(LoadEntities<OptimizationUserMapping>(_dbFrom), LoadEntities<OptimizationUserMapping>(_dbTo));
            OnDebug("User Roles");
            dw.UserRole(LoadEntities<UserRoleMapping>(_dbFrom), LoadEntities<UserRoleMapping>(_dbTo));
//            dw.UserRole(mergeFrom.UserRole, mergeTo.UserRole);

            OnDebug("User settings");
            dw.UserSettings(mergeFrom.UserSettings);
            OnDebug("User Passwords");
            dw.Passwords(mergeFrom.Passwords, mergeTo.Passwords);
        }

        private void TextMerge(ISystemDb mergeFrom, IMergeMetaDatabases dw) {
            OnDebug("Texts");

            OnDebug("optimization texts");
//            dw.OptimizationText(mergeFrom.Optimizations, mergeFrom.Languages);
            dw.OptimizationText(LoadEntities<OptimizationText>(_dbFrom), LoadEntities<OptimizationText>(_dbTo));
            OnDebug("column texts");
            dw.ColumnText(mergeFrom.Columns, mergeFrom.Languages);
            OnDebug("table text");
            dw.TableText(mergeFrom.Objects, mergeFrom.Languages);
            OnDebug("optimization group text");
            dw.OptimizationGroupText(LoadEntities<OptimizationGroupText>(_dbFrom), LoadEntities<OptimizationGroupText>(_dbTo));
//            dw.OptimizationGroupText(mergeFrom.OptimizationGroups, mergeFrom.Languages);
            OnDebug("categories text");
            dw.CategoryTexts(mergeFrom.Categories, mergeFrom.Languages);
            OnDebug("parameters text");
            foreach (var i in mergeFrom.Issues)
            {
                dw.ParameterTexts(i.Parameters, mergeFrom.Languages);
            }

            OnDebug("property text");
            dw.PropertyText(mergeFrom.Properties, mergeFrom.Languages);

            OnDebug("completed");
        }

        private void PropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            switch (propertyChangedEventArgs.PropertyName) {
                case "StatusText":
                    IMergeMetaDatabases dw = sender as IMergeMetaDatabases;
                    if (dw != null) OnDebug(dw.StatusText);
                    break;
            }
        }

    }
}
