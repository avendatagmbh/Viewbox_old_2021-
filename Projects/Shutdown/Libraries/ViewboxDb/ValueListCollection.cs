using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace ViewboxDb
{
	[Serializable]
	public class ValueListCollection : List<ValueListElement>
	{
		public int UseCount { get; set; }

		public ValueListCollection DeepClone()
		{
			MemoryStream stream = new MemoryStream();
			BinaryFormatter binaryFormatter = new BinaryFormatter();
			binaryFormatter.Serialize(stream, this);
			stream.Seek(0L, SeekOrigin.Begin);
			ValueListCollection result = (ValueListCollection)binaryFormatter.Deserialize(stream);
			stream.Close();
			return result;
		}
	}
}
