using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SystemDb;
using NUnit.Framework;
using Rhino.Mocks;
using ViewboxAdmin.CustomEventArgs;

namespace ViewBoxAdmin_Test
{
    [TestFixture]
     class OptimizationEventArgs_Test {

        private OptimizationEventArgs _optimizationEventArgs;
        private IOptimization _optMock;

        [SetUp]
        public void TestSetUp() {
            _optMock = MockRepository.GenerateMock<IOptimization>();
            _optimizationEventArgs = new OptimizationEventArgs(_optMock);
        }

        [Test]
        public void Constructor_Optimization_Injection_Test() {
            Assert.AreEqual(_optMock,_optimizationEventArgs.Optimization);
        }
    }
}
