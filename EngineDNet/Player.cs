using BepuPhysics;
using BepuPhysics.Collidables;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System.Numerics;

namespace EngineDNet;

public class Player
{
    public Vector3 Position = Vector3.UnitY * 6;
    public Vector3 MoveDirection = Vector3.Zero;
    public Vector3 WorldMoveDirection = Vector3.Zero;
    public Vector3 Velocity = Vector3.Zero;
    public float MoveSpeed = 6f;
    public float Height = 2f;
    public float Radius = 0.3f;

    public float GroundAcceleration = 40f;
    public float AirAcceleration = 20f;
    public float GroundFriction = 12f;
    public float Gravity = -20f;
    public float JumpSpeed = 6f;
    public float CollisionDistance = 0.45f;
    public float MaxSpeed => MoveSpeed + 0.1f;

    public SimpleRayHitHandler GroundHitHandler = new SimpleRayHitHandler();

    private bool grounded = false;
    private BodyHandle bodyHandle;
    private float mass = 10f;

    public Player() 
    {
        var capsule = new Capsule(Radius, Height - 2 * Radius);
        var shape = Core.CurrentScene.Simulation.Shapes.Add(capsule);
        var inertia = capsule.ComputeInertia(mass);
        var bodyDesc = BodyDescription.CreateDynamic(new RigidPose(Position), inertia, shape, new BodyActivityDescription(0.01f));
        bodyHandle = Core.CurrentScene.Simulation.Bodies.Add(bodyDesc);
        GroundHitHandler.DIgnored.Add(bodyHandle);
    }

    public void ProcessInput()
    {
        var targetMoveLocal = Vector3.Zero;
        var targetMoveWorld = Vector3.Zero;
        if (Core.IsKeyDown(Keys.W))
        {
            var l = -Vector3.UnitZ;
            var w = -Core.CurrentCamera.GetFrontVector() * Utils.FlatXZ;
            targetMoveLocal += l;
            targetMoveWorld += w;
        }
        if (Core.IsKeyDown(Keys.S))
        {
            var l = Vector3.UnitZ;
            var w = Core.CurrentCamera.GetFrontVector() * Utils.FlatXZ;
            targetMoveLocal += l;
            targetMoveWorld += w;
        }
        if (Core.IsKeyDown(Keys.D))
        {
            var l = -Vector3.UnitX;
            var w = -Core.CurrentCamera.GetRightVector() * Utils.FlatXZ;
            targetMoveLocal += l;
            targetMoveWorld += w;
        }
        if (Core.IsKeyDown(Keys.A))
        {
            var l = Vector3.UnitX;
            var w = Core.CurrentCamera.GetRightVector() * Utils.FlatXZ;
            targetMoveLocal += l;
            targetMoveWorld += w;
        }

        Vector3 worldXZ = new Vector3(targetMoveWorld.X, 0f, targetMoveWorld.Z);
        if (worldXZ.LengthSquared() > 0.0001f) worldXZ = Vector3.Normalize(worldXZ);
        else worldXZ = Vector3.Zero;

        Vector3 localXZ = new Vector3(targetMoveLocal.X, 0f, targetMoveLocal.Z);
        if (localXZ.LengthSquared() > 0.0001f) localXZ = Vector3.Normalize(localXZ);
        else localXZ = Vector3.Zero;

        MoveDirection = localXZ;
        WorldMoveDirection = worldXZ;
    }

