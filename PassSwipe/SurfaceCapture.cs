using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Storage;
using Microsoft.Surface;
using Microsoft.Surface.Core;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;

namespace PassSwipe
{
    class SurfaceCapture : App1
    {
        private float scale =
           (float)InteractiveSurface.DefaultInteractiveSurface.Width / InteractiveSurface.DefaultInteractiveSurface.Height;

        public Texture2D processedTexture;

        Image<Gray, byte> canny;
        Image<Gray, byte> emguCvImage;

        public Stopwatch timer = new Stopwatch();
        public long totalMillisec;

        public double xyRatio = 0.0;

        //SurfaceImg to EmguCV Img
        private Image<Gray, byte> CreateEmguCvImage(byte[] image, ImageMetrics metrics)
        {
            return new Image<Gray, byte>(metrics.Width, metrics.Height) { Bytes = image };
        }

        //helper method for OnContact method
        public void OnContactHelper()
        {
            timer.Reset();
            timer.Start();
        }

        //helper method for OnContactRecordGesture method
        public void OnContactRecordHelper(byte[] pnormImg, ImageMetrics pimgMet)
        {
            emguCvImage = CreateEmguCvImage(pnormImg, pimgMet);

            processedByteArray = processImage(emguCvImage);
        }

        //helper methods for OffContact method
        //spot where total time as a fea
        public void OffContactHelper()
        {
            timer.Stop();
            totalMillisec = timer.ElapsedMilliseconds;
        }

        //calculates x:y ratio as a feature for the data set
        public void calculateRatio(List<SurfaceTouch> pTouchList)
        {
            List<double> xValues = new List<double>();
            List<double> yValues = new List<double>();

            for (int i = 0; i < pTouchList.Count; i++)
            {
                xValues.Add(pTouchList[i].xPosition);
                yValues.Add(pTouchList[i].yPosition);
            }

            double totalX = xValues.Max() - xValues.Min();
            double totalY = yValues.Max() - yValues.Min();

            xyRatio = totalX / totalY;
        }

        //basic image processing algorithm
        private byte[] processImage(Image<Gray, byte> gestureImg)
        {
            canny = gestureImg;
            canny._ThresholdBinary(new Gray(75), new Gray(255));
            canny = canny.Canny(new Gray(75), new Gray(120));
            return canny.Bytes;
        }

    }
}
