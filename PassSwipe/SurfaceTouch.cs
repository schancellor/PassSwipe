using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PassSwipe
{
    public class SurfaceTouch
    {
        double xPosition;
        double yPosition;
        float majorAxis;
        float minorAxis;
        float orientation;

        public SurfaceTouch()
        {
            xPosition = 0.0;
            yPosition = 0.0;
            majorAxis = 0.0f;
            minorAxis = 0.0f;
            orientation = 0.0f;
        }

        public SurfaceTouch(double pXPos, double pYPos, float pMajor, float pMinor, float pOrientaion){
            xPosition = pXPos;
            yPosition = pYPos;
            majorAxis = pMajor;
            minorAxis = pMinor;
            orientation = pOrientaion;
        }

        public double getXPos(){
            return xPosition;
        }

    }
}
