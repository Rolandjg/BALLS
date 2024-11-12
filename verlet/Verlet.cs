using System.Numerics;
using Raylib_cs;

namespace verlet;

public class Verlet
{
    public Vector2 currentPosition {get; set;}
    public Vector2 previousPosition {get; set;}
    public Vector2 acceleration {get; set;}
    public int radius {get; set;}
    public Color color {get; set;}
    public Vector2 velocity {get; set;}
    private Vector2 velocityOld;
    
    public Verlet(Vector2 startPosition, Vector2 initialVelocity, int radius, Color color)
    {
        this.currentPosition = startPosition;
        this.previousPosition = startPosition - initialVelocity;
        this.radius = radius;
        this.color = color;
    }
    
    public int getX => (int)currentPosition.X;
    public int getY => (int)currentPosition.Y;

    public void updatePostion(float dt)
    {
        velocity = (currentPosition - previousPosition);
        // Max velocity
        if (velocity.Length() > 4)
        {
            velocity = velocityOld;
        }
        
        // Set current position to previous position
        previousPosition = currentPosition;
        
        // Verlet integration
        currentPosition += velocity + acceleration * dt * dt;
        
        // Reset acceleration
        acceleration = Vector2.Zero; // Reset acceleration after applying it
    }

    public void accelerate(Vector2 acceleration)
    {
        this.acceleration += acceleration;
    }
}