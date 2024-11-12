﻿using System.Diagnostics;
using System.Numerics;
using System.Xml.Schema;
using Raylib_cs;
using verlet;

class Program
{
    
    /// <summary>
    ///     Main method for the verlet simulation, initialized as 800x600
    /// </summary>
    static void Main()
    {
        int WIDTH = 800;
        int HEIGHT = 600;
        
        Raylib.InitWindow(WIDTH, HEIGHT, "Verlet");
        Raylib.SetTargetFPS(60);        

        // Set of verlets
        HashSet<Verlet> verlets = new();
        
        // Set of faucets
        Dictionary<Vector2, Color> faucets = new();

        Solver solver = new();
        
        int frameNumber = 0;
        while (!Raylib.WindowShouldClose() )
        {
            frameNumber++;

            Raylib.BeginDrawing();
            Raylib.ClearBackground(Color.Black); // Clear background


            // Show Bounds
            //Raylib.DrawCircle(WIDTH / 2, HEIGHT / 2, 300, Color.White);
            //Raylib.DrawCircle(WIDTH / 2, HEIGHT / 2, 299, Color.Black);


            Solve(solver, verlets);
            LeftMouseLogic(verlets, frameNumber);
            RightMouseLogic(verlets, faucets, frameNumber);
            
            Raylib.DrawFPS(20, 20); // Display FPS

            Raylib.EndDrawing();
        }
        
        Raylib.CloseWindow();
    }
    
    /// <summary>
    ///     Handles the logic for the left mouse button, this will spawn a ball at the mouse position every 4 frames
    /// </summary>
    /// <param name="verlets">Set of verlets to solve and draw</param>
    /// <param name="frameNumber">Frame index</param>
    private static void LeftMouseLogic(HashSet<Verlet> verlets, int frameNumber)
    {
        if (Raylib.IsMouseButtonDown(MouseButton.Left))
        {
            if (frameNumber % 4 == 0)
            {
                float mouseX = Raylib.GetMousePosition().X;
                float mouseY = Raylib.GetMousePosition().Y;

                Color color = new Color(frameNumber % 255, 0, 255, 255);
                
                Verlet verlet = new Verlet(new Vector2(mouseX, mouseY), new Vector2(0.5f, 0), 4, color);
                verlets.Add(verlet);
            }
        }
    }

    /// <summary>
    ///     Handles the logic for the right mouse button, this will create an inlet of balls (faucet) that spawns a ball once every 4th frame.
    /// </summary>
    /// <param name="verlets">Set of verlets to solve and draw</param>
    /// <param name="faucets">Set of existing faucets</param>
    /// <param name="frameNumber">frame index</param>
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
                Verlet verlet = new Verlet(faucet, new Vector2(0,-4f), 3, new Color(faucets[faucet].R, frameNumber/9 % 255, faucets[faucet].B, 255));
                verlets.Add(verlet);
            }
        }
    }

    /// <summary>
    ///     Method for solving the balls and drawing them to the screen
    /// </summary>
    /// <param name="solver">Solver object</param>
    /// <param name="verlets">Set of verlets to solve and draw</param>
    private static void Solve(Solver solver, HashSet<Verlet> verlets)
    {
        // dt is set to 1/60
        solver.Update(verlets,  0.0166f);
            
        foreach (Verlet verlet in verlets)
        {
            //Color color = Raylib.ColorFromHSV((verlet.velocity.Length() * 90)+180, 1, 1); // Velocity coloring
            //Raylib.DrawCircle(verlet.getX, verlet.getY, verlet.radius, color); // Draw circles by velocity
            Raylib.DrawCircle(verlet.getX, verlet.getY, verlet.radius, verlet.color);
        }
    }
}