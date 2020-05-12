using MathNet.Numerics.LinearAlgebra;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PathTracer
{
    public class Merl : BxDF
    {
        const int BRDF_SAMPLING_RES_THETA_H = 90;
        const int BRDF_SAMPLING_RES_THETA_D = 90;
        const int BRDF_SAMPLING_RES_PHI_D = 360 / 2;
        double[] brdf;
        public Merl(double[] brdfs) {
            brdf = brdfs;
        }

        public override Spectrum f(Vector3 wo, Vector3 wi)
        {
            //wo.x = 0.46996647;
            //wo.y = -0.52372897;
            //wo.z = 0.71052057;
            //wi.x = 0.9666774;
            //wi.y = 0.15925728;
            //wi.z = 0.20042926;

            //Console.WriteLine($"wo {wo}");
            //Console.WriteLine($"wi {wi}");

            Vector3 wh = wo + wi; 
            if (wh.z < 0.0)
            {
                wi = -wi;
                wh = -wh;
            }

            if (wh.LengthSquared() == 0.0) // epsilon ?
            {
                return Spectrum.ZeroSpectrum;
            }

            wh = wh.Normalize();
            // Directly compute the rows of the matrix performing the rotation of w_h to(0, 0, 1)
            double theta_h = SphericalTheta(wh);
            double cos_phi_h = Utils.CosPhi(wh);
            double sin_phi_h = Utils.SinPhi(wh);
            double cos_theta_h = Utils.CosTheta(wh);
            double sin_theta_h = Utils.SinTheta(wh);
            Vector3 w_hx = new Vector3(cos_phi_h * cos_theta_h, sin_phi_h * cos_theta_h, -sin_theta_h);
            Vector3 w_hy = new Vector3(-sin_phi_h, cos_phi_h, 0.0);
            Vector3 w_d = new Vector3(Vector3.Dot(wi, w_hx), Vector3.Dot(wi, w_hy), Vector3.Dot(wi, wh));
            double theta_d = SphericalTheta(w_d);
            // Wrap phi_d if needed to keep it in range

            double phi_d = SphericalPhi(w_d);
            if (phi_d > Math.PI) {
                phi_d = phi_d - Math.PI;
            }

            int theta_h_idx = MapIndex(Math.Sqrt(Math.Max(0.0, 2.0 * theta_h / Math.PI)), 1.0, BRDF_SAMPLING_RES_THETA_H); // theta half index
            int theta_d_idx = MapIndex(theta_d, Math.PI / 2.0, BRDF_SAMPLING_RES_THETA_D); // theta diff index
            int phi_d_idx = MapIndex(phi_d, Math.PI, BRDF_SAMPLING_RES_PHI_D); // phi diff index
            int i = phi_d_idx + BRDF_SAMPLING_RES_PHI_D * (theta_d_idx + theta_h_idx * BRDF_SAMPLING_RES_THETA_D);
            
            //Console.WriteLine(i);
            //if (i < brdf.Length)
            //{
            //    Console.WriteLine("EX");
            //    return Spectrum.ZeroSpectrum;
            //}
            //if (3 * i > brdf.Length) {
                
            //    Console.WriteLine("CX");
            //    return Spectrum.ZeroSpectrum;
            //}

            Debug.Assert(i < brdf.Length);

            return Spectrum.Create(Vector<double>.Build.Dense(new[] { brdf[3 * i], brdf[3 * i + 1], brdf[3 * i + 2] }));
            //return Spectrum.ZeroSpectrum;
        }
        private int MapIndex(double val, double max, int nVals) {
            return Clamp((int)(val / max * nVals), 0, nVals - 1);
        }
        private double SphericalTheta(Vector3 v) {
            return Math.Acos(Utils.Clamp(v.z, -1, 1));
        }
    /// Compute the value of phi for the vector in the spherical coordinate system
        private double SphericalPhi(Vector3 v) {
            double x = Math.Atan2(v.y, v.x);
            if (x < 0) {
                x = x + Math.PI * 2.0;
            }
            return x;
        }

        private int Clamp(int x, int min, int max){
            if (x < min){
                return min;
            } else if (x > max) {
                return max;
            } else {
                return x;
            }
        }


        public override (Spectrum, Vector3, double) Sample_f(Vector3 wo)
        {
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
