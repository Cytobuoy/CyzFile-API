using CytoSense.Data;
using CytoSense.CytoSettings;
using CytoSense.Data.ParticleHandling;
using CytoSense.Data.ParticleHandling.Channel;
using System.Collections.Generic;
using OpenCvSharp;

namespace CyzFileDump
{
    /*
     * This is an example program on how to use the CyzFile library to extract information from
     * CytoBuoy .cyz datafiles.
     * 
     * See http://www.cytobuoy.com/ for more information on CytoSense instruments.
     * 
     * This is just a quick demonstration to get you started, it is by no means a complete and production
     * ready piece of software.
     * 
     */
    internal class Program
    {
        static void Main(string[] args)
        {

            ParseArguments(args, out bool displayHelp, out bool displayInfo, out bool displayParticles, out bool exportImages, out string filename);

            if (displayHelp || ! (displayInfo||displayParticles||exportImages) || String.IsNullOrEmpty(filename) )
            {
                DisplayHelpInformation();
                return;
            }


            DataFileWrapper dfw = new DataFileWrapper(filename);
            dfw.CytoSettings.setChannelVisualisationMode(ChannelAccessMode.Optical_debugging);
            ChannelData.VarLength = 13; // Calculate the alternate variable height length parameter at a height of 13%


            // Some old files can have a problem where the pre-concentration measurement and the actual
            // concentration during the measurement disagree.  If this happens and you try to use
            // the data this will result in an exception.  You can check this before hand, and force the
            // use of one of the concentrations during the calculations.
            // In this case usually the pre-concentration is the correct one, but you can let the user
            // choose which one.
            double concentration = 0.0;
            double preconcentration = 0.0;
            if (dfw.CheckConcentration(ref concentration, ref preconcentration))
            {
                Console.WriteLine("CONCENTRATION MISMATCH!");
                Console.WriteLine($"Pre-Concentration: ${preconcentration}, Concentration: ${concentration}");
                Console.Write($"Would you like to use the PRE concentration instead of the concentration(Y/N)? :");
                char c =  Console.ReadKey().KeyChar;
                if (c =='y' || c=='Y')
                    dfw.ConcentrationMode = ConcentrationModeEnum.Pre_measurement_FTDI;
                else
                    dfw.ConcentrationMode = ConcentrationModeEnum.During_measurement_FTDI;
                Console.WriteLine();
            }

            Console.WriteLine($"CyzDumpFile -- Exporting data from '{filename}'");

            if (displayInfo)
                DumpGeneralInformation(dfw);

            if (displayParticles)
            { 
                if (dfw.SplittedParticles.Length > 0)
                    DumpCompleteParticle(dfw.SplittedParticles[0]);
                DumpParticleInformation(dfw);
            }

            if (exportImages)
            { 
                Console.WriteLine($"Exporting background image");
                Cv2.ImWrite("background_image.jpg", dfw.CytoSettings.iif.OpenCvBackground);
                ExportImages(dfw);
                ExportCroppedImages(dfw);
            }
        }

