using BepuPhysics;
using BepuPhysics.Collidables;
using EngineDNet.Assets.Meshes;
using EngineDNet.Assets.Textures;
using EngineDNet.Utilities;
using System.Numerics;

namespace EngineDNet.Objects;
public class GameObject : IDisposable
{
    public Vector3 Position = Vector3.Zero;
    public Vector3 Rotation = Vector3.Zero;
    public Vector3 Size = Vector3.One;
    public string Name = "GameObject";
    public float Mass = 1;
    public float DragCoefficient;
    public float CrossSectionalArea;
    public float TexCoordsMult = 1;
    public Vector2 VecTexCoordsMult = Vector2.One;
    public Mesh3D? Mesh;
    public Texture2D? Texture;
    public bool CanCollide = true;
    public bool Destroyed { get; private set; } = false;
    private GameObject? _parent;
    public GameObject? Parent
    {
        get => _parent;
        set
        {
            if (_parent != null)
                _parent.Children.Remove(this);
            _parent = value;
            value?.Children.Add(this);
        }
    }
    public List<GameObject> Children = new();
    public BodyHandle? PhysicsHandle { get; set; }
    public StaticHandle? StaticPhysicsHandle { get; set; }
    public bool PhysicsEnabled { get; set; } = false;

    public GameObject(string name, Vector3 position, Vector3 rotation, Vector3 size, Mesh3D? mesh = null, Texture2D? texture = null, GameObject? parent = null, float? mass = null)
    {
        Name = name;
        Position = position;
        Rotation = rotation;
        Size = size;
        Mesh = mesh;
        Texture = texture;
        Parent = parent;
        Mass = mass != null ? (float)mass : Mass;
        Mass = Math.Clamp(Mass, 1f, float.MaxValue);
    }

    public void MassCalculate()
    {
        var OldMass = Mass;
        Mass = Mesh != null ? Mesh.CalculateMass() : Mass;
        Mass = Math.Clamp(Mass, 1f, float.MaxValue);
    }

    public void InitializePhysics()
    {
        if (Mesh == null)
        {
            Utils.ColoredWriteLine($"[GameObject] Cannot initialize physics for {Name} because it has no mesh!", ConsoleColor.Yellow);
            return;
        }
        var boxSize = Mesh.GetBoundingSize();
        var boxShape = new Box(boxSize.X, boxSize.Y, boxSize.Z);
        var boxInertia = boxShape.ComputeInertia(Mass);
        var shapeHandle = Core.CurrentScene.Simulation.Shapes.Add(boxShape);
        var bodyDescription = BodyDescription.CreateDynamic(Position, boxInertia, shapeHandle, 0.01f);

        if (PhysicsEnabled)
        {
            PhysicsHandle = Core.CurrentScene.Simulation.Bodies.Add(bodyDescription);
        }
        else
        {
            StaticPhysicsHandle = Core.CurrentScene.Simulation.Statics.Add(new StaticDescription(Position, shapeHandle));
        }

    }

    public Matrix4x4 GetModelMatrix()
    {
        return Matrix4x4.CreateScale(Size) * Matrix4x4.CreateRotationY(Rotation.Y) * Matrix4x4.CreateRotationX(Rotation.X) * Matrix4x4.CreateRotationZ(Rotation.Z) * Matrix4x4.CreateTranslation(Position);
    }

    private void DisposeUnmanagedResources(bool disposed)
    {
        Parent = null;
        Destroyed = true;
    }

    ~GameObject()
    {
        DisposeUnmanagedResources(false);
    }

    public void Dispose()
    {
        DisposeUnmanagedResources(true);
        GC.SuppressFinalize(this);
    }
}