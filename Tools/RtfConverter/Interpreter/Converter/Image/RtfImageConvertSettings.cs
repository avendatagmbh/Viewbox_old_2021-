// -- FILE ------------------------------------------------------------------
// name       : RtfImageConvertSettings.cs
// project    : RTF Framelet
// created    : Jani Giannoudis - 2008.06.05
// language   : c#
// environment: .NET 2.0
// copyright  : (c) 2004-2010 by Itenso GmbH, Switzerland
// --------------------------------------------------------------------------
using System;
using System.Drawing;
using System.IO;

namespace Itenso.Rtf.Converter.Image
{

	// ------------------------------------------------------------------------
	public class RtfImageConvertSettings
	{

		// ----------------------------------------------------------------------
		public RtfImageConvertSettings()
			: this( new RtfVisualImageAdapter() )
		{
		} // RtfImageConvertSettings

		// ----------------------------------------------------------------------
		public RtfImageConvertSettings( IRtfVisualImageAdapter imageAdapter )
		{
			if ( imageAdapter == null )
			{
				throw new ArgumentNullException( "imageAdapter" );
			}

			this.imageAdapter = imageAdapter;
		} // RtfImageConvertSettings

		// ----------------------------------------------------------------------
		public IRtfVisualImageAdapter ImageAdapter
		{
			get { return imageAdapter; }
		} // ImageAdapter

		// ----------------------------------------------------------------------
		public Color? BackgroundColor
		{
			get { return backgroundColor; }
			set { backgroundColor = value; }
		} // BackgroundColor

		// ----------------------------------------------------------------------
		public string ImagesPath
		{
			get { return imagesPath; }
			set { imagesPath = value; }
		} // ImagesPath

		// ----------------------------------------------------------------------
		public bool ScaleImage
		{
			get { return scaleImage; }
			set { scaleImage = value; }
		} // ScaleImage

		// ----------------------------------------------------------------------
		public float ScaleOffset
		{
			get { return scaleOffset; }
			set { scaleOffset = value; }
		} // ScaleOffset

		// ----------------------------------------------------------------------
		public float ScaleExtension
		{
			get { return scaleExtension; }
			set { scaleExtension = value; }
		} // ScaleExtension

		// ----------------------------------------------------------------------
		public string GetImageFileName( int index, RtfVisualImageFormat rtfVisualImageFormat )
		{
			string imageFileName = imageAdapter.ResolveFileName( index, rtfVisualImageFormat );
			if ( !string.IsNullOrEmpty( imagesPath ) )
			{
				imageFileName = Path.Combine( imagesPath, imageFileName );
			}
			return imageFileName;
		} // GetImageFileName

		// ----------------------------------------------------------------------
		private readonly IRtfVisualImageAdapter imageAdapter;
		private Color? backgroundColor;
		private string imagesPath;
		private bool scaleImage = true;
		private float scaleOffset;
		private float scaleExtension;

	} // class RtfImageConvertSettings

} // namespace Itenso.Rtf.Converter.Image
// -- EOF -------------------------------------------------------------------
