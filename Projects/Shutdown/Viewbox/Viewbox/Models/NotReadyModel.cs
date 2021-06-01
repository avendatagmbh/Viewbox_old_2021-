using System;

namespace Viewbox.Models
{
	public class NotReadyModel : ErrorModel
	{
		public int Retry { get; internal set; }

		public string ReturnUrl { get; internal set; }

		public DateTime StartTime => ViewboxApplication.ServerStart;

		public TimeSpan Duration { get; internal set; }

		public int Countdown => (int)Math.Max(0.0, Duration.TotalSeconds - (DateTime.Now - StartTime).TotalSeconds);

		public double Percentage => Math.Min(100.0, 100.0 * (double)(DateTime.Now - StartTime).Ticks / (double)Duration.Ticks);

		public double PercentageBlockwise => Math.Min(100.0, Math.Round(Percentage * 17.0 / 100.0) * 100.0 / 17.0);

		public NotReadyModel()
		{
			Retry = 1000;
			Duration = TimeSpan.FromSeconds(5.0);
			ReturnUrl = "/";
		}
	}
}
