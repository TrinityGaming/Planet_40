using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

using System.Diagnostics;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Willow;
using Willow.VertexDefinition;


namespace Planet
{
  public class TerrainNode
  {
    int level;                              // tree depth of this node
    double radius;                          // radius of the sphere this node belongs to
    bool visible;
    bool hasMeshBorder;
    bool isSphere;

    int patchColumns;
    int patchRows;

    float geometricError;
    float maxScreenSpaceError = 1;

    TerrainTextureSurface diffuseSurface;
    TerrainNormalSurface normalSurface;

    Position3 position;                     // base planet space patch position for center pixel
    TerrainNodeBounds bounds;               // entire bounds for this node
    TerrainNode[] children;                 // child nodes - 4, top left, top right, bottom left, bottom right
    TerrainNode[] workingChildren;
    TerrainNodeVertexBuffer vertexBuffer;   // terrain vertex buffer
    CreatePositionDelegate createPosition;  // delegate for vertex position creation - allows various position offset algorithms
    CreateTerrainNodeVertexBufferDelegate createTerrainNodeVertexBuffer;

    Vector3 minVertex;
    Vector3 maxVertex;
    Position3 closestPosition;

    public volatile bool Splitting;
    public volatile bool CancelSplitting;
    public bool Visible { get { return visible; } set { visible = value; } }

    public Position3 Position { get { return position; } }
    public TerrainNodeVertexBuffer VertexBuffer { get { return vertexBuffer; } }
    public TerrainNode[] Children { get { return children; } }
    public Vector3 MinVertex { get { return minVertex; } }
    public Vector3 MaxVertex { get { return maxVertex; } }
    public int Level { get { return level; } }
    public TerrainNodeBounds Bounds { get { return bounds; } }
    public Position3 ClosestPosition { get { return closestPosition; } }
    public bool HasChildren { get { return children != null && ! Splitting; }}
    public Texture2D DiffuseTexture { get { if (diffuseSurface == null) return null; else return diffuseSurface.Texture; } }
    public Texture2D NormalTexture { get { if (normalSurface == null) return null; else return normalSurface.Texture; } }

    /// <summary>
    /// Construct a terrain node instance
    /// </summary>
    /// <param name="bounds">Bounds for this node</param>
    /// <param name="level">Tree depth of this node</param>
    /// <param name="radius">Radius of the sphere this node belongs to</param>
    /// <param name="createPosition">Delegate for vertex position creation</param>
    public TerrainNode(TerrainNodeBounds bounds, int level, double radius, 
                       CreatePositionDelegate createPosition, 
                       CreateTerrainNodeVertexBufferDelegate createTerrainNodeVertexBuffer,
                       bool isSphere)
    {
      this.bounds = bounds;
      this.level = level;
      this.radius = radius;
      this.createPosition = createPosition;
      this.createTerrainNodeVertexBuffer = createTerrainNodeVertexBuffer;
      this.isSphere = isSphere;

      Initialize(null);
    }


    /// <summary>
    /// Construct a terrain node instance from a pre-generated geometry map
    /// </summary>
    /// <param name="item"></param>
    /// <param name="level"></param>
    /// <param name="radius"></param>
    /// <param name="createTerrainNodeVertexBuffer"></param>
    public TerrainNode(TerrainNodeSplitItem item, int level, double radius, CreateTerrainNodeVertexBufferDelegate createTerrainNodeVertexBuffer, bool isSphere)
    {
      this.bounds = item.Bounds;
      this.level = level;
      this.radius = radius;
      this.createPosition = null;
      this.createTerrainNodeVertexBuffer = createTerrainNodeVertexBuffer;
      this.isSphere = isSphere;

      this.diffuseSurface = item.DiffuseSurface;
      this.normalSurface = item.NormalSurface;

      Initialize(item);
    }


    /// <summary>
    /// Initialize the terrain node
    /// </summary>
    private void Initialize(TerrainNodeSplitItem item)
    {
      geometricError = Constants.ErrorFactor * (float)Math.Pow(0.5, level + 1);
      geometricError *= (float)(radius / Constants.EarthRadius);    // TODO : adjust error based on earth-sized planet settings

      CalculatePatchPosition(item);
      GenerateMesh(item);
    }


