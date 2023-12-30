using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class DataPainter
{
    public class CreatePaint
    {
        public int[,] DataToDraw;
        public System.Action<Mesh> OnComplete;
    }

    private WorldGenerator Generator;
    private Queue<CreatePaint> PaintsToCreate;
    public bool Terminate;

    public DataPainter(WorldGenerator worldGen)
    {
        PaintsToCreate = new Queue<CreatePaint>();
        Generator = worldGen;
        Generator.StartCoroutine(PaintGenLoop());
    }


    public void QueueDataToDraw(CreatePaint createMeshData)
    {
        PaintsToCreate.Enqueue(createMeshData);
    }

    public IEnumerator PaintGenLoop()
    {
        while (Terminate == false)
        {
            if (PaintsToCreate.Count > 0)
            {
                CreatePaint createMesh = PaintsToCreate.Dequeue();
                yield return Generator.StartCoroutine(CreatePaintFromData(createMesh.DataToDraw, createMesh.OnComplete));
            }

            yield return null;
        }
    }


    public IEnumerator CreatePaintFromData(int[,] Data, System.Action<Mesh> callback)
    {
        Task t = Task.Factory.StartNew(delegate
        {
            for (int x = 0; x < WorldGenerator.ChunkSize.x; x++)
            {
                for (int y = 0; y < WorldGenerator.ChunkSize.y; y++)
                {
                    Vector3Int BlockPos = new Vector3Int(x, y);
                       
                }
            }
        });

        yield return new WaitUntil(() => {
            return t.IsCompleted || t.IsCanceled;
        });

        if (t.Exception != null)
            Debug.LogError(t.Exception);

    }
}
