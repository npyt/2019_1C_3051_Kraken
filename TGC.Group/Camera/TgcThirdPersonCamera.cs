using TGC.Core.Camara;
using TGC.Core.Mathematica;

namespace TGC.Group.Camera
{
    public class TgcThirdPersonCamera : TgcCamera
    {
        private TGCVector3 position;

        public TgcThirdPersonCamera()
        {
            resetValues();
        }

        public TgcThirdPersonCamera(TGCVector3 target, float offsetHeight, float offsetForward) : this()
        {
            Target = target;
            OffsetHeight = offsetHeight;
            OffsetForward = offsetForward;
        }

        public TgcThirdPersonCamera(TGCVector3 target, TGCVector3 targetDisplacement, float offsetHeight, float offsetForward)
            : this()
        {
            Target = target;
            TargetDisplacement = targetDisplacement;
            OffsetHeight = offsetHeight;
            OffsetForward = offsetForward;
        }
    }
}
