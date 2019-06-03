using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TGC.Group.VertexMovement
{
    class TimeManager
    {
        public float sum_elapsed = 0f;
        public int counter_elapsed = 0;
        public float medium_elapsed = 0f;

        public void update(float ElapsedTime)
        {
            counter_elapsed++;
            sum_elapsed += ElapsedTime;
            medium_elapsed = sum_elapsed / counter_elapsed;
            if (counter_elapsed >= float.MaxValue / 2 | sum_elapsed >= float.MaxValue)
            {
                sum_elapsed = 0f;
                counter_elapsed = 0;
            }
        }
    }
}
