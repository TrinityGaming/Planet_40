using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Willow.VertexDefinition
{

  public struct VertexPositionNormalTextureHeight : IVertexType
  {
    public Vector3 Position;
    public Vector3 Normal;
    public Vector2 TextureCoordinate;
    public float Height;
    public float Slope;
    public Vector4 Tangent;


    //private static int SizeInBytes = sizeof(float) * 14;
    private static VertexElement[] VertexElements = new VertexElement[]
    {
      /* Position          */ new VertexElement(sizeof(float) * 0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
      /* Normal            */ new VertexElement(sizeof(float) * 3, VertexElementFormat.Vector3, VertexElementUsage.Normal, 0),
      /* TextureCoordinate */ new VertexElement(sizeof(float) * 6, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0),
      /* Height            */ new VertexElement(sizeof(float) * 8, VertexElementFormat.Single,  VertexElementUsage.TextureCoordinate, 1),
      /* Slope             */ new VertexElement(sizeof(float) * 9, VertexElementFormat.Single,  VertexElementUsage.TextureCoordinate, 2),
      /* Tangent           */ new VertexElement(sizeof(float) * 10, VertexElementFormat.Vector4, VertexElementUsage.Tangent, 0)
    };

    public readonly static VertexDeclaration VertexDeclaration = new VertexDeclaration(VertexElements);


    VertexDeclaration IVertexType.VertexDeclaration { get { return VertexDeclaration; } }
  }

}