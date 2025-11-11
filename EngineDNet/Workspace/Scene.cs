using BepuPhysics;
using BepuPhysics.Collidables;
using BepuPhysics.CollisionDetection;
using BepuPhysics.Constraints;
using BepuPhysics.Trees;
using BepuUtilities;
using BepuUtilities.Memory;
using EngineDNet.Camera;
using EngineDNet.Objects;
using EngineDNet.Utilities;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace EngineDNet.Workspace;

public struct SimplePoseIntegratorCallbacks : IPoseIntegratorCallbacks
{
    public Vector3 Gravity;
    public float LinearDamping;
    public float AngularDamping;

    public readonly AngularIntegrationMode AngularIntegrationMode => AngularIntegrationMode.Nonconserving;

    public readonly bool AllowSubstepsForUnconstrainedBodies => false;

    public readonly bool IntegrateVelocityForKinematics => false;

    public void Initialize(Simulation simulation)
    {
    }

    public SimplePoseIntegratorCallbacks(Vector3 gravity, float linearDamping = 0.03f, float angularDamping = 0.03f) : this()
    {
        Gravity = gravity;
        LinearDamping = linearDamping;
        AngularDamping = angularDamping;
    }

    Vector3Wide gravityWideDt;
    Vector<float> linearDampingDt;
    Vector<float> angularDampingDt;

    public void PrepareForIntegration(float dt)
    {
        linearDampingDt = new Vector<float>(MathF.Pow(MathHelper.Clamp(1 - LinearDamping, 0, 1), dt));
        angularDampingDt = new Vector<float>(MathF.Pow(MathHelper.Clamp(1 - AngularDamping, 0, 1), dt));
        gravityWideDt = Vector3Wide.Broadcast(Gravity * dt);
    }

    public void IntegrateVelocity(Vector<int> bodyIndices, Vector3Wide position, QuaternionWide orientation, BodyInertiaWide localInertia, Vector<int> integrationMask, int workerIndex, Vector<float> dt, ref BodyVelocityWide velocity)
    {
        velocity.Linear = (velocity.Linear + gravityWideDt) * linearDampingDt;
        velocity.Angular = velocity.Angular * angularDampingDt;
    }
}

public struct SimpleNarrowPhaseCallbacks : INarrowPhaseCallbacks
{
    public SpringSettings ContactSpringiness;
    public float MaximumRecoveryVelocity;
    public float FrictionCoefficient;

    public SimpleNarrowPhaseCallbacks(SpringSettings contactSpringiness, float maximumRecoveryVelocity = 2f, float frictionCoefficient = 1f)
    {
        ContactSpringiness = contactSpringiness;
        MaximumRecoveryVelocity = maximumRecoveryVelocity;
        FrictionCoefficient = frictionCoefficient;
    }

