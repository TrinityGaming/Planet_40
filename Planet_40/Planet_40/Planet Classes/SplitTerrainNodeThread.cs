using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Planet
{
  public static class SplitTerrainNodeQueue
  {
    static Queue<TerrainNodeSplitItem> splitQueue;
    static Thread splitThread;
    static ManualResetEvent dataAvailableEvent;


    static SplitTerrainNodeQueue()    
    {
      dataAvailableEvent = new ManualResetEvent(false);

      splitQueue = new Queue<TerrainNodeSplitItem>();

      splitThread = new Thread(new ThreadStart(ProcessQueue));
      splitThread.Start();
    }

    public static void QueueTerrainNodeSplit(TerrainNodeSplitItem item)
    {
      lock (splitQueue)
      {
        splitQueue.Enqueue(item);
        dataAvailableEvent.Set();
      }
    }

    public static void Quit()
    {
      splitThread.Abort();
      splitThread = null;
    }


    private static void ProcessQueue()
    {
      TerrainNodeSplitItem item;

#if XBOX
      // run thread on differenet processor 
      Thread.CurrentThread.SetProcessorAffinity(3);
#endif

      try
      {
        while (true)
        {
          // wait a while for data to be available
#if XBOX
          dataAvailableEvent.WaitOne(1000, false);
#else
          dataAvailableEvent.WaitOne(1000);
#endif

          // if the queue has an item pull it off
          lock (splitQueue)
          {
            if (splitQueue.Count > 0)
              item = splitQueue.Dequeue();
            else
            {
              item = null;
              dataAvailableEvent.Reset();
            }
          }

          // if an item was found on the queue then generate the mesh
          // if ParentNode gets its final child it will turn off its Splitting flag
          // indicating the split is entirely complete
          if (item != null)
          {
            item.ParentNode.GenerateChild(item);
            item.Finished();
          }
        }
      }
      catch (ThreadAbortException)
      {
        // this is okay - somebody cancelled the thread
      }
      catch
      {
        throw;
      }
    }

  }

}
