using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PassSwipe
{
    class FeatureManager
    {
        public List<SurfaceTouch> touchList = new List<SurfaceTouch>();

        public float averageVelocity = 0.0f;

        //basic constructor
        public FeatureManager()
        {

        }

        public List<SurfaceTouch> getTouchList()
        {
            return touchList;
        }

        //method: calculate time

        //method: calculate delta change in eccentricity

        //method: 

        //method: calculate average velocity

        //method: calculate substroke velocity

        
    }
}
