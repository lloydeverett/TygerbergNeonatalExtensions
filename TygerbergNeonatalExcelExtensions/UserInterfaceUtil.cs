using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TygerbergNeonatalAddin
{
    class UserInterfaceUtil
    {
        public static string TextBoxContentFromValues(IEnumerable<string> values)
        {
            string ret = "";
            foreach (string value in values)
            {
                if (value == "=BLANK=") throw new ArgumentException();

                if (value == "")
                {
                    ret += "=BLANK=";
                }
                else
                {
                    ret += value;
                }
                ret += "\r\n";
            }
            return ret;
        }

        public static List<string> ValuesFromTextBoxContent(string contents)
        {
            List<string> ret = new List<string>();
            foreach (string line in contents.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries))
            {
                if (line == "=BLANK=")
                {
                    ret.Add("");
                }
                else
                {
                    ret.Add(line);
                }
            }
            return ret;
        }

    }
}
