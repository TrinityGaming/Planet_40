using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;

namespace Planet
{
  public enum Face { Front = 0, Back, Top, Bottom, Left, Right };

  public static class CubeFaces
  {

    private static CubeFace[] faces;


    static CubeFaces()
    {
      faces = new CubeFace[6];
      faces[0] = new CubeFace(Face.Front, GetFaceMatrix(Face.Front));
      faces[1] = new CubeFace(Face.Back, GetFaceMatrix(Face.Back));
      faces[2] = new CubeFace(Face.Top, GetFaceMatrix(Face.Top));
      faces[3] = new CubeFace(Face.Bottom, GetFaceMatrix(Face.Bottom));
      faces[4] = new CubeFace(Face.Left, GetFaceMatrix(Face.Left));
      faces[5] = new CubeFace(Face.Right, GetFaceMatrix(Face.Right));
    }

    public static CubeFace GetFace(Face face)
    {
      return faces[(int)face];
    }

    public static CubeFace Front { get { return GetFace(Face.Front); } }
    public static CubeFace Back { get { return GetFace(Face.Back); } }
    public static CubeFace Top { get { return GetFace(Face.Top); } }
    public static CubeFace Bottom { get { return GetFace(Face.Bottom); } }
    public static CubeFace Left { get { return GetFace(Face.Left); } }
    public static CubeFace Right { get { return GetFace(Face.Right); } }



    private static Matrix GetFaceMatrix(Face face)
    {
      Matrix result;

      switch (face)
      {
        case Face.Front:
          result = Matrix.CreateFromAxisAngle(new Vector3(0, 1, 0), MathHelper.ToRadians(0.0f));
          break;

        case Face.Right:
          result = Matrix.CreateFromAxisAngle(new Vector3(0, 1, 0), MathHelper.ToRadians(90.0f));
          break;

        case Face.Back:
          result = Matrix.CreateFromAxisAngle(new Vector3(0, 1, 0), MathHelper.ToRadians(180.0f));
          break;

        case Face.Left:
          result = Matrix.CreateFromAxisAngle(new Vector3(0, 1, 0), MathHelper.ToRadians(-90.0f));
          break;

        case Face.Top:
          result = Matrix.CreateFromAxisAngle(new Vector3(1, 0, 0), MathHelper.ToRadians(-90.0f));
          break;

        case Face.Bottom:
          result = Matrix.CreateFromAxisAngle(new Vector3(1, 0, 0), MathHelper.ToRadians(90.0f));
          break;

        default:
          result = Matrix.CreateFromAxisAngle(new Vector3(0, 1, 0), MathHelper.ToRadians(0.0f));
          break;
      }

      return result;
    }

  }

  public class CubeFace
  {
    public Face Face { get; private set; }
    public Matrix FaceMatrix { get; private set; }

    public CubeFace(Face face, Matrix faceMatrix)
    {
      this.Face = face;
      this.FaceMatrix = faceMatrix;
    }
  }
}
