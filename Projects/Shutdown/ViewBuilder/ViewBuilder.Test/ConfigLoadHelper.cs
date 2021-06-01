using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Xml;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ViewBuilderBusiness.Manager;
using ViewBuilderBusiness.Structures.Config;

namespace ViewBuilder.Test
{
    class ConfigLoadHelper
    {

        // Usage example
        // 
        // [TestInitialize()]
        // public void MyTestInitialize()
        // {
        //     _config = ConfigLoadHelper.ConfigLoad();
        // }

        public static ProfileConfig ConfigLoad()
        {
            XmlDocument doc = new XmlDocument();
            doc.Load("TestConfig.xml");
            ProfileConfig config = new ProfileConfig();
            ProfileManager.OpenDirectoryProfile(doc, config);
            config.Init();

            Stopwatch watch = new Stopwatch();
            watch.Start();
            while (!config.IsInitialized && watch.ElapsedMilliseconds < 60000)
            {
                Thread.Sleep(200);
            }
            watch.Stop();
            Debug.WriteLine("Config loaded ({0} sec)",watch.Elapsed.TotalSeconds);
            return config;
        }
    }
}
