using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.CV.ML;
using System.IO;

namespace DataCollection
{
    class SurfaceTouchSvm
    {
        int trainingSampleCount = 0;
        List<SurfaceTouch> csvTouchList = new List<SurfaceTouch>();

        private void csvToList(string pFileName)
        {
            StreamReader textReader = new StreamReader(File.OpenRead(pFileName));

            while (!textReader.EndOfStream)
            {
                string line = textReader.ReadLine();
                string[] splitLine = line.Split(',');
                csvTouchList.Add(new SurfaceTouch(Convert.ToDouble(splitLine[2]),
                                                  Convert.ToDouble(splitLine[3]),
                                                  float.Parse(splitLine[4]),
                                                  float.Parse(splitLine[5]),
                                                  float.Parse(splitLine[6]),
                                                  Convert.ToInt64(splitLine[7])
                                                  ));
            }
        }

        private Matrix<double> listToMatrix()
        {
            Matrix<double> tempMatrix = new Matrix<double>(trainingSampleCount, 3);

            tempMatrix.GetCol(0).SetValue((csvTouchList[0].xPosition));

            return tempMatrix;
        }

    }
}
