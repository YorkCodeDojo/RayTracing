using System;
using System.Linq;
using System.Collections.Generic;
using GlmNet;
using Tracer;

namespace RayTracerCSharp
{
    public static class RayTracer
    {

        private static vec3 ZERO_VECTOR = new vec3(0, 0, 0);

        private static bool Zero(vec3 vector)
        {
            return vector.x == 0 && vector.y == 0 && vector.z == 0;
        }

        // Trace a ray into the scene, return the accumulated light value
        public static vec3 TraceRay(vec3 rayorig, vec3 raydir, int depth, List<SceneObject> sceneObjects)
        {
            var lights = sceneObjects.Where(o => !Zero(o.GetMaterial(ZERO_VECTOR).emissive));

            return raydir * 0.5f + new vec3(.5f, .5f, .5f);
        }
    }
}
