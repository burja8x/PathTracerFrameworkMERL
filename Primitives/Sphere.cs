using System;
using MathNet.Numerics.Integration;

namespace PathTracer
{
    class Sphere : Shape
    {
        public double Radius { get; set; }
        public Sphere(double radius, Transform objectToWorld)
        {
            Radius = radius;
            ObjectToWorld = objectToWorld;
        }

        public override (double?, SurfaceInteraction) Intersect(Ray r)
        {
            var ray = WorldToObject.Apply(r);
            
            // TODO: Compute quadratic sphere coefficients

            // TODO: Initialize _double_ ray coordinate values
            double a = (ray.d.x * ray.d.x) + (ray.d.y * ray.d.y) + (ray.d.z * ray.d.z);
            double b = ((ray.d.x * ray.o.x) + (ray.d.y * ray.o.y) + (ray.d.z * ray.o.z)) * 2;
            double c = (ray.o.x * ray.o.x) + (ray.o.y * ray.o.y) + (ray.o.z * ray.o.z) - (Radius * Radius);

            // TODO: Solve quadratic equation for _t_ values
            (bool ok, double t0, double t1) = Utils.Quadratic(a,b,c);
            // TODO: Check quadric shape _t0_ and _t1_ for nearest intersection
            if (!ok /*|| t0 > ray.max*/|| t1 <= 0) {
                return (null, null);
            }
            // TODO: Compute sphere hit position and $\phi$
            double tShapeHit = t0;
            if (tShapeHit <= 0) { // skor 0.
                tShapeHit = t1;
                //if (t1 > ray.max) {
                    //return (null,null);
                //}
            }
            Vector3 pHit = ray.Point(tShapeHit);
            if (pHit.x == 0 && pHit.y == 0) {
                pHit.x = 1e-5 * Radius;
            }

            Vector3 dpdu = new Vector3(-pHit.y, pHit.x, 0);
            
            // TODO: Return shape hit and surface interaction

            return (tShapeHit, ObjectToWorld.Apply(new SurfaceInteraction(pHit, pHit, -ray.d, dpdu, this)));
        }

        public override (SurfaceInteraction, double) Sample()
        {
            // TODO: Implement Sphere sampling
            Vector3 pObj = Samplers.UniformSampleSphere() * Radius;
            pObj = pObj * (Radius / pObj.Length());

            // TODO: Return surface interaction and pdf
            var dpdu = new Vector3(-pObj.y, pObj.x, 0);
            double pdf = 1 / Area(); // a je kle inv 4 PI (1/Math.PI/4)
            return (ObjectToWorld.Apply(new SurfaceInteraction(pObj, ObjectToWorld.ApplyNormal(pObj), Vector3.ZeroVector, dpdu, this)), pdf);
        }

        public override double Area() { return 4 * Math.PI * Radius * Radius; }

        public override double Pdf(SurfaceInteraction si, Vector3 wi)
        {
            //throw new NotImplementedException();

            // Zakaj nikoli ne pride sm ?
            return Utils.PiInv / 4.0;
        }

    }
}
