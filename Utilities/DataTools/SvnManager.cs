using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Utilities
{
    public class SvnManager
    {
        public string FolderName { get; private set; }

        public SvnManager(string FolderName)
        {
            this.FolderName = FolderName;
        }

        public string GetRevisionNumber()
        {
            return RevisionNumber;
        }

        const string DB = "wc.db";
        const string PATTERN = "/!svn/ver/(?'version'[0-9]*)/";
        private string RevisionNumber
        {
            get
            {
                string SvnSubfolder = FolderName + "\\.svn";

                if (Directory.Exists(SvnSubfolder))
                {
                    int maxVer = int.MinValue;
                    string EntriesFile = Directory.GetFiles(SvnSubfolder, DB).FirstOrDefault();

                    if (!string.IsNullOrEmpty(EntriesFile))
                    {
                        byte[] fileData = File.ReadAllBytes(EntriesFile);
                        string fileDataString = Encoding.Default.GetString(fileData);

                        Regex regex = new Regex(PATTERN);

                        foreach (Match match in regex.Matches(fileDataString))
                        {
                            string version = match.Groups["version"].Value;

                            int curVer;
                            if (int.TryParse(version, out curVer))
                                if (curVer > maxVer)
                                    maxVer = curVer;
                        }

                        if (maxVer > int.MinValue)
                            return maxVer.ToString();
                    }
                }

                return string.Empty;
            }
        }
    }
}
