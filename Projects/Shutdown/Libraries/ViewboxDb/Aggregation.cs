using System;

namespace ViewboxDb
{
	public class Aggregation : ICloneable
	{
		public int cid { get; set; }

		public AggregationFunction agg { get; set; }

		public Aggregation()
		{
		}

		public Aggregation(int cid, AggregationFunction agg)
		{
			this.cid = cid;
			this.agg = agg;
		}

		public override int GetHashCode()
		{
			string hashString = cid + agg.ToString();
			return 0 + hashString.GetHashCode();
		}

		public object Clone()
		{
			return new Aggregation(cid, agg);
		}
	}
}
