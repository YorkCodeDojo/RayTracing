using System;
using GlmNet;

namespace Tracer
{

    public struct Material
    {
        public vec3 albedo;        // Base color of the surface
        public vec3 specular;      // Specular reflection color
        public float reflectance;               // How reflective the surface is
        public vec3 emissive;      // Light that the material emits
    };

    public enum SceneObjectType
    {
        Sphere,
        Plane
    };

    public abstract class SceneObject
    {
        // Given a point on the surface, return the material at that point
        public abstract Material GetMaterial(vec3 pos);

        // Is it a sphere or a plane?
        public abstract SceneObjectType GetSceneObjectType();

        // Given a point on the surface, return a normal
        public abstract vec3 GetSurfaceNormal(vec3 pos);

        // Given a source position, return a ray to this object's center
        public abstract vec3 GetRayFrom(vec3 from);

        // Intersect this object with a ray and figure out if it hits, and return the distance to the hit point 
        public abstract bool Intersects(vec3 rayOrigin, vec3 rayDir);

        public abstract float IntersectDistance(vec3 rayOrigin, vec3 rayDir);
    };

    // A sphere, at a coordinate, with a radius and a material
    public class Sphere : SceneObject
    {
        public vec3 center;
        public float radius;
        public Material material;

        public Sphere(Material mat, vec3 c, float r)
        {
            material = mat;
            center = c;
            radius = r;
        }
        public override Material GetMaterial(vec3 pos)
        {
            return material;
        }

        public override SceneObjectType GetSceneObjectType()
        {
            return SceneObjectType.Sphere;
        }

        public override vec3 GetSurfaceNormal(vec3 pos)
        {
            return glm.normalize(pos - center);
        }

        public override vec3 GetRayFrom(vec3 from)
        {
            return glm.normalize(center - from);
        }

        public override bool Intersects(vec3 rayOrigin, vec3 rayDir)
        {
            float distance;
            return GeometryMath.intersectRaySphere(rayOrigin, glm.normalize(rayDir), center, radius * radius, out distance);
        }

        public override float IntersectDistance(vec3 rayOrigin, vec3 rayDir) {
            float distance;
            GeometryMath.intersectRaySphere(rayOrigin, glm.normalize(rayDir), center, radius * radius, out distance);
            return distance;
        }
    };

    // A plane, centered at origin, with a normal direction
    public abstract class Plane : SceneObject
    {
        public override SceneObjectType GetSceneObjectType()
        {
            return SceneObjectType.Plane;
        }
    };

    // A tiled plane.  returns a different material based on the hit point to represent the grid
    public class TiledPlane : Plane
    {
        public vec3 normal;
        public vec3 origin;
        public Material mat;

        public TiledPlane(vec3 o, vec3 n)
        {
            normal = n;
            origin = o;
            mat.reflectance = 0.0f;
            mat.emissive = new vec3(0.0f, 0.0f, 0.0f);
            mat.specular = new vec3(1.0f, 1.0f, 1.0f);
        }

        public override Material GetMaterial(vec3 pos) 
        {
            bool white = (((int)(System.Math.Floor(pos.x) + System.Math.Floor(pos.z)) & 1) == 0);

            if (white)
            {
                mat.albedo = new vec3(1.0f, 1.0f, 1.0f);
            }
            else
            {
                mat.albedo = new vec3(0.0f, 0.0f, 0.0f);
            }
            return mat;
        }

        public override vec3 GetSurfaceNormal(vec3 pos)
        {
            return normal;
        }

        public override vec3 GetRayFrom(vec3 from)
        {
            return glm.normalize(origin - from);
        }

        public override bool Intersects(vec3 rayOrigin, vec3 rayDir)
        {
            float distance;
            return GeometryMath.intersectRayPlane(rayOrigin, glm.normalize(rayDir), origin, normal, out distance);
        }

        public override float IntersectDistance(vec3 rayOrigin, vec3 rayDir) {
            float distance;
            GeometryMath.intersectRayPlane(rayOrigin, glm.normalize(rayDir), origin, normal, out distance);
            return distance;
        }
    };

}