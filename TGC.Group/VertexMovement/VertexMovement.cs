using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TGC.Core.Mathematica;

namespace TGC.Group.VertexMovement
{
    public class VertexMovement
    {
        public void addVertexCollection(TGCVector3[] vertex_collection, TGCVector3 offset, List<TGCVector3>  vertex_pool, List<TGCVector3> permanent_pool)
        {
            for (int i = 0; i < vertex_collection.Length; i++)
            {
                Boolean present = false;
                TGCVector3 v = vertex_collection[i];
                v.Add(offset);

                for (int j = 0; j < vertex_pool.Count; j++)
                {
                    TGCVector3 comparator = vertex_pool[j];
                    if (comparator.X == v.X && comparator.Y == v.Y && comparator.Z == v.Z)
                    {
                        present = true;
                    }
                }

                if (!present)
                {
                    vertex_pool.Add(v);
                    permanent_pool.Add(v);
                }
            }
        }
    }
}
