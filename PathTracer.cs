using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static PathTracer.Samplers;

namespace PathTracer
{
    class PathTracer
    {
        public Spectrum Li(Ray ray, Scene s)
        {
            var L = Spectrum.ZeroSpectrum;
            var beta = Spectrum.Create(1.0);

            for (int nBounces = 0; nBounces < 20; nBounces++)
            {
                (double? d, SurfaceInteraction si) = s.Intersect(ray);

                if (d == null) {
                    break;
                }

                Vector3 wo = -ray.d;

                if (si.Obj is Light) {
                    if (nBounces == 0)
                    {
                        L.AddTo(beta * si.Le(wo));
                    }
                    break;
                }
                    
                L.AddTo(beta * Light.UniformSampleOneLight(si, s));

                (Spectrum f, Vector3 wiW, double pdf, bool bxdfIsSpecular) = ((Shape)si.Obj).BSDF.Sample_f(wo, si);
                
                if(f.IsBlack()) break;

                var wi = si.SpawnRay(wiW);

                beta = beta * f * Vector3.AbsDot(wiW, si.Normal) / pdf;
                ray = wi;

                if (nBounces > 3)
                {
                    double q = 1 - beta.Max();
                    if(ThreadSafeRandom.NextDouble() < q)
                    {
                        break;
                    }
                    beta = beta / (1 - q);
                }
            }

            return L;
        }
    }
}
