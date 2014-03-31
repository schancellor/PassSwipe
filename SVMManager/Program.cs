using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.CV.ML;
using Emgu.CV.ML.Structure;

namespace SVMManager
{
    class Program
    {
        static void Main(string[] args)
        {
            int trainSampleCount = 0;
            Image<Bgr, Byte> img = new Image<Bgr, byte>(500, 500);

            //conversion of CSV to gesture variables
            List<Gesture> gestureListClass1 = csvToGestureList(@"C:\Users\faculty\Desktop\testEB-3-20.csv");
            List<Gesture> gestureListClass2 = csvToGestureList(@"C:\Users\faculty\Desktop\testSNC-3-20.csv");

            trainSampleCount = (gestureListClass1.Count) + (gestureListClass2.Count); //set the sample count to the number of gestures we have available

            //create relevant matrices based on size of the gestureList
            Matrix<float> sample = new Matrix<float>(1, 16);
            Matrix<float> trainTestData = new Matrix<float>(trainSampleCount, 16);
            Matrix<float> trainTestClasses = new Matrix<float>(trainSampleCount, 1);

            //GESTURE MATH INCOMING
            foreach (Gesture g in gestureListClass1)
            {
                g.runMetrics();
            }

            foreach (Gesture g in gestureListClass2)
            {
                g.runMetrics();
            }

            #region Generate the training data and classes

            //fill first set of data
            for (int i = 0; i < gestureListClass1.Count; i++)
            {
                double[] gMetrics = (gestureListClass1[i].returnMetrics()).ToArray();
                /*
                 * //add gestures to list
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
                 */
                trainTestData[i, 0] = ((float)gMetrics[0]) * 150; //xy ratio
                trainTestData[i, 1] = ((float)gMetrics[1]) / 4; //totalGestureTime
                trainTestData[i, 2] = ((float)gMetrics[2]);
                trainTestData[i, 3] = ((float)gMetrics[3]);
                trainTestData[i, 4] = ((float)gMetrics[4]);
                trainTestData[i, 5] = ((float)gMetrics[5]);
                trainTestData[i, 6] = ((float)gMetrics[6]);
                trainTestData[i, 7] = ((float)gMetrics[7]);
                trainTestData[i, 8] = ((float)gMetrics[8]);
                trainTestData[i, 9] = ((float)gMetrics[9]);
                trainTestData[i, 10] = ((float)gMetrics[10]);
                trainTestData[i, 11] = ((float)gMetrics[11]);
                trainTestData[i, 12] = ((float)gMetrics[12]);
                trainTestData[i, 13] = ((float)gMetrics[13]);
                trainTestData[i, 14] = ((float)gMetrics[14]);
                trainTestData[i, 15] = ((float)gMetrics[15]);
            }

            Matrix<float> trainTestData1 = trainTestData.GetRows(0, gestureListClass1.Count, 1);

            for (int j = 0; j < gestureListClass2.Count; j++)
            {
                double[] gMetrics = (gestureListClass2[j].returnMetrics()).ToArray();

                trainTestData[(j + gestureListClass1.Count), 0] = (float)gMetrics[0] * 150;
                trainTestData[(j + gestureListClass1.Count), 1] = ((float)gMetrics[1]) / 4;
                trainTestData[(j + gestureListClass1.Count), 2] = ((float)gMetrics[2]);
                trainTestData[(j + gestureListClass1.Count), 3] = ((float)gMetrics[3]);
                trainTestData[(j + gestureListClass1.Count), 4] = ((float)gMetrics[4]);
                trainTestData[(j + gestureListClass1.Count), 5] = ((float)gMetrics[5]);
                trainTestData[(j + gestureListClass1.Count), 6] = ((float)gMetrics[6]);
                trainTestData[(j + gestureListClass1.Count), 7] = ((float)gMetrics[7]);
                trainTestData[(j + gestureListClass1.Count), 8] = ((float)gMetrics[8]);
                trainTestData[(j + gestureListClass1.Count), 9] = ((float)gMetrics[9]);
                trainTestData[(j + gestureListClass1.Count), 10] = ((float)gMetrics[10]);
                trainTestData[(j + gestureListClass1.Count), 11] = ((float)gMetrics[11]);
                trainTestData[(j + gestureListClass1.Count), 12] = ((float)gMetrics[12]);
                trainTestData[(j + gestureListClass1.Count), 13] = ((float)gMetrics[13]);
                trainTestData[(j + gestureListClass1.Count), 14] = ((float)gMetrics[14]);
                trainTestData[(j + gestureListClass1.Count), 15] = ((float)gMetrics[15]);
            }

            Matrix<float> trainTestData2 = trainTestData.GetRows(gestureListClass1.Count, trainSampleCount, 1);

            Matrix<float> trainTestClasses1 = trainTestClasses.GetRows(0, gestureListClass1.Count, 1);
            trainTestClasses1.SetValue(1);
            Matrix<float> trainTestClasses2 = trainTestClasses.GetRows(gestureListClass1.Count, trainSampleCount, 1);
            trainTestClasses2.SetValue(2);
            #endregion

            using (SVM model = new SVM())
            {
                SVMParams p = new SVMParams();
                p.KernelType = Emgu.CV.ML.MlEnum.SVM_KERNEL_TYPE.LINEAR;
                p.SVMType = Emgu.CV.ML.MlEnum.SVM_TYPE.C_SVC;
                //p.Gamma = 0.1;
                p.C = 10;
                p.TermCrit = new MCvTermCriteria(100, 0.00001);

                //bool trained = model.Train(trainTestData, trainTestClasses, null, null, p);
                bool trained = model.TrainAuto(trainTestData, trainTestClasses, null, null, p.MCvSVMParams, 5);

                for (int i = 0; i < img.Height; i++)
                {
                    for (int j = 0; j < img.Width; j++)
                    {
                        sample.Data[0, 0] = j;
                        sample.Data[0, 1] = i;

                        float response = model.Predict(sample);

                        img[i, j] =
                           response == 1 ? new Bgr(90, 0, 0) :
                           new Bgr(0, 0, 90);
                        //response == 2 ? new Bgr(0, 90, 0) :
                    }
                }

                int c = model.GetSupportVectorCount();
                for (int i = 0; i < c; i++)
                {
                    float[] v = model.GetSupportVector(i);
                    PointF p1 = new PointF(v[0], v[1]);
                    img.Draw(new CircleF(p1, 4), new Bgr(255, 255, 128), 2);
                }

                model.Save(@"C:\Users\faculty\Desktop\svm-function3coord16.xml");
            }

            // display the original training samples
            for (int i = 0; i < (trainSampleCount / 2); i++)
            {
                if (i < trainTestData1.Rows)
                {
                    PointF p1 = new PointF((trainTestData1[i, 0]), (trainTestData1[i, 1]));
                    img.Draw(new CircleF(p1, 2.0f), new Bgr(255, 100, 100), -1);
                }

                if (i < trainTestData2.Rows)
                {
                    PointF p2 = new PointF((trainTestData2[i, 0]), (trainTestData2[i, 1]));
                    img.Draw(new CircleF(p2, 2.0f), new Bgr(100, 100, 255), -1);
                }
            }

            Emgu.CV.UI.ImageViewer.Show(img);
        }

        public static List<Gesture> csvToGestureList(string fName)
        {
            //conversion of CSV to gesture variables
            List<Gesture> tempList = new List<Gesture>();
            StreamReader textReader = new StreamReader(File.OpenRead(@fName));
            Dictionary<int, List<string[]>> map = new Dictionary<int, List<string[]>>();

            while (!textReader.EndOfStream)
            {
                string line = textReader.ReadLine(); //read CSV and split 
                string[] split = line.Split(',');

                if (!line.Contains("ContactStart"))
                {
                    //if the map does not contains the key already, create new key value pair
                    if (!map.ContainsKey(Convert.ToInt16(split[1])))
                    {
                        List<string[]> list;
                        map.Add(Convert.ToInt16(split[1]), list = new List<string[]>());
                        list.Add(split);
                    }
                    //if map contains key, then add to list
                    else
                    {
                        map[Convert.ToInt16(split[1])].Add(split);
                    }
                }
            }

            //convert the dictionary into unique gestures, then we can do math with them!
            foreach (KeyValuePair<int, List<string[]>> pair in map)
            {
                tempList.Add(new Gesture(pair.Value));
            }

            return tempList;
        }
    }
}
