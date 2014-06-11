using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Utility
{
    public class Fund
    {

        public static List<int> _funds;
        public List<int> Funds
	    {
            get { return _funds; }
            set { _funds = value; }
	    }

        public static String _databaseName;
        public String DatabaseName
        {
            get { return _databaseName; }
            set { _databaseName = value; }
        }

        public static String _company;
        public String Company
        {
            get { return _company; }
            set { _company = value; }
        }
        
        public Fund() { }

        public Fund(List<int> funds)
        {
            _funds = funds;
        }
 }
}
