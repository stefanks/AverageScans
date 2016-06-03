using IO.MzML;
using IO.Thermo;
using MassSpectrometry;
using Spectra;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AverageScans
{
    public class Class1
    {
        private static double mzTolerance = 5e-4;
        
        public static void LoadFileAndAverage()
        {
            Console.WriteLine("In LoadFileAndAverage");
            ThermoRawFile myFile = new ThermoRawFile(@"E:\Stefan\data\IntactMassRaw\11-01-15_NS_fract7_rep1.raw");
            myFile.Open();

            DefaultMsDataFile newFile = new DefaultMsDataFile(@"E:\Stefan\data\IntactMassRaw\11-01-15_NS_fract7_rep1-Averaged.mzML");

            int firstInd = myFile.FirstSpectrumNumber + 1;
            int lastInd = myFile.LastSpectrumNumber-1;
            MsDataScan<DefaultMzSpectrum>[] Scans = new MsDataScan<DefaultMzSpectrum>[lastInd- firstInd+1];

            // Ignore first and last scans, because there is nothing to average\
            for (int scan = firstInd; scan <= lastInd; scan++)
                {
                //Console.WriteLine("Looking at scan " + scan);
                DefaultMzSpectrum yeah = AverageSpectra(myFile.GetSpectrum(scan - 1), myFile.GetSpectrum(scan), myFile.GetSpectrum(scan + 1));

                MsDataScan<DefaultMzSpectrum> okay = new MsDataScan<DefaultMzSpectrum>(scan - 1, yeah, "averaged scan " + scan, 1, myFile.GetMsScan(scan).isCentroid, myFile.GetMsScan(scan).Polarity, myFile.GetMsScan(scan).RetentionTime);

                Scans[scan - myFile.FirstSpectrumNumber - 1] = okay;
            }

            newFile.Add(Scans);

            MzmlMethods.CreateAndWriteMyIndexedMZmlwithCalibratedSpectra(newFile);
            
        }

        private static DefaultMzSpectrum AverageSpectra(IMzSpectrum<MzPeak> massSpectrum1, IMzSpectrum<MzPeak> massSpectrum2, IMzSpectrum<MzPeak> massSpectrum3)
        {
            List<double> mzs = new List<double>();
            List<double> intensities = new List<double>();

            int i = 0, j = 0, k = 0;
            int numPeaksCombinedHere = 0, numPeaks = 0;

            var prevMzPeakMZ = double.NaN;
            bool ended;
            int numTimesCombinedThis = 0;
            while (true)
            {
                ended = true;
                var smallestMzPeakMZ = double.NaN;
                var smallestMzPeakIntensity = double.NaN;

                var arrayToCheck = new List<double>();

                if (i < massSpectrum1.Count)
                {
                    arrayToCheck.Add(massSpectrum1[i].MZ);
                    ended = false;
                }
                else
                    arrayToCheck.Add(double.MaxValue);

                if (j < massSpectrum2.Count)
                {
                    arrayToCheck.Add(massSpectrum2[j].MZ);
                    ended = false;
                }
                else
                    arrayToCheck.Add(double.MaxValue);

                if (k < massSpectrum3.Count)
                {
                    arrayToCheck.Add(massSpectrum3[k].MZ);
                    ended = false;
                }
                else
                    arrayToCheck.Add(double.MaxValue);

                if (ended)
                    break;
                var smallest = arrayToCheck.IndexOf(arrayToCheck.Min());

                switch (smallest)
                {
                    case 0:
                        smallestMzPeakMZ = massSpectrum1[i].MZ;
                        smallestMzPeakIntensity = massSpectrum1[i].Intensity;
                        i += 1;
                        break;
                    case 1:
                        smallestMzPeakMZ = massSpectrum2[j].MZ;
                        smallestMzPeakIntensity = massSpectrum2[j].Intensity;
                        j += 1;
                        break;
                    case 2:
                        smallestMzPeakMZ = massSpectrum3[k].MZ;
                        smallestMzPeakIntensity = massSpectrum3[k].Intensity;
                        k += 1;
                        break;
                }

                if (smallestMzPeakMZ - prevMzPeakMZ < mzTolerance)
                {
                    intensities[intensities.Count - 1] += smallestMzPeakIntensity / 3;
                    numPeaksCombinedHere += 1;
                    numTimesCombinedThis += 1;
                    if (numTimesCombinedThis > 3)
                        Console.WriteLine(numTimesCombinedThis);
                }
                else
                {
                    mzs.Add(smallestMzPeakMZ);
                    intensities.Add(smallestMzPeakIntensity / 3);
                    numPeaks += 1;
                    numTimesCombinedThis = 1;
                }
                prevMzPeakMZ = smallestMzPeakMZ;
            }
            //Console.WriteLine("i = " + i + " j = " + j + " k = " + k);
            //Console.WriteLine("numTimesCombined = " + numTimesCombined + " numPeaks = " + numPeaks);

            DefaultMzSpectrum yeah = new DefaultMzSpectrum(mzs.ToArray(), intensities.ToArray(), false);

            return yeah;
        }
    }
}