    /// <summary>
    /// Calculate the sphere-space position of the patch by determining where the center vertex lies on the sphere
    /// </summary>
    private void CalculatePatchPosition(TerrainNodeSplitItem item)
    {
      // find the cube position of the center vertex
      double x = bounds.Left + bounds.Width / 2;
      double y = bounds.Bottom + bounds.Height / 2;


      // get the matrix used to rotate the a position to the appropriate cube face
      Matrix faceMatrix = CubeFaces.GetFace(bounds.Face).FaceMatrix;


      // create the vertex position in cube-space and rotate it to the appropriate face
      Position3 cubePosition = new Position3(x, y, 1);
      cubePosition.Transform(ref faceMatrix);

      // get the vertex position on the unit sphere and rotate it to the appropriate face
      Position3 spherePosition = Tools.CubeToSphereMapping(x, y, 1);
      spherePosition.Transform(ref faceMatrix);
      spherePosition.Normalize();

      Vector2 patchPosition = new Vector2(Constants.PatchWidth / 2, Constants.PatchHeight / 2);

      if (item == null)
      {
        // transform the vertex based on the user defined delegate - this returns the height value of the point on the sphere
        createPosition(ref spherePosition, ref cubePosition, ref patchPosition, radius, out position);
      }
      else
      {
        double h = item.HeightData[(int)((patchPosition.Y + 1) * (Constants.PatchWidth + 2) + (patchPosition.X + 1))];
        if (h < 0) h = 0;
        position = spherePosition * (radius + h);
      }
    }


    /// <summary>
    /// Translate sphere-space position to patch-space
    /// </summary>
    /// <param name="position">Sphere-space position</param>
    public void TranslateToPatchSpace(ref Position3 position)
    {
      position.Subtract(ref this.position);
    }


    /// <summary>
    /// Create a patch-space position by using the user supplied delegate.
    /// </summary>
    /// <param name="inPosition">Input position, located on the surface of a unit sphere</param>
    /// <param name="outPosition">Output position, moved out to the sphere surface, or altered with noise, as determined by the user supplied delegate</param>
    private void CreatePosition(ref Position3 spherePosition, ref Position3 cubePosition, ref Vector2 patchPosition, out Position3 outPosition, out double height)
    {
      height = createPosition(ref spherePosition, ref cubePosition, ref patchPosition, radius, out outPosition);
      TranslateToPatchSpace(ref outPosition);
    }


