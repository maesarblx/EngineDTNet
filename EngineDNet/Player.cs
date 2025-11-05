using System.Numerics;
using BepuPhysics;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace EngineDNet;



public class Player
{
    public Vector3 Position = Vector3.UnitY * 6;
    public Vector3 MoveDirection = Vector3.Zero;
    public Vector3 WorldMoveDirection = Vector3.Zero;
    public Vector3 Velocity = Vector3.Zero;
    public float MoveSpeed = 6f;
    public float Height = 2f;

    public float GroundAcceleration = 20f;
    public float AirAcceleration = 10f;
    public float GroundFriction = 12f;
    public float Gravity = -20f;
    public float JumpSpeed = 6f;
    public float CollisionDistance = 0.45f;
    public float MaxSpeed => MoveSpeed;


    public SimpleRayHitHandler GroundHitHandler = new SimpleRayHitHandler();
    public SimpleRayHitHandler WHitHandler = new SimpleRayHitHandler();
    public SimpleRayHitHandler AHitHandler = new SimpleRayHitHandler();
    public SimpleRayHitHandler SHitHandler = new SimpleRayHitHandler();
    public SimpleRayHitHandler DHitHandler = new SimpleRayHitHandler();

    private bool grounded = false;

    public Player() { }

    public void ProcessInput()
    {
        WHitHandler.Reset();
        AHitHandler.Reset();
        SHitHandler.Reset();
        DHitHandler.Reset();
        var targetMoveLocal = Vector3.Zero;
        var targetMoveWorld = Vector3.Zero;
        if (Core.IsKeyDown(Keys.W))
        {
            var l = -Vector3.UnitZ;
            var w = -Core.CurrentCamera.GetFrontVector() * Utils.FlatXZ;
            Core.CurrentScene.Simulation.RayCast(Position, w * CollisionDistance, CollisionDistance, ref WHitHandler);
            if (!WHitHandler.Hit)
            {
                targetMoveLocal += l;
                targetMoveWorld += w;
            }
        }
        if (Core.IsKeyDown(Keys.S))
        {
            var l = Vector3.UnitZ;
            var w = Core.CurrentCamera.GetFrontVector() * Utils.FlatXZ;
            Core.CurrentScene.Simulation.RayCast(Position, w * CollisionDistance, CollisionDistance, ref WHitHandler);
            if (!WHitHandler.Hit)
            {
                targetMoveLocal += l;
                targetMoveWorld += w;
            }
        }
        if (Core.IsKeyDown(Keys.D))
        {
            var l = -Vector3.UnitX;
            var w = -Core.CurrentCamera.GetRightVector() * Utils.FlatXZ;
            Core.CurrentScene.Simulation.RayCast(Position, w * CollisionDistance, CollisionDistance, ref WHitHandler);
            if (!WHitHandler.Hit)
            {
                targetMoveLocal += l;
                targetMoveWorld += w;
            }
        }
        if (Core.IsKeyDown(Keys.A))
        {
            var l = Vector3.UnitX;
            var w = Core.CurrentCamera.GetRightVector() * Utils.FlatXZ;
            Core.CurrentScene.Simulation.RayCast(Position, w * CollisionDistance, CollisionDistance, ref WHitHandler);
            if (!WHitHandler.Hit)
            {
                targetMoveLocal += l;
                targetMoveWorld += w;
            }
        }

        Vector3 worldXZ = new Vector3(targetMoveWorld.X, 0f, targetMoveWorld.Z);
        if (worldXZ.LengthSquared() > 0.0001f) worldXZ = Vector3.Normalize(worldXZ);
        else worldXZ = Vector3.Zero;

        Vector3 localXZ = new Vector3(targetMoveLocal.X, 0f, targetMoveLocal.Z);
        if (localXZ.LengthSquared() > 0.0001f) localXZ = Vector3.Normalize(localXZ);
        else localXZ = Vector3.Zero;

        MoveDirection = localXZ;
        WorldMoveDirection = worldXZ;

        if (Core.IsKeyDown(Keys.Space) && grounded)
        {
            Velocity = new Vector3(Velocity.X, JumpSpeed, Velocity.Z);
            grounded = false;
        }
    }

    public void Update(float dt)
    {
        GroundHitHandler.Reset();
        var Direction = Vector3.UnitY * -Height;
        Core.CurrentScene.Simulation.RayCast(Position, Direction, 1, ref GroundHitHandler);
        var hitPoint = GroundHitHandler.Hit ? Position + Direction * GroundHitHandler.T : Vector3.Zero;
        var groundY = GroundHitHandler.Hit ? hitPoint.Y+(Height/2) : -5000;
        if (Position.Y <= groundY + 0.001f)
        {
            grounded = true;
            Position.Y = groundY;
            if (Velocity.Y < 0) Velocity = new Vector3(Velocity.X, 0f, Velocity.Z);
        }
        else
        {
            grounded = false;
        }

        var velXZ = new Vector3(Velocity.X, 0f, Velocity.Z);
        var desiredVelXZ = WorldMoveDirection * MaxSpeed;

        var accel = grounded ? GroundAcceleration : AirAcceleration;

        var accelVector = (desiredVelXZ - velXZ) * accel;

        velXZ += accelVector * dt;

        if (WorldMoveDirection == Vector3.Zero && grounded)
        {
            var stopFactor = 1f - MathF.Min(1f, GroundFriction * dt);
            velXZ *= stopFactor;
            if (velXZ.LengthSquared() < 0.0001f) velXZ = Vector3.Zero;
        }

        var speedXZ = velXZ.Length();
        if (speedXZ > MaxSpeed)
        {
            velXZ = Vector3.Normalize(velXZ) * MaxSpeed;
        }

        var vy = Velocity.Y;
        vy += Gravity * dt;

        Velocity = new Vector3(velXZ.X, vy, velXZ.Z);

        Position += Velocity * dt;
    }
}