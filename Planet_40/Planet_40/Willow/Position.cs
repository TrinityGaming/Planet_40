using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;

namespace Willow
{

  public struct Position3
  {
    public double X;
    public double Y;
    public double Z;

    public Position3(double value)
    {
      X = value;
      Y = value;
      Z = value;
    }

    public Position3(double x, double y, double z)
    {
      X = x;
      Y = y;
      Z = z;
    }

    public Position3(float x, float y, float z)
    {
      X = x;
      Y = y;
      Z = z;
    }

    public Position3(Position3 V)
    {
      X = V.X;
      Y = V.Y;
      Z = V.Z;
    }

    public Position3(Vector3 V)
    {
      X = V.X;
      Y = V.Y;
      Z = V.Z;
    }


    public static Position3 Zero { get { return new Position3(0); } }

    public void Clear()
    {
      X = 0;
      Y = 0;
      Z = 0;
    }


    // cast
    public static explicit operator Vector3(Position3 V1)
    {
      return new Vector3((float)V1.X, (float)V1.Y, (float)V1.Z);
    }

    public static explicit operator Position3(Vector3 V1)
    {
      return new Position3(V1);
    }


    // logical
    public static bool operator ==(Position3 V1, Position3 V2)
    {
      return (V1.X == V2.X && V1.Y == V2.Y && V1.Z == V2.Z);
    }

    public static bool operator !=(Position3 V1, Position3 V2)
    {
      return (V1.X != V2.X || V1.Y != V2.Y || V1.Z != V2.Z);
    }


    // add
    public static Position3 operator +(Position3 V1, double Scalar)
    {
      return new Position3(V1.X + Scalar, V1.Y + Scalar, V1.Z + Scalar);
    }

    public static Position3 operator +(Position3 V1, Position3 V2)
    {
      return new Position3(V1.X + V2.X, V1.Y + V2.Y, V1.Z + V2.Z);
    }


    // subtract
    public static Position3 operator -(Position3 V1)
    {
      return new Position3(-V1.X, -V1.Y, -V1.Z);
    }

    public static Position3 operator -(Position3 V1, double Scalar)
    {
      return new Position3(V1.X - Scalar, V1.Y - Scalar, V1.Z - Scalar);
    }

    public static Position3 operator -(Position3 V1, Position3 V2)
    {
      return new Position3(V1.X - V2.X, V1.Y - V2.Y, V1.Z - V2.Z);
    }


    // multiply
    public static Position3 operator *(Position3 V1, double Scalar)
    {
      return new Position3(V1.X * Scalar, V1.Y * Scalar, V1.Z * Scalar);
    }

    public static Position3 operator *(double Scalar, Position3 V1)
    {
      return new Position3(V1.X * Scalar, V1.Y * Scalar, V1.Z * Scalar);
    }


    // divide
    public static Position3 operator /(Position3 V1, double Scalar)
    {
      return new Position3(V1.X / Scalar, V1.Y / Scalar, V1.Z / Scalar);
    }

    public static Position3 operator /(double Scalar, Position3 V1)
    {
      return new Position3(V1.X / Scalar, V1.Y / Scalar, V1.Z / Scalar);
    }


    public override bool Equals(object obj)
    {
      // return false if object is null
      if (obj == null) return false;

      // return false if the object can't be cast to a Position3
      Position3 P = (Position3)obj; //  as Position3;
      if ((System.Object)P == null) return false;

      return (X == ((Position3)obj).X &&
              Y == ((Position3)obj).Y &&
              Z == ((Position3)obj).Z);
    }

    public bool Equals(Position3 P)
    {
      // if (P == null) return false;

      return (X == P.X &&
              Y == P.Y &&
              Z == P.Z);
    }

    public override int GetHashCode()
    {
      return ToString().GetHashCode();
    }

    public override string ToString()
    {
      return String.Format("<{0},{1},{2}>", X, Y, Z);
    }

    public void Add(double Scalar)
    {
      X += Scalar;
      Y += Scalar;
      Z += Scalar;
    }

    public void Add(Position3 V1)
    {
      X += V1.X;
      Y += V1.Y;
      Z += V1.Z;
    }

    public void Subtract(double Scalar)
    {
      X -= Scalar;
      Y -= Scalar;
      Z -= Scalar;
    }

    public void Subtract(Position3 V1)
    {
      X -= V1.X;
      Y -= V1.Y;
      Z -= V1.Z;
    }

    public void Multiply(double Scalar)
    {
      X *= Scalar;
      Y *= Scalar;
      Z *= Scalar;
    }


    public void Divide(double Scalar)
    {
      X /= Scalar;
      Y /= Scalar;
      Z /= Scalar;
    }

    public double Length()
    {
      return Math.Sqrt(X * X + Y * Y + Z * Z);
    }

    public double LengthSquared()
    {
      return X * X + Y * Y + Z * Z;
    }

