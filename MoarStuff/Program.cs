using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace MoarStuff
{
    class Program
    {
        static void Main(string[] args)
        {
            StringBuilder finalProperties = new StringBuilder();
            List<string> propertyNames = new List<string>();

            propertyNames.Add("kasseSoll");
            propertyNames.Add("kasseHaben");

            propertyNames.Add("postSoll");
            propertyNames.Add("postHaben");

            foreach (string propName in propertyNames)
            {
                finalProperties.AppendLine(string.Format(@"private List<KontoDataGridEntry> _{0};
public ObservableCollection<KontoDataGridEntry> {1}
{{
    get {{ return _{0}; }}
    set
    {{
        SetProperty(ref _{0}, value);
    }}
}}
", propName.First().ToString().ToUpper() + propName.Substring(1), propName));
            }

            Clipboard.SetText(finalProperties.ToString());

            Console.ReadKey();
        }
    }
}
