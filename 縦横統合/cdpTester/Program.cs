using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OpenCvSharp;
using System.Runtime.InteropServices;
using LabImg;

namespace cdpTester
{
	class Program
	{
		static unsafe void Main(string[] args)
		{
			string leftPath = Environment.CurrentDirectory + @"\L100100.jpg";
			string rightPath = Environment.CurrentDirectory + @"\R100100.jpg";
			IplImage left = new IplImage(leftPath, LoadMode.Color);
			IplImage right = new IplImage(rightPath, LoadMode.Color);
			int[] disparityX = new int[100 * 100];
			int[] disparityY = new int[100 * 100];

			//アドレス固定
			fixed (int* ptrX = disparityX)
			fixed (int* ptrY = disparityY)
			{
				Disparity.twoDCDP((byte*)left.ImageData, left.Height, left.Width, left.WidthStep,
								  (byte*)right.ImageData, right.Height, right.Width, right.WidthStep,
								   ptrX, ptrY);  
			}
		}
		
	}

}
