using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PathTracer
{
    public static class ReadMerl
    {
        const int BRDF_SAMPLING_RES_THETA_H = 90;
        const int BRDF_SAMPLING_RES_THETA_D = 90;
        const int BRDF_SAMPLING_RES_PHI_D = 360;
        const double RED_SCALE = (1.0 / 1500.0);
        const double GREEN_SCALE = (1.15 / 1500.0); // zakaj je pr rust 1.0 ?
        const double BLUE_SCALE = (1.66 / 1500.0);

        public static double[] Read(string file) {
            BinaryReader binReader = new BinaryReader(File.Open(file, FileMode.Open));

            int n = 1;
            n *= binReader.ReadInt32();
            n *= binReader.ReadInt32();
            n *= binReader.ReadInt32();
            Console.WriteLine(n);

            if (n == (90 * 90 * 360 / 2))
            {
                Console.WriteLine("OK");
                double[] brdf = new double[3 * n];
                int x = 0;

                for (int i = 0; i < n; i++)
                {
                    brdf[3 * i] = binReader.ReadDouble() * RED_SCALE;
                }
                for (int i = 0; i < n; i++)
                {
                    brdf[3 * i + 1] = binReader.ReadDouble() * GREEN_SCALE;
                }
                for (int i = 0; i < n; i++)
                {
                    brdf[3 * i + 2] = binReader.ReadDouble() * BLUE_SCALE;
                }
                //while (x < n)
                //{
                //    //brdf[x] = binReader.ReadDouble();

                    
                //    brdf[x] = binReader.ReadDouble() * RED_SCALE;
                //    brdf[n + x] = binReader.ReadDouble() * GREEN_SCALE;
                //    brdf[2 * n + x] = binReader.ReadDouble() * BLUE_SCALE;
                    
                //    //if (x < n)
                //    //{
                //    //    brdf[x] = tmp * RED_SCALE;
                //    //}
                //    //else if (x < n * 2 && x > n)
                //    //{
                //    //    brdf[n + x] = tmp * GREEN_SCALE;
                //    //}
                //    //else
                //    //{
                //    //    brdf[2 * n + x] = tmp * BLUE_SCALE;
                //    //}

                //    x++;
                //}
                binReader.Close();

                //Console.WriteLine("first 10.");
                //for (int mm = 0; mm < 10; mm++){
                //    Console.WriteLine(brdf[mm]);
                //}
                
                //for(int nn = 1000000; nn < 1000010; nn++){
                //    Console.WriteLine(brdf[nn]);
                //}

                Console.WriteLine("READ");
                return brdf;

                int vvv = 0;

                const int z = 16;
                for (int i = 0; i < z; i++)
                {
                    double theta_in = i * 0.5 * Math.PI / z;
                    for (int j = 0; j < 4 * z; j++)
                    {
                        double phi_in = j * 2.0 * Math.PI / (4 * z);
                        for (int k = 0; k < z; k++)
                        {
                            double theta_out = k * 0.5 * Math.PI / z;
                            for (int l = 0; l < 4 * z; l++)
                            {
                                double phi_out = l * 2.0 * Math.PI / (4 * z);

                                (double red, double green, double blue) = lookup_brdf_val(brdf, theta_in, phi_in, theta_out, phi_out);
                                //Console.WriteLine($"{(decimal)red}  {(decimal)green}  {(decimal)blue}");
                                vvv++;
                            }
                        }
                    }
                }
                Console.WriteLine(vvv);
                Console.WriteLine("DONE");
            }
            else
            {
                Console.WriteLine("Dimensions don't match");
                binReader.Close();
                throw new Exception("MERL Dimensions don't match!");
            }
        }

        static (double, double, double) lookup_brdf_val(double[] brdf, double theta_in, double fi_in, double theta_out, double fi_out)
        {
            // Convert to halfangle / difference angle coordinates

            (double theta_half, double fi_half, double theta_diff, double fi_diff) = std_coords_to_half_diff_coords(theta_in, fi_in, theta_out, fi_out);

            // Find index.
            // Note that phi_half is ignored, since isotropic BRDFs are assumed
            int ind = phi_diff_index(fi_diff) +
                  theta_diff_index(theta_diff) * BRDF_SAMPLING_RES_PHI_D / 2 +
                  theta_half_index(theta_half) * BRDF_SAMPLING_RES_PHI_D / 2 *
                                     BRDF_SAMPLING_RES_THETA_D;

            double red_val = brdf[ind] * RED_SCALE;
            double green_val = brdf[ind + BRDF_SAMPLING_RES_THETA_H * BRDF_SAMPLING_RES_THETA_D * BRDF_SAMPLING_RES_PHI_D / 2] * GREEN_SCALE;
            double blue_val = brdf[ind + BRDF_SAMPLING_RES_THETA_H * BRDF_SAMPLING_RES_THETA_D * BRDF_SAMPLING_RES_PHI_D] * BLUE_SCALE;

            if (red_val < 0.0 || green_val < 0.0 || blue_val < 0.0)
            {
                Console.WriteLine("Below horizon");
            }
            return (red_val, green_val, blue_val);
        }

        static (double, double, double, double) std_coords_to_half_diff_coords(double theta_in, double fi_in, double theta_out, double fi_out)
        {
            // compute in vector
            double in_vec_z = Math.Cos(theta_in);
            double proj_in_vec = Math.Sin(theta_in);
            double in_vec_x = proj_in_vec * Math.Cos(fi_in);
            double in_vec_y = proj_in_vec * Math.Sin(fi_in);
            double[] in_neki = { in_vec_x, in_vec_y, in_vec_z };
            in_neki = normalize(in_neki);

            // compute out vector
            double out_vec_z = Math.Cos(theta_out);
            double proj_out_vec = Math.Sin(theta_out);
            double out_vec_x = proj_out_vec * Math.Cos(fi_out);
            double out_vec_y = proj_out_vec * Math.Sin(fi_out);
            double[] out_neki = { out_vec_x, out_vec_y, out_vec_z }; // TA SE NE UPORABLA !
            out_neki = normalize(out_neki); // TA SE NE UPORABLA !


            // compute halfway vector
            double half_x = (in_vec_x + out_vec_x) / 2.0f;
            double half_y = (in_vec_y + out_vec_y) / 2.0f;
            double half_z = (in_vec_z + out_vec_z) / 2.0f;
            double[] half = { half_x, half_y, half_z };
            half = normalize(half);

            // compute  theta_half, fi_half
            double theta_half = Math.Acos(half[2]);
            double fi_half = Math.Atan2(half[1], half[0]); // ta se ne uporabla!

            double[] bi_normal = { 0.0, 1.0, 0.0 };
            double[] normal = { 0.0, 0.0, 1.0 };
            double[] temp = new double[3];
            double[] diff = new double[3]; // w_d

            // compute diff vector
            temp = rotate_vector(in_neki, normal, -fi_half, temp);
            diff = rotate_vector(temp, bi_normal, -theta_half, diff);

            // compute  theta_diff, fi_diff	
            double theta_diff = Math.Acos(diff[2]);
            double fi_diff = Math.Atan2(diff[1], diff[0]);
            return (theta_half, fi_half, theta_diff, fi_diff);

        }

        static double[] normalize(double[] v)
        {
            double len = Math.Sqrt(v[0] * v[0] + v[1] * v[1] + v[2] * v[2]);
            v[0] = v[0] / len;
            v[1] = v[1] / len;
            v[2] = v[2] / len;
            return v;
        }
        static double[] rotate_vector(double[] vector, double[] axis, double angle, double[] out_neki)
        {
            double temp;
            double[] cross = new double[3];
            double cos_ang = Math.Cos(angle);
            double sin_ang = Math.Sin(angle);

            out_neki[0] = vector[0] * cos_ang;
            out_neki[1] = vector[1] * cos_ang;
            out_neki[2] = vector[2] * cos_ang;

            temp = axis[0] * vector[0] + axis[1] * vector[1] + axis[2] * vector[2];
            temp = temp * (1.0 - cos_ang);

            out_neki[0] += axis[0] * temp;
            out_neki[1] += axis[1] * temp;
            out_neki[2] += axis[2] * temp;

            cross = cross_product(axis, vector, cross);

            out_neki[0] += cross[0] * sin_ang;
            out_neki[1] += cross[1] * sin_ang;
            out_neki[2] += cross[2] * sin_ang;
            return out_neki;
        }
        static double[] cross_product(double[] v1, double[] v2, double[] out_neki)
        {
            out_neki[0] = v1[1] * v2[2] - v1[2] * v2[1];
            out_neki[1] = v1[2] * v2[0] - v1[0] * v2[2];
            out_neki[2] = v1[0] * v2[1] - v1[1] * v2[0];
            return out_neki;
        }

        // Lookup theta_half index
        // This is a non-linear mapping!
        // In:  [0 .. pi/2]
        // Out: [0 .. 89]
        static int theta_half_index(double theta_half)
        {
            if (theta_half <= 0.0)
                return 0;
            double theta_half_deg = ((theta_half / (Math.PI / 2.0)) * BRDF_SAMPLING_RES_THETA_H);
            double temp = theta_half_deg * BRDF_SAMPLING_RES_THETA_H;
            temp = Math.Sqrt(temp);
            int ret_val = (int)temp;
            if (ret_val < 0) ret_val = 0;
            if (ret_val >= BRDF_SAMPLING_RES_THETA_H)
                ret_val = BRDF_SAMPLING_RES_THETA_H - 1;
            return ret_val;
        }


        // Lookup theta_diff index
        // In:  [0 .. pi/2]
        // Out: [0 .. 89]
        static int theta_diff_index(double theta_diff)
        {
            int tmp = (int)(theta_diff / (Math.PI * 0.5) * BRDF_SAMPLING_RES_THETA_D);
            if (tmp < 0)
                return 0;
            else if (tmp < BRDF_SAMPLING_RES_THETA_D - 1)
                return tmp;
            else
                return BRDF_SAMPLING_RES_THETA_D - 1;
        }


        // Lookup phi_diff index
        static int phi_diff_index(double phi_diff)
        {
            // Because of reciprocity, the BRDF is unchanged under
            // phi_diff -> phi_diff + M_PI
            if (phi_diff < 0.0)
                phi_diff += Math.PI;

            // In: phi_diff in [0 .. pi]
            // Out: tmp in [0 .. 179]
            int tmp = (int)(phi_diff / Math.PI * BRDF_SAMPLING_RES_PHI_D / 2);
            if (tmp < 0)
                return 0;
            else if (tmp < BRDF_SAMPLING_RES_PHI_D / 2 - 1)
                return tmp;
            else
                return BRDF_SAMPLING_RES_PHI_D / 2 - 1;
        }
    }
}
