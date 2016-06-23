using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PickemTest
{
    public class Prediction_ResultWrapper
    {
        public Prediction_Result result { get; set; }
    }

    public class Prediction_Result
    {
        public List<Prediction_Pick> picks { get; set; }
    }

    public class Prediction_Pick
    {
        public int groupid { get; set; }
        public int index { get; set; }
        public int pick { get; set; }
    }
}
