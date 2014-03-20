using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using Microsoft.Surface;
using Microsoft.Surface.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Storage;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using System.Text;

namespace PassSwipe
{
    /// <summary>
    /// This is the main type for your application.
    /// </summary>
    /// 
    public class App1 : Microsoft.Xna.Framework.Game
    {
        protected readonly GraphicsDeviceManager graphics;
        private ContactTarget contactTarget;
        private UserOrientation currentOrientation = UserOrientation.Bottom;
        private Microsoft.Xna.Framework.Graphics.Color backgroundColor = new Microsoft.Xna.Framework.Graphics.Color(81, 81, 81);
        private bool applicationLoadCompleteSignalled;
        private Matrix screenTransform = Matrix.Identity;

        //initialization code for Surface image grabbing
        public SpriteBatch spriteBatch;
        SurfaceCapture capture;
        public Texture2D processedTexture;
        private SpriteFont font;

        // scale raw image back to full screen
        private float scale =
            (float)InteractiveSurface.DefaultInteractiveSurface.Width / InteractiveSurface.DefaultInteractiveSurface.Height;

        //normalized raw image data
        public byte[] normalizedImage;
        private ImageMetrics normalizedMetrics;
        Vector2 spriteOrigin = new Vector2(0f, 0f);
        public byte[] processedByteArray;
        bool isTouching;
        
        //random extractable bits of data from a Contact - currently on text output
        double xpos = 0.0;
        double ypos = 0.0;
        float majorAxis = 0.0f;
        float minorAxis = 0.0f;
        float orientation = 0.0f;
        long elapsedTime = 0;

        int totalGestureNum = 0;

        private List<SurfaceTouch> touchManager = new List<SurfaceTouch>();

        // application state: Activated, Previewed, Deactivated,
        // start in Activated state
        private bool isApplicationActivated = true;
        private bool isApplicationPreviewed;

        /// <summary>
        /// The graphics device manager for the application.
        /// </summary>

        protected GraphicsDeviceManager Graphics
        {
            get { return graphics; }
        }

        /// <summary>
        /// The target receiving all surface input for the application.
        /// </summary>
        protected ContactTarget ContactTarget
        {
            get { return contactTarget; }
        }

        public ImageMetrics returnMetrics()
        {
            return normalizedMetrics;
        }

        /// <summary>
        /// Default constructor.
        /// </summary>
        public App1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        /// <summary>
        /// Allows the app to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            SetWindowOnSurface();
            InitializeSurfaceInput();

            // Set the application's orientation based on the current launcher orientation
            currentOrientation = ApplicationLauncher.Orientation;

            // Subscribe to surface application activation events
            ApplicationLauncher.ApplicationActivated += OnApplicationActivated;
            ApplicationLauncher.ApplicationPreviewed += OnApplicationPreviewed;
            ApplicationLauncher.ApplicationDeactivated += OnApplicationDeactivated;

            // Setup the UI to transform if the UI is rotated.
            if (currentOrientation == UserOrientation.Top)
            {
                // Create a rotation matrix to orient the screen so it is viewed correctly when the user orientation is 180 degress different.
                Matrix rotation = Matrix.CreateRotationZ(MathHelper.ToRadians(180));
                Matrix translation = Matrix.CreateTranslation(graphics.GraphicsDevice.Viewport.Width, graphics.GraphicsDevice.Viewport.Height, 0);

                screenTransform = rotation * translation;
            }
            base.Initialize();
        }

        /// <summary>
        /// Moves and sizes the window to cover the input surface.
        /// </summary>
        private void SetWindowOnSurface()
        {
            System.Diagnostics.Debug.Assert(Window.Handle != System.IntPtr.Zero,
                "Window initialization must be complete before SetWindowOnSurface is called");
            if (Window.Handle == System.IntPtr.Zero)
                return;

            // We don't want to run in full-screen mode because we need
            // overlapped windows, so instead run in windowed mode
            // and resize to take up the whole surface with no border.

            // Make sure the graphics device has the correct back buffer size.
            InteractiveSurface interactiveSurface = InteractiveSurface.DefaultInteractiveSurface;
            if (interactiveSurface != null)
            {
                graphics.PreferredBackBufferWidth = interactiveSurface.Width;
                graphics.PreferredBackBufferHeight = interactiveSurface.Height;
                graphics.ApplyChanges();

                // Remove the border and position the window.
                Program.RemoveBorder(Window.Handle);
                Program.PositionWindow(Window);
            }
        }

