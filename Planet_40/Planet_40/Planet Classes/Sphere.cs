using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Willow;

namespace Planet
{
  public class Sphere
  {
    // quad trees for each cube face
    private TerrainNode front;
    private TerrainNode back;
    private TerrainNode left;
    private TerrainNode right;
    private TerrainNode top;
    private TerrainNode bottom;


    private Position3 position;       // sphere center
    private double radius;            // sphere radius
    private double scale;             // scale - used for drawing the planet at a lower scale to deal with depth buffer issues
    private Matrix scaleMatrix;
    private bool useNormalMap;
    bool isSphere;

    private CreatePositionDelegate createPosition;
    private CreateTerrainNodeVertexBufferDelegate createTerrainNodeVertexBuffer;

    public Position3 Position { get { return position; } }
    public double Scale { get { return scale; } }
    public Matrix ScaleMatrix { get { return scaleMatrix; } }
    public TerrainNode Front { get { return front; } }
    public TerrainNode Back { get { return back; } }
    public TerrainNode Left { get { return left; } }
    public TerrainNode Right { get { return right; } }
    public TerrainNode Top { get { return top; } }
    public TerrainNode Bottom { get { return bottom; } }

    public double Radius
    {
      get { return radius; }
      set
      {
        radius = value;
        scale = 1.0 / radius;
        scaleMatrix = Matrix.CreateScale((float)scale);
      }
    }


    // constructor
    public Sphere(Position3 position, double radius, bool useNormalMap, CreatePositionDelegate createPosition, CreateTerrainNodeVertexBufferDelegate createTerrainNodeVertexBuffer, bool isSphere)
    {
      this.position = position;
      this.Radius = radius;
      this.useNormalMap = useNormalMap;
      this.createPosition = createPosition;
      this.createTerrainNodeVertexBuffer = createTerrainNodeVertexBuffer;
      this.isSphere = isSphere;
      Initialize();
    }


    public void Initialize()
    {
      front = CreateRootTerrainNode(Face.Front);
      back = CreateRootTerrainNode(Face.Back);
      left = CreateRootTerrainNode(Face.Left);
      right = CreateRootTerrainNode(Face.Right);
      top = CreateRootTerrainNode(Face.Top);
      bottom = CreateRootTerrainNode(Face.Bottom);
    }

    public TerrainNode FindNodeUnderPosition(Position3 position, out Vector3[] triangle, out double height, out Vector3 normal)
    {
      TerrainNode result = null;

      result = front.FindNodeUnderPosition(position, out triangle, out height, out normal);

      if (result == null)
        result = back.FindNodeUnderPosition(position, out triangle, out height, out normal);

      if (result == null)
        result = left.FindNodeUnderPosition(position, out triangle, out height, out normal);

      if (result == null)
        result = right.FindNodeUnderPosition(position, out triangle, out height, out normal);

      if (result == null)
        result = top.FindNodeUnderPosition(position, out triangle, out height, out normal);

      if (result == null)
        result = bottom.FindNodeUnderPosition(position, out triangle, out height, out normal);

      return result;
    }

    private TerrainNode CreateRootTerrainNode(Face face)
    {
      // generate geometry map
      TerrainNodeSplitItem item = new TerrainNodeSplitItem(null, 0, new TerrainNodeBounds(-1.0, -1.0, 2.0, 2.0, TerrainNodeBounds.NodeQuadrant.None, face, 0), isSphere);
      TerrainNodeSplitManager.BuildNode(item);

      // build terrain node using geometry map
      return new TerrainNode(item, 0, radius, createTerrainNodeVertexBuffer, isSphere);
    }


    /// <summary>
    /// Calculate angle from camera to horizon
    /// </summary>
    /// <param name="cameraPosition">Planet space camera position</param>
    /// <returns>Horizon angle in radians</returns>
    public float CalculateHorizonAngle(Position3 cameraPosition)
    {
      double result = MathHelper.ToRadians(300);

      double h = cameraPosition.Length();
      if (h > radius)
      {
        result = Math.Acos(radius / h);
        result += MathHelper.ToRadians(5); // add 5 degrees to account for mountains on the horizon

        // if we're fairly far away from the planet we need to account
        // for the very large patch size of the 6 faces that make up the planet
        h = h - radius - 9;
        if (h > 1000)
          result += MathHelper.ToRadians(50);
      }

      return (float)result;
    }


    public void Update(Position3 cameraPosition, float fieldOfView)
    {
      // update nodes, allowing them to split if required
      front.Update(position, cameraPosition, fieldOfView);
      back.Update(position, cameraPosition, fieldOfView);
      left.Update(position, cameraPosition, fieldOfView);
      right.Update(position, cameraPosition, fieldOfView);
      top.Update(position, cameraPosition, fieldOfView);
      bottom.Update(position, cameraPosition, fieldOfView);

    }


  }
}
