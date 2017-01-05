import numpy as np
from raymath import *

class Camera:
    """description of class"""
    def __init__(self, pos, dir, fov, width, height):
        self.position = pos
        self.view_direction = dir
        self.fov = fov
        self.image_width = width
        self.image_height = height

        # The vector to the right of the camera - assume the 'Up' Vector is vertically up
        self.right = np.cross(self.view_direction, np.array([0.0, 1.0, 0.0]))
        
        # The 'up' vector pointing above the camera
        # We recalculate it from the existing right and view direction.
        # Imaging a line pointing up from your head, based on how far forward you are leaning
        self.up = np.cross(self.right, self.view_direction);

        # Ensure our vectors are normalized
        self.up = normalize(self.up);
        self.right = normalize(self.right);
        self.view_direction = normalize(self.view_direction);

        # Aspect ratio if the window is not square
        self.aspect_ratio = self.image_width / self.image_height;

        # The half-width of the viewport, in world space
        self.half_angle = np.tan(np.radians(self.fov) / 2.0)

    # Given a screen coordinate, return a ray leaving the camera and entering the world at that 'pixel'
    def GetWorldRay(self, pixel_x, pixel_y):
        # Could move some of this maths out of here for speed, but this isn't time critical
        dir = np.array(self.view_direction);
        x = ((pixel_x * 2.0) / self.image_width) - 1.0
        y = ((pixel_y * 2.0) / self.image_height) - 1.0

        # Take the view direction and adjust it to point at the given sample, based on the 
        # the frustum 
        dir += (self.right * (self.half_angle * self.aspect_ratio * x));
        dir -= (self.up * (self.half_angle * y));
        dir = normalize(dir);
        return dir
