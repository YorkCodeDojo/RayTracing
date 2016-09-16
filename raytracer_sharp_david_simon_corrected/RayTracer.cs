using System;
using System.Linq;
using GlmNet;
using System.Collections.Generic;
using System.Diagnostics;

namespace Tracer
{
    class RayTracer
    {
        public static int ImageWidth = 512;
        public static int ImageHeight = 512;

        private const float FieldOfView = 60.0f;
        private const int MaxDepth = 3;

        private List<SceneObject> sceneObjects = new List<SceneObject>();
        private Camera camera;

        public RayTracer()
        {
            InitScene();
        }

        void InitScene()
        {
            camera = new Camera(new vec3(0.0f, 6.0f, 8.0f),     // Where the camera is
                                new vec3(0.0f, -.8f, -1.0f),    // The point it is looking at
                                FieldOfView,                    // The field of view of the 'lens'
                                ImageWidth, ImageHeight);       // The size in pixels of the view plane

            // Red ball
            Material mat = new Material();
            mat.albedo = new vec3(.7f, .1f, .1f);
            mat.specular = new vec3(.9f, .1f, .1f);
            mat.reflectance = 0.5f;
            mat.emissive = new vec3(0.0f, 0.0f, 0.0f);
            sceneObjects.Add(new Sphere(mat, new vec3(0.0f, 2.0f, 0.0f), 2.0f));

            // Purple ball
            mat.albedo = new vec3(.7f, 0.0f, .7f);
            mat.specular = new vec3(.9f, .9f, .8f);
            mat.reflectance = 0.5f;
            mat.emissive = new vec3(0.0f, 0.0f, 0.0f);
            sceneObjects.Add(new Sphere(mat, new vec3(-2.5f, 1.0f, 2.0f), 1.0f));

            // Blue ball
            mat.albedo = new vec3(0.0f, 0.3f, 1.0f);
            mat.specular = new vec3(0.0f, 0.0f, 1.0f);
            mat.reflectance = 0.0f;
            mat.emissive = new vec3(0.0f, 0.0f, 0.0f);
            sceneObjects.Add(new Sphere(mat, new vec3(0.0f, 0.5f, 3.0f), 0.5f));

            // Yellow Ball on floor, ligt
            mat.albedo = new vec3(1.0f, 1.0f, 1.0f);
            mat.specular = new vec3(0.0f, 0.0f, 0.0f);
            mat.reflectance = 0.0f;
            mat.emissive = new vec3(2.0f, 2.0f, 2.2f);
            sceneObjects.Add(new Sphere(mat, new vec3(2.8f, 0.8f, 2.0f), 0.8f));

            // White light
            mat.albedo = new vec3(0.0f, 0.8f, 0.0f);
            mat.specular = new vec3(0.0f, 0.0f, 0.0f);
            mat.reflectance = 0.0f;
            mat.emissive = new vec3(1.0f, 1.0f, 1.0f);
            sceneObjects.Add(new Sphere(mat, new vec3(-10.8f, 6.4f, 10.0f), 0.4f));

            sceneObjects.Add(new TiledPlane(new vec3(0.0f, 0.0f, 0.0f), new vec3(0.0f, 1.0f, 0.0f)));
        }