    /// <summary>
    /// Generate terrain mesh for this node
    /// </summary>
    public void GenerateMesh(TerrainNodeSplitItem item)
    {
      // the minus one here is correct: e.g. 3x3 vertices labeled 0, 1, 2: v0.x = 0, v1.x = 0.5, v2.x = 1.  The increment = 1 / (3 - 1) = 0.5
      double horizontalStep = bounds.Width / (Constants.PatchWidth - 1);
      double verticalStep = bounds.Height / (Constants.PatchHeight - 1);
      float horizontalTextureStep = 1.0f / (Constants.PatchWidth - 1);
      float verticalTextureStep = 1.0f / (Constants.PatchHeight - 1);

      float MinX, MinY, MinZ;
      float MaxX, MaxY, MaxZ;

      // initialize min and max vertex tracking
      MinX = MinY = MinZ = 999999;
      MaxX = MaxY = MaxZ = -999999;


      // get matrix used to transform vertices to the proper cube face
      Matrix faceMatrix = CubeFaces.GetFace(bounds.Face).FaceMatrix;

      // create vertex storage using user supplied delegate
      vertexBuffer = createTerrainNodeVertexBuffer(item.HeightData.Length);


      //int cubeVertexIndex = 0;
      //CubeVertices = new Vector2[item.HeightData.Length];


      hasMeshBorder = false;
      int vertexIndex = 0;
      int Rows = Constants.PatchHeight;
      int Columns = Constants.PatchWidth;


      if (item.HeightData.Length > Constants.PatchWidth * Constants.PatchHeight)
      {
        hasMeshBorder = true;
        Rows += 2;
        Columns += 2;
      }

      patchRows = Rows;
      patchColumns = Columns;


      float v = 0;
      double y = bounds.Bottom;

      if (hasMeshBorder)
      {
        v -= verticalTextureStep;
        y -= verticalStep;
      }


      for (int hy = 0; hy < Rows; hy++)
      {
        float u = 0;
        double x = bounds.Left;

        if (hasMeshBorder)
        {
          u -= horizontalTextureStep;
          x -= horizontalStep;
        }

        for (int hx = 0; hx < Columns; hx++)
        {
          // create the vertex position and rotate it to the appropriate face
          Position3 cubePosition = new Position3(x, y, 1);

          Position3 spherePosition = Tools.CubeToSphereMapping(x, y, 1);
          spherePosition.Transform(ref faceMatrix);
          cubePosition.Transform(ref faceMatrix);

          // transform the vertex based on the user defined delegate
          double height;
          Position3 finalPosition;
          Vector2 patchPosition = new Vector2(hx, hy);

          if (item == null)
            CreatePosition(ref spherePosition, ref cubePosition, ref patchPosition, out finalPosition, out height);
          else
          {
            height = item.HeightData[hy * Columns + hx];
            if (height < 0) height = 0;
            finalPosition = spherePosition * (radius + height);
            TranslateToPatchSpace(ref finalPosition);
          }

          VertexPositionNormalTextureHeight vertex = new VertexPositionNormalTextureHeight();

          vertex.Position = (Vector3)finalPosition;
          vertex.Height = (float)height;
          vertex.Normal = (Vector3)spherePosition;
          vertex.TextureCoordinate = new Vector2(u, v);
          vertex.Tangent = Vector4.Zero;
          vertexBuffer.Vertices[vertexIndex++] = vertex;


          // track min and max coordinates, but only  for the vertices that will be in the final mesh
          if (hx >= 1 && hx < Columns - 1 && hy >= 1 && hy < Rows - 1)
          {
            if (vertex.Position.X < MinX) MinX = vertex.Position.X;
            if (vertex.Position.Y < MinY) MinY = vertex.Position.Y;
            if (vertex.Position.Z < MinZ) MinZ = vertex.Position.Z;

            if (vertex.Position.X > MaxX) MaxX = vertex.Position.X;
            if (vertex.Position.Y > MaxY) MaxY = vertex.Position.Y;
            if (vertex.Position.Z > MaxZ) MaxZ = vertex.Position.Z;
          }


          x += horizontalStep;
          u += horizontalTextureStep;
        }

        y += verticalStep;
        v += verticalTextureStep;
      }


      // create min and max bounding vertices, in patch-space
      minVertex = new Vector3(MinX, MinY, MinZ);
      maxVertex = new Vector3(MaxX, MaxY, MaxZ);


      // calculate normals and tangents
      CalculateNormals();
      CalculateTangents();


      // save vertex buffer changes - this effectively copies the raw data to a vertex buffer on the device
      vertexBuffer.CommitChanges(Constants.PatchWidth, Constants.PatchHeight);
    }



    private void CalculateNormals()
    {
      // start at row 1 since the outer edge are vertices shared with neighboring patches - the border vertices
      // are used to calculate the edge normals correctly to avoid seams
      int yIndex = patchColumns;  // row 1

      for (int y = 1; y < patchRows - 1; y++)
      {
        int xIndex = yIndex;

        for (int x = 1; x < patchColumns - 1; x++)
        {
          xIndex++;

          Vector3 p1 = vertexBuffer.Vertices[xIndex + 1].Position -
                       vertexBuffer.Vertices[xIndex - 1].Position;

          Vector3 p2 = vertexBuffer.Vertices[xIndex + patchColumns].Position -
                       vertexBuffer.Vertices[xIndex - patchColumns].Position;

          // calculate normal
          vertexBuffer.Vertices[xIndex].Normal = Vector3.Normalize(Vector3.Cross(p1, p2));
        }

        yIndex += patchColumns;
      }

    }


