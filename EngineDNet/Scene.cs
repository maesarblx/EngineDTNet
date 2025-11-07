using BepuPhysics;
using BepuPhysics.Collidables;
using BepuPhysics.CollisionDetection;
using BepuPhysics.Constraints;
using BepuPhysics.Trees;
using BepuUtilities;
using BepuUtilities.Memory;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace EngineDNet;

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

    public SimplePoseIntegratorCallbacks(Vector3 gravity, float linearDamping = .03f, float angularDamping = .03f) : this()
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
    public Skybox Skybox { get; private set; }
    public LightingSettings SceneLightingSettings = new LightingSettings();
    public ThreadDispatcher ThreadDispatcher = new(2);
    public BufferPool BufferPool = new();
    public SimplePoseIntegratorCallbacks PoseIntegrator = new(new Vector3(0, -10, 0));
    public SimpleNarrowPhaseCallbacks NarrowPhase = new(new SpringSettings(30, 1));
    public SolveDescription SolveDescription = new(8, 1);
    public Simulation Simulation; 
    public Scene()
    {
        Root = new("Root", Vector3.Zero, Vector3.Zero, Vector3.One);
        Simulation = Simulation.Create(BufferPool, NarrowPhase, PoseIntegrator, SolveDescription);
    }

    public void Init()
    {
        Skybox = new();
    }

    public void RenderUpdate(Camera camera)
    {
        Skybox.Object.Position = camera.Position;
    }

    public void Update(float deltaTime)
    {
        Simulation.Timestep(deltaTime, ThreadDispatcher);
        foreach (var obj in Root.Children)
        {
            if (obj.PhysicsEnabled && obj.PhysicsHandle != null)
            {
                var bodyRef = Simulation.Bodies.GetBodyReference(obj.PhysicsHandle.GetValueOrDefault());
                var pos = bodyRef.Pose.Position;
                var rot = bodyRef.Pose.Orientation;

                obj.Position = new Vector3(pos.X, pos.Y, pos.Z);
                obj.Rotation = new Vector3(rot.X, rot.Y, rot.Z);
            }
        }
    }
}