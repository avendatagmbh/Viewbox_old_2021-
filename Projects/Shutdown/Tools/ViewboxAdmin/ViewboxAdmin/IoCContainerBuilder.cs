using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SystemDb;
using Autofac;
using ViewboxAdmin.Factories;
using ViewboxAdmin.Manager;
using ViewboxAdmin.Models;
using ViewboxAdmin.Structures;
using ViewboxAdmin.ViewModels;
using ViewboxAdmin.ViewModels.Collections;
using ViewboxAdmin.ViewModels.MergeDataBase;
using ViewboxAdmin.ViewModels.Roles;
using ViewboxAdmin.ViewModels.Users;
using ViewboxAdmin.Windows;
using ViewboxAdminBusiness.Manager;

namespace ViewboxAdmin
{
    /// <summary>
    ///  Hopefully you now what an IoC container is :)
    /// If you dont, google for Dependency Injection, (Inversion of Control), Inversion of Control containers, Unity framework, etc...
    /// My framework of choice is:
    /// http://code.google.com/p/autofac/
    /// </summary>
    class IoCContainerBuilder
    {
        public IoCContainerBuilder() {
                Builder = new ContainerBuilder();
        }

        public ContainerBuilder Builder { get; private set; }

        public IContainer GetDependencyInjectionContainer() {
            Builder.Register(c => new DlgMainWindow()).SingleInstance();
            Builder.Register(c => new NavigationTree(c.Resolve<DlgMainWindow>())).As<INavigationTree>();
            Builder.Register(c => new ProfileModelRepository()).As<IProfileModelRepository>();
            Builder.Register(c => new Dispatcher()).As<IDispatcher>();
            Builder.Register(c => new NavigationEntriesDataContext(c.Resolve<INavigationTree>())).As<INavigationEntriesDataContext>();
            Builder.Register(c => new XmlProfileManager()).As<IXmlProfileManager>();
            Builder.Register(c => new FileManager()).As<IFileManager>().SingleInstance();
            Builder.Register(c => new XmlConfigManager()).As<IXmlConfigManager>();

            // I dont like additional initianilization methods like public Init, Load, ... etc method, but that s the way AvenDATA guys worked. :) 
            Builder.Register(c =>{var am = new ApplicationConfigManager(c.Resolve<IFileManager>(), c.Resolve<IXmlConfigManager>()); am.Load();return am;}).As<IApplicationManager>();
            Builder.Register(c =>{ var pm = new ProfileManager(c.Resolve<IApplicationManager>(), c.Resolve<IFileManager>(), c.Resolve<IXmlProfileManager>());pm.LoadProfiles();return pm;}).As<IProfileManager>();



            Builder.Register(c => c.Resolve<MainWindow_ViewModel>().SelectedProfile.SystemDb).As<ISystemDb>();
            Builder.Register(c =>new ProfileManagement_ViewModel(c.Resolve<MainWindow_ViewModel>().Profilemananger,c.Resolve<MainWindow_ViewModel>()));
            Builder.Register(c => new ParameterEditor(c.Resolve<MainWindow_ViewModel>().SelectedProfile.SystemDb));
            Builder.Register(c => new CollectionUnitOfWork(c.Resolve<ParameterEditor>())).As<ICollectionsUnitOfWork>();
            Builder.Register(c => new BusinessObjectLoader(c.Resolve<MainWindow_ViewModel>().SelectedProfile.SystemDb)).As<IBusinessObjectLoader>();
            Builder.Register(c =>new Parameters_ViewModel(c.Resolve<IBusinessObjectLoader>().GetParameterCollections(),c.Resolve<IBusinessObjectLoader>().EmptyLanguageTextModels(),
                                         c.Resolve<ICollectionsUnitOfWork>())).As<IParameters_ViewModel>();

            Builder.Register((c, p) => new IoCResolver(p.Named<IComponentContext>("container"))).As<IIoCResolver>();
            Builder.Register((c, p) =>new MainWindow_ViewModel(c.Resolve<IProfileManager>(), c.Resolve<IProfileModelRepository>(),c.Resolve<INavigationEntriesDataContext>(), c.Resolve<IDispatcher>(),c.Resolve<IIoCResolver>(new NamedParameter("container",p.Named<IComponentContext>("container"))))).SingleInstance();

            Builder.Register(c => new OptimizationVM_Factory()).As<IOptimizationVM_Factory>();
            Builder.Register(c =>new OptimizationTree_ViewModel(c.Resolve<MainWindow_ViewModel>().SelectedProfile.SystemDb,c.Resolve<IOptimizationVM_Factory>()));

            //register classes for SYSTEMDELETE_VIEWMODEL
            Builder.Register(c => new DeleteSystemReporter(c.Resolve<MainWindow_ViewModel>().SelectedProfile.SystemDb)).As<IDeleteSystemStrategy>();
            Builder.Register(c =>new SystemDeleteViewModel(c.Resolve<MainWindow_ViewModel>().SelectedProfile.SystemDb,c.Resolve<IDeleteSystemStrategy>()));

            //register classes for MERGING_VIEWMODEL
            Builder.Register(c => new MergeMetaDatabaseFactory()).As<IMergeMetaDatabaseFactory>();
            Builder.Register(c => new CloneBusinessObjects()).As<ICloneBusinessObjects>();
            Builder.Register(c => new MergeDataBase(c.Resolve<IMergeMetaDatabaseFactory>())).As<IMergeDataBase>();
            Builder.Register(c =>new Merger_ViewModel(c.Resolve<MainWindow_ViewModel>().ProfileModelRepository,c.Resolve<MainWindow_ViewModel>().Profilemananger, c.Resolve<IMergeDataBase>()));


            //regiater classes for EDITTEXT_VIEWMODEL
            Builder.Register(c => new ItemLoaderFactory()).As<IItemLoaderFactory>();
            Builder.Register(c => new EditText_ViewModel(c.Resolve<MainWindow_ViewModel>().SelectedProfile.SystemDb,c.Resolve<IItemLoaderFactory>()));

            //register classes for USERS_VIEWMODEL
            Builder.Register(c => new UserDataBaseMapper(c.Resolve<ISystemDb>())).As<IUserDataBaseMapper>();
            Builder.Register(c => new UserUnitOfWork(c.Resolve<IUserDataBaseMapper>())).As<IUnitOfWork<UserModel>>();
            Builder.Register(c => new LoadUsers(c.Resolve<ISystemDb>())).As<LoadUsers>().SingleInstance();
            Builder.Register(c => new UsersViewModel(c.Resolve<LoadUsers>().GetUsers(), c.Resolve<IUnitOfWork<UserModel>>()));

            //register classes for ROLES_VIEWMODEL
            Builder.Register(c => new RoleUnitOfWork()).As<IUnitOfWork<RoleModel>>();
            Builder.Register(c => new MessageBoxProvider()).As<IMessageBoxProvider>();
            Builder.Register(c => c.Resolve<LoadUsers>().GetUsers()).As<TrulyObservableCollection<UserModel>>().SingleInstance();
            Builder.Register(c => new RoleLoader(c.Resolve<ISystemDb>(), c.Resolve<TrulyObservableCollection<UserModel>>())).As<RoleLoader>();
            Builder.Register(c => new RoleEditViewModel(c.Resolve<TrulyObservableCollection<UserModel>>(), c.Resolve<RoleLoader>().GetRoles(), c.Resolve<IUnitOfWork<RoleModel>>(), c.Resolve<IMessageBoxProvider>()));



               return Builder.Build();
        }

    }
}
