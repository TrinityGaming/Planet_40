using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.IO;
using System.Xml.Serialization;

#if !XBOX
using System.Runtime.Serialization.Formatters.Binary;
#endif

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;





namespace Willow
{
  public static class Tools
  {
    private static Texture2D whiteDotTexture;

    private static void CreateWhiteDotTexture(GraphicsDevice graphicsDevice)
    {
      Color[] colors = { Color.White };
      whiteDotTexture = new Texture2D(graphicsDevice, 1, 1, false, SurfaceFormat.Color);
      whiteDotTexture.SetData(colors);
    }


    public static Texture2D GetWhiteDotTexture(GraphicsDevice graphicsDevice)
    {
      if (whiteDotTexture == null)
        CreateWhiteDotTexture(graphicsDevice);

      return whiteDotTexture;
    }

    public static void DrawWindow(SpriteBatch sprites, Rectangle bounds, Color color)
    {
      if (whiteDotTexture == null)
        CreateWhiteDotTexture(sprites.GraphicsDevice);

      sprites.Draw(whiteDotTexture, bounds, color);
    }


    public static void DrawWindowBorder(SpriteBatch sprites, Rectangle bounds, Color color, int Width)
    {
      if (whiteDotTexture == null)
        CreateWhiteDotTexture(sprites.GraphicsDevice);

      // top
      sprites.Draw(whiteDotTexture, new Rectangle(bounds.Left + Width, bounds.Top, bounds.Width - Width * 2, Width), color);

      // bottom
      sprites.Draw(whiteDotTexture, new Rectangle(bounds.Left + Width, bounds.Bottom - Width, bounds.Width - Width * 2, Width), color);

      // left
      sprites.Draw(whiteDotTexture, new Rectangle(bounds.Left, bounds.Top, Width, bounds.Height), color);

      // right
      sprites.Draw(whiteDotTexture, new Rectangle(bounds.Right - Width, bounds.Top, Width, bounds.Height), color);
    }


    public static void DrawWindowBorder(SpriteBatch sprites, Rectangle bounds, Color color)
    {
      DrawWindowBorder(sprites, bounds, color, 2);
    }


