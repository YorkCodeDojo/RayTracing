#include "common.h"

#include "writebitmap.h"
#include "sceneobjects.h"
#include "camera.h"

const int ImageWidth = 1024;
const int ImageHeight = 768;
const float FieldOfView = 60.0f;
const int MaxDepth = 4;

// A list of objects in the scene
std::vector<std::shared_ptr<SceneObject>> sceneObjects;

// The camera
std::shared_ptr<Camera> pCamera;

void InitScene()
{
    pCamera = std::make_shared<Camera>(vec3(0.0f, 6.0f, 8.0f),      // Where the camera is
        vec3(0.0f, -.8f, -1.0f),                                    // The point it is looking at
        FieldOfView,                                                // The field of view of the 'lens'
        ImageWidth, ImageHeight);                                   // The size in pixels of the view plane

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
    
    // White light (distant)
    mat.albedo = vec3(0.0f, 0.8f, 0.0f);
    mat.specular = vec3(0.0f, 0.0f, 0.0f);
    mat.reflectance = 0.0f;
    mat.emissive = vec3(1.0f, 1.0f, 1.0f);
    sceneObjects.push_back(std::make_shared<Sphere>(mat, vec3(-10.8f, 6.4f, 10.0f), 0.4f));

    // Plane on the ground
    sceneObjects.push_back(std::make_shared<TiledPlane>(vec3(0.0f, 0.0f, 0.0f), normalize(vec3(0.0f, 1.0f, 0.0f))));
}

// Find the nearest object in the scene, given a ray start and direction
SceneObject* FindNearestObject(vec3 rayorig, vec3 raydir, float& nearestDistance)
{
    SceneObject* nearestObject = nullptr;
    nearestDistance = std::numeric_limits<float>::max();

    // find intersection of this ray with the object in the scene
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

// Trace a ray into the scene, return the accumulated light value
vec3 TraceRay(const vec3& rayorig, const vec3 &raydir, const int depth)
{
    // Here's how to print out a vector for inspection
    //std::cout << "Ray: " << glm::to_string(raydir) << std::endl;

    // For now, just convert the incoming ray to a 'color' to display it has it changes 
    return vec3((raydir * .5f) + vec3(0.5f, 0.5f, 0.5f));
}

// Draw the scene by generating camera rays and tracing rays
void DrawScene(Bitmap* pBitmap)
{
    for (int y = 0; y < ImageHeight; y++)
    {
        for (int x = 0; x < ImageWidth; x++)
        {
            // Make a ray pointing into the center of the pixel.
            auto ray = pCamera->GetWorldRay(vec2((float)x + .5f, (float)y + .5f));

            // Trace the ray and get the color
            auto color = TraceRay(pCamera->position, ray, 0);

            // Color might have maxed out, so clamp.
            // Also convert it to 8 bit color
            color = color * 255.0f;
            color = clamp(color, vec3(0.0f, 0.0f, 0.0f), vec3(255.0f, 255.0f, 255.0f));

            // Draw it on the bitmap
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
