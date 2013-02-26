This implements an Earth-sized, noise-generated planet, using XNA. The planet height maps, textures, and normal maps are 
generated on the GPU and retrieved to build the geometry. Quad-tree based subdivision is used to allow seamlessly roaming
the planet from space, all the way down to the surface level.

As mentioned, the planet is earth-sized. The sun is accurately sized as well and you can fly to it. It is not rendered
in any great detail however.

The terrain node generation is much faster than it was on the CPU, but still isn't optimial by any stretch of the imagination.
Whe moving quickly toward the surface there is a lot of catching up it has to do. If you move at a normal pace once near
the surface it does a pretty decent job of keeping up though.

Finally got this converted over to XNA 4. Not everything survived the conversion, mostly having
to do with PointList being no longer supported. PointList was used for the stars in the space dome,
and for finding occlusion to fade out the lens flares. The "texture pack" based texture generation
isn't working properly after the conversion either - the terrain heights are somehow scaled
differently - so currently the gradient texture generation is enabled.

At one point this was runnable on the Xbox, albeit very slowly. I haven't tested it since the XNA 4 conversion, but
it should be able to work with minimal effort. Although it uses so much memory it's more of a cool thing to do
rather than anything useful. 

Licensed under the MIT License (http://opensource.org/licenses/MIT). Please see license.txt for details.

Some of the code, particularly the noise code in both C# and HLSL, is based on Perlin's work. The atmospheric
scattering shaders are based on Sean O'Neil's atmospheric scattering work. Some of it is from various XNA
examples. There's also the occasional piece of code gathered from various discussion forums throughout the web.
I tried to document this in the code, but I'm sure I missed it in places. I'm fairly certain that all the code
is freely usable, but I never intended to release this publicly, so I didn't do a very good job of keeping track. 

Finally, much of the code is quite ugly, particularly that in PlanetGame.cs. Some of it is better, such as the planet
generation classes. None of it is brilliant.

Good luck!  I hope someone finds at least a few moments of entertainment from this, if not usefulness.


---- Usage -----


Key           Function
-----------------------
Esc           Exit
H             Toggle HUD display
T             Toggle driving tank on the surface - this may or may not work correctly
O             Toggle frustum camera - allows viewing frustum culling from "outside" - terrain node
              culling will be based on a separte camera, allowing you to see what's being cullded,
              use the normal camera movement keys to move the viewing camera, and the arrow keys
              to move the frustum camera

R             Reset camera to starting position - this is useful if you get lost and can't find the planet
M             Change render mode between Solid and Wireframe - this is useful to see the terrain node splitting in action
P             Change planet display mode to show just geometry with colors, generated textures, bump mapping, or normal map

WASD+Mouse    Camera movement and strafing
Mouse Wheel   Change forward speed - this can be cranked up quite fast to allow flying to the sun - if you get lost press R

