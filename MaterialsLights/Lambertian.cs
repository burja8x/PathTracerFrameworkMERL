using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PathTracer
{
   public class Lambertian : BxDF
  {
    private Spectrum kd;
    public Lambertian(Spectrum r)
    {
      kd = r;
    }

    public override Spectrum f(Vector3 wo, Vector3 wi)
    {
      if (!Utils.SameHemisphere(wo, wi))
        return Spectrum.ZeroSpectrum;
      return kd * Utils.PiInv;
    }

    public override (Spectrum, Vector3, double) Sample_f(Vector3 wo)
    {
      var wi = Samplers.CosineSampleHemisphere();
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