    private void CalculateTangents()
    {
      // see if we have a border
      int columns = patchColumns; //  PatchWidth;
      int rows = patchRows; //  PatchHeight;

      //if (hasMeshBorder)
      //{
      //  columns += 2;
      //  rows += 2;
      //}

      // position 0 and PatchHeight contain border vertices so we can calculate normals correctly, they won't be used in the final vertex buffer
      for (int y = 0; y < rows - 1; y++)
        for (int x = 0; x < columns - 1; x++)
        {
          // calculate tangent
          VertexPositionNormalTextureHeight v0 = vertexBuffer.Vertices[MapVertexIndex(x, y)];
          VertexPositionNormalTextureHeight v1 = vertexBuffer.Vertices[MapVertexIndex(x + 1, y)];
          VertexPositionNormalTextureHeight v2 = vertexBuffer.Vertices[MapVertexIndex(x, y + 1)];

          Vector4 tangent;

          Tools.CalcTangent(ref v0.Position, ref v1.Position, ref v2.Position,
                            ref v0.TextureCoordinate, ref v1.TextureCoordinate, ref v2.TextureCoordinate,
                            ref v0.Normal, out tangent);

          vertexBuffer.Vertices[MapVertexIndex(x, y)].Tangent = tangent;
        }
    }



    /// <summary>
    /// Determine if this node's children are leaf nodes
    /// </summary>
    /// <returns>True if all children are leaf nodes</returns>
    private bool ChildrenAreLeafNodes()
    {
      if (children == null)
        return false;
      else
        return !children[0].HasChildren &&
               !children[1].HasChildren &&
               !children[2].HasChildren &&
               !children[3].HasChildren;
    }




    /// <summary>
    /// Clear this node, and optionally this node's children
    /// </summary>
    /// <param name="clearChildren">Clear child nodes</param>
    public void Clear(bool clearChildren)
    {
      // return vertex buffer to poool
      if (vertexBuffer != null)
        vertexBuffer.Finished();

      // return diffuse surface to the pool
      if (diffuseSurface != null)
        diffuseSurface.Finished();

      // return normal surface to the pool
      if (normalSurface != null)
        normalSurface.Finished();

      // clear child nodes if requested
      if (clearChildren)
        ClearChildren();
    }


    /// <summary>
    /// Clear this node and child nodes
    /// </summary>
    public void ClearChildren()
    {
      if (children != null)
      {
        children[0].Clear(true);
        children[1].Clear(true);
        children[2].Clear(true);
        children[3].Clear(true);

        children[0] = null;
        children[1] = null;
        children[2] = null;
        children[3] = null;
        children = null;
      }
    }



    private int MapVertexIndex(int x, int y)
    {
      //int w = PatchWidth;
      //if (hasMeshBorder) w += 2;
      //return y * w + x;

      return y * patchColumns + x;
    }


    private void CheckDistance(int x, int y, ref Vector3 v, ref float minDistance)
    {
      VertexPositionNormalTextureHeight p = vertexBuffer.Vertices[y * patchColumns + x]; // MapVertexIndex(x, y)];

      float d = Vector3.DistanceSquared(p.Position, v);

      if (d < minDistance)
      {
        minDistance = d;
        closestPosition = new Position3(p.Position);
      }
    }




