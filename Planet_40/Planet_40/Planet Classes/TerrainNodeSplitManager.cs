using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Willow;

namespace Planet
{
  public static class TerrainNodeSplitManager
  {
    static Game game;

    static Pool<TerrainGeometrySurface> geometrySurfacePool;
    static Pool<TerrainHeightmapSurface> heightmapSurfacePool;
    static Pool<TerrainTextureSurface> textureSurfacePool;
    static Pool<TerrainNormalSurface> normalSurfacePool;

    static Queue<TerrainNodeSplitItem> geometryQueue;
    static Queue<TerrainNodeSplitItem> heightmapQueue;
    static Queue<TerrainNodeSplitItem> textureQueue;
    static Queue<TerrainNodeSplitItem> meshQueue;

    static TerrainNodeGenerator terrainNodeGenerator;
    static TerrainNodeSphereGenerator terrainNodeSphereGenerator;
    static TerrainHeightmapGenerator heightmapGenerator;
    static TerrainTextureGenerator textureGenerator;
    static TerrainNormalGenerator normalGenerator;


    public static void Initialize(Game game)
    {
      TerrainNodeSplitManager.game = game;

      // create queues
      geometryQueue = new Queue<TerrainNodeSplitItem>();
      heightmapQueue = new Queue<TerrainNodeSplitItem>();
      textureQueue = new Queue<TerrainNodeSplitItem>();
      meshQueue = new Queue<TerrainNodeSplitItem>();


      terrainNodeGenerator = new TerrainNodeGenerator(game);
      terrainNodeGenerator.LoadContent();

      terrainNodeSphereGenerator = new TerrainNodeSphereGenerator(game);
      terrainNodeSphereGenerator.LoadContent();

      heightmapGenerator = new TerrainHeightmapGenerator(game);
      heightmapGenerator.LoadContent();

      textureGenerator = new TerrainTextureGenerator(game);
      textureGenerator.LoadContent();

      normalGenerator = new TerrainNormalGenerator(game);
      normalGenerator.LoadContent();

      CreatePools();
    }


    private static void CreatePools()
    {
      geometrySurfacePool = new Pool<TerrainGeometrySurface>(50, t => t.Active)
      {
 			  // Initialize is invoked whenever we get an instance through New()
 			  Initialize = t => 
			  {
          t.Initialize(game.GraphicsDevice, Constants.PatchWidth, Constants.PatchHeight, true);
			  },

 			  // Deinitialize is invoked whenever an object is reclaimed during CleanUp()
 			  Deinitialize = t =>
			  {
			  }
   		};



      heightmapSurfacePool = new Pool<TerrainHeightmapSurface>(50, t => t.Active)
      {
        // Initialize is invoked whenever we get an instance through New()
        Initialize = t =>
        {
          t.Initialize(game.GraphicsDevice, Constants.TerrainNodeTextureSize, Constants.TerrainNodeTextureSize, true);
        },

        // Deinitialize is invoked whenever an object is reclaimed during CleanUp()
        Deinitialize = t =>
        {
        }
      };

      textureSurfacePool = new Pool<TerrainTextureSurface>(512, t => t.Active)
      {
        Initialize = t =>
        {
          t.Initialize(game.GraphicsDevice, Constants.TerrainNodeTextureSize, Constants.TerrainNodeTextureSize);
        },

        Deinitialize = t =>
          {
          }
      };

      normalSurfacePool = new Pool<TerrainNormalSurface>(512, t => t.Active)
      {
        Initialize = t =>
        {
          t.Initialize(game.GraphicsDevice, Constants.TerrainNodeTextureSize, Constants.TerrainNodeTextureSize);
        },

        Deinitialize = t =>
        {
        }
      };


      object[] items = new object[512];

      // initialize pools - we want actual initialized items in each pool

      // geometry pool
      for (int i = 0; i < 50; i++)
        items[i] = (object)geometrySurfacePool.New();

      // release items
      for (int i = 0; i < 50; i++)
        ((TerrainGeometrySurface)items[i]).Finished();


      // heightmap surface pool
      for (int i = 0; i < 50; i++)
        items[i] = (object)heightmapSurfacePool.New();

      // release items
      for (int i = 0; i < 50; i++)
        ((TerrainHeightmapSurface)items[i]).Finished();


      // texture surface pool
      for (int i = 0; i < 512; i++)
        items[i] = (object)textureSurfacePool.New();

      // release items
      for (int i = 0; i < 512; i++)
        ((TerrainTextureSurface)items[i]).Finished();


      // normal surface pool
      for (int i = 0; i < 512; i++)
        items[i] = (object)normalSurfacePool.New();

      // release items
      for (int i = 0; i < 512; i++)
        ((TerrainNormalSurface)items[i]).Finished();

    }



