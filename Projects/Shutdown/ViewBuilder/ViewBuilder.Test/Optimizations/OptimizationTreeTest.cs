using Microsoft.VisualStudio.TestTools.UnitTesting;
using ViewBuilderBusiness.Optimizations;
using System;

namespace ViewBuilder.Test.Optimizations
{
	[TestClass]
	public class OptimizationTreeTest
	{
		[TestMethod]
		public void RootNodeTest()
		{
			// Arrange
			const string systemName = "SystemName";
			// Act
			var optimizationTree = new OptimizationTree(systemName);
			// Assert
			Assert.AreEqual(1, optimizationTree.Count);
			Assert.IsTrue(optimizationTree.Contains(new OptimizationTree.Node()
				{
					Id = 1,
					ParentId = 0,
					Level = 1,
					Value = systemName
				}));
		}

		[TestMethod]
		public void MoreNodeTest()
		{
			// Arrange
			const string systemName = "SystemName";
			const string colors = "Colors";
			const string numbers = "Numbers";
			const string blue = "blue";
			var optimizationTree = new OptimizationTree(systemName);
			// Act
			optimizationTree.Add(systemName, colors, 1, null);
            optimizationTree.Add(systemName, numbers, 1, null);
            optimizationTree.Add(colors, blue, 2, null);
			// Assert
			Assert.AreEqual(4, optimizationTree.Count);
			Assert.IsTrue(optimizationTree.Contains(new OptimizationTree.Node()
				{
					Id = 1,
					ParentId = 0,
					Level = 1,
					Value = systemName
				}));
			Assert.IsTrue(optimizationTree.Contains(new OptimizationTree.Node()
				{
					Id = 2,
					ParentId = 1,
					Level = 2,
					Value = colors
				}));
			Assert.IsTrue(optimizationTree.Contains(new OptimizationTree.Node()
				{
					Id = 3,
					ParentId = 1,
					Level = 2,
					Value = numbers
				}));
			Assert.IsTrue(optimizationTree.Contains(new OptimizationTree.Node()
				{
					Id = 4,
					ParentId = 2,
					Level = 3,
					Value = blue
				}));
		}
	
		[TestMethod]
		[ExpectedException(typeof(ArgumentException))]
		public void NotExistingParent()
		{
			// Arrange
			const string systemName = "SystemName";			
			var optimizationTree = new OptimizationTree(systemName);
			// Act
            optimizationTree.Add("xyxysadasda", "ChildValue", 1, null);
			// Assert
		}

		[TestMethod]
		public void ExistingChildNode()
		{
			// Arrange
			const string systemName = "SystemName";
			const string childValue = "ChildValue";
			var optimizationTree = new OptimizationTree(systemName);
			// Act
            optimizationTree.Add(systemName, childValue, 1, null);
            optimizationTree.Add(systemName, childValue, 1, null);
			// Assert
			Assert.AreEqual(2, optimizationTree.Count);
			Assert.IsTrue(optimizationTree.Contains(new OptimizationTree.Node()
				{
					Id = 1,
					ParentId = 0,
					Level = 1,
					Value = systemName
				}));
			Assert.IsTrue(optimizationTree.Contains(new OptimizationTree.Node()
			{
				Id = 2,
				ParentId = 1,
				Level = 2,
				Value = childValue
			}));
		}

		[TestMethod]
		public void ChildNodeWithSameValueButDifferentParent()
		{
			// Arrange
			const string systemName = "SystemName";
			const string childValue = "ChildValue";
			var optimizationTree = new OptimizationTree(systemName);
			// Act
            optimizationTree.Add(systemName, childValue, 1, null);
            optimizationTree.Add(childValue, childValue, 2, null);
			// Assert
			Assert.AreEqual(3, optimizationTree.Count);
			Assert.IsTrue(optimizationTree.Contains(new OptimizationTree.Node()
				{
					Id = 1,
					ParentId = 0,
					Level = 1,
					Value = systemName
				}));
			Assert.IsTrue(optimizationTree.Contains(new OptimizationTree.Node()
				{
					Id = 2,
					ParentId = 1,
					Level = 2,
					Value = childValue
				}));
			Assert.IsTrue(optimizationTree.Contains(new OptimizationTree.Node()
				{
					Id = 3,
					ParentId = 2,
					Level = 3,
					Value = childValue
				}));
		}
	}
}
