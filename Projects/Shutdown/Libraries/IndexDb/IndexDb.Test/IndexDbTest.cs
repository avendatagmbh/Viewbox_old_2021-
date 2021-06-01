using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace IndexDb.Test
{
    [TestClass]
    public class IndexDbTest
    {
        private const string ViewboxDbName = "viewbox";
        private const int ConnectFailCount = 100;
        private IndexDb target;
        public TestContext TestContext { get; set; }

        [Timeout(86399000), TestMethod]
        public void T01IndexDb()
        {
/*
            using (target = IndexDb.GetInstance())
            {
                Assert.IsNotNull(target);
                Assert.IsFalse(target.IsInitialized.HasValue && target.IsInitialized.Value);
                Assert.IsFalse(target.IsDisposed);
                GlobalSettings.ColumnCount = 50;
                DbConfig viewboxDb = new DbConfig("MySQL")
                                         {
                                             DbName = ViewboxDbName,
                                             Hostname = "localhost",
                                             Username = "root",
                                             Password = "avendata"
                                         };
                target.Init(viewboxDb, 4);
                
                Assert.IsTrue(target.IndexDbConnection.DbConfig.DbName.Contains(ViewboxDbName + "_index"));
                int attempts = 0;
                while (target.ViewboxDbConnection.ConnectionState == ConnectionStates.Connecting)
                {
                    System.Threading.Thread.Sleep(100);
                    attempts++;
                    if (ConnectFailCount < attempts)
                        break;
                }
                attempts = 0;
                while (target.IndexDbConnection.ConnectionState == ConnectionStates.Connecting)
                {
                    System.Threading.Thread.Sleep(100);
                    attempts++;
                    if (ConnectFailCount < attempts)
                        break;
                }
                Assert.IsTrue(target.ViewboxDbConnection.ConnectionState == ConnectionStates.Online);
                Assert.IsTrue(target.IndexDbConnection.ConnectionState == ConnectionStates.Online);
                System.Threading.Thread.Sleep(200);
                
                while (!target.IsInitialized.HasValue || !target.IsInitializedIndex.HasValue)
                    System.Threading.Thread.Sleep(100);
                
                var sw = new Stopwatch();
                sw.Start();
                while (!target.IsInitialized.Value && !target.IsInitializedIndex.Value && sw.ElapsedMilliseconds < 10000)
                {
                    System.Threading.Thread.Sleep(100);
                }
                Assert.IsTrue(target.IsInitialized.HasValue && target.IsInitialized.Value);
                Assert.IsTrue(target.IsInitializedIndex.HasValue && target.IsInitializedIndex.Value);
                sw.Restart();
                JobQueue queue = new JobQueue();
                ProgressCalculator calc = new ProgressCalculator();
                queue.QueueJobs(calc);
                queue.StartJobs(calc);
                sw.Stop();
                Debug.WriteLine(sw.Elapsed.TotalSeconds);
            }*/
        }
    }
}