    /// <summary>
    /// Given the 3 vertices (position and texture coordinate) and the
    /// face normal of a triangle calculate and return the triangle's
    /// tangent vector. This method is designed to work with XNA's default
    /// right handed coordinate system and clockwise triangle winding order.
    /// Undefined behavior will result if any other coordinate system
    /// and/or winding order is used. The handedness of the local tangent
    /// space coordinate system is stored in the tangent's w component.
    /// </summary>
    /// <param name="pos1">Triangle vertex 1 position</param>
    /// <param name="pos2">Triangle vertex 2 position</param>
    /// <param name="pos3">Triangle vertex 3 position</param>
    /// <param name="texCoord1">Triangle vertex 1 texture coordinate</param>
    /// <param name="texCoord2">Triangle vertex 2 texture coordinate</param>
    /// <param name="texCoord3">Triangle vertex 3 texture coordinate</param>
    /// <param name="normal">Triangle face normal</param>
    /// <param name="tangent">Calculated tangent vector</param>
    public static void CalcTangent(ref Vector3 pos1,
                                   ref Vector3 pos2,
                                   ref Vector3 pos3,
                                   ref Vector2 texCoord1,
                                   ref Vector2 texCoord2,
                                   ref Vector2 texCoord3,
                                   ref Vector3 normal,
                                   out Vector4 tangent)
    {
      // Create 2 vectors in object space.
      // edge1 is the vector from vertex positions pos1 to pos3.
      // edge2 is the vector from vertex positions pos1 to pos2.
      Vector3 edge1 = pos3 - pos1;
      Vector3 edge2 = pos2 - pos1;

      edge1.Normalize();
      edge2.Normalize();

      // Create 2 vectors in tangent (texture) space that point in the
      // same direction as edge1 and edge2 (in object space).
      // texEdge1 is the vector from texture coordinates texCoord1 to texCoord3.
      // texEdge2 is the vector from texture coordinates texCoord1 to texCoord2.
      Vector2 texEdge1 = texCoord3 - texCoord1;
      Vector2 texEdge2 = texCoord2 - texCoord1;

      texEdge1.Normalize();
      texEdge2.Normalize();

      // These 2 sets of vectors form the following system of equations:
      //
      //  edge1 = (texEdge1.x * tangent) + (texEdge1.y * bitangent)
      //  edge2 = (texEdge2.x * tangent) + (texEdge2.y * bitangent)
      //
      // Using matrix notation this system looks like:
      //
      //  [ edge1 ]     [ texEdge1.x  texEdge1.y ]  [ tangent   ]
      //  [       ]  =  [                        ]  [           ]
      //  [ edge2 ]     [ texEdge2.x  texEdge2.y ]  [ bitangent ]
      //
      // The solution is:
      //
      //  [ tangent   ]        1     [ texEdge2.y  -texEdge1.y ]  [ edge1 ]
      //  [           ]  =  -------  [                         ]  [       ]
      //  [ bitangent ]      det A   [-texEdge2.x   texEdge1.x ]  [ edge2 ]
      //
      //  where:
      //        [ texEdge1.x  texEdge1.y ]
      //    A = [                        ]
      //        [ texEdge2.x  texEdge2.y ]
      //
      //    det A = (texEdge1.x * texEdge2.y) - (texEdge1.y * texEdge2.x)
      //
      // From this solution the tangent space basis vectors are:
      //
      //    tangent = (1 / det A) * ( texEdge2.y * edge1 - texEdge1.y * edge2)
      //  bitangent = (1 / det A) * (-texEdge2.x * edge1 + texEdge1.x * edge2)
      //     normal = cross(tangent, bitangent)

      Vector3 t;
      Vector3 b;
      float det = (texEdge1.X * texEdge2.Y) - (texEdge1.Y * texEdge2.X);

      if ((float)Math.Abs(det) < 1e-6f)    // almost equal to zero
      {
        t = Vector3.UnitX;
        b = Vector3.UnitY;
      }
      else
      {
        det = 1.0f / det;

        t = Vector3.Zero;
        b = Vector3.Zero;

        t.X = (texEdge2.Y * edge1.X - texEdge1.Y * edge2.X) * det;
        t.Y = (texEdge2.Y * edge1.Y - texEdge1.Y * edge2.Y) * det;
        t.Z = (texEdge2.Y * edge1.Z - texEdge1.Y * edge2.Z) * det;

        b.X = (-texEdge2.X * edge1.X + texEdge1.X * edge2.X) * det;
        b.Y = (-texEdge2.X * edge1.Y + texEdge1.X * edge2.Y) * det;
        b.Z = (-texEdge2.X * edge1.Z + texEdge1.X * edge2.Z) * det;

        t.Normalize();
        b.Normalize();
      }

      // Calculate the handedness of the local tangent space.
      // The bitangent vector is the cross product between the triangle face
      // normal vector and the calculated tangent vector. The resulting bitangent
      // vector should be the same as the bitangent vector calculated from the
      // set of linear equations above. If they point in different directions
      // then we need to invert the cross product calculated bitangent vector. We
      // store this scalar multiplier in the tangent vector's 'w' component so
      // that the correct bitangent vector can be generated in the normal mapping
      // shader's vertex shader.

      Vector3 bitangent = Vector3.Cross(normal, t);
      float handedness = (Vector3.Dot(bitangent, b) < 0.0f) ? -1.0f : 1.0f;

      tangent = Vector4.Zero;
      tangent.X = t.X;
      tangent.Y = t.Y;
      tangent.Z = t.Z;
      tangent.W = handedness;
    }


    //public static float LerpAngle(float start, float end, float scale)
    //{
    //  float delta = end - start;