        /// <summary>
        /// Write some generic infomation on the datafile to the console.  This is by no means exhaustive, just to give you an idea of
        /// the kind of information that is present in the file, and how to access it.
        /// </summary>
        /// <param name="dfw"></param>
        private static void DumpGeneralInformation( DataFileWrapper dfw )
        {
            Console.WriteLine("----------------------------------------------------------------------------");
            Console.WriteLine($"Datafile name:  {dfw.rawDataFile.path}");

            Console.WriteLine($"Instrument Information:");
            Console.WriteLine($"    Name:               {dfw.CytoSettings.name}");
            Console.WriteLine($"    Serial Number:      {dfw.CytoSettings.SerialNumber}");
            Console.WriteLine($"    Number of Channels: {dfw.CytoSettings.ChannelList.Count}");
            int numChannels = dfw.CytoSettings.ChannelList.Count;
            for (int chIdx=0; chIdx<numChannels; chIdx++ ) 
                Console.WriteLine($"        Chan {chIdx}:         {dfw.CytoSettings.ChannelList[chIdx].Description}");

                
            Console.WriteLine($"Measurement Settings:");
            Console.WriteLine($"    Name:              {dfw.MeasurementSettings.TabName}");
            Console.WriteLine($"    Duration:          {dfw.MeasurementSettings.StopafterTimertext} (seconds)");
            Console.WriteLine($"    Samplepump Speed:  {dfw.MeasurementSettings.ConfiguredSamplePompSpeed} (muL/s)");
            Console.WriteLine($"    Trigger Channel:   {dfw.MeasurementSettings.TriggerChannel}");
            Console.WriteLine($"    Trigger Level:     {dfw.MeasurementSettings.TriggerLevel1e}");
            Console.WriteLine($"    Use Smart Trigger: {dfw.MeasurementSettings.SmartTriggeringEnabled}");
            if (dfw.MeasurementSettings.SmartTriggeringEnabled)
                Console.WriteLine($"    Smart Trigger:     {dfw.MeasurementSettings.SmartTriggerSettingDescription}");
            Console.WriteLine($"    Take Images:       {dfw.MeasurementSettings.IIFCheck}");


            Console.WriteLine($"Measurement Results:");
            Console.WriteLine($"    Start:                        {dfw.MeasurementInfo.MeasurementStart}");
            Console.WriteLine($"    Duration:                     {dfw.MeasurementInfo.ActualMeasureTime}");
            Console.WriteLine($"    Number of Counted Particles:  {dfw.MeasurementInfo.NumberofCountedParticles}");
            Console.WriteLine($"    Number of Particles in File:  {dfw.MeasurementInfo.NumberofSavedParticles}");
            Console.WriteLine($"    Number of Pictures:           {dfw.MeasurementInfo.NumberOfPictures}");
            Console.WriteLine($"    Pumped Volume:                {dfw.pumpedVolume} (muL)");
            Console.WriteLine($"    Analyzed Volume:              {dfw.analyzedVolume} (muL)");
            Console.WriteLine($"    Particle Concentration:       {dfw.Concentration} (n/muL)");

            Console.WriteLine($"Auxiliary Sensor Data:");
            Console.WriteLine($"    Average System Temperature:    {dfw.MeasurementInfo.SystemTemp} (C)");
            Console.WriteLine($"    Average Sheath Temperature:    {dfw.MeasurementInfo.SheathTemp} (C)");
            Console.WriteLine($"    Average Absolute Pressure:     {dfw.MeasurementInfo.ABSPressure} (mbar)");
            Console.WriteLine($"    Average Differential Pressure: {dfw.MeasurementInfo.DiffPressure} (mbar)");

            // etc.
        }

        /// <summary>
        /// Write some per particle information to the console.
        /// </summary>
        /// <param name="dfw"></param>
        /// <remarks>
        /// To keep the console output somewhat manageable, I abort after the first 1000 particles.
        /// Also I only show a few per particle parameters.
        /// Depending on the actual underlying file format, the particles get loaded only when accessing the SplittedParticles array, so accessing
        /// the first one can take a long time.
        /// </remarks>
        private static void DumpParticleInformation( DataFileWrapper dfw )
        {
            int numParticles = dfw.SplittedParticles.Length;
            for(int pIdx=0; pIdx<numParticles; ++pIdx)
            {
                if (pIdx >= 100)
                    break;
                var p = dfw.SplittedParticles[pIdx];
                Console.WriteLine($"{pIdx,3}: #samples={p.ChannelData[0].Data.Length}, " + 
                                  $"length={p.getChannelByType(ChannelTypesEnum.FWS).get_Parameter(ChannelData.ParameterSelector.Length)}, " +
                                  $"total red={p.getChannelByType(ChannelTypesEnum.FLRed).get_Parameter(ChannelData.ParameterSelector.Total)}, " + 
                                  $"image={p.hasImage}");
            }
        }


