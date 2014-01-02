using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Surface;
using Microsoft.Surface.Core;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;

namespace PassSwipe
{
    class SurfaceCapture
    {
        private float scale =
           (float)InteractiveSurface.DefaultInteractiveSurface.Width / InteractiveSurface.DefaultInteractiveSurface.Height;

        public Texture2D processedTexture;

        //normalized raw images
        public byte[] normalizedImage;
        private ImageMetrics normalizedMetrics;
        Vector2 spriteOrigin = new Vector2(0f, 0f);
        public byte[] processedByteArray;

        //random extractable bits of data from a Contact
        double xpos = 0.0;
        double ypos = 0.0;
        float majorAxis = 0.0f;
        float minorAxis = 0.0f;
        float orientation = 0.0f;
        Int64 timestamp = 0;

        Image<Gray, byte> canny;
        Image<Gray, byte> emguCvImage;

        public void OnContactRecordGesture(object sender, FrameReceivedEventArgs e)
        {
            if (normalizedImage == null)
            {
                bool testVar = e.TryGetRawImage(
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