    /// <summary>
    /// Get distance from camera
    /// </summary>
    /// <param name="planetPosition">Position of planet center</param>
    /// <param name="cameraPosition">Camera position in planet space</param>
    /// <returns></returns>
    private float GetViewDistance(Position3 planetPosition, Position3 cameraPosition)
    {
      //return (float)Position3.Distance(this.Position, cameraPosition);

      // need to get the camera position in patch-space to compare the distance
      Vector3 v = (cameraPosition - this.Position).AsVector3;

      // translate to object space by adding patch position to planet position then subtracting from the camera position
      //Vector3 v = (cameraPosition - (planetPosition + this.position)).AsVector3;

      float minDistance = 999999999999;
      closestPosition = new Position3(vertexBuffer.Vertices[MapVertexIndex(Constants.PatchWidth / 2, Constants.PatchHeight / 2)].Position);

      // center center
      CheckDistance(Constants.PatchWidth / 2, Constants.PatchHeight / 2, ref v, ref minDistance);

      // bottom left
      CheckDistance(0, 0, ref v, ref minDistance);

      // bottom center
      CheckDistance(Constants.PatchWidth / 2, 0, ref v, ref minDistance);

      // bottom right
      CheckDistance(Constants.PatchWidth - 1, 0, ref v, ref minDistance);

      // top left
      CheckDistance(0, Constants.PatchHeight - 1, ref v, ref minDistance);

      // top center
      CheckDistance(Constants.PatchWidth / 2, Constants.PatchHeight - 1, ref v, ref minDistance);

      // top right
      CheckDistance(Constants.PatchWidth - 1, Constants.PatchHeight - 1, ref v, ref minDistance);

      // left center
      CheckDistance(0, Constants.PatchHeight / 2, ref v, ref minDistance);

      // right center
      CheckDistance(Constants.PatchWidth - 1, Constants.PatchHeight / 2, ref v, ref minDistance);

      // transform to planet-space
      closestPosition += this.Position;

      // return closest distance
      return (float)Math.Sqrt(minDistance);
    }


    /// <summary>
    /// Determine if the node needs to be split based on camera distance and field of view
    /// </summary>
    /// <param name="cameraPosition">Planet-space camera position </param>
    /// <param name="fieldOfView">Camera's field of view</param>
    /// <returns>True if the terrain node needs to be split</returns>
    private bool NeedsSplit(Position3 planetPosition, Position3 cameraPosition, float fieldOfView)
    {
      // limit splitting
      if (isSphere && level >= Constants.MaxAtmosphereNodeLevel) return false;
      if (!isSphere && level >= Constants.MaxTerrainNodeLevel) return false;

      // get distance between camera and terrain node center - both are already in planet space
      float viewDistance = GetViewDistance(planetPosition, cameraPosition);

      // TODO : splitting not working properly when zooming in/out - seems to be the opposite of what's needed

      // calculate screen space error
      float k = 1024.0f * 0.5f * (float)Math.Tan(fieldOfView * 0.5f);
      float screenSpaceError = (geometricError / viewDistance) * k;

      // return true if the screen space error is greater than the max
      return (screenSpaceError > maxScreenSpaceError);
    }


    /// <summary>
    /// Split this node's children
    /// </summary>
    /// <param name="cameraPosition">Planet-space camera position</param>
    /// <param name="fieldOfView">Camera's field of view</param>
    private void SplitChildren(Position3 planetPosition, Position3 cameraPosition, float fieldOfView)
    {
      if (HasChildren)
      {
        children[0].Split(planetPosition, cameraPosition, fieldOfView);
        children[1].Split(planetPosition, cameraPosition, fieldOfView);
        children[2].Split(planetPosition, cameraPosition, fieldOfView);
        children[3].Split(planetPosition, cameraPosition, fieldOfView);
      }
    }


    /// <summary>
    /// Split all the way down to leaf nodes before generating the actual terrain.
    /// The deepest node with data will still be displayed until all the leaf nodes
    /// are ready.
    /// </summary>
    /// <param name="planetPosition"></param>
    /// <param name="cameraPosition"></param>
    /// <param name="fieldOfView"></param>
    private void Split2(Position3 planetPosition, Position3 cameraPosition, float fieldOfView)
    {

    }