        /// <summary>
        /// Dump all available information for one particle.
        /// </summary>
        /// <param name="p"></param>
        private static void DumpCompleteParticle( Particle p)
        {
            Console.WriteLine($"Particle ID: {p.ID}");

            Console.WriteLine($"Pulse Shapes:");
            foreach( ChannelData cd in p.ChannelData)
            {
                string[] values = cd.Data.Select((v) => v.ToString(System.Globalization.CultureInfo.InvariantCulture)).ToArray();
                Console.WriteLine($"    - {cd.Information.Description}: {String.Join(',', values)}");
            }

            Console.WriteLine($"Parameter Values:");
            foreach( ChannelData cd in p.ChannelData)
            {
                Console.WriteLine($"    - {cd.Information.Description}:");
                Console.WriteLine($"        + {ChannelData.ParameterNames[(int)ChannelData.ParameterSelector.Length]}: {cd.get_Parameter(ChannelData.ParameterSelector.Length)}");
                Console.WriteLine($"        + {ChannelData.ParameterNames[(int)ChannelData.ParameterSelector.Total]}: {cd.get_Parameter(ChannelData.ParameterSelector.Total)}");
                Console.WriteLine($"        + {ChannelData.ParameterNames[(int)ChannelData.ParameterSelector.Maximum]}: {cd.get_Parameter(ChannelData.ParameterSelector.Maximum)}");
                Console.WriteLine($"        + {ChannelData.ParameterNames[(int)ChannelData.ParameterSelector.Average]}: {cd.get_Parameter(ChannelData.ParameterSelector.Average)}");
                Console.WriteLine($"        + {ChannelData.ParameterNames[(int)ChannelData.ParameterSelector.Inertia]}: {cd.get_Parameter(ChannelData.ParameterSelector.Inertia)}");
                Console.WriteLine($"        + {ChannelData.ParameterNames[(int)ChannelData.ParameterSelector.CentreOfGravity]}: {cd.get_Parameter(ChannelData.ParameterSelector.CentreOfGravity)}");
                Console.WriteLine($"        + {ChannelData.ParameterNames[(int)ChannelData.ParameterSelector.FillFactor]}: {cd.get_Parameter(ChannelData.ParameterSelector.FillFactor)}");
                Console.WriteLine($"        + {ChannelData.ParameterNames[(int)ChannelData.ParameterSelector.Asymmetry ]}: {cd.get_Parameter(ChannelData.ParameterSelector.Asymmetry )}");
                Console.WriteLine($"        + {ChannelData.ParameterNames[(int)ChannelData.ParameterSelector.NumberOfCells ]}: {cd.get_Parameter(ChannelData.ParameterSelector.NumberOfCells )}");
                Console.WriteLine($"        + {ChannelData.ParameterNames[(int)ChannelData.ParameterSelector.SampleLength]}: {cd.get_Parameter(ChannelData.ParameterSelector.SampleLength)}");
                Console.WriteLine($"        + {ChannelData.ParameterNames[(int)ChannelData.ParameterSelector.TimeOfArrival]}: {cd.get_Parameter(ChannelData.ParameterSelector.TimeOfArrival)}");
                Console.WriteLine($"        + {ChannelData.ParameterNames[(int)ChannelData.ParameterSelector.First]}: {cd.get_Parameter(ChannelData.ParameterSelector.First)}");
                Console.WriteLine($"        + {ChannelData.ParameterNames[(int)ChannelData.ParameterSelector.Last]}: {cd.get_Parameter(ChannelData.ParameterSelector.Last)}");
                Console.WriteLine($"        + {ChannelData.ParameterNames[(int)ChannelData.ParameterSelector.Minimum]}: {cd.get_Parameter(ChannelData.ParameterSelector.Minimum)}");
                Console.WriteLine($"        + {ChannelData.ParameterNames[(int)ChannelData.ParameterSelector.SWSCOV ]}: {cd.get_Parameter(ChannelData.ParameterSelector.SWSCOV )}");
                Console.WriteLine($"        + {ChannelData.ParameterNames[(int)ChannelData.ParameterSelector.VariableLength ]}: {cd.get_Parameter(ChannelData.ParameterSelector.VariableLength)}");
            }
        }



        /// <summary>
        /// Export all images to jpg files in the current folder.
        /// For now we export the raw images, not the automatically cropped ones.
        /// </summary>
        /// <param name="dfw"></param>
        /// <remarks>
        /// To save some time we only export the first 100 in the demo.</remarks>
        private static void ExportImages( DataFileWrapper dfw )
        {
            int numImagedParticles = dfw.SplittedParticlesWithImages.Length;
            for( int imgIdx=0; imgIdx<numImagedParticles;++imgIdx)
            {
                if (imgIdx >= 100)
                    break;

                var imgP = dfw.SplittedParticlesWithImages[imgIdx];
                string imgName = $"particle_{imgP.ID}.jpg";
                Console.WriteLine($"{imgIdx,2}: Writing {imgName}");
                using (FileStream fileStream = new FileStream(imgName, FileMode.Create, FileAccess.Write) )
                {
                    imgP.ImageHandling.ImageStream.Position = 0;
                    imgP.ImageHandling.ImageStream.CopyTo( fileStream );
                }
            }
        }

