using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace SVMManager
{
    class Gesture
    {
        List<double[]> numData;

        //gesture vars
        double totalGestureTime;
        double xyRatio;
        double majorAvg; //average of major axis
        double minorAvg; //average of minor axis
        double majorVariance; //variance on major axis
        double minorVariance; //variance on minor axis

        double avgXVelo; //average x velocity across full gesture
        double avgYVelo; //average y velocity acros full gesture

        //substroke components
        double firstVeloMag, secondVeloMag, thirdVeloMag, fourthVeloMag;
        double firstVeloDir, secondVeloDir, thirdVeloDir, fourthVeloDir;

        //finger orientation
        double avgFingerOrientation, fingerOrientationVariance;

        private List<double> xValues = new List<double>();
        private List<double> yValues = new List<double>();

        public Gesture(string csvLocation)
        {
            numData = csvToDoubleList(csvLocation);   
        }

        public Gesture(List<string[]> dividedList)
        {
            numData = stringListToDoubleList(dividedList);
        }

        public Gesture(List<double[]> processedList)
        {
            numData = processedList;
        }

        public void runMetrics()
        {
            generateXYLists();

            xyRatio = calculateRatio();
            totalGestureTime = calculateGestureTime();
            majorAvg = calculateAvgMajorFingerWidth();
            minorAvg = calculateAvgMinorFingerWidth();
            majorVariance = calculateMajorWidthVariance();
            minorVariance = calculateMinorFingerVariance();
            avgXVelo = calcAvgXVelo();
            avgYVelo = calcAvgYVelo();
            
            //calculate substroke velocity metrics (8 total)
            calcAllSubstrokes();

            //finger orientation
            avgFingerOrientation = calcAvgFingerOrientation();
            fingerOrientationVariance = calcOrientationVariance();
        }

        public List<double> returnMetrics()
        {
            List<double> temp = new List<double>();

            //add gestures to list
            temp.Add(xyRatio); //[0]
            temp.Add(totalGestureTime); //[1]
            temp.Add(majorAvg); //[2]
            temp.Add(minorAvg); //[3]

            temp.Add(avgXVelo); //[4]
            temp.Add(avgYVelo); //[5]

            temp.Add(majorVariance); //[6]
            temp.Add(minorVariance); //[7]

            //add substrokes
            temp.Add(firstVeloMag); //[8]
            temp.Add(firstVeloDir); //[9]
            temp.Add(secondVeloMag); //[10]
            temp.Add(secondVeloDir); //[11]
            temp.Add(thirdVeloMag); //[12]
            temp.Add(thirdVeloDir); //[13]
            temp.Add(fourthVeloMag); //[14]
            temp.Add(fourthVeloDir); //[15]

            return temp;
        }

        #region Feature Calculations
        //calculates total gesture time
        public double calculateGestureTime()
        {
            List<double> timeValues = new List<double>();

            for (int i = 0; i < numData.Count; i++)
            {
                timeValues.Add(numData[i][7]);
            }

            return timeValues.Max();
        }

        //calculates x:y ratio
        public double calculateRatio()
        {
            List<double> xValues = new List<double>();
            List<double> yValues = new List<double>();

            for (int i = 0; i < numData.Count; i++)
            {
                xValues.Add(numData[i][2]);
                yValues.Add(numData[i][3]);
            }

            double totalX = xValues.Max() - xValues.Min();
            double totalY = yValues.Max() - yValues.Min();

            return (totalX / totalY);
        }

        #region Finger Orientation Calculations

        //calculate average finger orientation
        public double calcAvgFingerOrientation()
        {
            double avgOrienSum = 0;
            for (int i = 0; i < numData.Count; i++)
            {
                avgOrienSum += numData[i][6];
            }

            return (avgOrienSum / numData.Count);
        }

        //calculate average finger variance
        public double calcOrientationVariance()
        {
            List<double> tempList = new List<double>();

            for (int i = 0; i < numData.Count; i++)
            {
                tempList.Add(numData[i][6]);
            }

            return calculateVariance(tempList);
        }

        #endregion

        #region Velocity calculations

        //calculate average x velocity of the entire gesture
        public double calcAvgXVelo()
        {
            return (xValues[(xValues.Count) - 1] - xValues[0]) / (totalGestureTime);
        }

        //calculate average y velocity of the entire gesture
        public double calcAvgYVelo()
        {
            return (yValues[((yValues.Count) - 1)] - yValues[0]) / (totalGestureTime);
        }

        //calculate all 8 substroke variables
        public void calcAllSubstrokes()
        {
            double[] firstVelo, secondVelo, thirdVelo, fourthVelo;

            List<List<double[]>> splitList = splitMainList(4);
            firstVelo = calcSubstrokeVelocityVector(splitList[0]);
            secondVelo = calcSubstrokeVelocityVector(splitList[1]);
            thirdVelo = calcSubstrokeVelocityVector(splitList[2]);
            fourthVelo = calcSubstrokeVelocityVector(splitList[3]);

            firstVeloMag = firstVelo[0];
            firstVeloDir = firstVelo[1];
            secondVeloMag = secondVelo[0];
            secondVeloDir = secondVelo[1];
            thirdVeloMag = thirdVelo[0];
            thirdVeloDir = thirdVelo[1];
            fourthVeloMag = fourthVelo[0];
            fourthVeloDir = fourthVelo[1];

        }
        //calculate substroke velocity of one small list
        public double[] calcSubstrokeVelocityVector(List<double[]> substrokeList){
            double[] vector = new double[2];
            double magnitude;
            double direction;

            List<double> subXValues = new List<double>();
            List<double> subYValues = new List<double>();

            for (int i = 0; i < substrokeList.Count; i++)
            {
                subXValues.Add(substrokeList[i][2]);
                subYValues.Add(substrokeList[i][3]);
            }

            //distance function
            //sqrt((x2 - x1)^2 + (y2 - y1)^2  )
            magnitude = Math.Sqrt((Math.Pow((subXValues[subXValues.Count - 1] - subXValues[0]) ,2) + Math.Pow(subYValues[subYValues.Count - 1] - subYValues[0] ,2)));

            //calculate angle
            //(total x distance / total dsitance aka the magnitude)
            direction = Math.Acos((subXValues[subXValues.Count - 1] - subXValues[0]) / magnitude);

            vector[0] = magnitude;
            vector[1] = direction;

            return vector;
        }

        #endregion

        #region Finger width calcs
        //calculates average width of major axis of finger
        public double calculateAvgMajorFingerWidth()
        {
            double totalSum = 0.0;

            for (int i = 0; i < numData.Count; i++)
            {
                totalSum += numData[i][4];
            }

            return (totalSum / numData.Count);
        }

        //calculates major Finger Width Variance
        public double calculateMajorWidthVariance()
        {
            List<double> tempList = new List<double>();

            for (int i = 0; i < numData.Count; i++)
            {
                tempList.Add(numData[i][4]);
            }

            return calculateVariance(tempList);
        }

        //calculates average width of minor axis of finger
        public double calculateAvgMinorFingerWidth()
        {
            double totalSum = 0.0;

            for (int i = 0; i < numData.Count; i++)
            {
                totalSum += numData[i][5];
            }

            return (totalSum / numData.Count);
        }

        //calculate minor finger width variance
        public double calculateMinorFingerVariance()
        {
            List<double> tempList = new List<double>();

            for (int i = 0; i < numData.Count; i++)
            {
                tempList.Add(numData[i][5]);
            }

            return calculateVariance(tempList);
        }

        #endregion

        #endregion

        #region Private Math Function

        //private method to generate easily accessible x-y lists
        //avoids multiple scans of the main list
        private void generateXYLists()
        {
            for (int i = 0; i < numData.Count; i++)
            {
                xValues.Add(numData[i][2]);
                yValues.Add(numData[i][3]);
            }
        }

        private List<List<double[]>> splitMainList(int sections)
        {
            List<List<double[]>> splitList = new List<List<double[]>>();

            for (int i = 0; i <= sections; i++)
            {
                List<double[]> newList;
                newList = numData.Skip(i * sections).Take(sections).ToList();
                splitList.Add(newList);
            }

            return splitList;
        }

        //calc variance
        private double calculateVariance(List<double> variList)
        {
            double vari = 0;

            if (variList.Count() > 0)
            {
                double avg = variList.Average();
                double sum = variList.Sum(d => Math.Pow(d - avg, 2));
                vari = ((sum) / (variList.Count() - 1));
            }

            return vari;
        }

        //calc standar deviation
        private double calculateStdDev(List<double> stdDevList)
        {
            return Math.Sqrt(calculateVariance(stdDevList));
        }

        #endregion

        //converts a specific file to a list of double arrays containing raw touch data
        private List<double[]> csvToDoubleList(string pFileName)
        {
            List<double[]> tempList = new List<double[]>();

            StreamReader textReader = new StreamReader(File.OpenRead(pFileName));

            while (!textReader.EndOfStream)
            {
                string line = textReader.ReadLine();
                double[] data = Array.ConvertAll(line.Split(','), new Converter<string, double>(Double.Parse));
                tempList.Add(data);
            }

            return tempList;
        }

        //converts a List of string arrays to a List of double Arrays
        private List<double[]> stringListToDoubleList(List<string[]> pStringList)
        {
            List<double[]> tempList = new List<double[]>();

            foreach (string[] s in pStringList)
            {
                double[] data = Array.ConvertAll(s, new Converter<string, double>(Double.Parse));
                tempList.Add(data);
            }

            return tempList;
        }

    }

}