        /// <summary>
        /// Initializes the surface input system. This should be called after any window
        /// initialization is done, and should only be called once.
        /// </summary>
        private void InitializeSurfaceInput()
        {
            System.Diagnostics.Debug.Assert(Window.Handle != System.IntPtr.Zero,
                "Window initialization must be complete before InitializeSurfaceInput is called");
            if (Window.Handle == System.IntPtr.Zero)
                return;
            System.Diagnostics.Debug.Assert(contactTarget == null,
                "Surface input already initialized");
            if (contactTarget != null)
                return;

            capture = new SurfaceCapture();

            // Create a target for surface input.
            contactTarget = new ContactTarget(Window.Handle, EventThreadChoice.OnBackgroundThread);
            contactTarget.EnableInput();
            contactTarget.EnableImage(ImageType.Normalized);

            // Register events
            contactTarget.ContactAdded += OnContactStartRecord;
            contactTarget.FrameReceived += OnContactRecordGesture;
            contactTarget.ContactRemoved += OffContactStopRecord;
        }

        public void OnContactStartRecord(object sender, ContactEventArgs e)
        {
            isTouching = true;
            capture.OnContactHelper();
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

                capture.OnContactRecordHelper(normalizedImage, normalizedMetrics);
            }
        }

        public void OffContactStopRecord(object sender, ContactEventArgs e)
        {
            capture.OffContactHelper();

            if (capture.totalMillisec > 800)
            {
                writeToCSV();

                capture.calculateRatio(touchManager);

                advanceGesture();
            }
        }

        /// <summary>
        /// A small helper method that advances the number of gestures executed in a single
        /// run of the program. Also clears the touchManager list after each completed gesture
        /// NOTE: Only runs if a gesture is longer than 800 milliseconds
        /// </summary>
        public void advanceGesture()
        {
            totalGestureNum++;

            touchManager.Clear();
        }

        //Draw method that adds contact analytics to the screen
        private void DrawText()
        {
            spriteBatch.DrawString(font, "X pos: " + xpos, new Vector2(20, 45), Microsoft.Xna.Framework.Graphics.Color.White);
            spriteBatch.DrawString(font, "Y pos: " + ypos, new Vector2(20, 85), Microsoft.Xna.Framework.Graphics.Color.White);

            spriteBatch.DrawString(font, "Major Axis: " + majorAxis, new Vector2(20, 115), Microsoft.Xna.Framework.Graphics.Color.White);
            spriteBatch.DrawString(font, "Minor Axis: " + minorAxis, new Vector2(20, 155), Microsoft.Xna.Framework.Graphics.Color.White);
            spriteBatch.DrawString(font, "Orientation: " + orientation, new Vector2(20, 185), Microsoft.Xna.Framework.Graphics.Color.White);
            spriteBatch.DrawString(font, "Total Gesture Time: " + capture.totalMillisec, new Vector2(20, 245), Microsoft.Xna.Framework.Graphics.Color.White);

            spriteBatch.DrawString(font, "X-Y Ratio: " + capture.xyRatio, new Vector2(20, 285), Microsoft.Xna.Framework.Graphics.Color.White);
        }

        public void writeToCSV()
        {
            string filePath = @"C:\Users\faculty\Desktop\test-3-19.csv";  
            string delimiter = ",";

            StringBuilder sb = new StringBuilder();

            //for each line of the touchList
            //take each object and push it into a string
            //regx the string with commas
            //add string to csv
            sb.AppendLine("ContactStart");

            for (int i = 0; i < this.touchManager.Count; i++)
            {
                //order:
                /* 0 - in theory represents the user (not implemented yet)
                 * gesture number touch is association
                 * x position
                 * y position
                 * major finger axis
                 * minor finger axis
                 * finger orientation
                 * timestamp for that touch
                 */
                string[] strOut = {
                                    "0",
                                    (totalGestureNum.ToString()),
                                    (touchManager[i].xPosition).ToString(),
                                    (touchManager[i].yPosition).ToString(),
                                    (touchManager[i].majorFingerAxis).ToString(),
                                    (touchManager[i].minorFingerAxis).ToString(),
                                    (touchManager[i].fingerOrientation).ToString(),
                                    (touchManager[i].timeInMillisecond).ToString()
                                  };
                sb.AppendLine(string.Join(delimiter, strOut));
            }            
                
            File.AppendAllText(filePath, sb.ToString());

        }
            
        /// <summary>
        /// Load your graphics content.
        /// </summary>
        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            font = Content.Load<SpriteFont>("myFont");
            // TODO: Load any content
        }

        /// <summary>
        /// Unload your graphics content.
        /// </summary>
        protected override void UnloadContent()
        {
            Content.Unload();
        }

        /// <summary>
        /// Allows the app to run logic such as updating the world,
        /// checking for collisions, gathering input and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // TODO: Process contacts
            // Use the following code to get the state of all current contacts.
            ReadOnlyContactCollection contacts = contactTarget.GetState();

            if (contacts.Count > 0)
            {
                xpos = contacts[0].CenterX;
                ypos = contacts[0].CenterY;
                majorAxis = contacts[0].MajorAxis;
                minorAxis = contacts[0].MinorAxis;
                orientation = contacts[0].Orientation;
                elapsedTime = capture.timer.ElapsedMilliseconds;

                touchManager.Add(new SurfaceTouch(xpos, 
                                                  ypos, 
                                                  majorAxis, 
                                                  minorAxis, 
                                                  orientation,
                                                  elapsedTime));


                if (returnMetrics() != null)
                {
                    if (processedTexture == null)
                    {
                        processedByteArray = new byte[returnMetrics().Width * returnMetrics().Height];

                        processedTexture = new Texture2D(graphics.GraphicsDevice,
                                                         returnMetrics().Width,
                                                         returnMetrics().Height,
                                                         1,
                                                         TextureUsage.AutoGenerateMipMap,
                                                         SurfaceFormat.Luminance8);

                        graphics.GraphicsDevice.Textures[0] = null;
                    }

                    else
                    {
                        processedTexture.SetData<Byte>(this.processedByteArray,
                                            0,
                                            normalizedMetrics.Width * normalizedMetrics.Height,
                                            SetDataOptions.Discard
                                            );
                    }
                }
            }

            base.Update(gameTime);
        }


        /// <summary>
        /// This is called when the app should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            if (!applicationLoadCompleteSignalled)
            {
                // Dismiss the loading screen now that we are starting to draw
                ApplicationLauncher.SignalApplicationLoadComplete();
                applicationLoadCompleteSignalled = true;
            }

            //TODO: Rotate the UI based on the value of screenTransform here if desired

            graphics.GraphicsDevice.Clear(backgroundColor);

            spriteBatch.Begin();

            //Draw screen capture of touch dot (located in SurfaceCapture class)
            if (processedTexture != null)
            {
                spriteBatch.Draw(processedTexture,
                                 spriteOrigin,
                                 null,
                                 new Microsoft.Xna.Framework.Graphics.Color(81, 81, 81),
                                 0f,
                                 spriteOrigin,
                                 scale,
                                 SpriteEffects.FlipVertically,
                                 0f);
            }
            
            //Draw text analytics
            DrawText();
            
            spriteBatch.End();

            //TODO: Add your drawing code here
            //TODO: Avoid any expensive logic if application is neither active nor previewed

            base.Draw(gameTime);
        }

        /// <summary>
        /// This is called when application has been activated.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnApplicationActivated(object sender, EventArgs e)
        {
            // update application state
            isApplicationActivated = true;
            isApplicationPreviewed = false;

            //TODO: Enable audio, animations here

            //TODO: Optionally enable raw image here
        }

        /// <summary>
        /// This is called when application is in preview mode.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnApplicationPreviewed(object sender, EventArgs e)
        {
            // update application state
            isApplicationActivated = false;
            isApplicationPreviewed = true;

            //TODO: Disable audio here if it is enabled

            //TODO: Optionally enable animations here
        }

        /// <summary>
        ///  This is called when application has been deactivated.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnApplicationDeactivated(object sender, EventArgs e)
        {
            // update application state
            isApplicationActivated = false;
            isApplicationPreviewed = false;

            //TODO: Disable audio, animations here

            //TODO: Disable raw image if it's enabled
        }

        #region IDisposable

        protected override void Dispose(bool disposing)
        {
            try
            {
                if (disposing)
                {
                    IDisposable graphicsDispose = graphics as IDisposable;
                    if (graphicsDispose != null)
                    {
                        graphicsDispose.Dispose();
                        graphicsDispose = null;
                    }

                    if (contactTarget != null)
                    {
                        contactTarget.Dispose();
                        contactTarget = null;
                    }
                }
            }
            finally
            {
                base.Dispose(disposing);
            }
        }

        #endregion

    }
}