    /// <summary>
    /// Split or unsplit nodes and child nodes as required
    /// </summary>
    /// <param name="cameraPosition">Planet-space camera position</param>
    /// <param name="fieldOfView">Camera's field of view</param>
    private void Split(Position3 planetPosition, Position3 cameraPosition, float fieldOfView)
    {
      // if we're already splitting then we don't want to do any other split processing until the thread is complete
      // this node will continue to be the leaf node in this branch so it will continue to draw until the
      // splitting is completed
      if (Splitting)
      {
        // if we no longer need to split, but we're still queued up for splitting then cancel it - the thread
        // will see this when it gets to the the node in the queue and won't do anything
        if (!NeedsSplit(planetPosition, cameraPosition, fieldOfView))
          CancelSplitting = true;

        return;
      }


      // if this node needs to be split then check its children or queue up a split
      if (NeedsSplit(planetPosition, cameraPosition, fieldOfView))
      {
        // if it already has children then we've already split, so recurse through the children
        if (HasChildren)
        {
          SplitChildren(planetPosition, cameraPosition, fieldOfView);
        }

        // otherwise we need to be split and have no children, so queue up the child node to the node splitter queue thread
        else
        {
          // setting this will cause drawing code to pretend this is a leaf node until the split is complete
          // it will also become non-splittable during this time
          CancelSplitting = false;
          Splitting = true;

          QueueNodeGeneration();
        }
      }

      // if we don't need to be split then we can remove child nodes
      else
      {
        // if our children are all leaf nodes then we no longer need them, so get rid of them and this node is now a leaf node
        if (ChildrenAreLeafNodes())
          ClearChildren();

        // if we do have non-leaf children then recurse down until we find a node whose children are leaves, and remove those children
        // the lower level children will continue to be drawn until they've been cleared out, at only 1 level per frame
        if (HasChildren)
          SplitChildren(planetPosition, cameraPosition, fieldOfView);
      }
    }


    private void QueueNodeGeneration()
    {
      TerrainNodeSplitManager.QueueNodeSplit(this, 0, bounds.BottomLeftQuadrant, isSphere);
      TerrainNodeSplitManager.QueueNodeSplit(this, 1, bounds.TopLeftQuadrant, isSphere);
      TerrainNodeSplitManager.QueueNodeSplit(this, 2, bounds.BottomRightQuadrant, isSphere);
      TerrainNodeSplitManager.QueueNodeSplit(this, 3, bounds.TopRightQuadrant, isSphere);

      //Debug.WriteLine(String.Format("bl: {0} tl: {1}  br: {2}  tr: {3}",
      //                bounds.BottomLeftQuadrant,
      //                bounds.TopLeftQuadrant,
      //                bounds.BottomRightQuadrant,
      //                bounds.TopRightQuadrant));
      
    }

    private void ClearWorkingChildren()
    {
      if (workingChildren != null)
        for (int i = 0; i < 4; i++)
          workingChildren[i] = null;

      workingChildren = null;
    }


    /// <summary>
    /// Executed inside a worker thread to handle generating a single child node
    /// </summary>
    /// <param name="item"></param>
    public void GenerateChild(TerrainNodeSplitItem item)
    {
      if (!CancelSplitting)
      {
        if (workingChildren == null)
          workingChildren = new TerrainNode[4];

        workingChildren[item.ChildIndex] = new TerrainNode(item, level + 1, radius, createTerrainNodeVertexBuffer, item.IsSphere);
      }


      // if this was the last child then we're done!
      if (item.ChildIndex == 3)
      {
        if (!CancelSplitting)
        {
          // transfer working children over to actual children
          if (children == null)
            children = new TerrainNode[4];

          for (int i = 0; i < 4; i++)
            children[i] = workingChildren[i];

          ClearWorkingChildren();
        }
        else
        {
          ClearWorkingChildren();
        }

        CancelSplitting = false;
        Splitting = false;
      }
    }

    public void Update(Position3 planetPosition, Position3 cameraPosition, float fieldOfView)
    {
      Split(planetPosition, cameraPosition, fieldOfView);
    }


