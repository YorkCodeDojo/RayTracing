using System;
using GlmNet;
using System.Collections.Generic;

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
            sceneObjects.Add(new Sphere(mat, new vec3(0.0f, 2.0f, 0.0f), 2.0f));

            // Purple ball
            mat.albedo = new vec3(0.7f, 0.0f, 0.7f);
            mat.specular = new vec3(0.9f, 0.9f, 0.8f);
            mat.reflectance = 0.5f;
            sceneObjects.Add(new Sphere(mat, new vec3(-2.5f, 1.0f, 2.0f), 1.0f));

            // Blue ball
            mat.albedo = new vec3(0.0f, 0.3f, 1.0f);
            mat.specular = new vec3(0.0f, 0.0f, 1.0f);
            mat.reflectance = 0.0f;
            mat.emissive = new vec3(0.0f, 0.0f, 0.0f);
            sceneObjects.Add(new Sphere(mat, new vec3(-0.0f, 0.5f, 3.0f), 0.5f));

            // Green ball
            mat.albedo = new vec3(1.0f, 1.0f, 1.0f);
            mat.specular = new vec3(0.0f, 0.0f, 0.0f);
            mat.reflectance = 0.0f;
            mat.emissive = new vec3(2.0f, 2.0f, 2.0f);
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
            // For now, just convert the incoming ray to a 'color' to display it has it changes 
            return new vec3((raydir * .5f) + new vec3(0.5f, 0.5f, 0.5f));
        }

        public unsafe void Run(byte* pData, int stride)
        {
            for (int y = 0; y < ImageHeight; y++)
            {
                for (int x = 0; x < ImageWidth; x++)
                {
                    vec2 coord = new vec2((float)x + .5f, (float)y + .5f);
                    var ray = camera.GetWorldRay(coord);

                    // Fire a ray through this pixel, into the world
                    vec3 color = TraceRay(camera.Position, ray, 0);

                    color *= 255.0f;

                    UInt32 outColor = ((UInt32)color.x << 16) | ((UInt32)color.y << 8) | ((UInt32)color.z) | 0xFF000000; 
                    var pPixel = (UInt32*)(pData + (y * stride) + (x * 4));
                    *pPixel = outColor;
                }
            }
        }
    }
}
