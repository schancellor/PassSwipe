using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace PassSwipe
{
    public class FeatureManager
    {
        List<SurfaceTouch> touchList;

        public float averageVelocity = 0.0f;

        //basic constructor
        public FeatureManager()
        {
            touchList = new List<SurfaceTouch>();
        }

        public void Add(SurfaceTouch pST)
        {
            touchList.Add(pST);
        }

        public List<SurfaceTouch> getTouchList()
        {
            return touchList;
        }

        public void writeToFile()
        {
            string filePath = @"C:\Users\faculty\Desktop\test.csv";
            string delimiter = ",";
            List<SurfaceTouch> testX = App1.touchManager.getTouchList();

            string[][] output = new string[][]{  
                new string[]{ ( testX[0].getXPos()).ToString(), "Col 2 Row 1", "Col 3 Row 1"},  
                new string[]{"Col1 Row 2", "Col2 Row 2", "Col3 Row 2"}  
            };

            int length = output.GetLength(0);
            StringBuilder sb = new StringBuilder();

            for (int index = 0; index < length; index++)
                sb.AppendLine(string.Join(delimiter, output[index]));

            File.AppendAllText(filePath, sb.ToString());
        }

        /*public double calculateTime()
        {

        }*/
        //method: calculate time

        //method: calculate delta change in eccentricity

        //method: 

        //method: calculate average velocity

        //method: calculate substroke velocity

        
    }
}
