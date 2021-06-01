using System.Diagnostics;
using System.Threading;
using System.Xml;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ViewBuilderBusiness.Manager;
using ViewBuilderBusiness.Structures.Config;

namespace ViewBuilder.Test
{
    [TestClass]
    public abstract class TestBase
    {
        /// <summary>
        /// The maximum time in milliseconds until the profile initialization process should finish.
        /// </summary>
        private const int MaxWaitingTime = 300000;
        /// <summary>
        /// The time in milliseconds to wait until the next check, whether the profile initialization process is finished.
        /// </summary>
        private const int WaitingTime = 200;

        /// <summary>
        /// The current profile.
        /// </summary>
        protected ProfileConfig ProfileConfig { get; private set; }    

        /// <summary>
        /// Loads the current profile from the given config file.
        /// </summary>
        /// <param name="configFileName"></param>
        protected void LoadProfile(string configFileName)
        {
            var xmlDocument = new XmlDocument();
            xmlDocument.Load(configFileName);
            ProfileConfig = new ProfileConfig();
            ProfileManager.OpenDirectoryProfile(xmlDocument, ProfileConfig);
            ProfileConfig.Init();

            //ProfileConfig.ViewboxDb = new SystemDb.SystemDb();
            //ProfileConfig.ViewboxDb.SystemDbInitialized += ViewboxDbOnSystemDbInitialized;
            //ProfileConfig.ViewboxDb.Connect(this.DbConfig.DbType, newViewboxDbConfig.ConnectionString, MaxWorkerThreads.Value);

            var stopwatch = new Stopwatch();
            stopwatch.Start();
            while (!ProfileConfig.IsInitialized && stopwatch.ElapsedMilliseconds < MaxWaitingTime)
            {
                Thread.Sleep(WaitingTime);
            }
            stopwatch.Stop();
            Assert.IsTrue(ProfileConfig.IsInitialized);
        }

        /// <summary>
        /// Cleans up the environment.
        /// </summary>
        [TestCleanup]
        public void TestCleanup()
        {
            if(ProfileConfig != null) ProfileConfig.Dispose();
        }
    }
}
