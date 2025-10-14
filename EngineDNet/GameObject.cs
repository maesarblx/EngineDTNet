using BepuPhysics;
using BepuPhysics.Collidables;
using System.Numerics;
//using OpenTK.Mathematics;

namespace EngineDNet
{
    public class GameObject
    {
        public Vector3 Position = Vector3.Zero;
        public Vector3 Rotation = Vector3.Zero;
        public Vector3 Size = Vector3.Zero;
        public string Name = "GameObject";
        public Mesh3D? Mesh;
        public Texture2D? Texture;
        private GameObject? _parent;
        public GameObject? Parent
        {
            get => _parent;
            set
            {
                _parent = value;
                value?.Children.Add(this);
            }
        }
        public List<GameObject> Children = new List<GameObject>();
        public BodyHandle? PhysicsHandle { get; set; }
        public StaticHandle? StaticPhysicsHandle { get; set; }
        public bool PhysicsEnabled { get; set; } = false;

        public GameObject(string name, Vector3 position, Vector3 rotation, Vector3 size, Mesh3D? mesh = null, Texture2D? texture = null, GameObject? parent = null)
        {
            Name = name;
            Position = position;
            Rotation = rotation;
            Size = size;
            Mesh = mesh;
            Texture = texture;
            Parent = parent;
        }

        public void InitializePhysics()
        {
            if (Mesh == null)
            {
                Utils.ColoredWriteLine($"[GameObject] Cannot initialize physics for {Name} because it has no mesh!", ConsoleColor.Yellow);
                return;
            }
            var boxSize = Mesh.GetBoundingSize();
            var boxShape = new Box(boxSize.X, boxSize.Y, boxSize.Z); // размеры
            var boxInertia = boxShape.ComputeInertia(1f); // масса = 1
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

        public void AddChild(GameObject child)
        {
            child.Parent = this;
            Children.Add(child);
        }

        public Matrix4x4 GetModelMatrix()
        {
            return Matrix4x4.CreateScale(Size) * Matrix4x4.CreateRotationY(Rotation.Y) * Matrix4x4.CreateRotationX(Rotation.X) * Matrix4x4.CreateRotationZ(Rotation.Z) * Matrix4x4.CreateTranslation(Position);
        }
    }
}
