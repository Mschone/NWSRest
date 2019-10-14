using System;

namespace NationalWeatherServiceRestCall
{
    class Program
    {
        static void Main(string[] args)
        {
            NationalDigitalForcastDatabaseRest ndfdGetForcast = new NationalDigitalForcastDatabaseRest();
            string stringZip;
            int zip;    
            if (args.Length>0 && args[0] == "help")
            {
                Console.WriteLine("This is the NDFD Rest Call Test Program.\n"+
                                  "Usage:\n"+
                                  "NDFDTest - This will enter into a loop so you can enter a zip\n"+
                                  "NDFDTest [Zip] - Returns the data for one zip and then exit\n"+
                                  "NDFDTest help - Returns this help message\n" +
                                  "****Note do not include routing information after the zip.");
            }
            else
            {
                //Test Cases
                if (args.Length == 0)
                {
                    Console.Write("Please Enter a ZipCode - ");
                    stringZip = Console.ReadLine();
                }
                else
                {
                    stringZip = args[0];
                }
                if (Int32.TryParse(stringZip, out zip) && stringZip.Length == 5)
                {
                    ndfdGetForcast.SetZip(zip);
                    int[] forcastedHighs = ndfdGetForcast.GetForcastedHighs();
                    if (forcastedHighs != null)
                    {
                        int dayCounter = 0;
                        foreach (int high in forcastedHighs)
                        {
                            Console.WriteLine(DateTime.Today.AddDays(dayCounter).ToString("MM-dd-yyyy") + " " + high.ToString());
                            dayCounter++;
                        }
                    }
                    else { Console.WriteLine("Ivalid Zip - Try again. Use the help option for more information."); }
                }
                else
                {
                    Console.WriteLine("Ivalid Zip - Try again. Use the help option for more information.");
                }
            }
        }
    }
}