        /// <summary>
        /// Besides the complete stored image, our CytoClus also has the option to extract cropped images
        /// with just the object.  Often the image is much larger then the particle in it, and cropping it
        /// will make it much clearer what is in the image.
        /// 
        /// The process and the parameters available for the object detection are described in the
        /// CytoClus documentation, so I will not repeat that here.  I will just use default settings.
        /// </summary>
        /// <param name="dfw"></param>
        /// <remarks>The object detection and cropping is implemented using the OpenCV library, and
        /// the results of the cropping function are OpenCV image objects.  Therefore we also
        /// use OpenCV functions to save them to a file.</remarks>
        private static void ExportCroppedImages( DataFileWrapper dfw )
        {
            int numImagedParticles = dfw.SplittedParticlesWithImages.Length;
            for( int imgIdx=0; imgIdx<numImagedParticles;++imgIdx)
            {
                if (imgIdx >= 100)
                    break;

                var imgP = dfw.SplittedParticlesWithImages[imgIdx];
                string imgName = $"particle_{imgP.ID}_cropped.jpg";

                var crpImg = imgP.ImageHandling.GetCroppedImage(25, 1.1, 7, 1);
                if (imgP.ImageHandling.CropResult == CytoImage.CropResultEnum.CropOK)
                { 
                    Console.WriteLine($"{imgIdx,2}: Writing {imgName}");
                    Cv2.ImWrite(imgName, crpImg);
                }
                else // there was a problem cropping the image, examine the result enum to see what the problem was.
                {
                    Console.WriteLine($"{imgIdx,2}: Cropping failed ('{imgP.ImageHandling.CropResult}')");
                }
            }
        }


        /// <summary>
        /// Scan the arguments to get the users commandline options and the filename to operate on.
        /// </summary>
        /// <param name="args"></param>
        /// <param name="displayHelp"></param>
        /// <param name="displayInfo"></param>
        /// <param name="displayParticles"></param>
        /// <param name="exportImages"></param>
        /// <param name="filename"></param>
        /// <remarks>Function handles the happy flow, if you double switches, pass extra switches or multiple filenames this is all silently ignored.</remarks>
        private static void ParseArguments( string[] args, out bool displayHelp, out bool displayInfo, out bool displayParticles, out bool exportImages, out string filename)
        {
            displayHelp      = false;
            displayInfo      = false;
            displayParticles = false;
            exportImages     = false;
            filename         = "";

            for(int i =0; i < args.Length; i++) 
            {
                string arg = args[i].Trim();
                if (arg.StartsWith("-"))
                {
                    if (arg.Contains("h"))
                        displayHelp = true;
                    if (arg.Contains("i"))
                        displayInfo = true;
                    if (arg.Contains("p"))
                        displayParticles = true;
                    if (arg.Contains("m"))
                        exportImages = true;
                }
                else if (arg.ToLower().EndsWith(".cyz"))
                {
                    filename = arg;
                }
            }
        }



        /// <summary>
        ///  Display basic help information
        /// </summary>
        private static void DisplayHelpInformation()
        {
            Console.WriteLine("SYNOPSIS                                                                    ");
            Console.WriteLine("                                                                            ");
            Console.WriteLine("    CyzDumpFile [options] <filename>.cyz                                    ");
            Console.WriteLine("                                                                            ");
            Console.WriteLine("DESCRIPTION                                                                 ");
            Console.WriteLine("                                                                            ");
            Console.WriteLine("    This is an example on using the CyzFile library it is not meant to be   ");
            Console.WriteLine("    be a useful standalone program. It could be the start of one, but it is ");
            Console.WriteLine("    not at the moment.                                                      ");
            Console.WriteLine("                                                                            ");
            Console.WriteLine("    Extract information from the data file and dump it into a file or to the");
            Console.WriteLine("    console.                                                                ");
            Console.WriteLine("                                                                            ");
            Console.WriteLine("OPTIONS                                                                     ");
            Console.WriteLine("                                                                            ");
            Console.WriteLine("    -h   - Display this help screen.                                        ");
            Console.WriteLine("    -i   - Output some basic information.                                   ");
            Console.WriteLine("    -p   - Display particle information (only the first 100).               ");
            Console.WriteLine("    -m   - iMages output the images as JPEG into the current folder.        ");
            Console.WriteLine("                                                                            ");
            Console.WriteLine("                                                                            ");
            Console.WriteLine("EXAMPLES                                                                    ");
            Console.WriteLine("                                                                            ");
            Console.WriteLine(" CyzFileDump -h                                                             ");
            Console.WriteLine(" CyzFileDump -i MyDataFile.cyz                                              ");
            Console.WriteLine(" CyzFileDump -i -p -m MyDataFile.cyz                                        ");
            Console.WriteLine(" CyzFileDump -ip MyDataFile.cyz                                             ");
            Console.WriteLine(" CyzFileDump -ip -m MyDataFile.cyz                                          ");
            Console.WriteLine(" CyzFileDump -ipm MyDataFile.cyz                                            ");
            Console.WriteLine("                                                                            ");
        }

    }
}