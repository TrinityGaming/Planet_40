using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Content;

using Willow.Camera;


// PointList no longer exists in XNA4, so so stars - this needs to be changed to draw stars as tiny triangles or something


namespace Planet
{
  public class SpaceDome
  {
    const int StarCount = 5000;
    const int BigStarPercent = 20;

    
    Game game;
    float minRadius;
    float maxRadius;

    Effect effect;
    VertexDeclaration vertexDeclaration;
    VertexBuffer vertexBuffer;

    ICameraManager cameraManager;


    public SpaceDome(double minRadius, double maxRadius)
    {
      this.minRadius = (float)minRadius;
      this.maxRadius = (float)maxRadius;
    }

    public void LoadContent(Game game)
    {
      this.game = game;
      cameraManager = (ICameraManager)game.Services.GetService(typeof(ICameraManager));

      effect = game.Content.Load<Effect>(@"Effects\SpaceDome");
      vertexDeclaration = VertexPositionColor.VertexDeclaration; // new VertexDeclaration(game.GraphicsDevice, VertexPositionColor.VertexElements);
      vertexBuffer = new VertexBuffer(game.GraphicsDevice, typeof(VertexPositionColor), StarCount, BufferUsage.WriteOnly);

      GenerateStars();
    }


    private void GenerateStars()
    {
      VertexPositionColor[] data = new VertexPositionColor[StarCount];
      int count = 0;
      Random r = new Random(0);

      while (count < StarCount)
      {
        byte greyValue = (byte)(r.Next(200) + 56);  // 56 - 255
        Color c = new Color(greyValue, greyValue, greyValue);

        // get random position and move it out to the sky dome radius
        Vector3 p = new Vector3(r.Next(1000) + 1, r.Next(1000) + 1, r.Next(1000) + 1);
        p.Normalize();
        p *= r.Next((int)minRadius, (int)maxRadius);

        // randomly change sign of each component
        if (r.Next(100) > 50) p.X = -p.X;
        if (r.Next(100) > 50) p.Y = -p.Y;
        if (r.Next(100) > 50) p.Z = -p.Z;




        /*
        if (R.Next(100) >= (100 - BigStarPercent) && Count + 4 <= StarCount)
        {
          Data[Count++] = new VertexPositionColor(P, C);

          float Offset = 30.0f;
          P.X += Offset;
          Data[Count++] = new VertexPositionColor(P, C);

          P.Y += Offset;
          Data[Count++] = new VertexPositionColor(P, C);

          P.X -= Offset;
          P.Y -= Offset;
          Data[Count++] = new VertexPositionColor(P, C);
        }
        else
        */

        data[count++] = new VertexPositionColor(p, c);
      }

      vertexBuffer.SetData(data);
    }


    public void Draw()
    {

      // TODO : no such thing as PrimitiveType.PointList in XNA 4 - need to draw stars in some other way

      //game.GraphicsDevice.DepthStencilState = DepthStencilState.None;
      //game.GraphicsDevice.SetVertexBuffer(vertexBuffer);

      //cameraManager.ActiveCamera.NearClip = (float)(5.0 / minRadius);
      //cameraManager.ActiveCamera.FarClip = 20000.0f;
      //cameraManager.ActiveCamera.UpdateProjectionMatrix();

      //Matrix M = Matrix.Identity;
      //M *= cameraManager.ActiveCamera.ViewMatrix * cameraManager.ActiveCamera.ProjectionMatrix;
      //effect.Parameters["WorldViewProjectionMatrix"].SetValue(M);

      //effect.CurrentTechnique.Passes[0].Apply();
      //game.GraphicsDevice.DrawPrimitives(PrimitiveType.PointList, 0, StarCount);
    }
  }
}