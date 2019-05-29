using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TGC.Core.Mathematica;

namespace TGC.Group.VertexMovement
{
    public class VertexMovementManager
    {
        TGCVector3 position;
        TGCVector3 target;
        float MOVEMENT_SPEED;

        public List<TGCVector3> vertex_pool { get; set; }
        List<TGCVector3> permanent_pool { get; set; }

        public VertexMovementManager (TGCVector3 pos, TGCVector3 tar, float SPEED)
        {
            position = pos;
            target = tar;
            MOVEMENT_SPEED = SPEED;
            vertex_pool = new List<TGCVector3>();
            permanent_pool = new List<TGCVector3>();
        }

        public void init()
        {
            target = findNextTarget(vertex_pool);
        }

        public TGCVector3 update (float ElapsedTime)
        {
            TGCVector3 shipMovement = TGCVector3.Empty;
            var shipOriginalPos = position;

            shipMovement = new TGCVector3(target);
            shipMovement.Subtract(shipOriginalPos);

            if (shipMovement.Length() < (MOVEMENT_SPEED * ElapsedTime))
            {
                shipMovement = new TGCVector3(target);
                shipMovement.Subtract(shipOriginalPos);
            }
            else
            {
                shipMovement.Normalize();
                shipMovement.Multiply(MOVEMENT_SPEED * ElapsedTime);

            }
            //test_hit.Position = getPositionAtMiliseconds(1000);
            if (shipMovement.Length() < (MOVEMENT_SPEED * ElapsedTime) / 2)
            {
                target = findNextTarget(vertex_pool);
            }

            return shipMovement;
        }

        public TGCVector3 findNextTarget(List<TGCVector3> pool)
        {
            TGCVector3 current_target = target;
            pool.Remove(target);

            float distance = -1f;
            TGCVector3 new_target = new TGCVector3();

            if (pool.Count > 0)
            {
                for (int i = 0; i < pool.Count; i++)
                {
                    TGCVector3 this_vertex = pool[i];
                    this_vertex.Subtract(current_target);
                    float this_distance = this_vertex.Length();

                    if (distance < 0)
                    {
                        distance = this_distance;
                        new_target = pool[i];
                    }
                    else if (this_distance < distance)
                    {
                        distance = this_distance;
                        new_target = pool[i];
                    }
                }
            }
            else
            {
                new_target = current_target;
            }
            target = new_target;

            return target;
        }

        public void addVertexCollection(TGCVector3[] vertex_collection, TGCVector3 offset)
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