    //  // If delta is not the shortest way, travel in the opposite direction   
    //  if (delta > MathHelper.Pi)
    //  {
    //    return (MathHelper.WrapAngle(start - (MathHelper.TwoPi - delta) * scale));
    //  }
    //  else if (delta < -MathHelper.Pi)
    //  {
    //    return (MathHelper.WrapAngle(start + (MathHelper.TwoPi + delta) * scale));
    //  }

    //  // No special case needed   
    //  return (start + delta * scale);
    //}


    public static float WrapAngleDegrees(float degrees)
    {
      if (degrees < -180)
        degrees += 360;

      if (degrees > 180)
        degrees -= 360;

      return degrees;
    }


    public static float WrapAngle(float radians)
    {
      //return MathHelper.WrapAngle(radians);

      if (radians < -MathHelper.Pi)
        radians += MathHelper.TwoPi;

      if (radians > MathHelper.Pi)
        radians -= MathHelper.TwoPi;

      return radians;
    }


    public static bool LineCircleIntersect(Vector2 circlePosition, float radius, Vector2 p1, Vector2 p2, out Vector2 intersection1,
                                           out Vector2 intersection2)
    {
      double a, b, c;
      double mu;


      // Quadratic equations

      a = (p2.X - p1.X) * (p2.X - p1.X) + (p2.Y - p1.Y) * (p2.Y - p1.Y);

      b = 2 * ((p2.X - p1.X) * (p1.X - circlePosition.X) + (p2.Y - p1.Y) * (p1.Y - circlePosition.Y));

      c = (circlePosition.X * circlePosition.X) + (circlePosition.Y * circlePosition.Y) +
          (p1.X * p1.X) + (p1.Y * p1.Y) -
          2 * (circlePosition.X * p1.X + circlePosition.Y * p1.Y) -
          (radius * radius);


      double discriminant = b * b - 4 * a * c;

      if (discriminant == 0)
      {

        // one intersection
        mu = -b / (2 * a);
        intersection1 = new Vector2((float)(p1.X + mu * (p2.X - p1.X)), (float)(p1.Y + mu * (p2.Y - p1.Y)));
        intersection2 = Vector2.Zero;
      }

      else if (discriminant > 0.0)
      {
        // first intersection
        mu = (-b - Math.Sqrt((b * b) - 4 * a * c)) / (2 * a);
        intersection1 = new Vector2((float)(p1.X + mu * (p2.X - p1.X)), (float)(p1.Y + mu * (p2.Y - p1.Y)));

        // second intersection
        mu = (-b + Math.Sqrt((b * b) - 4 * a * c)) / (2 * a);
        intersection2 = new Vector2((float)(p1.X + mu * (p2.X - p1.X)), (float)(p1.Y + mu * (p2.Y - p1.Y)));


      }
      else
      {
        intersection1 = Vector2.Zero;
        intersection2 = Vector2.Zero;
      }


      return Tools.PointWithinSegment(p1, p2, intersection1) ||
             Tools.PointWithinSegment(p1, p2, intersection2);
    }


    public static bool PointWithinSegment(Vector2 p1, Vector2 p2, Vector2 point)
    {
      if (p1.X > p2.X)
      {
        if (point.X > p1.X || point.X < p2.X)
        {
          return false;
        }
      }
      else
      {
        if (point.X > p2.X || point.X < p1.X)
        {
          return false;
        }
      }


      if (p1.Y > p2.Y)
      {
        if (point.Y > p1.Y || point.Y < p2.Y)
        {
          return false;
        }
      }

      else
      {
        if (point.Y > p2.Y || point.Y < p1.Y)
        {
          return false;
        }
      }

      return true;
    }


    public static void Run<T>() where T : Game, new()
    {
      if (Debugger.IsAttached)
      {
        using (var g = new T())
          g.Run();
      }
      else
      {
        try
        {
          using (var g = new T())
            g.Run();
        }
        catch (Exception e)
        {
          using (var g = new ExceptionGame(e))
            g.Run();
        }
      }
    }


    public static object DeepClone(object obj)
    {
#if XBOX
      return DeepCloneXml(obj);
#else
      return DeepCloneBinary(obj);
#endif
    }

