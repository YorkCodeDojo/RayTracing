#include "common.h"

#include "writebitmap.h"
#include "sceneobjects.h"
#include "camera.h"

const int ImageWidth = 1024;
const int ImageHeight = 768;
const float FieldOfView = 60.0f;

#define MAX_DEPTH 3

std::vector<std::shared_ptr<SceneObject>> sceneObjects;
std::shared_ptr<Camera> pCamera;


void InitScene()
{
    pCamera = std::make_shared<Camera>(vec3(0.0f, 6.0f, 8.0f),      // Where the camera is
        vec3(0.0f, -.8f, -1.0f),    // The point it is looking at
        FieldOfView,                // The field of view of the 'lens'
        ImageWidth, ImageHeight);   // The size in pixels of the view plane

    // Red ball
    Material mat;
    mat.albedo = vec3(.7f, .1f, .1f);
    mat.specular = vec3(.9f, .1f, .1f);
    mat.reflectance = 0.5f;
    sceneObjects.push_back(std::make_shared<Sphere>(mat, vec3(0.0f, 2.0f, 0.f), 2.0f));

    // Purple ball
    mat.albedo = vec3(0.7f, 0.0f, 0.7f);
    mat.specular = vec3(0.9f, 0.9f, 0.8f);
    mat.reflectance = 0.5f;
    sceneObjects.push_back(std::make_shared<Sphere>(mat, vec3(-2.5f, 1.0f, 2.f), 1.0f));

    // Blue ball
    mat.albedo = vec3(0.0f, 0.3f, 1.0f);
    mat.specular = vec3(0.0f, 0.0f, 1.0f);
    mat.reflectance = 0.0f;
    mat.emissive = vec3(0.0f, 0.0f, 0.0f);
    sceneObjects.push_back(std::make_shared<Sphere>(mat, vec3(-0.0f, 0.5f, 3.f), 0.5f));

    // Yellow ball on floor
    mat.albedo = vec3(1.0f, 1.0f, 1.0f);
    mat.specular = vec3(0.0f, 0.0f, 0.0f);
    mat.reflectance = .0f;
    mat.emissive = vec3(1.0f, 1.0f, 0.2f);
    sceneObjects.push_back(std::make_shared<Sphere>(mat, vec3(2.8f, 0.8f, 2.0f), 0.8f));

    // White light behind and to left of viewer
    mat.albedo = vec3(0.0f, 0.8f, 0.0f);
    mat.specular = vec3(0.0f, 0.0f, 0.0f);
    mat.reflectance = 0.0f;
    mat.emissive = vec3(1.0f, 1.0f, 1.0f);
    sceneObjects.push_back(std::make_shared<Sphere>(mat, vec3(-10.8f, 6.4f, 10.0f), 0.4f));

    sceneObjects.push_back(std::make_shared<TiledPlane>(vec3(0.0f, 0.0f, 0.0f), normalize(vec3(0.0f, 1.0f, 0.0f))));
}

SceneObject* FindNearestObject(vec3 rayorig, vec3 raydir, float& nearestDistance)
{
    SceneObject* nearestObject = nullptr;
    nearestDistance = std::numeric_limits<float>::max();

    // find intersection of this ray with the sphere in the scene
    for (auto pObject : sceneObjects)
    {
        float distance;
        if (pObject->Intersects(rayorig, glm::normalize(raydir), distance) &&
            nearestDistance > distance)
        {
            nearestObject = pObject.get();
            nearestDistance = distance;
        }
    }
    return nearestObject;
}

