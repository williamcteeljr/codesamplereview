using PolicyTracker.DomainModel.Brokers;
using PolicyTracker.DomainModel.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PolicyTracker.Platform
{
    public static class ORA
    {
        public static Agent DefaultAgent = new Agent() { IndID = 0, FirstName = "UNASSIGNED", LastName = "AGENT", UserEmail = "NoReply@oraero.com" };
        public static IEnumerable<string> WorkersCompPolicyCodes = new[] { "CAD", "CAN", "CAV", "CAW", "CDB", "CAL", "CAR" };

        public class SecurityRole : StringEnum
        {
            public static SecurityRole UW = new SecurityRole() { Value = "Underwriter", DisplayText = "Underwriter" };
            public static SecurityRole UA = new SecurityRole() { Value = "Underwriter Assistant", DisplayText = "Underwriter Assistant" };
            public static SecurityRole ASSC = new SecurityRole() { Value = "Associate", DisplayText = "Claims Associate" };
            public static SecurityRole BM = new SecurityRole() { Value = "Branch Manager", DisplayText = "Branch Manager" };
            public static SecurityRole PLM = new SecurityRole() { Value = "Product Line Manager", DisplayText = "Product Line Manager" };
            public static SecurityRole ADMIN = new SecurityRole() { Value = "Admin", DisplayText = "Administrator" };
            public static SecurityRole EXEC = new SecurityRole() { Value = "Exec", DisplayText = "Execuative" };
            public static SecurityRole CLEAR = new SecurityRole() { Value = "Clearance", DisplayText = "Clearance" };

            private SecurityRole(string val, string text) : base(val, text) { }
            private SecurityRole() { }
        }

        /// <summary>
        /// Contains members and functions used in figuring time spans/periods between dates.
        /// </summary>
        public static class TimePeriod
        {
            public static int LeapYearsBetween(int start, int end)
            {
                //System.Diagnostics.Debug.Assert(start < end);
                return LeapYearsBefore(end) - LeapYearsBefore(start + 1);
            }

            private static int LeapYearsBefore(int year)
            {
                System.Diagnostics.Debug.Assert(year > 0);
                year--;
                return (year / 4) - (year / 100) + (year / 400);
            }
        }
    }
}
