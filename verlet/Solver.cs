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
            
            Thread u1 = new Thread(() => CollisionCells(verlets, 1, 1, 22, 17, hash));
            Thread u2 = new Thread(() => CollisionCells(verlets, 24, 1, 24, 17, hash));
            Thread u3 = new Thread(() => CollisionCells(verlets, 49, 1, 24, 17, hash));
            Thread u4 = new Thread(() => CollisionCells(verlets, 74, 1, 25, 17, hash));
            
            Thread m1 = new Thread(() => CollisionCells(verlets, 1, 19, 22, 17, hash));
            Thread m2 = new Thread(() => CollisionCells(verlets, 24, 19, 24, 17, hash));
            Thread m3 = new Thread(() => CollisionCells(verlets, 49, 19, 24, 17, hash));
            Thread m4 = new Thread(() => CollisionCells(verlets, 74, 19, 25, 17, hash));
            Thread m5 = new Thread(() => CollisionCells(verlets, 1, 37, 22, 17, hash));
            Thread m6 = new Thread(() => CollisionCells(verlets, 24, 37, 24, 17, hash));
            Thread m7 = new Thread(() => CollisionCells(verlets, 49, 37, 24, 17, hash));
            Thread m8 = new Thread(() => CollisionCells(verlets, 74, 37, 25, 17, hash));
            
            Thread b2 = new Thread(() => CollisionCells(verlets, 13, 55, 11, 10, hash));
            Thread b3 = new Thread(() => CollisionCells(verlets, 25, 55, 11, 10, hash));
            Thread b4 = new Thread(() => CollisionCells(verlets, 37, 55, 11, 10, hash));
            Thread b5 = new Thread(() => CollisionCells(verlets, 49, 55, 11, 10, hash));
            Thread b6 = new Thread(() => CollisionCells(verlets, 61, 55, 12, 10, hash));
            Thread b7 = new Thread(() => CollisionCells(verlets, 74, 55, 11, 10, hash));
            
            Thread b8 = new Thread(() => CollisionCells(verlets, 25, 66, 11, 9, hash));
            Thread b9 = new Thread(() => CollisionCells(verlets, 37, 66, 11, 9, hash));
            Thread b10 = new Thread(() => CollisionCells(verlets, 49, 66, 11, 9, hash));
            Thread b11 = new Thread(() => CollisionCells(verlets, 61, 66, 11, 9, hash));
            Thread b12 = new Thread(() => CollisionCells(verlets, 73, 66, 11, 9, hash));
            
            u1.Start();
            u2.Start();
            u3.Start();
            u4.Start();
            
            m1.Start();
            m2.Start();
            m3.Start();
            m4.Start();
            m5.Start();
            m6.Start();
            m7.Start();
            m8.Start();
            
            b2.Start();
            b3.Start();
            b4.Start();
            b5.Start();
            b6.Start();
            b7.Start();
            b8.Start();
            b9.Start();
            b10.Start();
            b11.Start();
            b12.Start();
            
            u1.Join();
            u2.Join();
            u3.Join();
            u4.Join();
            
            m1.Join();
            m2.Join();
            m3.Join();
            m4.Join();
            m5.Join();
            m6.Join();
            m7.Join();
            m8.Join();
            
            b2.Join();
            b3.Join();
            b4.Join();
            b5.Join();
            b6.Join();
            b7.Join();
            b8.Join();
            b9.Join();
            b10.Join();
            b11.Join();
            b12.Join();
            
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
        
        
        Raylib.DrawRectangle(gridX*8, gridY*8, gridWidth*8, gridHeight*8, Color.Orange);
        Raylib.DrawRectangle(gridX*8+1, gridY*8+1, gridWidth*8-2, gridHeight*8-2, Color.Black);
        
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
        float delta = verlet1.radius + verlet2.radius - distance;
        verlet1.currentPosition += delta * 0.5f * n;
        verlet2.currentPosition -= delta * 0.5f * n;
    }
}