    // face space method, using ray intersection in planet space
    public TerrainNode FindNodeUnderPosition(Position3 position, out Vector3[] triangle, out double height, out Vector3 normal)
    {
      triangle = null;
      height = 0;
      normal = Vector3.Zero;


      Position3 originalPosition = position;

      // normalize the position so it points out from the planet center towards the actual position
      position.Normalize();

      // get the direction vector
      Vector3 direction = position.AsVector3;

      // get a position 1km under the planet surface
      position *= radius - 1;

      // get ray from position to planet center, translate to patch space
      Ray ray = new Ray((position - this.Position).AsVector3, direction);


      Vector3 p1 = MinVertex;
      Vector3 p2 = MaxVertex;

      // first do a bounding box/ray intersection test
      BoundingBox box = new BoundingBox(MinVertex, MaxVertex);

      // if the ray doesn't intersect the bounding box then it can't intersect terrain node
      if (ray.Intersects(box) == null) return null;

      // we have a potential match - do mesh collision to be sure
      Vector3[] t = this.GetHeightAndNormal(originalPosition, out height, out normal);

      // if no mesh collision then we're done
      if (t == null) return null;


      // if the position is over this node and this is a leaf node then we have what we want
      if (!HasChildren)
      {
        triangle = t;
        return this;
      }


      // otherwise we want to recurse into the children to find the leaf node
      for (int i = 0; i < children.Length; i++)
      {
        TerrainNode node = children[i].FindNodeUnderPosition(position, out triangle, out height, out normal);
        if (node != null) return node;
      }

      // if we get here then we have a problem - the position is allegedly above this node, which means
      // it should also be above one of this node's children, but we didn't find a child
      // return this;

      if (Globals.Game.Graphics.IsFullScreen)
        Globals.Game.Graphics.ToggleFullScreen();
      throw new Exception("Unable to find leaf node.");
    }

    

