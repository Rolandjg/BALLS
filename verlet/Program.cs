using System.Diagnostics;
using System.Numerics;
using System.Xml.Schema;
using Raylib_cs;
using verlet;

class Program
{
    static void Main()
    {
        int WIDTH = 800;
        int HEIGHT = 600;
        
        Raylib.InitWindow(WIDTH, HEIGHT, "Verlet");
        Raylib.SetTargetFPS(60);            

        HashSet<Verlet> verlets = new();
        Dictionary<Vector2, Color> faucets = new();

        Solver solver = new();
        
        int frameNumber = 0;
        while (!Raylib.WindowShouldClose() )
        {
            frameNumber++;

            Raylib.BeginDrawing();
            Raylib.ClearBackground(Color.Black); // Clear background


            // Show Bounds
            Raylib.DrawCircle(WIDTH / 2, HEIGHT / 2, 300, Color.White);
            Raylib.DrawCircle(WIDTH / 2, HEIGHT / 2, 299, Color.Black);


            Solve(solver, verlets);
            LeftMouseLogic(verlets, frameNumber);
            RightMouseLogic(verlets, faucets, frameNumber);
                

            Raylib.DrawFPS(20, 20);

            Raylib.EndDrawing();
        }
        
        Raylib.CloseWindow();
    }
    
    private static void LeftMouseLogic(HashSet<Verlet> verlets, int frameNumber)
    {
        if (Raylib.IsMouseButtonDown(MouseButton.Left))
        {
            if (frameNumber % 4 == 0)
            {
                float mouseX = Raylib.GetMousePosition().X;
                float mouseY = Raylib.GetMousePosition().Y;

                Color color = new Color(frameNumber % 255, 0, 255, 255);
                
                Verlet verlet = new Verlet(new Vector2(mouseX, mouseY), new Vector2(0.5f, 0), 3, color);
                verlets.Add(verlet);
            }
        }
    }

    private static void RightMouseLogic(HashSet<Verlet> verlets, Dictionary<Vector2, Color> faucets, int frameNumber)
    {
        Random rng = new Random();

        if (Raylib.IsMouseButtonPressed(MouseButton.Right))
        {
            float mouseX = Raylib.GetMousePosition().X;
            float mouseY = Raylib.GetMousePosition().Y;
            
            faucets.TryAdd(new Vector2(mouseX, mouseY), new Color(rng.Next(0, 255), rng.Next(0, 255), rng.Next(150, 255), 255));
        }

        if (frameNumber % 4 == 0)
        {
            foreach (Vector2 faucet in faucets.Keys)
            {
                Verlet verlet = new Verlet(faucet, new Vector2(1f,-0.5f), 3, faucets[faucet]);
                verlets.Add(verlet);
            }
        }
    }

    private static void Solve(Solver solver, HashSet<Verlet> verlets)
    {
        solver.Update(verlets,  0.0166f);
            
        foreach (Verlet verlet in verlets)
        {
            Raylib.DrawCircle(verlet.getX, verlet.getY, verlet.radius, verlet.color);
        }
    }
}