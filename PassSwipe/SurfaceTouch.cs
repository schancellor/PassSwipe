using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PassSwipe
{
    public class SurfaceTouch
    {
        public double xPosition;
        public double yPosition;
        public float majorFingerAxis;
        public float minorFingerAxis;
        public float fingerOrientation;

        public SurfaceTouch(double pXPos, double pYPos, float pMajor, float pMinor, float pOrien)
        {
            xPosition = pXPos;
            yPosition = pYPos;
            majorFingerAxis = pMajor;
            minorFingerAxis = pMinor;
            fingerOrientation = pOrien;
        }
    }
}
