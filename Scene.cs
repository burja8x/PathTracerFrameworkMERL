using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PathTracer
{
  public class Scene
  {

    /// <summary>
    /// Elements contains all scene elements (lights and shapes)
    /// </summary>
    public List<Primitive> Elements { get; set; } = new List<Primitive>();
    public List<Light> Lights => Elements.Where(x => x is Light).Cast<Light>().ToList();
    public Vector3 CameraOrigin { get; set; } = new Vector3(0, 0, 0);
    public double AspectRatio { get; set; } = 16.0 / 9;

    public double ImagePlaneWidth { get; set; } = 16;
    public double ImagePlaneHeight => ImagePlaneWidth / AspectRatio;
    public double ImagePlaneDistance { get; set; } = 8;
    public double ImagePlaneVerticalOffset { get; set; } = 0;

        public static string brdfFileName = "alum-bronze.binary";

    /// <summary>
    /// Finds closest intersection of ray with scene
    /// </summary>
    /// <param name="r"></param>
    /// <returns></returns>
    public (double?, SurfaceInteraction) Intersect(Ray r)
    {
      double? mint = null;
      SurfaceInteraction si = null;
      foreach (var sh in Elements)
      {
        (double? t, SurfaceInteraction shi) = sh.Intersect(r);
        if (t.HasValue && t>Renderer.Epsilon && (!mint.HasValue || t<mint))
        {
          mint = t.Value;
          si = shi;
        }
      }

      return (mint, si);
    }
    /// <summary>
    /// Returns true if points are not ocludded (no element is between them)
    /// </summary>
    /// <param name="p1"></param>
    /// <param name="p2"></param>
    /// <returns></returns>
    public bool Unoccluded(Vector3 p1, Vector3 p2)
    {
      Ray r = new Ray(p1, p2 - p1);
      (double? t, SurfaceInteraction it) = Intersect(r);

      if (!t.HasValue || (p2-it.Point).Length()<Renderer.Epsilon)
        return true;

      return false;
    }


    /// <summary>
    /// Generate Cornell Box Geometry
    /// </summary>
    /// <returns></returns>
        public static Scene CornellBox()
        {
            var s = new Scene()
            {
                CameraOrigin = new Vector3(278, 274.4, -548),// -800
                AspectRatio = 1.0 / 1.0,
                ImagePlaneWidth = 7 // 5.5
            };

            Shape el;

            // floor
            el = new Quad(556.0, 559.2*3, Transform.Translate(556.0 / 2, 0, 559.2 / 2).A(Transform.RotateX(-90)));
            el.BSDF.Add(new Lambertian(Spectrum.ZeroSpectrum.FromRGB(Color.White)));
            s.Elements.Add(el);
      
            // celing
            el = new Quad(556.0, 559.2*3, Transform.Translate(556.0 / 2, 548.8, 559.2 / 2).A(Transform.RotateX(90)));
            el.BSDF.Add(new Lambertian(Spectrum.ZeroSpectrum.FromRGB(Color.White)));
            s.Elements.Add(el);

            // back
            el = new Quad(556.0, 548.8, Transform.Translate(556.0/2, 548.8/2, 559.2).A(Transform.RotateX(180)));
            el.BSDF.Add(new Lambertian(Spectrum.ZeroSpectrum.FromRGB(Color.White)));
            s.Elements.Add(el);

            // back2
            el = new Quad(556.0, 548.8, Transform.Translate(556.0 / 2, 548.8 / 2, -550).A(Transform.RotateX(180)));
            el.BSDF.Add(new Lambertian(Spectrum.ZeroSpectrum.FromRGB(Color.White)));
            s.Elements.Add(el);


            //double[] brdfs2 = ReadMerl.Read(@"D:\NRG seminarska\green-plastic.binary");
            //right
            el = new Quad(559.2*3, 548.8, Transform.Translate(556.0, 548.8/2, 559.2/2).A(Transform.RotateY(90)));
            el.BSDF.Add(new Lambertian(Spectrum.ZeroSpectrum.FromRGB(Color.Green)));
            //el.BSDF.Add(new Merl(brdfs2));
            s.Elements.Add(el);

            //left
            el = new Quad(559.2*3, 548.8, Transform.Translate(0, 548.8/2, 559.2/2).A(Transform.RotateY(-90)));
            el.BSDF.Add(new Lambertian(Spectrum.ZeroSpectrum.FromRGB(Color.Red)));
            s.Elements.Add(el);

            // luč
            // kvadratna luč
            s.Elements.Add(new DiffuseAreaLight(new Quad(170, 170, Transform.Translate(278, 548, 220).A(Transform.RotateX(90))), Spectrum.Create(1), 25));
            s.Elements.Add(new DiffuseAreaLight(new Quad(170, 170, Transform.Translate(278, 548, -280).A(Transform.RotateX(90))), Spectrum.Create(1), 25));

            //s.Elements.Add(new DiffuseAreaLight(new Disk(100, 0.1, Transform.Translate(278, 548, 280).A(Transform.RotateX(90))), Spectrum.Create(1), 20));
            //s.Elements.Add(new DiffuseAreaLight(new Disk(100, 0.1, Transform.Translate(278, 548, 0).A(Transform.RotateX(90))), Spectrum.Create(1), 30));
            //s.Elements.Add(new DiffuseAreaLight(new Disk(100, 0.1, Transform.Translate(278, 548, -280).A(Transform.RotateX(90))), Spectrum.Create(1), 20));
            //s.Elements.Add(new DiffuseAreaLight(new Sphere(80, Transform.Translate(278, 548, 280).A(Transform.RotateX(90))), Spectrum.Create(1), 20));
            //s.Elements.Add(new DiffuseAreaLight(new Sphere(80, Transform.Translate(278, 548, 280)), Spectrum.Create(1), 20));
            //s.Elements.Add(new DiffuseAreaLight(new Sphere(60, Transform.Translate(278, 280, 300).A(Transform.RotateX(90))), Spectrum.Create(1), 20));
            //s.Elements.Add(new DiffuseAreaLight(new Sphere(80, Transform.Translate(400, 80, 400).A(Transform.RotateX(90))), Spectrum.Create(1), 20));
            //s.Elements.Add(new DiffuseAreaLight(new Sphere(1100, Transform.Translate(278, 280, 280).A(Transform.RotateX(90))), Spectrum.Create(1), 20));

            //double[] brdfs1 = ReadMerl.Read(@"D:\NRG seminarska\gold-metallic-paint.binary");
            //double[] brdfs0 = ReadMerl.Read(@"D:\NRG seminarska\white-acrylic.binary");
            //double[] brdfs0 = ReadMerl.Read(@"D:\NRG seminarska\white-diffuse-bball.binary");
            //double[] brdfs0 = ReadMerl.Read(@"D:\NRG seminarska\white-fabric.binary");
            //double[] brdfs0 = ReadMerl.Read(@"D:\NRG seminarska\white-fabric2.binary");
            //double[] brdfs0 = ReadMerl.Read(@"D:\NRG seminarska\white-marble.binary");
            //double[] brdfs0 = ReadMerl.Read(@"D:\NRG seminarska\white-paint.binary");
            double[] brdfs0 = ReadMerl.Read(@"D:\NRG seminarska\" + brdfFileName);

            // modra krogla
            //el = new Sphere(100, Transform.Translate(150, 100, 420));
            //el = new Sphere(130, Transform.Translate(180, 130, 400));
            //el.BSDF.Add(new SpecularReflection(Spectrum.ZeroSpectrum.FromRGB(Color.White), 0, 0));
            //el.BSDF.Add(new Lambertian(Spectrum.ZeroSpectrum.FromRGB(Color.Blue)));
            //el.BSDF.Add(new OrenNayar(Spectrum.ZeroSpectrum.FromRGB(Color.Blue), 10));
            //el.BSDF.Add(new Merl(brdfs1));
            //s.Elements.Add(el);

            //rumena krogla
            //el = new Sphere(130, Transform.Translate(400, 130, 230));
            el = new Sphere(200, Transform.Translate(250, 200, 130));

            //el.BSDF.Add(new SpecularReflection(Spectrum.ZeroSpectrum.FromRGB(Color.White),0,0));
            //el.BSDF.Add(new SpecularReflection(Spectrum.ZeroSpectrum.FromRGB(Color.White),0.5,1.2));
            //el.BSDF.Add(new Lambertian(Spectrum.ZeroSpectrum.FromRGB(Color.Yellow)));
            //el.BSDF.Add(new OrenNayar(Spectrum.ZeroSpectrum.FromRGB(Color.Yellow), 5));
            el.BSDF.Add(new Merl(brdfs0));
            //el.BSDF.Add(new SpecularReflection(Spectrum.ZeroSpectrum.FromRGB(Color.White), 1, 1.5));
            s.Elements.Add(el);


            //el = new Sphere(60, Transform.Translate(180, 60, 100));
            //el.BSDF.Add(new Lambertian(Spectrum.ZeroSpectrum.FromRGB(Color.Yellow)));
            //s.Elements.Add(el);

            return s;

        }

        public static Scene AllMerlInOne()
        {
            var s = new Scene()
            {
                CameraOrigin = new Vector3(278, 274.4, -548),// -800
                AspectRatio = 1.0 / 1.0,
                ImagePlaneWidth = 5 // 5.5
            };

            Shape el;

            // floor
            el = new Quad(556.0, 559.2 * 2, Transform.Translate(556.0 / 2, 0, 559.2 / 2).A(Transform.RotateX(-90)));
            el.BSDF.Add(new Lambertian(Spectrum.ZeroSpectrum.FromRGB(Color.White)));
            s.Elements.Add(el);

            // celing
            el = new Quad(556.0, 559.2 * 2, Transform.Translate(556.0 / 2, 548.8, 559.2 / 2).A(Transform.RotateX(90)));
            el.BSDF.Add(new Lambertian(Spectrum.ZeroSpectrum.FromRGB(Color.White)));
            s.Elements.Add(el);

            // back
            el = new Quad(556.0, 548.8, Transform.Translate(556.0 / 2, 548.8 / 2, 559.2).A(Transform.RotateX(180)));
            el.BSDF.Add(new Lambertian(Spectrum.ZeroSpectrum.FromRGB(Color.White)));
            s.Elements.Add(el);

            // back2
            el = new Quad(556.0, 548.8, Transform.Translate(556.0 / 2, 548.8 / 2, -550).A(Transform.RotateX(180)));
            el.BSDF.Add(new Lambertian(Spectrum.ZeroSpectrum.FromRGB(Color.White)));
            s.Elements.Add(el);

            // right
            el = new Quad(559.2 * 2, 548.8, Transform.Translate(556.0, 548.8 / 2, 559.2 / 2).A(Transform.RotateY(90)));
            el.BSDF.Add(new Lambertian(Spectrum.ZeroSpectrum.FromRGB(Color.Green)));
            s.Elements.Add(el);

            // left
            el = new Quad(559.2 * 2, 548.8, Transform.Translate(0, 548.8 / 2, 559.2 / 2).A(Transform.RotateY(-90)));
            el.BSDF.Add(new Lambertian(Spectrum.ZeroSpectrum.FromRGB(Color.Red)));
            s.Elements.Add(el);

            // luč
            // kvadratna luč
            s.Elements.Add(new DiffuseAreaLight(new Quad(170, 170, Transform.Translate(278, 548, 200).A(Transform.RotateX(90))), Spectrum.Create(1), 31));
            s.Elements.Add(new DiffuseAreaLight(new Quad(170, 170, Transform.Translate(278, 548, -280).A(Transform.RotateX(90))), Spectrum.Create(1), 35));
            


            DirectoryInfo d = new DirectoryInfo(@"D:\NRG seminarska\");
            FileInfo[] Files = d.GetFiles("*.binary");


            //int xOff = 25;
            //int yOff = 25;
            //int countSpheres = 0;
            //foreach (FileInfo file in Files)
            //{
            //    double[] brdf0 = ReadMerl.Read(@"D:\NRG seminarska\" + file.Name);

            //    el = new Sphere(25, Transform.Translate(xOff, yOff, 400));
            //    el.BSDF.Add(new Merl(brdf0));
            //    s.Elements.Add(el);

            //    countSpheres++;
            //    xOff += 52;
            //    if (countSpheres % 10 == 0)
            //    {
            //        yOff += 52;
            //        xOff = 25;
            //    }

            //    if (countSpheres == 49)
            //    {
            //        //break;
            //    }
            //}


            int xOff = 38;
            int yOff = 38;
            int countSpheres = 0;
            foreach (FileInfo file in Files)
            {
                double[] brdf0 = ReadMerl.Read(@"D:\NRG seminarska\" + file.Name);

                el = new Sphere(35, Transform.Translate(xOff, yOff, 480));
                el.BSDF.Add(new Merl(brdf0));
                s.Elements.Add(el);

                countSpheres++;
                xOff += 77;
                if (countSpheres % 7 == 0)
                {
                    yOff += 77;
                    xOff = 38;
                }

                if (countSpheres == 49)
                {
                    break;
                }
            }


            //int xOff = 55;
            //int yOff = 55;
            //int countSpheres = 0;
            //foreach (FileInfo file in Files)
            //{
            //    double[] brdf0 = ReadMerl.Read(@"D:\NRG seminarska\" + file.Name);

            //    el = new Sphere(50, Transform.Translate(xOff, yOff, 480));
            //    el.BSDF.Add(new Merl(brdf0));
            //    //el.BSDF.Add(new SpecularReflection(Spectrum.ZeroSpectrum.FromRGB(Color.White), 2, 2));
            //    s.Elements.Add(el);

            //    countSpheres++;
            //    xOff += 105;
            //    if (countSpheres % 5 == 0)
            //    {
            //        yOff += 105;
            //        xOff = 55;
            //    }

            //    if (countSpheres == 25)
            //    {
            //        break;
            //    }
            //}



            return s;
        }
    }
}
