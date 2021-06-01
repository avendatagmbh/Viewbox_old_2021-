// ImageHelper.h
#include "opencv2/highgui/highgui.hpp"
#include "opencv2/imgproc/imgproc.hpp"
#include <string>
#include <msclr\marshal_cppstd.h>

#pragma once

using namespace System;

//using namespace cv;

namespace ImageHelper {

	public value struct ScreenshotInfo
	{
		int foundX;
		int foundY;
	};

	public ref class ImageHelperClass
	{
		cv::Mat *_refImg;
	public:
		ImageHelperClass(String ^refScreenshotPath, int x, int y, int width, int height){
			msclr::interop::marshal_context context;
			_refImg = new cv::Mat;
			*_refImg = cv::imread( context.marshal_as<std::string>(refScreenshotPath), 1 );
			*_refImg = (*_refImg)(cv::Rect(x, y, width, height)); 
		}
		~ImageHelperClass(){
			delete _refImg;
		}

		ScreenshotInfo registerScreenshot(String ^screenshotPath){
			ScreenshotInfo screenshot;
			msclr::interop::marshal_context context;
			std::string path = context.marshal_as<std::string>(screenshotPath);
			cv::Mat img = cv::imread(path, 1);

			/// Create the result matrix
			int result_cols =  img.cols - _refImg->cols + 1;
			int result_rows = img.rows - _refImg->rows + 1;
			cv::Mat result;
			result.create( result_cols, result_rows, CV_32FC1 );

			/// Do the Matching and Normalize
			int match_method = 0;
			matchTemplate( *_refImg, img, result, match_method );
			cv::normalize( result, result, 0, 1, cv::NORM_MINMAX, -1, cv::Mat() );

			/// Localizing the best match with minMaxLoc
			double minVal; double maxVal; cv::Point minLoc; cv::Point maxLoc;
			cv::Point matchLoc;

			cv::minMaxLoc( result, &minVal, &maxVal, &minLoc, &maxLoc, cv::Mat() );
			screenshot.foundX = minLoc.x;
			screenshot.foundY = minLoc.y;
			return screenshot;
		}

		/*void registerScreenshots(String ^refScreenshotPath, array<ScreenshotInfo >^ screenshots, int count, int x, int y, int width, int height){
		//void registerScreenshots(String ^refScreenshotPath, int count){
			msclr::interop::marshal_context context;
			cv::Mat refImg = cv::imread( context.marshal_as<std::string>(refScreenshotPath), 1 );
			refImg = refImg(cv::Rect(x, y, width, height)); 
			//cv::Mat refImg = cv::imread( "C:\\Users\\beh\\Documents\\Verprobungsbilder\\01_01.PNG", 1);
			for(int i = 0; i < count; ++i){
				std::string path = context.marshal_as<std::string>(screenshots[i].path);
				cv::Mat img = cv::imread(path, 1);

				/// Create the result matrix
				int result_cols =  img.cols - refImg.cols + 1;
				int result_rows = img.rows - refImg.rows + 1;
				cv::Mat result;
				result.create( result_cols, result_rows, CV_32FC1 );

				/// Do the Matching and Normalize
				int match_method = 0;
				matchTemplate( refImg, img, result, match_method );
				cv::normalize( result, result, 0, 1, cv::NORM_MINMAX, -1, cv::Mat() );

				/// Localizing the best match with minMaxLoc
				double minVal; double maxVal; cv::Point minLoc; cv::Point maxLoc;
				cv::Point matchLoc;

				cv::minMaxLoc( result, &minVal, &maxVal, &minLoc, &maxLoc, cv::Mat() );
				screenshots[i].foundX = minLoc.x;
				screenshots[i].foundY = minLoc.y;
			}
		}*/
	};
}