    public void FixedUpdate(float dt)
    {
        var bodyRef = Core.CurrentScene.Simulation.Bodies.GetBodyReference(bodyHandle);

        var pos = bodyRef.Pose.Position;

        GroundHitHandler.Reset();

        var halfHeight = Height * 0.5f;
        var groundCheckMargin = 0.12f;
        var rayLength = halfHeight + groundCheckMargin;

        Core.CurrentScene.Simulation.RayCast(pos, -Vector3.UnitY, rayLength, ref GroundHitHandler);

        grounded = GroundHitHandler.Hit && Vector3.Dot(GroundHitHandler.Normal, Vector3.UnitY) > 0.7f;

        var vel = bodyRef.Velocity.Linear;
        var velXZ = new Vector3(vel.X, 0f, vel.Z);
        var desiredXZ = WorldMoveDirection * MaxSpeed;

        var accel = grounded ? GroundAcceleration : AirAcceleration;
        var maxDeltaThisFrame = accel * dt;

        var deltaV = desiredXZ - velXZ;
        var deltaLen = deltaV.Length();
        if (deltaLen > 0f)
        {
            if (deltaLen > maxDeltaThisFrame)
                deltaV = Vector3.Normalize(deltaV) * maxDeltaThisFrame;

            vel.X += deltaV.X;
            vel.Z += deltaV.Z;
        }

        if (WorldMoveDirection == Vector3.Zero && grounded)
        {
            var stopFactor = 1f - MathF.Min(1f, GroundFriction * dt);
            vel.X *= stopFactor;
            vel.Z *= stopFactor;
            if (new Vector3(vel.X, 0f, vel.Z).LengthSquared() < 0.0001f)
            {
                vel.X = 0f; vel.Z = 0f;
            }
        }

        var speedXZ = new Vector3(vel.X, 0f, vel.Z).Length();
        if (speedXZ > MaxSpeed)
        {
            var norm = Vector3.Normalize(new Vector3(vel.X, 0f, vel.Z));
            vel.X = norm.X * MaxSpeed;
            vel.Z = norm.Z * MaxSpeed;
        }

        var wantJump = Core.IsKeyDown(Keys.Space);
        if (wantJump && grounded)
        {
            vel.Y += JumpSpeed * 0.01f;
            grounded = false;
        }

        bodyRef.Velocity.Angular = Vector3.Zero;

        bodyRef.Velocity.Linear = vel;
        bodyRef.Awake = true;

        Position = bodyRef.Pose.Position;
    }


    [Obsolete("This method is no longer supported and will be removed. Use FixedUpdate instead.", true)]
    public void Update(float dt)
    {
        //GroundHitHandler.Reset();
        //var groundDirection = Vector3.UnitY * -Height;
        //Core.CurrentScene.Simulation.RayCast(Position, groundDirection, 1, ref GroundHitHandler);
        //var hitPoint = GroundHitHandler.Hit ? Position + groundDirection * GroundHitHandler.T : Vector3.Zero;
        //var groundY = GroundHitHandler.Hit ? hitPoint.Y+(Height/2) : -5000;
        //if (Position.Y <= groundY + 0.001f)
        //{
        //    grounded = true;
        //    Position.Y = groundY;
        //    if (Velocity.Y < 0) Velocity = new Vector3(Velocity.X, 0f, Velocity.Z);
        //}
        //else
        //{
        //    grounded = false;
        //}

        //var velXZ = new Vector3(Velocity.X, 0f, Velocity.Z);
        //var desiredVelXZ = WorldMoveDirection * MaxSpeed;

        //var accel = grounded ? GroundAcceleration : AirAcceleration;

        //var accelVector = (desiredVelXZ - velXZ) * accel;

        //velXZ += accelVector * dt;

        //if (WorldMoveDirection == Vector3.Zero && grounded)
        //{
        //    var stopFactor = 1f - MathF.Min(1f, GroundFriction * dt);
        //    velXZ *= stopFactor;
        //    if (velXZ.LengthSquared() < 0.0001f) velXZ = Vector3.Zero;
        //}

        //var speedXZ = velXZ.Length();
        //if (speedXZ > MaxSpeed)
        //{
        //    velXZ = Vector3.Normalize(velXZ) * MaxSpeed;
        //}

        //var vy = Velocity.Y;
        //vy += Gravity * dt;

        //Velocity = new Vector3(velXZ.X, vy, velXZ.Z);

        //Position += Velocity * dt;
    }
}