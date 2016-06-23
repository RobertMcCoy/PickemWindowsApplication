using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PickemTest
{
    public class Layout_ResultWrapper
    {
        public Layout_Result result { get; set; }
    }

    public class Layout_Result
    {
        public int @event { get; set; }
        public string name { get; set; }
        public List<Layout_Section> sections { get; set; }
        public List<Layout_Team> teams { get; set; }
    }



    public class Layout_Section
    {
        public int sectionid { get; set; }
        public string name { get; set; }
        public List<Layout_Group> groups { get; set; }
    }

    public class Layout_Team
    {
        public int pickid { get; set; }
        public string name { get; set; }
    }

    public class Layout_Group
    {
        public int groupid { get; set; }
        public string name { get; set; }
        public int points_per_pick { get; set; }
        public bool picks_allowed { get; set; }
        public IList<Layout_Team> teams { get; set; }
        public List<Layout_Pick> picks { get; set; }
    }

    public class Layout_Pick
    {
        public int index { get; set; }
        public IList<int> pickids { get; set; }
    }


}
