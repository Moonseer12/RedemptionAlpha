using ParticleLibrary.Core.V3.Particles;
using ParticleLibrary.Utilities;
using Redemption.Globals;
using Terraria;
using SystemVector2 = System.Numerics.Vector2;

namespace Redemption.Particles
{
    public class SharpParticleBehavior : Behavior<ParticleInfo>
    {
        public override string Texture { get; } = "Redemption/Particles/PixelCircle";
        public override void Update(ref ParticleInfo info)
        {
            float dec = info.Data[0];
            float ex = info.Data[1];
            float grav = info.Data[2];

            float progress = info.Time / (float)info.Duration;
            float ScaleX = info.InitialScale.X * ex * (0.5f + 0.5f * EaseFunction.EaseCubicOut.Ease(progress));

            info.Scale = new SystemVector2(ScaleX, info.InitialScale.Y);

            float Opacity = Utils.GetLerpValue(0, 10, info.Time, true);
            info.Color = info.InitialColor * Opacity;

            info.Velocity *= dec;
            info.Velocity.Y += grav;
            info.Position += info.Velocity;
            info.Rotation = info.Velocity.ToRotation();
            info.Time--;
        }
    }
}