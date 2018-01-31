using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Autofac;
using Autofac.Features.ResolveAnything;
using Classroom.View;
using Common.Contract;
using Common.CustomControl;
using Common.Helper;
using Common.Model;
using Serilog;
using Service;
using Squirrel;
using Application = System.Windows.Application;
using Caliburn.Micro;
using System.Collections.Generic;
using System.Reflection;
using Classroom.Service;
using System.IO;
using System.Diagnostics;
using IWshRuntimeLibrary;

namespace Classroom
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {
        public static VideoControl VideoControl = null;

        public static string UpdateUrl;

        protected override async void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            RegisterModuleComponents();
            CreateLogFile();

            HandleStartupArgs(e);

            await GetUpldateUrl();

            MakeAppSquirrelAware();

            ShutdownMode = ShutdownMode.OnExplicitShutdown;

            await Task.Run(async () =>
            {
                await CheckUpdate();
            });

            ShutdownMode = ShutdownMode.OnLastWindowClose;
            var loginView = DependencyResolver.Current.GetService<LoginView>();
            loginView.Show();
            Current.DispatcherUnhandledException += Current_DispatcherUnhandledException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
        }


        private async Task<bool> CheckUpdate()
        {
            var hasUpdate = false;

            try
            {
                var updateUrl = GlobalData.Instance.LocalSetting.UpdateUrl;
                Log.Logger.Debug($"CheckUpdate => {updateUrl}");

                using (var updateMgr = new UpdateManager(updateUrl))
                {
                    var updateInfo = await updateMgr.CheckForUpdate();
                    if (updateInfo != null && updateInfo.ReleasesToApply?.Any() == true)
                    {
                        // 包含更新
                        hasUpdate = true;
                    }
                }
                Log.Logger.Information($"是否包含更新：{hasUpdate}");
                if (hasUpdate)
                {
                    var updateMsg = "检测到有新版本，是否升级？";
                    var updateDialog = new Dialog($"{updateMsg}", "是", "否");
                    var result = updateDialog.ShowDialog();
                    if (!result.HasValue || !result.Value)
                    {
                        Log.Logger.Debug("【refuse to update】");
                    }
                    else
                    {
                        Log.Logger.Debug("【agree to update】");
                        var updatingView = new UpdatingView();
                        updatingView.ShowDialog();
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Logger.Error($"CheckUpdate => {ex}");
            }
            return false;
        }

        private async Task GetUpldateUrl()
        {
            await Task.Run(() =>
            {
                try
                {
                    var localDataManager = new LocalDataManager();
                    GlobalData.Instance.LocalSetting = localDataManager.GetSettingConfig() ?? new LocalSetting();
                    UpdateUrl = GlobalData.Instance.LocalSetting.UpdateUrl;
                }
                catch (Exception ex)
                {
                    Log.Logger.Error($"【read SettingConfig.xml】 exception：{ex}");
                }
            });
            // return App.UpdateUrl;
        }


        protected override void OnExit(ExitEventArgs e)
        {
            //停止录播系统对接
            RecordingSystemService.Instance.Stop();
            RecordingSystemService.Instance.SetControlComand(ControlComand.ResetWorkMode);
            Environment.Exit(0);
            base.OnExit(e);
        }

        private void RegisterModuleComponents()
        {
            var builder = new ContainerBuilder();

            // Make sure any not specifically registered concrete type can resolve.
            builder.RegisterSource(new AnyConcreteTypeNotAlreadyRegisteredSource());

            builder.RegisterModule(new ServiceModule());

            IContainer container = builder.Build();

            DependencyResolver.SetContainer(container);

            IoC.GetInstance = GetInstance;
            IoC.GetAllInstances = GetAllInstances;
            IoC.BuildUp = BuildUp;
        }
        private void Current_DispatcherUnhandledException(object sender,
           System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            Log.Logger.Error("!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
            Log.Logger.Error($"【unhandled exception】：{e.Exception}");
            System.Windows.MessageBox.Show("当前应用程序遇到一些问题，将要退出！", "意外的操作", MessageBoxButton.OK, MessageBoxImage.Information);
            e.Handled = true;
        }


        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var exception = e.ExceptionObject as Exception;
            Log.Logger.Error("!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
            Log.Logger.Error($"【unhandled exception】：{exception}");
        }

        private void CreateLogFile()
        {
            ILogManager logManager = DependencyResolver.Current.GetService<ILogManager>();

            logManager.CreateLogFile();
        }

        private void HandleStartupArgs(StartupEventArgs e)
        {
            foreach (string t in e.Args)
            {
                if (t.Contains("uninstall"))
                {
                    RemoveShortcut();
                }
            }
        }

        private void MakeAppSquirrelAware()
        {
            try
            {
                Log.Logger.Debug($"MakeAppSquirrelAware => {UpdateUrl}");
                using (var mgr = new UpdateManager(UpdateUrl))
                {
                    SquirrelAwareApp.HandleEvents(
                        onInitialInstall: v =>
                        {
                            Log.Logger.Information($"onInitialInstall => 版本号：{v.ToString(3)}");
                            CreateShrotcut();
                            Current.Shutdown();
                        },

                        onAppUpdate: v =>
                        {
                            //await CopyConfigFile();
                            Log.Logger.Information($"onAppUpdate => 版本号：{v.ToString(3)}");
                            CreateShrotcut();
                            Current.Shutdown();
                        },

                        onAppUninstall: v =>
                        {
                            Log.Logger.Information($"onAppUninstall => 版本号：{v.ToString(3)}");
                            RemoveShortcut();
                            Current.Shutdown();
                        },

                        onAppObsoleted: v =>
                        {
                            Current.Shutdown();
                        },

                        onFirstRun: () =>
                        {
                            Log.Logger.Information("onFirstRun =>");
                            CreateShrotcut();
                        }
                    );
                }
            }
            catch (Exception ex)
            {
                Log.Logger.Error($"MakeAppSquirrelAware => {ex}");
            }
        }

        private void CreateShrotcut()
        {
            try
            {
                string location = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory),
                    "互动教室.lnk");

                Log.Logger.Debug($"create shortcut => {location}");

                WshShell shell = new WshShell();
                IWshShortcut shortcut = (IWshShortcut)shell.CreateShortcut(location);

                string currentVersionExeFullName = Process.GetCurrentProcess().MainModule.FileName;

                DirectoryInfo curDirectoryInfo = new DirectoryInfo(currentVersionExeFullName);

                string lastLevelDirName = curDirectoryInfo.Parent.ToString();
                string exeName = Path.GetFileName(currentVersionExeFullName);

                string lastLevelPath = Path.Combine(lastLevelDirName, exeName);

                string targetDir = currentVersionExeFullName.Replace(lastLevelPath, string.Empty);

                string targetExeFullName = Path.Combine(targetDir, exeName);

                shortcut.TargetPath = targetExeFullName;

                Log.Logger.Debug($"target path => {shortcut.TargetPath}");
                shortcut.Save();

            }
            catch (Exception ex)
            {
                Log.Logger.Error($"CreateShrotcut => {ex}");
            }
        }

        private void RemoveShortcut()
        {
            try
            {
                DirectoryInfo desktopDirectoryInfo =
                    new DirectoryInfo(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory));

                if (desktopDirectoryInfo.GetFiles("*.lnk")
                    .Any(file => file.Name == "互动教室.lnk"))
                {
                    Log.Logger.Debug($"RemoveShortcut => has shortcut on desktop");
                    System.IO.File.Delete(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory),
                        "互动教室.lnk"));
                }
                else
                {
                    Log.Logger.Debug($"RemoveShortcut => no shortcut no desktop");
                }
            }
            catch (Exception ex)
            {
                Log.Logger.Error($"RemoveShortcut => {ex}");
            }
        }



        #region Caliburn

        protected object GetInstance(System.Type service, string key)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                object instance;
                if (DependencyResolver.Current.Container.TryResolve((System.Type)service, out instance))
                    return instance;
            }
            else
            {
                object instance;
                if (DependencyResolver.Current.Container.TryResolveNamed(key, (System.Type)service, out instance))
                    return instance;
            }
            throw new Exception(string.Format("Could not locate any instances of contract {0}.", (object)(key ?? service.Name)));
        }

        protected IEnumerable<object> GetAllInstances(System.Type service)
        {
            return DependencyResolver.Current.Container.Resolve((System.Type)typeof(IEnumerable<>).MakeGenericType(service)) as IEnumerable<object>;
        }

        protected void BuildUp(object instance)
        {
            DependencyResolver.Current.Container.InjectProperties<object>(instance);
        }

        private void InitializeCaliburn()
        {
            PlatformProvider.Current = new XamlPlatformProvider();

            var baseExtractTypes = AssemblySourceCache.ExtractTypes;
            AssemblySourceCache.ExtractTypes = assembly =>
            {
                var baseTypes = baseExtractTypes(assembly);
                var elementTypes = assembly.GetExportedTypes()
                    .Where(t => typeof(UIElement).IsAssignableFrom(t));

                return baseTypes.Union(elementTypes);
            };

            AssemblySourceCache.Install();
            AssemblySource.Instance.AddRange(SelectAssemblies());
        }

        IEnumerable<Assembly> SelectAssemblies()
        {
            return new[] { ThisAssembly };
        }

        #endregion

        private Assembly _assembly;
        public Assembly ThisAssembly => _assembly ?? (_assembly = Assembly.GetEntryAssembly());
    }
}
