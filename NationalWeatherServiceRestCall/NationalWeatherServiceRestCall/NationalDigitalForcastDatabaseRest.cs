using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace NationalWeatherServiceRestCall
{
    class NationalDigitalForcastDatabaseRest
    {
        
        //The NDFD currently updates on the 45 of every hour. Caching the data locally to return to the user
        //will speed up the process. Caching to local disk for this example though other solutions would be used in Prod
        
        private int zip;

        private readonly string fileDirectory = "Cache";
        private readonly string logDirectory = "Logs";
        private readonly int cacheResetTimehr = -1;

        public void SetZip(int zipToSet)
        {
            //Set Zip
            zip = zipToSet;
        }

        private string CallWebService(gov.weather.graphical.weatherParametersType weatherParameters)
        {
            try
            {
                gov.weather.graphical.ndfdXML ndfdClient = new gov.weather.graphical.ndfdXML();
                string latlonglist = GetLatLongListFromZip();
                if (latlonglist == ",") { return null; }
                string dwmlReturn = ndfdClient.NDFDgenLatLonList(latlonglist,
                                                               gov.weather.graphical.productType.timeseries,
                                                               DateTime.Today,
                                                               DateTime.Today.AddDays(7),
                                                               gov.weather.graphical.unitType.e,
                                                               weatherParameters);
                return dwmlReturn;
            }
            catch (Exception e)
            {
                NdfdErrorHandler(e);
                return null;
            }
        }

        private string GetLatLongListFromZip()
        {
            try
            {
                gov.weather.graphical.ndfdXML ndfdClient = new gov.weather.graphical.ndfdXML();
                string latlon = ndfdClient.LatLonListZipCode(zip.ToString());
                var latLonXElement = XElement.Parse(latlon);
                var latLonNode = latLonXElement.Descendants().Where(n => n.Name == "latLonList").FirstOrDefault();
                return latLonNode.Value;
            }
            catch (Exception e)
            {
                NdfdErrorHandler(e);
                return null;
            }
        }

        public int[] GetForcastedHighs()
        {
            string dwmlReturn;
            //Cache by Zip and Process 
            if (!IsCached("ForcasedHighs"))
            {   
                gov.weather.graphical.weatherParametersType weatherParameters = new gov.weather.graphical.weatherParametersType();
                //Set Weather Parameters to get max temp.
                weatherParameters.maxt = true;
                //Call web service
                dwmlReturn = CallWebService(weatherParameters);
                if (dwmlReturn == null)
                {
                    return null;
                }
                CacheXML(dwmlReturn, "ForcasedHighs");
            }
            else
            {
                dwmlReturn = GetCachedXMLbyProcess("ForcasedHighs");
                if (dwmlReturn == null)
                {
                    return null;
                }
            }
            var temperatureXElement = XElement.Parse(dwmlReturn);
            var maxTemps = temperatureXElement.Descendants("temperature")
                            .Where(e => (string)e.Element("name") == "Daily Maximum Temperature")
                            .SelectMany(e => e.Elements("value"))
                            .Select(e => (int)e);

            //Return Array
            return maxTemps.ToArray();
        }

        private void CacheXML(string DWMLReturn, string Process)
        {
            string fileName = fileDirectory + "\\" + Process + "_" + zip;
            try
            {
                if (!Directory.Exists(fileDirectory))
                {
                    Directory.CreateDirectory(fileDirectory);
                }
                if (File.Exists(fileName))
                {
                    File.Delete(fileName);
                }
                File.WriteAllText(fileName, DWMLReturn);
            }
            catch (Exception e)
            {
                NdfdErrorHandler(e);
            }
        }

        private bool IsCached(string Process)
        {
            string fileName = fileDirectory + "\\" + Process + "_" + zip;
            try
            {
                //Check Cache to see if xml Cached
                if (File.Exists(fileName) && File.GetCreationTime(fileName) >= DateTime.Now.AddHours(cacheResetTimehr))
                {
                    return true;
                }
                return false;
            }
            catch (Exception e)
            {
                NdfdErrorHandler(e);
                return false;
            }
        }

        private string GetCachedXMLbyProcess(string Process)
        {
            try
            {
                string fileName = fileDirectory + "\\" + Process + "_" + zip;
                return File.ReadAllText(fileName);
            }
            catch (Exception e)
            {
                NdfdErrorHandler(e);
                return null;
            }
        }

        public void CleanCache()
        {
            try
            {
                //Clean Cache Here
                DateTime oneHourAgo = DateTime.Now.AddHours(cacheResetTimehr);

                foreach (string Cachefile in Directory.GetFiles(fileDirectory))
                {
                    if (File.GetCreationTime(Cachefile) < oneHourAgo)
                    {
                        File.Delete(Cachefile);
                    }
                }
            }
            catch (Exception e)
            {
                NdfdErrorHandler(e);
            }
        }

        private void NdfdErrorHandler(Exception exception)
        {
            //If used in production log file handling would need to be added
            string logFileName = logDirectory + "\\NDFDLogFile";
            try
            {
                if (!Directory.Exists(logDirectory))
                {
                    Directory.CreateDirectory(logDirectory);
                }
                if (File.Exists(logFileName))
                {
                    File.Delete(logFileName);
                    File.AppendAllText(logFileName, exception.Message + "\n" + exception.StackTrace);
                }
                else
                {
                    File.WriteAllText(logFileName, exception.Message + "\n" + exception.StackTrace);
                }
            }
            catch //(Exception e)
            {
                //Exit otherwise you are in a infinate loop.
               
            }
        }
    }
}
