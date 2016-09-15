using GlmNet;
namespace Tracer
{

    class GeometryMath
    {
        // Return the distance along a ray that a sphere is found - if possible
        public static bool intersectRaySphere(vec3 rayStarting, vec3 rayNormalizedDirection, vec3 sphereCenter, float sphereRadiusSquared, out float intersectionDistance)
        {
            var diff = sphereCenter - rayStarting;
            float t0 = glm.dot(diff, rayNormalizedDirection);
            var dSquared = glm.dot(diff, diff) - t0 * t0;
            if (dSquared > sphereRadiusSquared)
            {
                intersectionDistance = 0.0f;
                return false;
            }
            float t1 = (float)System.Math.Sqrt(sphereRadiusSquared - dSquared);
            intersectionDistance = (t0 > t1 + (float)float.Epsilon) ? t0 - t1 : t0 + t1;
            return intersectionDistance > float.Epsilon;
        }

        // Return the distance along a ray that a plane is found - if possible
        public static bool intersectRayPlane(vec3 orig, vec3 dir, vec3 planeOrig, vec3 planeNormal, out float intersectionDistance)
        {
            float d = glm.dot(dir, planeNormal);

            if (d < float.Epsilon)
            {
                intersectionDistance = glm.dot(planeOrig - orig, planeNormal) / d;
                return true;
            }

            intersectionDistance = 0.0f;
            return false;
        }

        // Given an incident ray and a normal to the surface, return a reflected ray
        vec3 reflect(vec3 I, vec3 N)
        {
            return I - N * glm.dot(N, I) * 2.0f;
        }
    }
}