    public static void QueueNodeSplit(TerrainNodeSplitItem item)
    {
      geometryQueue.Enqueue(item);
    }

    public static void QueueNodeSplit(TerrainNode parentNode, int childIndex, TerrainNodeBounds bounds, bool isSphere)
    {
      QueueNodeSplit(new TerrainNodeSplitItem(parentNode, childIndex, bounds, isSphere));
    }


    private static void GenerateGeometryMap(TerrainNodeSplitItem item)
    {
      // get surface from pool
      item.GeometrySurface = geometrySurfacePool.New();

      // generate the geometry map on the gpu
      if (item.IsSphere)
        terrainNodeSphereGenerator.Execute(item.GeometrySurface, item.Bounds);
      else
        terrainNodeGenerator.Execute(item.GeometrySurface, item.Bounds);
    }



    private static void GenerateHeightmap(TerrainNodeSplitItem item)
    {
      if (Constants.ForceHeightmapGeneration || !Constants.DisableDiffuseTextureGeneration || !Constants.DisableNormalMapGeneration)
      {
        // get surface from pool
        item.HeightmapSurface = heightmapSurfacePool.New();

        // generate the heightmap on the gpu
        heightmapGenerator.Execute(item.HeightmapSurface, item.Bounds);
      }
    }


    private static void GenerateDiffuseTexture(TerrainNodeSplitItem item)
    {
      if (!Constants.DisableDiffuseTextureGeneration)
      {
        // get diffuse surface from pool
        item.DiffuseSurface = textureSurfacePool.New();

        // generate diffuse texture based on heightmap
        textureGenerator.Execute(item.HeightmapSurface, item.NormalSurface, item.DiffuseSurface, 1.0f / 135.0f); //  135.0f);
      }
    }


    private static void GenerateNormalTexture(TerrainNodeSplitItem item)
    {
      if (!Constants.DisableNormalMapGeneration)
      {
        // get normal surface from pool
        item.NormalSurface = normalSurfacePool.New();

        // generate normal map based on heightmap
        normalGenerator.Execute(item.HeightmapSurface, item.NormalSurface, item.Bounds.Level);
      }
    }



    private static void ProcessGeometryQueue()
    {
      Globals.GeometryQueueDepth = geometryQueue.Count;

      int processed = 0;

      while (geometryQueue.Count > 0 && processed < 8)
      {
        // take a look at the top item
        TerrainNodeSplitItem item = geometryQueue.Peek();

        // see if it's time to process the item
        if (Globals.FrameNumber >= item.ScheduledFrame)
        {
          // remove it from the queue
          geometryQueue.Dequeue();

          // if the node hasn't been cancelled then start the node generation process
          // if it has been cancelled then add it to the node builder thread so it can pass the cancel
          // on to other processes that need to know about it
          if (!item.ParentNode.CancelSplitting)
          {
            GenerateGeometryMap(item);

            // schedule the next step - don't get the texture data until a few frames in the future, 
            // giving it enough time to draw so we don't stall when we pull the data back
            item.ScheduledFrame = Globals.FrameNumber + 3;

            // add it to the next queue
            heightmapQueue.Enqueue(item);

            // only count items that we actuall processed
            processed++;
          }
          else
            SplitTerrainNodeQueue.QueueTerrainNodeSplit(item);

        }
        else
          break;
      }
    }


    private static void ProcessHeightmapQueue()
    {
      Globals.HeightmapQueueDepth = heightmapQueue.Count;

      int processed = 0;

      while (heightmapQueue.Count > 0 && processed < 1)
      {
        // take a look at the top item
        TerrainNodeSplitItem item = heightmapQueue.Peek();

        // see if it's time to process the item
        if (Globals.FrameNumber >= item.ScheduledFrame)
        {
          // remove it from the queue
          heightmapQueue.Dequeue();

          if (!item.ParentNode.CancelSplitting)
          {
            // handle the results from the previous step
            item.GeometrySurface.ResolveHeightData();
            item.CopyHeightData(ref item.GeometrySurface.HeightData);

            if (/*item.ParentNode != null &&*/ item.Bounds.Face == Face.Left)
            {
              //item.GeometrySurface.Save(item);
            }


            // if this is a sphere then we're done otherwise start generating the heightmap
            if (item.IsSphere)
              SplitTerrainNodeQueue.QueueTerrainNodeSplit(item);
            else
            {
              GenerateHeightmap(item);

              // add it to the next queue
              item.ScheduledFrame = Globals.FrameNumber + 1;
              textureQueue.Enqueue(item);
            }
          }
          else
            SplitTerrainNodeQueue.QueueTerrainNodeSplit(item);

        }
        else
          break;

        processed++;
      }
    }


