#pragma once

// A simple camera
struct Camera
{
    vec3 position;          // Position of the camera in world space
    vec3 viewDirection;     // The direction the camera is looking in
    vec3 right;             // The vector to the right
    vec3 up;                // The vector up

    float aspectRatio;      // Ratio of x to y of the viewport

    float halfAngle;        // Half angle of the view frustum

    float imageWidth;       // Width/height of the view plane
    float imageHeight;

    Camera(const vec3& cameraPosition, const vec3& dir, float fov, int width, unsigned int height)
    {
        position = cameraPosition;

        viewDirection = dir;

        imageWidth = (float)width;
        imageHeight = (float)height;

        // The vector to the right of the camera - assume the 'Up' Vector is vertically up
        right = glm::cross(viewDirection, vec3(0.0f, 1.0, 0.0));

        // The 'up' vector pointing above the camera
        // We recalculate it from the existing right and view direction.
        // Imaging a line pointing up from your head, based on how far forward you are leaning
        up = glm::cross(right, viewDirection);

        // Ensure our vectors are normalized
        up = normalize(up);
        right = normalize(right);
        viewDirection = normalize(viewDirection);

        // Aspect ratio if the window is not square
        aspectRatio = float(width) / float(height);

        // The half-width of the viewport, in world space
        halfAngle = float(tan(glm::radians(fov) / 2.0));
    }

    // Given a screen coordinate, return a ray leaving the camera and entering the world at that 'pixel'
    vec3 GetWorldRay(const vec2& imageSample)
    {
        // Could move some of this maths out of here for speed, but this isn't time critical
        vec3 dir(viewDirection);
        float x = ((imageSample.x * 2.0f) / imageWidth) - 1.0f;
        float y = ((imageSample.y * 2.0f) / imageHeight) - 1.0f;

        // Take the view direction and adjust it to point at the given sample, based on the 
        // the frustum 
        dir += (right * (halfAngle * aspectRatio * x));
        dir -= (up * (halfAngle * y));
        dir = normalize(dir);

        return dir;
    }
};