        // Trace a ray into the scene, return the accumulated light value
        vec3 TraceRay(vec3 rayorig, vec3 raydir, int depth)
        {
            // Walk the scene objects

            //Step 1
            //var distance = 0f;
            //if (obj.Intersects(rayorig, raydir, out distance))
            //{
            //    var material = obj.GetMaterial(rayorig + (raydir * distance));
            //    return material.albedo;
            //}

            float minDistance = float.MaxValue;
            SceneObject found = null;
            foreach (var obj in sceneObjects)
            {
                var distance = 0f;
                if (obj.Intersects(rayorig, raydir, out distance))
                {
                    if (distance < minDistance)
                    {
                        minDistance = distance;
                        found = obj;
                    }
                }
            }

            if (found == null)
            {
                return new vec3(0.0f, 0.0f, 0.0f);
            }
            var intersectionPoint = rayorig + (raydir * minDistance);
            var normal = found.GetSurfaceNormal(intersectionPoint);
            var material = found.GetMaterial(intersectionPoint);
            var lightSource = new vec3(-raydir.x, -raydir.y, -raydir.z);
            var lightIntensity = glm.dot(normal, lightSource);
            if (lightIntensity < 0) lightIntensity = 0;

            var colour = material.emissive;// new vec3(0, 0, 0);//  material.albedo * lightIntensity;
            if (material.reflectance > 0 && depth < 3)
            {
                var reflectionVector = GeometryMath.reflect(raydir, normal);
                var newColour = TraceRay(intersectionPoint + (reflectionVector * 0.01f), reflectionVector, depth + 1);
                colour = colour + (newColour * material.reflectance);
            }
            foreach (var possibleLight in sceneObjects)
            {
                var possibleLightMaterial = possibleLight.GetMaterial(rayorig + (raydir * minDistance));
                var emissive = possibleLightMaterial.emissive;
                if (emissive.x != 0 || emissive.y != 0 || emissive.z != 0)
                {
                    var fromObjectToLight = possibleLight.GetRayFrom(intersectionPoint);
                    var inTheWay = false;

                    float distanceToLight;
                    possibleLight.Intersects(intersectionPoint, fromObjectToLight, out distanceToLight);

                    foreach (var occluder in sceneObjects)
                    {
                        float distancetooccluder;
                        if (occluder.Intersects(intersectionPoint, fromObjectToLight, out distancetooccluder))
                        {
                            if (distancetooccluder > 0.01f && distancetooccluder < distanceToLight)
                                inTheWay = true;
                        }
                    }

                    if (!inTheWay)
                    {
                        var lightIntensity2 = glm.dot(normal, fromObjectToLight);
                        if (lightIntensity2 < 0) lightIntensity2 = 0;

                        var reflectionVector = GeometryMath.reflect(raydir, normal);
                        var specularIntensity = glm.dot(glm.normalize(reflectionVector), fromObjectToLight);
                        specularIntensity = Math.Max(0, specularIntensity);

                        var specularIntensityColour = (float)Math.Pow(specularIntensity, 10) * material.specular * possibleLightMaterial.emissive;
                        colour = colour + (material.albedo * emissive * lightIntensity2) + specularIntensityColour;
                        /*if (material.specular.z != 1.0f && depth == 0)
                        {
                            return new vec3(0.0f, 0.0f, 0.0f);
                        }*/
                    }

                }
            }
            return colour;
        }

        public unsafe void Run(byte* pData, int stride)
        {
            for (int y = 0; y < ImageHeight; y++)
            {
                for (int x = 0; x < ImageWidth; x++)
                {
                    vec3 color1 = TraceTheRay(y, x, 0.1f, 0.2f);
                    vec3 color2 = TraceTheRay(y, x, 0.3f, 0.4f);
                    vec3 color3 = TraceTheRay(y, x, 0.5f, 0.6f);
                    vec3 color4 = TraceTheRay(y, x, 0.7f, 0.8f);

                    var color = new vec3(((color1.x + color2.x + color3.x + color4.x) / 4),
                                         ((color1.y + color2.y + color3.y + color4.y) / 4),
                                         ((color1.z + color2.z + color3.z + color4.z) / 4));

                    color *= 255.0f;

                    color.x = Math.Max(0, color.x);
                    color.y = Math.Max(0, color.y);
                    color.z = Math.Max(0, color.z);

                    color.x = Math.Min(255.0f, color.x);
                    color.y = Math.Min(255.0f, color.y);
                    color.z = Math.Min(255.0f, color.z);
                    // Better way to do this in C# ?
                    UInt32 outColor = ((UInt32)color.x << 16) | ((UInt32)color.y << 8) | ((UInt32)color.z) | 0xFF000000;
                    var pPixel = (UInt32*)(pData + (y * stride) + (x * 4));
                    *pPixel = outColor;
                }
            }
        }

        private unsafe vec3 TraceTheRay(int y, int x, float yOffset, float xOffet)
        {
            vec2 coord = new vec2((float)x + xOffet, (float)y + yOffset);
            var ray = camera.GetWorldRay(coord);

            // Fire a ray through this pixel, into the world
            vec3 color = TraceRay(camera.Position, ray, 0);
            return color;
        }

    }
}