    private static void ProcessTextureQueue()
    {
      Globals.TextureQueueDepth = textureQueue.Count;

      int processed = 0;

      while (textureQueue.Count > 0 && processed < 1)
      {
        // take a look at the top item
        TerrainNodeSplitItem item = textureQueue.Peek();

        // see if it's time to process the item
        if (Globals.FrameNumber >= item.ScheduledFrame)
        {
          // remove it from the queue
          textureQueue.Dequeue();


          if (!item.ParentNode.CancelSplitting)
          {
            // handle the results from the previous step
            if (item.ParentNode != null && item.Bounds.Face == Face.Left)
            {
              if (item.HeightmapSurface != null)
              {
                //item.HeightmapSurface.ResolveHeightData();
                //item.HeightmapSurface.Save(item);
              }
            }


            // TODO : create queues for these so they can be spread out over multiple frames

            // start generating diffuse and normal textures
            GenerateNormalTexture(item);
            GenerateDiffuseTexture(item);


            // add it to the next queue
            meshQueue.Enqueue(item);
          }
          else
            SplitTerrainNodeQueue.QueueTerrainNodeSplit(item);
        }
        else
          break;

        processed++;
      }
    }


    private static void ProcessMeshQueue()
    {
      Globals.MeshQueueDepth = meshQueue.Count;

      int processed = 0;

      while (meshQueue.Count > 0 && processed < 32)
      {
        // take a look at the top item
        TerrainNodeSplitItem item = meshQueue.Peek();

        // see if it's time to process the item
        if (Globals.FrameNumber >= item.ScheduledFrame)
        {
          // remove it from the queue
          meshQueue.Dequeue();

          if (!item.ParentNode.CancelSplitting)
          {
            //// handle the results from the previous step, if any
            //if (item.TextureSurface != null)
            //  item.DiffuseTexture = item.TextureSurface.Texture;

            //if (item.NormalSurface != null)
            //  item.NormalTexture = item.NormalSurface.Texture;

            if (item.ParentNode != null && item.Bounds.Face == Face.Left)
            {
              //item.TextureSurface.Save();
              //item.NormalSurface.Save();
            }
          }


          // queue the item to another thread for the remaining work
          // we always need to queue it even if it's cancelled so we can make sure any previously queued items flow through
          // the thread properly  - the node shouldn't be changed to "not splitting" until all processing has had a chance
          SplitTerrainNodeQueue.QueueTerrainNodeSplit(item);
        }
        else
          break;

        processed++;
      }
    }


    /// <summary>
    /// Fully build a terrain node - used for root nodes
    /// </summary>
    /// <param name="item">Contains data to manage building the node</param>
    public static void BuildNode(TerrainNodeSplitItem item)
    {
      //// preserve current depth buffer and detach it from the device
      //DepthStencilBuffer previousDepth = game.GraphicsDevice.DepthStencilBuffer;
      //game.GraphicsDevice.DepthStencilBuffer = null;

      ///// geometry map /////
      GenerateGeometryMap(item);
      item.GeometrySurface.ResolveHeightData();
      item.CopyHeightData(ref item.GeometrySurface.HeightData);

      if (!item.IsSphere)
      {
        ///// height map /////
        GenerateHeightmap(item);

        ///// normal texture /////
        GenerateNormalTexture(item);

        ///// diffuse texture /////
        GenerateDiffuseTexture(item);
      }


      //// re-attach the normal depth buffer
      //game.GraphicsDevice.DepthStencilBuffer = previousDepth;
    }


    /// <summary>
    /// Process a single terrain node generation request - produces a geometry map for use in building a terrain node mesh
    /// </summary>
    public static void Execute()
    {
      //// preserve current depth buffer and detach it from the device
      //DepthStencilBuffer previousDepth = game.GraphicsDevice.DepthStencilBuffer;
      //game.GraphicsDevice.DepthStencilBuffer = null;


      // process queues
      ProcessGeometryQueue();
      ProcessHeightmapQueue();
      ProcessTextureQueue();
      ProcessMeshQueue();


      // add free pooled objects back to the pool
      geometrySurfacePool.CleanUp();
      heightmapSurfacePool.CleanUp();
      textureSurfacePool.CleanUp();
      normalSurfacePool.CleanUp();


      //// re-attach the normal depth buffer
      //game.GraphicsDevice.DepthStencilBuffer = previousDepth;
    }
  }
}
