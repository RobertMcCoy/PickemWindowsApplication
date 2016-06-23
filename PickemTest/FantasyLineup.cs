using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PickemTest
{
    public class FantasyLineup_ResultWrapper
    {
        public FantasyLineup_Result result { get; set; }
    }

    public class FantasyLineup_Result
    {
        public List<FantasyLineup_Teams> teams { get; set; }
    }

    public class FantasyLineup_Teams
    {
        public int sectionid { get; set; }
        public List<int> picks { get; set; }
    }
}
