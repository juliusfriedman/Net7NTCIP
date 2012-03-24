using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Vaisala
{
    public class Message
    {
        DateTime date;
        int unitId;
        string messageId;
        string model;
        Dictionary<int, double> dataItems = new Dictionary<int, double>();
        public Message(byte[] bytes)
        {
            string result = Encoding.ASCII.GetString(bytes);
            string[] parts = result.Split(new char[] {'\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            string[] subparts = parts[0].Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            date = DateTime.Parse(subparts[0]);
            unitId = Convert.ToInt32(subparts[1]);
            messageId = subparts[2];
            model = subparts[3];
            for (int index = 1, end = parts.Length; index < end; index++)
            {
                string[] paramaters = parts[index].Split(new char[]{';'}, StringSplitOptions.RemoveEmptyEntries);
                foreach (string subparams in paramaters)
                {
                    if (subparams == "=") break;//End of command
                    string[] arguments = subparams.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    if (arguments.Length == 1) break;//End of command
                    if (arguments[1] == "/////") continue;//Invalid measurement
                    dataItems.Add(Convert.ToInt32(arguments[0]), Convert.ToDouble(arguments[1]));
                }                
            }

        }
    }
}
