using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessServices
{
    public static class Extensions
    {
        public static string PolicySuffixZeroFill(this int number)
        {
            return (number < 10) ? "0" + number.ToString() : number.ToString();
        }
    }
}
