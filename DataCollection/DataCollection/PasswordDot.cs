using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace DataCollection
{
    public class PasswordDot
    {
        public Vector2 position = new Vector2(0, 0);

        public Texture2D touchTexture { get; set; }
        
        /*
        public void LoadContent(ContentManager theContentManager)
        {
            touchTexture = theContentManager.Load<Texture2D>("passCircle");
        }
         */

        //Draw our sprite on the table
        public void Draw(SpriteBatch theSpriteBatch)
        {
            theSpriteBatch.Draw(touchTexture, position, Color.White);
        }

        public PasswordDot(Vector2 location, Texture2D texture)
        {
            this.position = location;
            this.touchTexture = texture;

        }
    }
}
