using BepuPhysics;
using BepuPhysics.Collidables;
using BepuPhysics.CollisionDetection;
using BepuPhysics.Constraints;
using BepuUtilities;
using BepuUtilities.Memory;
using System.Numerics;
//using OpenTK.Mathematics;

namespace EngineDNet;

public struct SimplePoseIntegratorCallbacks : IPoseIntegratorCallbacks
{
    public System.Numerics.Vector3 Gravity;
    public AngularIntegrationMode AngularIntegrationMode => AngularIntegrationMode.Nonconserving;

    public bool AllowSubstepsForUnconstrainedBodies => throw new NotImplementedException();

    public bool IntegrateVelocityForKinematics => throw new NotImplementedException();

    public void Initialize(Simulation simulation) { }

    public void IntegrateVelocity(Vector3 position, ref BodyVelocity velocity, float dt)
    {
        velocity.Linear += Gravity * dt;

        const float linearDamping = 0.999f;
        velocity.Linear *= linearDamping;
    }

    public void IntegrateVelocity(System.Numerics.Vector<int> bodyIndices, Vector3Wide position, QuaternionWide orientation, BodyInertiaWide localInertia, System.Numerics.Vector<int> integrationMask, int workerIndex, System.Numerics.Vector<float> dt, ref BodyVelocityWide velocity)
    {
        throw new NotImplementedException();
    }

    public void PrepareForIntegration(float dt)
    {
        throw new NotImplementedException();
    }
}

public struct SimpleNarrowPhaseCallbacks : INarrowPhaseCallbacks
{
    public void Initialize(Simulation simulation) { }

    public bool AllowContactGeneration(int workerIndex, CollidableReference a, CollidableReference b)
    {
        return true;
    }

    public bool ConfigureContactManifold<TManifold>(int workerIndex, CollidableReference a, CollidableReference b,
        ref TManifold manifold, out PairMaterialProperties pairMaterial) where TManifold : unmanaged
    {
        pairMaterial.FrictionCoefficient = 1.0f;
        pairMaterial.MaximumRecoveryVelocity = 2f;
        pairMaterial.SpringSettings = new SpringSettings(30, 1);
        return true;
    }

    public void Dispose() { }
    public void AllowContactGeneration(int workerIndex, CollidableReference a, CollidableReference b, out bool allow) { allow = true; }

    public bool AllowContactGeneration(int workerIndex, CollidableReference a, CollidableReference b, ref float speculativeMargin)
    {
        throw new NotImplementedException();
    }

    public bool ConfigureContactManifold<TManifold>(int workerIndex, CollidablePair pair, ref TManifold manifold, out PairMaterialProperties pairMaterial) where TManifold : unmanaged, IContactManifold<TManifold>
    {
        throw new NotImplementedException();
    }

    public bool AllowContactGeneration(int workerIndex, CollidablePair pair, int childIndexA, int childIndexB)
    {
        throw new NotImplementedException();
    }

    public bool ConfigureContactManifold(int workerIndex, CollidablePair pair, int childIndexA, int childIndexB, ref ConvexContactManifold manifold)
    {
        throw new NotImplementedException();
    }
}

public class Scene
{
    public GameObject Root { get; private set; }
    public LightingSettings SceneLightingSettings = new LightingSettings();
    public ThreadDispatcher ThreadDispatcher = new(2);
    public BufferPool BufferPool = new();
    public SimplePoseIntegratorCallbacks PoseIntegrator = new() { Gravity = System.Numerics.Vector3.UnitY * -9.81f };
    public SimpleNarrowPhaseCallbacks NarrowPhase = new();
    public SolveDescription SolveDesc = new(8, 1);
    public Simulation CurrentSimulation;
    public Scene()
    {
        Root = new GameObject("Root", Vector3.Zero, Vector3.Zero, Vector3.One);
        //CurrentSimulation = Simulation.Create(BufferPool, ThreadDispatcher, PoseIntegrator, SolveDesc);


    }
    public void Update(float deltaTime)
    {
        CurrentSimulation.Timestep(deltaTime, ThreadDispatcher);
        CurrentSimulation.Solve(deltaTime, ThreadDispatcher);
    }
}
