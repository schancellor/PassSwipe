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

        //normalized raw images
        /*
        public byte[] normalizedImage;
        private ImageMetrics normalizedMetrics;
        Vector2 spriteOrigin = new Vector2(0f, 0f);
        public byte[] processedByteArray;
        bool isTouching;
         */

        Image<Gray, byte> canny;
        Image<Gray, byte> emguCvImage;

        public Stopwatch timer = new Stopwatch();
        //feature sets
        public DateTime startRecordTime;
        public DateTime endRecordTime;
        public TimeSpan totalTimeElapsed = new TimeSpan(0, 0, 0, 0, 0);

        public long totalMillisec;

        public double xyRatio = 0.0;

        //helper method for OnContact method
        public void OnContactHelper()
        {
            timer.Reset();
            timer.Start();
            startRecordTime = System.DateTime.Now;
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
            //endRecordTime = System.DateTime.Now;

            /*
            if ((startRecordTime != null) && (endRecordTime != null))
            {
                totalTimeElapsed = endRecordTime.Subtract(startRecordTime);
            }
             */
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

        //SurfaceImg to EmguCV Img
        private Image<Gray, byte> CreateEmguCvImage(byte[] image, ImageMetrics metrics)
        {
            //BUG 1-2-14: This returns with a NullReferenceException 75% of the time, but 25% of the time it runs. What?
            return new Image<Gray, byte>(metrics.Width, metrics.Height) { Bytes = image };
        }

    }
}
