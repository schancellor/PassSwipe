using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PassSwipe
{
    class SurfaceTouch
    {
        double xPosition = 0.0;
        double yPosition = 0.0;
        float majorFingerAxis = 0.0f;
        float minorFingerAxis = 0.0f;
        float fingerOrientation = 0.0f;

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
