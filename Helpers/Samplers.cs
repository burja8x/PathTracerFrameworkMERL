﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PathTracer
{
  public static class Samplers
  {
    public static Vector3 UniformSampleSphere()
    {
            // TODO: Implement unifrom Sphere sampler
            double z = 1 - (2 * ThreadSafeRandom.NextDouble());
            double phi = 2 * Math.PI * ThreadSafeRandom.NextDouble();
            double r = Math.Sqrt(Math.Max(0, 1 - z * z));

            // * radius // to se po naredi.
            return new Vector3(r * Math.Cos(phi), r * Math.Sin(phi), z);
    }
        public static double UniformSpherePdf() {
            return Utils.PiInv / 4;
        }

        public static Vector3 UniformSampleHemisphere()
        {
            double z = ThreadSafeRandom.NextDouble(); // samo tuki je razlika
            double r = Math.Sqrt(Math.Max(0, 1.0 - z * z));
            double phi = 2 * Math.PI * ThreadSafeRandom.NextDouble();
            return new Vector3(r * Math.Cos(phi), r * Math.Sin(phi), z);
        }
        public static double UniformHemispherePdf()
        {
            return Utils.PiInv / 2;
        }

        public static (double, double) UniformSampleSquare()
    {
      var u = ThreadSafeRandom.NextDouble();
      var v = ThreadSafeRandom.NextDouble();
      return (u, v);
    }

    public static (double, double) UniformSampleDisk()
    {
      var u = ThreadSafeRandom.NextDouble();
      var v = ThreadSafeRandom.NextDouble();
      var r = Math.Sqrt(u);
      var theta = 2 * Math.PI * v;
      return (r * Math.Cos(theta), r * Math.Sin(theta));
    }

    /// <summary>
    /// Cosine sample hemisphere with projection of uniform-sampled disk. Returns local coords.
    /// </summary>
    /// <returns></returns>
    public static Vector3 CosineSampleHemisphere()
    {
      (double x, double y) = UniformSampleDisk();
      var z = Math.Sqrt(Math.Max(0, 1 - x * x - y * y));
      return new Vector3(x, y, z);
    }
        public static double CosineHemispherePdf(Vector3 wo, Vector3 wi) {
            if (!Utils.SameHemisphere(wo, wi))
                return 0;

            return Math.Abs(wi.z) * Utils.PiInv;
        }

    /// <summary>
    /// Thread safe random function implementation
    /// </summary>
    public static class ThreadSafeRandom
    {
      private static Random _global = new Random(12);
      [ThreadStatic]
      private static Random _local;
      [ThreadStatic]
      public static List<double> buffer;
      [ThreadStatic]
      public static int cnt = -1;
      public static double NextDouble()
      {
        Random inst = _local;
        if (inst == null)
        {
          int seed;
          lock (_global) seed = _global.Next();
          _local = inst = new Random(seed);
          buffer = new List<double>(2000);
          cnt = -1;
        }
        if (cnt == -1)
        {
          for (int i = 0; i < 1000; i++)
            buffer.Add(inst.NextDouble());
        }
        else if (cnt >= 999)
        {
          buffer.RemoveRange(0, 500);
          for (int i = 0; i < 500; i++)
            buffer.Add(inst.NextDouble());
          cnt = 499;
        }
        //var r = inst.NextDouble();
        cnt++;
        var r = buffer[cnt];
        return r;
      }
    }
  }
}
