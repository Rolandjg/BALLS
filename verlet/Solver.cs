using System.Diagnostics;
using System.Numerics;
using System.Xml.Schema;
using Raylib_cs;

namespace verlet;

public class Solver
{
    private Vector2 GRAVITY = new Vector2(0, 1500.0f);
    private int GRID_WIDTH = 100;
    private int GRID_HEIGHT = 75;
    
    public void Update(HashSet<Verlet> verlets, float dt)
    {
        int substeps = 8;
        var hash = CreateSpatialHash(verlets);
        
        for (int i = 0; i < substeps; i++)
        {
            Gravity(verlets);
            Constrain(verlets);

            // Compute spatial hash once every 3 substeps
            if (i % 3 == 1)
            {
                hash = CreateSpatialHash(verlets);
            }

            var tasks = new Task[10];
            tasks[0] = Task.Run(() => CollisionCells(verlets, 1, 1, 9, 74, hash));
            tasks[1] = Task.Run(() => CollisionCells(verlets, 10, 1, 9, 74, hash));
            tasks[2] = Task.Run(() => CollisionCells(verlets, 20, 1, 9, 74, hash));
            tasks[3] = Task.Run(() => CollisionCells(verlets, 30, 1, 9, 74, hash));
            tasks[4] = Task.Run(() => CollisionCells(verlets, 40, 1, 9, 74, hash));
            tasks[5] = Task.Run(() => CollisionCells(verlets, 50, 1, 9, 74, hash));
            tasks[6] = Task.Run(() => CollisionCells(verlets, 60, 1, 9, 74, hash));
            tasks[7] = Task.Run(() => CollisionCells(verlets, 70, 1, 9, 74, hash));
            tasks[8] = Task.Run(() => CollisionCells(verlets, 80, 1, 9, 74, hash));
            tasks[9] = Task.Run(() => CollisionCells(verlets, 90, 1, 9, 74, hash));

            
            Task.WaitAll(tasks);
            
            UpdatePositions(verlets, dt/substeps);
        }
    }
    
    private Dictionary<(int, int), List<Verlet>> CreateSpatialHash(IEnumerable<Verlet> verlets)
    {
        Dictionary<(int, int), List<Verlet>> hash = new();

        foreach (Verlet verlet in verlets)
        {
            int cellX = (int)(verlet.currentPosition.X /8);
            int cellY = (int)(verlet.currentPosition.Y /8);

            var key = (cellX, cellY);
            if (!hash.ContainsKey(key))
            {
                hash[key] = new List<Verlet>();
            }

            hash[key].Add(verlet);
        }

        return hash;
    }
    
    private void UpdatePositions(IEnumerable<Verlet> verlets, float dt)
    {
        foreach (Verlet verlet in verlets)
        {
            verlet.updatePostion(dt);
        }
    }
    
    private void Gravity(IEnumerable<Verlet> verlets)
    {
        foreach (Verlet verlet in verlets)
        {
            verlet.accelerate(GRAVITY);
        }
    }

    private void Constrain(IEnumerable<Verlet> verlets)
    {
        Vector2 position = new Vector2(800/2, 600/2);
        float radius = 300f;
        
        foreach (Verlet verlet in verlets)
        {
            Vector2 distToConstraint = verlet.currentPosition - position;
            float distance = distToConstraint.Length();
            
            if (distance > radius - verlet.radius)
            {
                Vector2 n = distToConstraint / distance;

                // Correct the position to the boundary of the circle
                verlet.currentPosition = position + n * (radius - verlet.radius);

            }
        }
    }
    
    private void CollisionCells(IEnumerable<Verlet> verlets, int gridX, int gridY, int gridWidth, int gridHeight, Dictionary<(int, int), List<Verlet>> hash)
    {
        // Draw thread bounds   
        //Raylib.DrawRectangle(gridX*8, gridY*8, gridWidth*8, gridHeight*8, Color.Orange);
        //Raylib.DrawRectangle(gridX*8+1, gridY*8+1, gridWidth*8-2, gridHeight*8-2, Color.Black);
        
        foreach (var cell in hash)
        {
            (int x, int y) = cell.Key;

            if (x >= gridX && x <= gridX + gridWidth && y >= gridY && y <= gridY + gridHeight)
            {

                List<Verlet> cellVerlets = cell.Value;

                // Check collisions within the cell and its 8 neighbors
                for (int dx = -1; dx <= 1; dx++)
                {
                    for (int dy = -1; dy <= 1; dy++)
                    {
                        var neighborKey = (x + dx, y + dy);
                        if (hash.TryGetValue(neighborKey, out var neighborVerlets))
                        {
                            foreach (Verlet verlet1 in cellVerlets)
                            {
                                foreach (Verlet verlet2 in neighborVerlets)
                                {
                                    if (verlet1 != verlet2 && IsColliding(verlet1, verlet2))
                                    {
                                        Collide(verlet1, verlet2);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    private bool IsColliding(Verlet verlet1, Verlet verlet2)
    {
        Vector2 collisionAxis = verlet1.currentPosition - verlet2.currentPosition;
        float distance = collisionAxis.Length();
        return (distance < verlet1.radius + verlet2.radius);
    }
    
    private void Collide(Verlet verlet1, Verlet verlet2)
    {
        Vector2 collisionAxis = verlet1.currentPosition - verlet2.currentPosition;
        float distance = collisionAxis.Length();
        Vector2 n = collisionAxis / distance;
        
        // Allow for balls to clip a little bit
        float delta = verlet1.radius + verlet2.radius - distance;
        verlet1.currentPosition += delta * 0.5f * n;
        verlet2.currentPosition -= delta * 0.5f * n;
    }
}