    public static object DeepCloneXml(object obj)
    {
      if (obj == null) return null;

      object result = null;

      using (MemoryStream ms = new MemoryStream())
      {
        XmlSerializer serializer = new XmlSerializer(obj.GetType());
        serializer.Serialize(ms, obj);

        ms.Position = 0;
        result = serializer.Deserialize(ms);
      }

      return result;
    }

#if !XBOX
    public static object DeepCloneBinary(object obj)
    {
      // Create a "deep" clone of 
      // an object. That is, copy not only
      // the object and its pointers
      // to other objects, but create 
      // copies of all the subsidiary 
      // objects as well. This code even 
      // handles recursive relationships.

      object result = null;

      using (MemoryStream ms = new MemoryStream())
      {
        BinaryFormatter bf = new BinaryFormatter();
        bf.Serialize(ms, obj);

        // Rewind back to the beginning 
        // of the memory stream. 
        // Deserialize the data, then
        // close the memory stream.
        ms.Position = 0;
        result = bf.Deserialize(ms);
      }

      return result;
    }
#endif


    /*
    // r,g,b values are from 0 to 1
    // h = [0,360], s = [0,1], v = [0,1]
    //		if s == 0, then h = -1 (undefined)
    public static Color RGBtoHSV(float r, float g, float b)
    {
      float min, max, delta;
      min = Math.Min(Math.Min(r, g), b);
      max = Math.Max(Math.Max(r, g), b);

      float h;
      float s;
      float v;

      v = max;				// v
      delta = max - min;
      if (max != 0)
        *s = delta / max;		// s
      else
      {
        // r = g = b = 0		// s = 0, v is undefined
        *s = 0;
        *h = -1;
        return;
      }
      if (r == max)
        *h = (g - b) / delta;		// between yellow & magenta
      else if (g == max)
        *h = 2 + (b - r) / delta;	// between cyan & yellow
      else
        *h = 4 + (r - g) / delta;	// between magenta & cyan
      *h *= 60;				// degrees
      if (*h < 0)
        *h += 360;
    }
    */


    public static Color HSVtoRGB(float h, float s, float v)
    {
      int i;
      float r, g, b;
      float f, p, q, t;


      if (s == 0)
      {
        // achromatic (grey)
        return new Color(v, v, v, 1.0f);
      }

      h /= 60.0f; // sector 0 to 5
      i = (int)Math.Floor(h);
      f = h - i;			// factorial part of h
      p = v * (1 - s);
      q = v * (1 - s * f);
      t = v * (1 - s * (1 - f));

      switch (i)
      {
        case 0:
          r = v;
          g = t;
          b = p;
          break;
        case 1:
          r = q;
          g = v;
          b = p;
          break;
        case 2:
          r = p;
          g = v;
          b = t;
          break;
        case 3:
          r = p;
          g = q;
          b = v;
          break;
        case 4:
          r = t;
          g = p;
          b = v;
          break;
        default:		// case 5:
          r = v;
          g = p;
          b = q;
          break;
      }

      return new Color(r, g, b, 1.0f);
    }


    /// Given coordinates in the [-1,1] range, maps the vector as if it were
    /// a cube deformed into a sphere. The output vector is on the surface of
    /// the unit sphere.
    /// URL: http://mathproofs.blogspot.com/2005/07/mapping-cube-to-sphere.html
    public static Position3 CubeToSphereMapping(double x, double y, double z)
    {
      double x2 = x * x;
      double y2 = y * y;
      double z2 = z * z;
      double x2Half = x2 * 0.5;
      double y2Half = y2 * 0.5;
      double z2Half = z2 * 0.5;

      return new Position3(x * Math.Sqrt(1.0f - y2Half - z2Half + y2 * z2 / 3.0f),
                           y * Math.Sqrt(1.0f - z2Half - x2Half + z2 * x2 / 3.0f),
                           z * Math.Sqrt(1.0f - x2Half - y2Half + x2 * y2 / 3.0f));
    }

