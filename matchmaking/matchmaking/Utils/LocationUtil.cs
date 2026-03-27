using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Windows.ApplicationModel;

namespace matchmaking.Utils
{
    internal class LocationUtil
    {
        private String locationsFile="locations.csv";
        private Dictionary<String, Tuple<float, float>> locations = new Dictionary<string, Tuple<float, float>>();

        public LocationUtil()
        {
            string root = Package.Current.InstalledLocation.Path;
            string fullPath = Path.Combine(root, "locations.csv");
            System.Diagnostics.Debug.WriteLine("Looking for file at: " + fullPath);

            if (File.Exists(fullPath))
            {
                foreach (var line in File.ReadLines(fullPath).Skip(1)) 
                {
                    var parts = line.Split(',');
                    string county = parts[0];
                    float lat = float.Parse(parts[1], System.Globalization.CultureInfo.InvariantCulture);
                    float lon = float.Parse(parts[2], System.Globalization.CultureInfo.InvariantCulture);
                    locations[county] = new Tuple<float, float>(lat, lon);
                }
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("File NOT found at: " + fullPath);
                throw new FileNotFoundException("The file doesn't exist", fullPath);
            }
        }

        public List<String> GetAllLocations()
        {
            return locations.Keys.ToList();
        }

        public Tuple<float,float> GetCoords(String location)
        {
            if (locations.ContainsKey(location))
            {
                return locations[location];
            }
            return null;
        }
    }
}
