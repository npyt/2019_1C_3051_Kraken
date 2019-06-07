using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TGC.Core.Camara;
using TGC.Core.Example;
using TGC.Core.Input;
using TGC.Group.Camara;
using TGC.Group.Model;

namespace TGC.Group.StateMachine
{
    abstract class State
    {
        protected GameModel parent;

        public State(GameModel mparent)
        {
            parent = mparent;
        }

        public abstract void init();
        public abstract void update(float ElapsedTime);
        public abstract void render(float ElapsedTime);
        public abstract void dispose();
    }
}
