using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PassSwipe
{
    class FeatureManager
    {
        public List<SurfaceTouch> contactList = new List<SurfaceTouch>();

        //basic constructor
        public FeatureManager()
        {

        }

        public List<SurfaceTouch> getTouchList()
        {
            return contactList;
        }

        public void addToTouchList(SurfaceTouch _st)
        {
            contactList.Add(_st);
        }
        //method: calculate time

        //method: calculate delta change in eccentricity

        //method: 

        //method: calculate average velocity

        //method: calculate substroke velocity

        
    }
}
