using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PathTracer
{
    class OrenNayar : BxDF
    {
        private Spectrum kd;
        private double roughness2; // valovitost od 0 naprej...

        public OrenNayar(Spectrum r, double roughness)
        {
            kd = r;
            roughness2 = roughness * (Math.PI / 180);
            roughness2 = roughness2 * roughness2;
        }

        public override Spectrum f(Vector3 wo, Vector3 wi)
        {
            double a = 1 - ((roughness2 / 2 * (roughness2 + 0.33)));
            double b = 0.45 * roughness2 / (roughness2 + 0.09);
            
            double sinThetaI = Utils.SinTheta(wi);
            double sinThetaO = Utils.SinTheta(wo);

            double sinAlpha;
            double tanBeta;
            if (Utils.AbsCosTheta(wi) > Utils.AbsCosTheta(wo))
            {
                sinAlpha = sinThetaO;
                tanBeta = sinThetaI / Utils.AbsCosTheta(wi);
            }
            else
            {
                sinAlpha = sinThetaI;
                tanBeta = sinThetaO / Utils.AbsCosTheta(wo);
            }
            double maxCos = 0;
            if (sinThetaI > 1e-4 && sinThetaO > 1e-4)
            {
                double dCos = Utils.CosPhi(wi) * Utils.CosPhi(wo) + Utils.SinPhi(wi) * Utils.SinPhi(wo);
                maxCos = Math.Max(0, dCos);
            }

            Spectrum nnn = (kd / Math.PI) * (a + b * maxCos * sinAlpha * tanBeta);

            return nnn;
        }

        public override (Spectrum, Vector3, double) Sample_f(Vector3 wo)
        {
            //var wi = Samplers.CosineSampleHemisphere();
            var wi = Samplers.UniformSampleSphere();
            if (wo.z < 0)
                wi.z *= -1;
            double pdf = Pdf(wo, wi);
            return (f(wo, wi), wi, pdf);
        }

        public override double Pdf(Vector3 wo, Vector3 wi)
        {
            if (!Utils.SameHemisphere(wo, wi))
                return 0;

            return Math.Abs(wi.z) * Utils.PiInv; // wi.z == cosTheta
        }
    }
}