    public Vector3[] GetHeightAndNormal(Position3 position, out double height, out Vector3 normal)
    {
      // we know position is over this node, now we need to find which triangle within the node

      // normalize the position so it points out from the planet center towards the actual position
      position.Normalize();

      // get the direction vector
      Position3 direction3 = position;
      Vector3 direction = position.AsVector3;

      // get a position 1km under the planet surface, in planet space
      position *= radius - 1;

      // translate to patch space
      position -= this.Position;


      // get ray from position to planet center, translate to patch space
      Ray ray = new Ray(position.AsVector3, direction);
      float? intersection = null;
      float u = 0;
      float v = 0;

      Vector3 v0 = Vector3.Zero;
      Vector3 v1 = Vector3.Zero;
      Vector3 v2 = Vector3.Zero;
      Vector3 v4 = Vector3.Zero;

      Vector3 n0 = Vector3.Zero;
      Vector3 n1 = Vector3.Zero;
      Vector3 n2 = Vector3.Zero;
      Vector3 n4 = Vector3.Zero;


      int triangleIndex = 0;
      int quadIndex = 0;
      int index;
      int y;
      int x;

      // now we need to loop through each triangle and see if the ray intersects
      for (int i = 0; i < TerrainNodeIndexBuffer.IndexCount; i += 3)
      {
        index = TerrainNodeIndexBuffer.IndexData[i];
        y = index / Constants.PatchHeight;
        x = index % Constants.PatchWidth;
        index = (y + 1) * patchColumns + (x + 1);
        v0 = vertexBuffer.Vertices[index].Position;
        n0 = vertexBuffer.Vertices[index].Normal;

        index = TerrainNodeIndexBuffer.IndexData[i + 1];
        y = index / Constants.PatchHeight;
        x = index % Constants.PatchWidth;
        index = (y + 1) * patchColumns + (x + 1);
        v1 = vertexBuffer.Vertices[index].Position;
        n1 = vertexBuffer.Vertices[index].Normal;

        index = TerrainNodeIndexBuffer.IndexData[i + 2];
        y = index / Constants.PatchHeight;
        x = index % Constants.PatchWidth;
        index = (y + 1) * patchColumns + (x + 1);
        v2 = vertexBuffer.Vertices[index].Position;
        n2 = vertexBuffer.Vertices[index].Normal;

        Tools.RayIntersectsTriangle(ref ray, ref v0, ref v1, ref v2, out intersection, out u, out v);
  
        if (intersection != null)
        {
          triangleIndex = i;
          quadIndex = i;
          if (quadIndex % 6 != 0)
            quadIndex -= 3;
          break;
        }
      }

      Globals.TriangleIndex = triangleIndex;


      if (intersection == null)
      {
        height = radius;
        normal = direction;
        return null;
      }

      // Now that we've calculated the indices of the corners of our cell, and
      // where we are in that cell, we'll use bilinear interpolation to calculuate
      // our height. This process is best explained with a diagram, so please see
      // the accompanying doc for more information.
      // First, calculate the heights on the bottom and top edge of our cell by
      // interpolating from the left and right sides.

      // get the vertices for the quad containing this triangle

      index = TerrainNodeIndexBuffer.IndexData[quadIndex + 0];
      y = index / Constants.PatchHeight;
      x = index % Constants.PatchWidth;
      index = (y + 1) * patchColumns + (x + 1);
      v0 = vertexBuffer.Vertices[index].Position;
      n0 = vertexBuffer.Vertices[index].Normal;

      index = TerrainNodeIndexBuffer.IndexData[quadIndex + 1];
      y = index / Constants.PatchHeight;
      x = index % Constants.PatchWidth;
      index = (y + 1) * patchColumns + (x + 1);
      v1 = vertexBuffer.Vertices[index].Position;
      n1 = vertexBuffer.Vertices[index].Normal;

      index = TerrainNodeIndexBuffer.IndexData[quadIndex + 2];
      y = index / Constants.PatchHeight;
      x = index % Constants.PatchWidth;
      index = (y + 1) * patchColumns + (x + 1);
      v2 = vertexBuffer.Vertices[index].Position;
      n2 = vertexBuffer.Vertices[index].Normal;

      index = TerrainNodeIndexBuffer.IndexData[quadIndex + 4];
      y = index / Constants.PatchHeight;
      x = index % Constants.PatchWidth;
      index = (y + 1) * patchColumns + (x + 1);
      v4 = vertexBuffer.Vertices[index].Position;
      n4 = vertexBuffer.Vertices[index].Normal;

      /*
      Vector3 top, bottom, p, topNormal, bottomNormal, n;

      if (triangleIndex % 6 == 0)
      {
        // we're in triangle A - the top left triangle
        top = Vector3.Lerp(v0, v1, u);
        bottom = Vector3.Lerp(v2, v4, u);
        p = Vector3.Lerp(top, bottom, v);
        topNormal = Vector3.Lerp(n0, n1, u);
        bottomNormal = Vector3.Lerp(n2, n4, u);
        n = Vector3.Lerp(topNormal, bottomNormal, v);
        n.Normalize();
      }
      else
      {
        // we're in triangle B - the bottom right triangle
        // this means u and v are positions within *that* triangle, so we need to do our lerping differently
        // we're in triangle A - the top left triangle

        // TODO : except u and v are acting very odd!  they don't seem to be correct
        // let's try getting the quad all at once, then doing the Ray check on each triangle in the quad

        top = Vector3.Lerp(v0, v1, u);
        bottom = Vector3.Lerp(v2, v4, u);
        p = Vector3.Lerp(top, bottom, v);
        topNormal = Vector3.Lerp(n0, n1, u);
        bottomNormal = Vector3.Lerp(n2, n4, u);
        n = Vector3.Lerp(topNormal, bottomNormal, v);
        n.Normalize();
      }
       
      */


      // intersection gives us the length on the ray of the exact intersection position, 
      // which gives us the exact height value to use
      Position3 vp = this.Position + (position + (direction3 * (double)intersection));
      height = vp.Length();

      // but now, how do we get the normal from that?  let's start with just averaging the normals
      normal = (n0 + n1 + n2 + n4) * 0.25f;



      Vector3[] result = new Vector3[5];
      result[0] = v0;
      result[1] = v1;
      result[2] = v2;
      result[3] = v4;
      result[4] = (vp - this.Position).AsVector3;

      return result;


    }
  }
}
