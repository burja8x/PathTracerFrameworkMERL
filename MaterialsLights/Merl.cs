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
        const int BRDF_SAMPLING_RES_PHI_D = 360;
        double[] brdf;
        public Merl(double[] brdfs) {
            brdf = brdfs;
        }

        public override Spectrum f(Vector3 wo, Vector3 wi)
        {
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
            

            double cos_phi_h = Utils.CosPhi(wh);
            double sin_phi_h = Utils.SinPhi(wh);
            double cos_theta_h = Utils.CosTheta(wh);
            double sin_theta_h = Utils.SinTheta(wh);
            Vector3 w_hx = new Vector3(cos_phi_h * cos_theta_h, sin_phi_h * cos_theta_h, -sin_theta_h);
            Vector3 w_hy = new Vector3(-sin_phi_h, cos_phi_h, 0.0);
            Vector3 w_d = new Vector3(Vector3.Dot(wi, w_hx), Vector3.Dot(wi, w_hy), Vector3.Dot(wi, wh));

            double theta_h = SphericalTheta(wh);
            double theta_d = SphericalTheta(w_d);
            double phi_d = SphericalPhi(w_d);
            if (phi_d > Math.PI) {
                phi_d = phi_d - Math.PI;
            }


            // Get i.
            int theta_h_idx = ThetaHalfIndex(theta_h);
            int theta_d_idx = ThetaDiffIndex(theta_d);
            int phi_d_idx = PhiDiffIndex(phi_d);
            int i = phi_d_idx + (BRDF_SAMPLING_RES_PHI_D / 2) * (theta_d_idx + theta_h_idx * BRDF_SAMPLING_RES_THETA_D);

            return Spectrum.Create(Vector<double>.Build.Dense(new[] { brdf[3 * i], brdf[3 * i + 1], brdf[3 * i + 2] }));
        }

        public Spectrum f1(Vector3 wo, Vector3 wi)
        {
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


            double theta_half = Math.Acos(wh.z);
            double fi_half = Math.Atan2(wh.y, wh.x);

            Vector3 normal = new Vector3(0, 0, 1);
            Vector3 temp = RotateVector(wi, normal, -fi_half);
            Vector3 biNormal = new Vector3(0, 1, 0);
            Vector3 diff = RotateVector(temp, biNormal, -theta_half);

            double theta_diff = Math.Acos(diff.z);
            double fi_diff = Math.Atan2(diff.y, diff.x);


            // Get i.
            int theta_h_idx = ThetaHalfIndex(theta_half);
            int theta_d_idx = ThetaDiffIndex(theta_diff);
            int phi_d_idx = PhiDiffIndex(fi_diff);
            int i = phi_d_idx + (BRDF_SAMPLING_RES_PHI_D / 2) * (theta_d_idx + theta_h_idx * BRDF_SAMPLING_RES_THETA_D);

            return Spectrum.Create(Vector<double>.Build.Dense(new[] { brdf[3 * i], brdf[3 * i + 1], brdf[3 * i + 2] }));
        }

        static Vector3 RotateVector(Vector3 vector, Vector3 axis, double angle)
        {
            double temp;

            double cos_ang = Math.Cos(angle);
            double sin_ang = Math.Sin(angle);

            Vector3 out_neki = vector * cos_ang;

            temp = Vector3.Dot(axis, vector);
            temp = temp * (1.0 - cos_ang);

            out_neki = out_neki + (axis * temp);

            Vector3 cross = Vector3.Cross(axis, vector);

            out_neki = out_neki + (cross * sin_ang);

            return out_neki;
        }

        private double SphericalTheta(Vector3 v) {
            return Math.Acos(Utils.Clamp(v.z, -1, 1));
        }
        /// Compute the value of phi for the vector in the spherical coordinate system
        private double SphericalPhi(Vector3 v) {
            double x = Math.Atan2(v.y, v.x);
            if (x < 0)
            {
                x = x + Math.PI * 2.0;
            }
            return x;
        }


        // Lookup theta_half index
        // This is a non-linear mapping!
        // In:  [0 .. pi/2]
        // Out: [0 .. 89]
        static int ThetaHalfIndex(double thetaHalf)
        {
            if (thetaHalf <= 0.0)
                return 0;
            double thetaHalfDeg = ((thetaHalf / (Math.PI / 2.0)) * BRDF_SAMPLING_RES_THETA_H);
            double temp = thetaHalfDeg * BRDF_SAMPLING_RES_THETA_H;
            temp = Math.Sqrt(temp);
            int retVal = (int)temp;
            if (retVal < 0) retVal = 0;
            if (retVal >= BRDF_SAMPLING_RES_THETA_H)
                retVal = BRDF_SAMPLING_RES_THETA_H - 1;
            return retVal;
        }


        // Lookup theta_diff index
        // In:  [0 .. pi/2]
        // Out: [0 .. 89]
        static int ThetaDiffIndex(double thetaDiff)
        {
            int tmp = (int)(thetaDiff / (Math.PI * 0.5) * BRDF_SAMPLING_RES_THETA_D);
            if (tmp < 0)
                return 0;
            else if (tmp < BRDF_SAMPLING_RES_THETA_D - 1)
                return tmp;
            else
                return BRDF_SAMPLING_RES_THETA_D - 1;
        }


        // Lookup phi_diff index
        static int PhiDiffIndex(double phiDiff)
        {
            // Because of reciprocity, the BRDF is unchanged under
            if (phiDiff < 0.0)
                phiDiff += Math.PI;

            // In: phi_diff in [0 .. pi]
            // Out: tmp in [0 .. 179]
            int tmp = (int)(phiDiff / Math.PI * BRDF_SAMPLING_RES_PHI_D / 2);
            if (tmp < 0)
                return 0;
            else if (tmp < BRDF_SAMPLING_RES_PHI_D / 2 - 1)
                return tmp;
            else
                return BRDF_SAMPLING_RES_PHI_D / 2 - 1;
        }

        public override (Spectrum, Vector3, double) Sample_f(Vector3 wo)
        {
            var wi = Samplers.CosineSampleHemisphere();
            //var wi = Samplers.UniformSampleHemisphere();
            if (wo.z < 0)
                wi.z *= -1;
            double pdf = Pdf(wo, wi);
            return (f(wo, wi), wi, pdf);
        }

        public override double Pdf(Vector3 wo, Vector3 wi)
        {
            //return Samplers.UniformHemispherePdf();
            return Samplers.CosineHemispherePdf(wo, wi);
        }
    }
}
