import numpy as np
import matplotlib.pyplot as plt
from raymath import *
import camera

# Make smaller for faster tracing
w = 128
h = 128

def intersect(O, D, obj):
    if obj['type'] == 'plane':
        return intersect_plane(O, D, obj['position'], obj['normal'])
    elif obj['type'] == 'sphere':
        return intersect_sphere(O, D, obj['position'], obj['radius'])

def get_normal(obj, M):
    # Find normal.
    if obj['type'] == 'sphere':
        N = normalize(M - obj['position'])
    elif obj['type'] == 'plane':
        N = obj['normal']
    return N

def get_color(obj, M):
    color = obj['color']
    if not hasattr(color, '__len__'):
        color = color(M)
    return color

def get_specular(obj, M):
    color = obj['specular']
    if not hasattr(specular, '__len__'):
        specular = specular(M)
    return specular

def add_sphere(position, radius, color, specular, emissive, reflection):
    return dict(type='sphere', position=np.array(position), 
        radius=np.array(radius), color=np.array(color), specular=np.array(specular), emissive=np.array(emissive), reflection=reflection)

def add_plane(position, normal):
    return dict(type='plane', position=np.array(position), 
        normal=np.array(normal),
        color=lambda M: (color_plane0 
            if (int(M[0] * 2) % 2) == (int(M[2] * 2) % 2) else color_plane1),
        diffuse_c=.75, specular_c=.5, reflection=.25)


# List of scene objects.
color_plane0 = 1. * np.ones(3)
color_plane1 = 0. * np.ones(3)
scene = [add_sphere([0.0, 2.0, 0.0], 2.0, [0.7, 0.1, .1], [0.9, 0.1, 0.1], [0.0, 0.0, 0.0], .5),   # Red
         add_sphere([-2.5, 1.0, 2.0], 1.0, [0.7, 0.0, 0.7], [0.9, 0.9, 0.8], [0.0, 0.0, 0.0], .5), # Purple
         add_sphere([-0.0, 0.5, 3.0], .5, [0.0, 0.3, 1.0], [0.0, 0.0, 1.0], [0.0, 0.0, 0.0], 0.0), # Blue
         #add_sphere([2.8, 0.8, 2.0], .8, [1.0, 1.0, 1.0], [0.0, 0.0, 0.0], [1.0, 1.0, 0.2], 0.0), # Light ball
         #add_sphere([-10.8, 6.4, 10.0], .4, [0.0, 0.8, 0.0], [0.0, 0.0, 0.0], [1.0, 1.0, 1.0], 0.0), # Light ball
         add_plane([0., 0.0, 0.], [0., 1., 0.]),
    ]

# Light position and color, if you just want the global light and not a sphere.
# Note - you can use the 'emissive' property of the spheres to find the lights...
L = np.array([-10.8, 6.4, 10.])
color_light = np.ones(3)

col = np.zeros(3)  # Current color.
O = np.array([0., 6., 8.0])  # Camera.
Q = np.array([0., -.8, -1.0])  # Camera pointing to.
img = np.zeros((h, w, 3))

# Screen coordinates: x0, y0, x1, y1.
S = (-8.0, -8.0, 8.0, 8.0)

depth_max = 3

cam = camera.Camera(O, Q, 60, w, h)

# For now, just return the ray through the pixel as a 'color'
def trace_ray(rayO, rayD, current_depth):
    return (rayD * .5) + np.array([.5, .5, .5])

# Loop through all pixels.
for i, x in enumerate(np.linspace(S[0], S[2], w)):
    if i % 10 == 0:
        print(i / float(w) * 100, "%")

    for j, y in enumerate(np.linspace(S[1], S[3], h)):
        col[:] = 0
        rayO = O
        rayD = cam.GetWorldRay(i, h - j - 1)
        col = trace_ray(rayO, rayD, 0)

        img[h - j - 1, i, :] = np.clip(col, 0, 1)

plt.imsave('out.png', img)
plt.imshow(img)
plt.show()

# NOTE:
# Here's how to make a reflection ray, given a Normal and a ray direction
# normalize(rayD - 2 * np.dot(rayD, N) * N)