    public void Normalize()
    {
      double L = 1 / Math.Sqrt(X * X + Y * Y + Z * Z);
      X *= L;
      Y *= L;
      Z *= L;
    }


    public static double Dot(Position3 V1, Position3 V2)
    {
      return V1.X * V2.X + V1.Y * V2.Y + V1.Z * V2.Z;
    }

    public static Position3 Cross(Position3 V1, Position3 V2)
    {
      return new Position3(V1.Y * V2.Z - V1.Z * V2.Y,
                           V1.Z * V2.X - V1.X * V2.Z,
                           V1.X * V2.Y - V1.Y * V2.X);
    }

    public static double Distance(Position3 V1, Position3 V2)
    {
      return (V2 - V1).Length();
    }


    public void Add(Position3 v1, Position3 v2)
    {
      X = v1.X + v2.X;
      Y = v1.Y + v2.Y;
      Z = v1.Z + v2.Z;
    }

    public void Add(ref Position3 V1, ref Position3 V2)
    {
      X = V1.X + V2.X;
      Y = V1.Y + V2.Y;
      Z = V1.Z + V2.Z;
    }

    public void Subtract(ref Position3 V1, ref Position3 V2)
    {
      X = V1.X - V2.X;
      Y = V1.Y - V2.Y;
      Z = V1.Z - V2.Z;
    }

    public void Subtract(ref Position3 V1)
    {
      X -= V1.X;
      Y -= V1.Y;
      Z -= V1.Z;
    }

    public void SubtractFrom(ref Position3 V1)
    {
      X = V1.X - X;
      Y = V1.Y - Y;
      Z = V1.Z - Z;
    }

    public void AddNormalize(ref Position3 V1, ref Position3 V2)
    {
      X = V1.X + V2.X;
      Y = V1.Y + V2.Y;
      Z = V1.Z + V2.Z;

      double L = 1 / Math.Sqrt(X * X + Y * Y + Z * Z);
      X *= L;
      Y *= L;
      Z *= L;
    }

    public void SubtractNormalize(ref Position3 V1, ref Position3 V2)
    {
      X = V1.X - V2.X;
      Y = V1.Y - V2.Y;
      Z = V1.Z - V2.Z;

      double L = 1 / Math.Sqrt(X * X + Y * Y + Z * Z);
      X *= L;
      Y *= L;
      Z *= L;
    }


    public void Add(Vector3 V1)
    {
      X += V1.X;
      Y += V1.Y;
      Z += V1.Z;
    }

    public Vector3 AsVector3 { get { return new Vector3((float)X, (float)Y, (float)Z); } }

    public static double Distance(Vector3 V1, Position3 V2)
    {
      Position3 V = new Position3(V1);
      return (V2 - V).Length();
    }


    public void AddNormalize(ref Position3 V1, ref Vector3 V2)
    {
      X = V1.X + V2.X;
      Y = V1.Y + V2.Y;
      Z = V1.Z + V2.Z;

      double L = 1 / Math.Sqrt(X * X + Y * Y + Z * Z);
      X *= L;
      Y *= L;
      Z *= L;
    }

    public void SubtractNormalize(ref Position3 V1, ref Vector3 V2)
    {
      X = V1.X - V2.X;
      Y = V1.Y - V2.Y;
      Z = V1.Z - V2.Z;

      double L = 1 / Math.Sqrt(X * X + Y * Y + Z * Z);
      X *= L;
      Y *= L;
      Z *= L;
    }

    public void Transform(ref Matrix M)
    {
      double X;
      double Y;
      double Z;
      double W;

      X = this.X * M.M11 + this.Y * M.M21 + this.Z * M.M31 + M.M41;
      Y = this.X * M.M12 + this.Y * M.M22 + this.Z * M.M32 + M.M42;
      Z = this.X * M.M13 + this.Y * M.M23 + this.Z * M.M33 + M.M43;
      W = this.X * M.M14 + this.Y * M.M24 + this.Z * M.M34 + M.M44;

      if (W > -1.0e-5 && W < 1.0e-5)
      {
        this.X = 0;
        this.Y = 0;
        this.Z = 0;
      }
      else
      {
        this.X = X / W;
        this.Y = Y / W;
        this.Z = Z / W;
      }
    }


    public void Transform(Matrix M)
    {
      double X;
      double Y;
      double Z;
      double W;

      X = this.X * M.M11 + this.Y * M.M21 + this.Z * M.M31 + M.M41;
      Y = this.X * M.M12 + this.Y * M.M22 + this.Z * M.M32 + M.M42;
      Z = this.X * M.M13 + this.Y * M.M23 + this.Z * M.M33 + M.M43;
      W = this.X * M.M14 + this.Y * M.M24 + this.Z * M.M34 + M.M44;

      if (W > -1.0e-5 && W < 1.0e-5)
      {
        this.X = 0;
        this.Y = 0;
        this.Z = 0;
      }
      else
      {
        this.X = X / W;
        this.Y = Y / W;
        this.Z = Z / W;
      }
    }
  }
}
