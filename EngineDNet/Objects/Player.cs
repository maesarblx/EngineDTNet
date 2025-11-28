using BepuPhysics;
using BepuPhysics.Collidables;
using EngineDNet.Workspace;
using EngineDNet.Utilities;
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

    public bool Sprinting = false;
    public bool Crouching = false;

    public float GroundAcceleration = 40f;
    public float AirAcceleration = 20f;
    public float GroundFriction = 1f;
    public float Gravity = -20f;
    public float JumpSpeed = 6f;
    public float MaxSpeed => 150;

    public float MoveAcceleration = 16;

    public SimpleRayHitHandler GroundHitHandler = new SimpleRayHitHandler();

    private bool _grounded = false;
    private BodyHandle bodyHandle;
    private float _mass = 8f;

    public Player()
    {
        Capsule capsule = new(Radius, Height - 2 * Radius);
        TypedIndex shape = Core.CurrentScene.Simulation.Shapes.Add(capsule);
        BodyInertia inertia = capsule.ComputeInertia(_mass);

        BodyDescription bodyDesc = BodyDescription.CreateDynamic(new RigidPose(Position), inertia, shape, new BodyActivityDescription(0.01f));
        bodyHandle = Core.CurrentScene.Simulation.Bodies.Add(bodyDesc);
        GroundHitHandler.DIgnored.Add(bodyHandle);
    }

    public void ProcessInput(float dt)
    {
        Vector3 targetMoveLocal = Vector3.Zero;
        Vector3 targetMoveWorld = Vector3.Zero;
        if (Core.IsKeyDown(Keys.W))
        {
            Vector3 l = -Vector3.UnitZ;
            Vector3 w = -Core.CurrentCamera.GetFrontVector() * Utils.FlatXZ;
            targetMoveLocal += l;
            targetMoveWorld += w;
        }
        if (Core.IsKeyDown(Keys.S))
        {
            Vector3 l = Vector3.UnitZ;
            Vector3 w = Core.CurrentCamera.GetFrontVector() * Utils.FlatXZ;
            targetMoveLocal += l;
            targetMoveWorld += w;
        }
        if (Core.IsKeyDown(Keys.D))
        {
            Vector3 l = -Vector3.UnitX;
            Vector3 w = -Core.CurrentCamera.GetRightVector() * Utils.FlatXZ;
            targetMoveLocal += l;
            targetMoveWorld += w;
        }
        if (Core.IsKeyDown(Keys.A))
        {
            Vector3 l = Vector3.UnitX;
            Vector3 w = Core.CurrentCamera.GetRightVector() * Utils.FlatXZ;
            targetMoveLocal += l;
            targetMoveWorld += w;
        }

        Vector3 worldXZ = new Vector3(targetMoveWorld.X, 0f, targetMoveWorld.Z);
        if (worldXZ.LengthSquared() > 0.0001f) worldXZ = Vector3.Normalize(worldXZ);
        else worldXZ = Vector3.Zero;

        Vector3 localXZ = new Vector3(targetMoveLocal.X, 0f, targetMoveLocal.Z);
        if (localXZ.LengthSquared() > 0.0001f) localXZ = Vector3.Normalize(localXZ);
        else localXZ = Vector3.Zero;

        MoveDirection = Vector3.Lerp(MoveDirection, localXZ, 1f - MathF.Exp(-MoveAcceleration * dt));
        WorldMoveDirection = Vector3.Lerp(WorldMoveDirection, worldXZ, 1f - MathF.Exp(-MoveAcceleration * dt));
    }

    public void RenderUpdate(float dt)
    {
        Sprinting = Core.IsKeyDown(Keys.LeftShift);
        Crouching = Core.IsKeyDown(Keys.LeftControl);

        Height = Utils.Lerp(Height, Crouching ? 1 : 2, 1f - MathF.Exp(-8 * dt));
        MoveSpeed = Utils.Lerp(MoveSpeed, (Sprinting && !Crouching) ? 10 : 6 * (Height / 2), 1f - MathF.Exp(-8 * dt));
    }

    public void FixedUpdate(float dt)
    {
        BodyReference bodyRef = Core.CurrentScene.Simulation.Bodies.GetBodyReference(bodyHandle);

        Vector3 pos = bodyRef.Pose.Position;

        GroundHitHandler.Reset();

        float halfHeight = Height * 0.5f;
        float groundCheckMargin = 0.12f;
        float rayLength = halfHeight + groundCheckMargin;

        Core.CurrentScene.Simulation.RayCast(pos, -Vector3.UnitY, rayLength, ref GroundHitHandler);

        _grounded = GroundHitHandler.Hit && Vector3.Dot(GroundHitHandler.Normal, Vector3.UnitY) > 0.7f;

        Vector3 vel = bodyRef.Velocity.Linear;
        Vector3 velXZ = new(vel.X, 0f, vel.Z);
        Vector3 desiredXZ = WorldMoveDirection * MoveSpeed;

        float accel = _grounded ? GroundAcceleration : AirAcceleration;
        float maxDeltaThisFrame = accel * dt;

        Vector3 deltaV = desiredXZ - velXZ;
        float deltaLen = deltaV.Length();
        if (deltaLen > 0f)
        {
            if (deltaLen > maxDeltaThisFrame)
                deltaV = Vector3.Normalize(deltaV) * maxDeltaThisFrame;

            vel.X += deltaV.X * MoveSpeed * 0.25f;
            vel.Z += deltaV.Z * MoveSpeed * 0.25f;
        }

        if (WorldMoveDirection == Vector3.Zero && _grounded)
        {
            vel.X = 0f;
            vel.Z = 0f;
        }

        float speedXZ = new Vector3(vel.X, 0f, vel.Z).Length();
        if (speedXZ > MaxSpeed)
        {
            Vector3 norm = Vector3.Normalize(new Vector3(vel.X, 0f, vel.Z));
            vel.X = norm.X * MaxSpeed;
            vel.Z = norm.Z * MaxSpeed;
        }


        bool wantJump = Core.IsKeyDown(Keys.Space);
        if (wantJump && _grounded)
        {
            vel.Y += JumpSpeed * 0.01f;
            _grounded = false;
        }

        bodyRef.Velocity.Angular = Vector3.Zero;

        bodyRef.Velocity.Linear = vel;
        bodyRef.Awake = true;

        Position = bodyRef.Pose.Position;
    }
}