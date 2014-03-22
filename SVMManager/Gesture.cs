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
        double totalGestureTime;
        double xyRatio;

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
            xyRatio = calculateRatio();
            totalGestureTime = calculateGestureTime();
        }

        public List<double> returnMetrics()
        {
            List<double> temp = new List<double>();

            //add gestures to list
            temp.Add(xyRatio);
            temp.Add(totalGestureTime);

            return temp;
        }

        #region featureCalc
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

