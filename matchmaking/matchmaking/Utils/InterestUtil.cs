using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace matchmaking.Utils
{
    internal class InterestUtil
    {
        private string interestsFile;
        private List<String> interests;
       
        public InterestUtil()
        {
            interests = new List<String>();
            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            interestsFile = Path.Combine(baseDirectory, "interests.txt");

            if (File.Exists(interestsFile))
            {
                String allLines = File.ReadAllText(interestsFile);
                interests = allLines.Split(',').Select(i=>i.Trim().ToLower()).Where(i=>!string.IsNullOrEmpty(i)).ToList();
            }
            else
            {
                throw new Exception("The file doesn't exist");
            }
        }

        public List<String> GetAll()
        {
            return interests;
        }

    }
}