vec3 TraceRay(const vec3& rayorig, const vec3 &raydir, const int depth)
{
    const SceneObject* nearestObject = nullptr;
    float distance;
    nearestObject = FindNearestObject(rayorig, raydir, distance);

    if (!nearestObject)
    {
        // Can return an 'ambient' color here.
        return vec3{ 0.0f, 0.0f, 0.0f };
    }

    // Find the intersection position, normal, and the material color
    vec3 pos = rayorig + (raydir * distance);
    vec3 normal = nearestObject->GetSurfaceNormal(pos);
    const Material& material = nearestObject->GetMaterial(pos);

    // Output color is initially just the emissive
    vec3 outputColor = material.emissive;

    // Get a reflection ray
    vec3 reflect = glm::normalize(glm::reflect(raydir, normal));

    // If the object is transparent, get the reflection color by bouncing a ray
    if (depth < MAX_DEPTH && (material.reflectance > 0.0f))
    {
        vec3 reflectColor(0.0f, 0.0f, 0.0f);

        // Offset the ray a bit to avoid self-intersection of the ray with the hit object!
        reflectColor = TraceRay(pos + (reflect * 0.001f), reflect, depth + 1);
        outputColor += (reflectColor * material.reflectance);
    }

    // For every emitter, gather the light
    for (auto& emitterObj : sceneObjects)
    {
        float emitterDistance;
        // Find the details of the hit point on the emitter
        vec3 emitterDir = emitterObj->GetRayFrom(pos);
        emitterObj->Intersects(pos + (emitterDir * 0.001f), emitterDir, emitterDistance);
        auto emissiveMat = emitterObj->GetMaterial(pos + emitterDir * emitterDistance);

        // Not an emitter
        if (emissiveMat.emissive == vec3(0.0f))
        {
            continue;
        }

        // Look for occluders
        bool occluded = false;
        for (auto& occluder : sceneObjects)
        {
            // Ignore ourselves
            if (occluder == emitterObj)
            {
                continue;
            }

            // If we hit something, it automatically obscures us ...
            if (occluder->Intersects(pos + (emitterDir * 0.001f), emitterDir, distance))
            {
                // ... if is closer
                if (emitterDistance > distance)
                {
                    occluded = true;
                    break;
                }
            }
        }

        // We can 'see' this light
        if (!occluded)
        {
            // Simple phong lighting
            float specI = 0.0f;

            // Clamp to >= 0.  Because negative means the light points away from the normal
            float diffuseI = std::max(dot(normal, emitterDir), 0.0f);
            if (diffuseI > 0.0f)
            {
                specI = std::max(dot(reflect, emitterDir), 0.0f);

                // Increase the specular by a power to give it a nice falloff
                specI = pow(specI, 10);
            }

            // light intensity * light color * material color + specular intensity * specular color
            outputColor += (emissiveMat.emissive * material.albedo * diffuseI) + (material.specular * specI * emissiveMat.emissive);
        }
    }
    return outputColor;
}

void DrawScene(Bitmap* pBitmap)
{
    for (int y = 0; y < ImageHeight; y++)
    {
        for (int x = 0; x < ImageWidth; x++)
        {
            const int numSamples = 4;
            vec3 color{ 0.0f, 0.0f, 0.0f };

            // Dither pattern for sampling
            static vec2 patterns[4]{ vec2(0.1f, 0.2f), vec2(0.6f, 0.5f), vec2(0.8f, 0.7f), vec2(0.2f, 0.8f) };
            for (auto i = 0; i < numSamples; i++)
            {
                // A sample within the pixel
                vec2 sample(float(x) + patterns[i].x, float(y) + patterns[i].y);

                auto ray = pCamera->GetWorldRay(sample);
                color += TraceRay(pCamera->position, ray, 0);
            }

            // Average the samples
            color *= (1.0f / numSamples);

            // Color might have maxed out, so clamp.
            color = color * 255.0f;
            color = clamp(color, vec3(0.0f, 0.0f, 0.0f), vec3(255.0f, 255.0f, 255.0f));

            PutPixel(pBitmap, x, y, Color{ uint8_t(color.x), uint8_t(color.y),uint8_t(color.z) });
        }
    }
}

void main(void* arg, void** args)
{
    Bitmap* pBitmap = CreateBitmap(ImageWidth, ImageHeight);

    Color col{ 127, 127, 127 };
    ClearBitmap(pBitmap, col);

    InitScene();

    DrawScene(pBitmap);

    WriteBitmap(pBitmap, "image.bmp");

    DestroyBitmap(pBitmap);

    system("start image.bmp");
}
