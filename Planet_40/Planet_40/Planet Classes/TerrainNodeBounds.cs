using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Planet
{

  public class TerrainNodeBounds
  {
    public enum NodeQuadrant { None, TopLeft, BottomLeft, TopRight, BottomRight };

    private double x;
    private double y;
    private double width;
    private double height;
    private NodeQuadrant quadrant;
    private Face face;
    private int level;

    public TerrainNodeBounds(double x, double y, double width, double height, NodeQuadrant quadrant, Face face, int level)
    {
      this.x = x;
      this.y = y;
      this.width = width;
      this.height = height;
      this.quadrant = quadrant;
      this.face = face;
      this.level = level;
    }

    public override string ToString()
    {
      return string.Format("x: {0}, y: {1}, w: {2}, h: {3}", x, y, width, height);
    }

    public double Right { get { return x + width; } }
    public double Top { get { return y + height; } }
    public double Left { get { return x; } }
    public double Bottom { get { return y; } }
    public double Width { get { return width; } }
    public double Height { get { return height; } }
    public double HalfWidth { get { return width * 0.5f; } }
    public double HalfHeight { get { return height * 0.5f; } }
    public NodeQuadrant Quadrant { get { return quadrant; } }
    public Face Face { get { return face; } }
    public int Level { get { return level; } }

    public TerrainNodeBounds BottomLeftQuadrant { get { return new TerrainNodeBounds(x, y, HalfWidth, HalfHeight, NodeQuadrant.BottomLeft, face, level + 1); } }
    public TerrainNodeBounds TopLeftQuadrant { get { return new TerrainNodeBounds(x, y + HalfHeight, HalfWidth, HalfHeight, NodeQuadrant.TopLeft, face, level + 1); } }
    public TerrainNodeBounds BottomRightQuadrant { get { return new TerrainNodeBounds(x + HalfWidth, y, HalfWidth, HalfHeight, NodeQuadrant.BottomRight, face, level + 1); } }
    public TerrainNodeBounds TopRightQuadrant { get { return new TerrainNodeBounds(x + HalfWidth, y + HalfHeight, HalfWidth, HalfHeight, NodeQuadrant.TopRight, face, level + 1); } }
  }

}
