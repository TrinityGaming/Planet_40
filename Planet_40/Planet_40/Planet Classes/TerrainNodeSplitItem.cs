using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework.Graphics;

namespace Planet
{
  /// <summary>
  /// Encapsulates a single node split job item
  /// </summary>
  public class TerrainNodeSplitItem
  {
    public TerrainNode ParentNode { get; set; }
    public int ChildIndex { get; set; }
    public TerrainNodeBounds Bounds { get; set; }
    public float[] HeightData;
    public long ScheduledFrame { get; set; }

    public TerrainGeometrySurface GeometrySurface { get; set; }
    public TerrainHeightmapSurface HeightmapSurface { get; set; }
    public TerrainTextureSurface DiffuseSurface { get; set; }
    public TerrainNormalSurface NormalSurface { get; set; }
    public bool IsSphere { get; set; }


    public TerrainNodeSplitItem(TerrainNode parentNode, int childIndex, TerrainNodeBounds bounds, bool isSphere)
    {
      ParentNode = parentNode;
      ChildIndex = childIndex;
      Bounds = bounds;
      IsSphere = isSphere;
    }

    public void CopyHeightData(ref float[] heightData)
    {
      HeightData = new float[heightData.Length];
      Array.Copy(heightData, HeightData, heightData.Length);
    }

    public void Finished()
    {
      if (GeometrySurface != null)
        GeometrySurface.Finished();

      if (HeightmapSurface != null)
        HeightmapSurface.Finished();

      /* these are referenced elsewhere when we're done with the item, so they must be Finished() there
      if (TextureSurface != null)
        TextureSurface.Finished();

      if (NormalSurface != null)
        NormalSurface.Finished();
      */
    }
  }



}
