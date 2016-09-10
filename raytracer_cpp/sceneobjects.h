#pragma once

struct Material
{
    vec3 albedo;        // Base color of the surface
    vec3 specular;      // Specular reflection color
    float reflectance;  // How reflective the surface is
    vec3 emissive;      // Light that the material emits
};

enum class SceneObjectType
{
    Sphere,
    Plane
};

struct SceneObject
{
    // Given a point on the surface, return the material at that point
    virtual const Material& GetMaterial(const vec3& pos) const = 0;

    // Is it a sphere or a plane?
    virtual SceneObjectType GetSceneObjectType() const = 0;

    // Given a point on the surface, return a normal
    virtual vec3 GetSurfaceNormal(const vec3& pos) const = 0;

    // Given a source position, return a ray to this object's center
    virtual vec3 GetRayFrom(const vec3& from) const = 0;

    // Intersect this object with a ray and figure out if it hits, and return the distance to the hit point 
    virtual bool Intersects(const vec3& rayOrigin, const vec3& rayDir, float& distance) const = 0;
};

// A sphere, at a coordinate, with a radius and a material
struct Sphere : SceneObject
{
    vec3 center;
    float radius;
    Material material;

    Sphere(const Material& mat, const vec3& c, const float r)
    {
        material = mat;
        center = c;
        radius = r;
    }

    virtual const Material& GetMaterial(const vec3& pos) const override
    {
        return material;
    }

    virtual SceneObjectType GetSceneObjectType() const override
    {
        return SceneObjectType::Sphere;
    }

    virtual vec3 GetSurfaceNormal(const vec3& pos) const
    {
        return normalize(pos - center);
    }
    
    virtual vec3 GetRayFrom(const vec3& from) const override
    {
        return normalize(center - from);
    }

    virtual bool Intersects(const vec3& rayOrigin, const vec3& rayDir, float& distance) const
    {
        bool hit = glm::intersectRaySphere(rayOrigin, glm::normalize(rayDir), center, radius * radius, distance);
        return hit;
    }
};

// A plane, centered at origin, with a normal direction
struct Plane : SceneObject
{
    vec3 normal;
    vec3 origin;
    virtual SceneObjectType GetSceneObjectType() const override 
    {
        return SceneObjectType::Plane;
    }
};

// A tiled plane.  returns a different material based on the hit point to represent the grid
struct TiledPlane : Plane
{
    mutable Material mat;
    TiledPlane(const vec3& o, const vec3& n)
    {
        normal = n;
        origin = o;
        mat.reflectance = 0.0f;
        mat.specular = vec3(1.0f, 1.0f, 1.0f);
    }

    virtual const Material& GetMaterial(const vec3& pos) const override
    {
        bool white = ((int(floor(pos.x) + /*floor(pos.y) +*/ floor(pos.z)) & 1) == 0);

        if (white)
        {
            mat.albedo = vec3(1.0f, 1.0f, 1.0f);
        }
        else
        {
            mat.albedo = vec3(0.0f, 0.0f, 0.0f);
        }
        return mat;
    }
    
    virtual vec3 GetSurfaceNormal(const vec3& pos) const
    {
        return normal;
    }
    
    virtual vec3 GetRayFrom(const vec3& from) const override
    {
        return normalize(origin - from);
    }
    
    virtual bool Intersects(const vec3& rayOrigin, const vec3& rayDir, float& distance) const override
    {
        return glm::intersectRayPlane(rayOrigin, glm::normalize(rayDir), origin, normal, distance);
    }
};