    /// Given coordinates in the [-1,1] range, maps the vector as if it were
    /// a cube deformed into a sphere. The output vector is on the surface of
    /// the unit sphere.
    /// URL: http://mathproofs.blogspot.com/2005/07/mapping-cube-to-sphere.html
    public static Vector3 CubeToSphereMapping(float x, float y, float z)
    {
      float x2 = x * x;
      float y2 = y * y;
      float z2 = z * z;
      float x2Half = x2 * 0.5f;
      float y2Half = y2 * 0.5f;
      float z2Half = z2 * 0.5f;

      return new Vector3(x * (float)Math.Sqrt(1.0f - y2Half - z2Half + y2 * z2 / 3.0f),
                         y * (float)Math.Sqrt(1.0f - z2Half - x2Half + z2 * x2 / 3.0f),
                         z * (float)Math.Sqrt(1.0f - x2Half - y2Half + x2 * y2 / 3.0f));
    }


    /// <summary>
    /// Checks whether a ray intersects a triangle. This uses the algorithm
    /// developed by Tomas Moller and Ben Trumbore, which was published in the
    /// Journal of Graphics Tools, volume 2, "Fast, Minimum Storage Ray-Triangle
    /// Intersection".
    /// 
    /// This method is implemented using the pass-by-reference versions of the
    /// XNA math functions. Using these overloads is generally not recommended,
    /// because they make the code less readable than the normal pass-by-value
    /// versions. This method can be called very frequently in a tight inner loop,
    /// however, so in this particular case the performance benefits from passing
    /// everything by reference outweigh the loss of readability.
    /// </summary>
    public static void RayIntersectsTriangle(ref Ray ray,
                                      ref Vector3 vertex1,
                                      ref Vector3 vertex2,
                                      ref Vector3 vertex3, out float? result,
                 out float u,
                 out float v)
    {
      // Compute vectors along two edges of the triangle.
      Vector3 edge1, edge2;

      Vector3.Subtract(ref vertex2, ref vertex1, out edge1);
      Vector3.Subtract(ref vertex3, ref vertex1, out edge2);

      // Compute the determinant.
      Vector3 directionCrossEdge2;
      Vector3.Cross(ref ray.Direction, ref edge2, out directionCrossEdge2);

      float determinant;
      Vector3.Dot(ref edge1, ref directionCrossEdge2, out determinant);

      // If the ray is parallel to the triangle plane, there is no collision.
      if (determinant > -float.Epsilon && determinant < float.Epsilon)
      {
        result = null;
        u = 0;
        v = 0;
        return;
      }

      float inverseDeterminant = 1.0f / determinant;

      // Calculate the U parameter of the intersection point.
      Vector3 distanceVector;
      Vector3.Subtract(ref ray.Position, ref vertex1, out distanceVector);

      float triangleU;
      Vector3.Dot(ref distanceVector, ref directionCrossEdge2, out triangleU);
      triangleU *= inverseDeterminant;

      // Make sure it is inside the triangle.
      if (triangleU < 0 || triangleU > 1)
      {
        result = null;
        u = 0;
        v = 0;
        return;
      }

      // Calculate the V parameter of the intersection point.
      Vector3 distanceCrossEdge1;
      Vector3.Cross(ref distanceVector, ref edge1, out distanceCrossEdge1);

      float triangleV;
      Vector3.Dot(ref ray.Direction, ref distanceCrossEdge1, out triangleV);
      triangleV *= inverseDeterminant;

      // Make sure it is inside the triangle.
      if (triangleV < 0 || triangleU + triangleV > 1)
      {
        result = null;
        u = 0;
        v = 0;
        return;
      }

      // Compute the distance along the ray to the triangle.
      float rayDistance;
      Vector3.Dot(ref edge2, ref distanceCrossEdge1, out rayDistance);
      rayDistance *= inverseDeterminant;

      // Is the triangle behind the ray origin?
      if (rayDistance < 0)
      {
        result = null;
        u = 0;
        v = 0;
        return;
      }

      u = triangleU;
      v = triangleV;
      result = rayDistance;
    }
  }
}
