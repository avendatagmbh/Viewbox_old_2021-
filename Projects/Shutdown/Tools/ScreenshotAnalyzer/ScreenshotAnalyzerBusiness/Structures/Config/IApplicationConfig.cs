namespace ScreenshotAnalyzerBusiness.Structures.Config {
    public interface IApplicationConfig {
        string LastProfile { get; set; }
        string LastQueryTable { get; set; }
        //string ConfigDirectory { get; set; }
    }
}