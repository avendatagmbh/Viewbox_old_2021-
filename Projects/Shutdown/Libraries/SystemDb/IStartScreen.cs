namespace SystemDb
{
	public interface IStartScreen
	{
		int Id { get; set; }

		string Name { get; set; }

		string ImgBase64 { get; set; }

		bool IsDefault { get; set; }
	}
}
