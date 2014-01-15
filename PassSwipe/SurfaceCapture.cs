using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
        public FeatureManager fm;

        //normalized raw images
        public byte[] normalizedImage;
        private ImageMetrics normalizedMetrics;
        Vector2 spriteOrigin = new Vector2(0f, 0f);
        public byte[] processedByteArray;
        bool isTouching;

        Image<Gray, byte> canny;
        Image<Gray, byte> emguCvImage;

        //feature sets
        DateTime startRecordTime;
        DateTime endRecordTime;
        public TimeSpan totalTimeElapsed = new TimeSpan(0,0,0,0,0);

        //method to start grabbing contact data/feature data
        public void OnContactStartRecord(object sender, ContactEventArgs e)
        {
            isTouching = true;
            startRecordTime = System.DateTime.Now;
        }

        //method to stop grabbing data, calculate feature data
        public void OffContactStopRecord(object sender, ContactEventArgs e)
        {
            endRecordTime = System.DateTime.Now;

            if ((startRecordTime != null) && (endRecordTime != null))
            {
                totalTimeElapsed = endRecordTime.Subtract(startRecordTime);
            }
        }

        public void OnContactRecordGesture(object sender, FrameReceivedEventArgs e)
        {
            if (isTouching)
            {
                if (normalizedImage == null)
                {
                    e.TryGetRawImage(
                        ImageType.Normalized,
                        0, 0,
                        InteractiveSurface.DefaultInteractiveSurface.Width,
                        InteractiveSurface.DefaultInteractiveSurface.Height,
                        out normalizedImage,
                        out normalizedMetrics);
                }
                else //updates raw image data
                {
                    e.UpdateRawImage(
                        ImageType.Normalized,
                        normalizedImage,
                        0, 0,
                        InteractiveSurface.DefaultInteractiveSurface.Width,
                        InteractiveSurface.DefaultInteractiveSurface.Height);
                }

                //create img
                emguCvImage = CreateEmguCvImage(normalizedImage, normalizedMetrics);

                processedByteArray = processImage(emguCvImage);
            }
        }

        //basic image processing algorithm
        private byte[] processImage(Image<Gray, byte> gestureImg)
        {
            canny = gestureImg;
            canny._ThresholdBinary(new Gray(75), new Gray(255));
            canny = canny.Canny(new Gray(125), new Gray(120));
            return canny.Bytes;
        }

        //SurfaceImg to EmguCV Img
        private Image<Gray, byte> CreateEmguCvImage(byte[] image, ImageMetrics metrics)
        {
            //BUG 1-2-14: This returns with a NullReferenceException 75% of the time, but 25% of the time it runs. What?
            return new Image<Gray, byte>(metrics.Width, metrics.Height) { Bytes = image };
        }

        public ImageMetrics returnMetrics()
        {
            return normalizedMetrics;
        }

        public void Update(GameTime gameTime)
        {
            processedTexture.SetData<Byte>(this.processedByteArray,
                                            0,
                                            normalizedMetrics.Width * normalizedMetrics.Height,
                                            SetDataOptions.Discard
                                            );
        }

        //Draw our sprite on the table
        public void Draw(SpriteBatch tsb)
        {
            // Adds the rawimage sprite to Spritebatch for drawing.
            if (processedTexture != null)
            {
                tsb.Draw(processedTexture,
                                spriteOrigin,
                                null,
                                new Microsoft.Xna.Framework.Graphics.Color(81, 81, 81),
                                0f,
                                spriteOrigin,
                                scale,
                                SpriteEffects.FlipVertically,
                                0f);
            }
        }

    }
}
