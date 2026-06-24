using ParticleLibrary.Core.V3.Particles;
using Terraria;
using SystemVector2 = System.Numerics.Vector2;

namespace Redemption.Particles
{
    public class LaserParticleBehavior : Behavior<ParticleInfo>
    {
        public override string Texture { get; } = "Terraria/Images/Extra_98";
        public override void Initialize(ref ParticleInfo info)
        {
            SystemVector2 scale = new SystemVector2(0.1f * info.InitialScale.X, info.InitialScale.Y);
            info.Scale = scale;
            info.Color = info.InitialColor with { A = 0 };
        }
        public override void Update(ref ParticleInfo info)
        {
            float progress = Utils.GetLerpValue(0, 1, (float)info.Time / info.Duration, true);

            float initialScale = info.Data[0] == 0 ? 0 : Utils.GetLerpValue(0, info.Data[0], info.Duration - info.Time, true);
            float scaleX = 0.1f + 0.1f * initialScale * Utils.GetLerpValue(info.Duration, info.Data[0], info.Duration - info.Time, true);
            SystemVector2 scale = new SystemVector2(scaleX * info.InitialScale.X, info.InitialScale.Y);

            info.Scale = scale;
            info.Color = info.InitialColor with { A = 0 } * progress;

            info.Time--;
        }
    }
}