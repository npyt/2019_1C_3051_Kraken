using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TGC.Core.Geometry;

namespace TGC.Group.VertexMovement
{
    class PowerBox : TGCBox
    {

        public int miliseconds_in_path;

        public PowerBox(int miliseconds) : base()
        {
            miliseconds_in_path = miliseconds;
        }
    }
}