    public void Initialize(Simulation simulation)
    {
        if (ContactSpringiness.AngularFrequency == 0 && ContactSpringiness.TwiceDampingRatio == 0)
        {
            ContactSpringiness = new(30, 1);
            MaximumRecoveryVelocity = 2f;
            FrictionCoefficient = 1f;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool AllowContactGeneration(int workerIndex, CollidableReference a, CollidableReference b, ref float speculativeMargin)
    {
        return a.Mobility == CollidableMobility.Dynamic || b.Mobility == CollidableMobility.Dynamic;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool AllowContactGeneration(int workerIndex, CollidablePair pair, int childIndexA, int childIndexB)
    {
        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool ConfigureContactManifold<TManifold>(int workerIndex, CollidablePair pair, ref TManifold manifold, out PairMaterialProperties pairMaterial) where TManifold : unmanaged, IContactManifold<TManifold>
    {
        pairMaterial.FrictionCoefficient = FrictionCoefficient;
        pairMaterial.MaximumRecoveryVelocity = MaximumRecoveryVelocity;
        pairMaterial.SpringSettings = ContactSpringiness;
        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool ConfigureContactManifold(int workerIndex, CollidablePair pair, int childIndexA, int childIndexB, ref ConvexContactManifold manifold)
    {
        return true;
    }

    public void Dispose()
    {
    }
}

public struct SimpleRayHitHandler : IRayHitHandler
{
    public bool Hit;
    public float T;
    public Vector3 Normal;
    public CollidableReference Collidable;
    public int ChildIndex;
    public List<BodyHandle> SIgnored;
    public List<BodyHandle> DIgnored;

    public SimpleRayHitHandler()
    {
        DIgnored = new();
        SIgnored = new();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Reset()
    {
        Hit = false;
        T = float.MaxValue;
        Normal = Vector3.Zero;
        Collidable = default;
        ChildIndex = -1;
    }


    public bool AllowTest(CollidableReference collidable)
    {
        if (collidable.Mobility == CollidableMobility.Dynamic)
            return !DIgnored.Contains(collidable.BodyHandle);
        if (collidable.Mobility == CollidableMobility.Static)
            return !SIgnored.Contains(collidable.BodyHandle);
        return true;
    }

    public bool AllowTest(CollidableReference collidable, int childIndex) => AllowTest(collidable);


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void OnRayHit(in RayData ray, ref float maximumT, float t, in Vector3 normal, CollidableReference collidable, int childIndex)
    {
        if (t <= T)
        {
            Hit = true;
            T = t;
            Normal = normal;
            Collidable = collidable;
            ChildIndex = childIndex;
        }
    }
}

public class Scene
{
    public GameObject Root { get; private set; }
    public Skybox? Skybox { get; private set; }
    public LightingSettings SceneLightingSettings = new();
    public ThreadDispatcher ThreadDispatcher = new(2);
    public BufferPool BufferPool = new();
    public SimplePoseIntegratorCallbacks PoseIntegrator = new(new Vector3(0, -10, 0));
    public SimpleNarrowPhaseCallbacks NarrowPhase = new(new SpringSettings(30, 1));
    public SolveDescription SolveDescription = new(8, 1);
    public Simulation Simulation;

    public Vector3 Wind = Vector3.Zero;

    const float airDensity = 1.225f;
    const float defaultCd = 1.0f;
    const float defaultArea = 0.5f;
    const float gustAmplitude = 3f;
    const float gustFrequency = 0.5f;

    public Scene()
    {
        Root = new("Root", Vector3.Zero, Vector3.Zero, Vector3.One);
        Simulation = Simulation.Create(BufferPool, NarrowPhase, PoseIntegrator, SolveDescription);
    }

    public void Init()
    {
        Skybox = new();
    }

    public void RenderUpdate(Camera3D camera)
    {
        if (Skybox == null)
            return;
        Skybox.Object.Rotation = Vector3.UnitY * Core.ElapsedTime * 0.01f + Vector3.UnitX * 180 * Utils.Deg2Rad;
        Skybox.Object.Position = camera.Position;
    }

    public void Update(float deltaTime)
    {
        ApplyWindForces(deltaTime);
        Simulation.Timestep(deltaTime, ThreadDispatcher);

        foreach (var obj in Root.Children)
        {
            if (obj.PhysicsEnabled && obj.PhysicsHandle != null)
            {
                var bodyHandle = obj.PhysicsHandle.GetValueOrDefault();
                var bodyRef = Simulation.Bodies.GetBodyReference(bodyHandle);

                var pos = bodyRef.Pose.Position;
                var rot = bodyRef.Pose.Orientation;

                obj.Position = new Vector3(pos.X, pos.Y, pos.Z);
                obj.Rotation = new Vector3(rot.X, rot.Y, rot.Z);
            }
        }
    }

    private void ApplyWindForces(float deltaTime)
    {
        const float airDensity = 1.225f;
        const float defaultCd = 1.0f;
        const float defaultArea = 0.5f;
        const float gustAmplitude = 2.0f;
        const float gustFrequency = 0.4f;
        const float maxAcceleration = 50f;
        const float minSpeedEpsilon = 1e-4f;

        float time = Core.ElapsedTime;
        Vector3 baseWindDir = Wind == Vector3.Zero ? Vector3.UnitX : Vector3.Normalize(Wind);
        Vector3 windWithGust = Wind + baseWindDir * (gustAmplitude * MathF.Sin(time * MathF.PI * 2f * gustFrequency));

        foreach (var obj in Root.Children)
        {
            if (!(obj.PhysicsEnabled && obj.PhysicsHandle != null))
                continue;

            var handle = obj.PhysicsHandle.GetValueOrDefault();
            var bodyRef = Simulation.Bodies.GetBodyReference(handle);

            var invMass = bodyRef.LocalInertia.InverseMass;
            if (invMass <= 0f)
                continue;

            Vector3 vRel = windWithGust - bodyRef.Velocity.Linear;
            float speed = vRel.Length();
            if (speed <= minSpeedEpsilon)
                continue;

            float Cd = (obj is GameObject a && a.DragCoefficient > 0) ? a.DragCoefficient : defaultCd;
            float area = (obj is GameObject b && b.CrossSectionalArea > 0) ? b.CrossSectionalArea : defaultArea;

            Vector3 dragForce = -0.5f * airDensity * Cd * area * speed * vRel;

            float mass = 1f / invMass;
            float maxForce = mass * maxAcceleration;
            float forceMag = dragForce.Length();
            if (forceMag > maxForce)
                dragForce = Vector3.Normalize(dragForce) * maxForce;

            Vector3 impulse = dragForce * deltaTime;

            Vector3 dv = impulse * invMass;
            const float maxDeltaVPerFrame = 30f;
            float dvMag = dv.Length();
            if (dvMag > maxDeltaVPerFrame)
                dv = Vector3.Normalize(dv) * maxDeltaVPerFrame;

            bodyRef.Velocity.Linear += dv;

            bodyRef.Awake = true;
        }
    }
}