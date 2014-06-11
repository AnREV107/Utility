using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml.Serialization;

namespace Utility
{
    class FundStateManager
    {

        public static XmlSerializer outputManager;
        public static String FileLocation;
        public static string fileName = string.Empty;
        public FundStateManager() { }

        static FundStateManager()
        {
           FileLocation = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
          
        }
        public static Boolean saveFundState(List<int> funds)
        {
            if(!FileLocation.Contains(".xml"))
            FileLocation = FileLocation + fileName + ".xml"; 
            try
            {
                if (!File.Exists(@FileLocation))
                    File.Create(@FileLocation);
                using (StreamWriter objectWriter = new StreamWriter(@FileLocation))
                {
                    Fund fundData = new Fund(funds);
                    outputManager = new XmlSerializer(typeof(Fund));
                    outputManager.Serialize(objectWriter, fundData);
                    return true;
                }
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public static List<int> getFundState()
        {
            try
            {
                if (!FileLocation.Contains(".xml"))
                FileLocation = FileLocation + fileName + ".xml"; 
                using (StreamReader objectReader = new StreamReader(@FileLocation))
                {
                    outputManager = new XmlSerializer(typeof(Fund));
                    Fund funds = new Fund();
                    funds = (Fund)outputManager.Deserialize(objectReader);
                    return getFundIDs(funds);
                }
            }
            catch (Exception e)
            {
                return null;
            }
        }

        public static List<int> getFundIDs(Fund funds)
        {
            List<int> fundIDs = new List<int>();
            if(funds!=null)
            {
                foreach(int id in funds.Funds)
                    fundIDs.Add(id);
                return fundIDs;
            }
            else
                return null;
        }

    }
}
