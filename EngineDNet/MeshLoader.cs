using System.Numerics;

namespace EngineDNet;
public static class MeshLoader
{
    public static List<float> Load(string filePath)
    {
        var tempPositions = new List<Vector3>();
        var tempTexCoords = new List<Vector2>();
        var tempNormals = new List<Vector3>();

        var finalVertices = new List<float>();

        try
        {
            string[] lines = File.ReadAllLines(filePath);

            foreach (string line in lines)
            {
                if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#"))
                    continue;

                string[] parts = line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                string prefix = parts[0];

                switch (prefix)
                {
                    case "v":
                        tempPositions.Add(new Vector3(
                            float.Parse(parts[1], System.Globalization.CultureInfo.InvariantCulture),
                            float.Parse(parts[2], System.Globalization.CultureInfo.InvariantCulture),
                            float.Parse(parts[3], System.Globalization.CultureInfo.InvariantCulture)
                        ));
                        break;

                    case "vt":
                        tempTexCoords.Add(new Vector2(
                            float.Parse(parts[1], System.Globalization.CultureInfo.InvariantCulture),
                            float.Parse(parts[2], System.Globalization.CultureInfo.InvariantCulture)
                        ));
                        break;

                    case "vn":
                        tempNormals.Add(new Vector3(
                            float.Parse(parts[1], System.Globalization.CultureInfo.InvariantCulture),
                            float.Parse(parts[2], System.Globalization.CultureInfo.InvariantCulture),
                            float.Parse(parts[3], System.Globalization.CultureInfo.InvariantCulture)
                        ));
                        break;

                    case "f":
                        int vertexCount = parts.Length - 1;

                        if (vertexCount < 3)
                            continue;

                        string[] faceVertices = new string[vertexCount];
                        for (int i = 0; i < vertexCount; i++)
                        {
                            faceVertices[i] = parts[i + 1];
                        }

                        if (vertexCount == 3)
                        {
                            for (int i = 0; i < 3; i++)
                            {
                                ProcessFaceVertex(faceVertices[i], tempPositions, tempTexCoords, tempNormals, finalVertices);
                            }
                        }

                        else if (vertexCount == 4)
                        {
                            ProcessFaceVertex(faceVertices[0], tempPositions, tempTexCoords, tempNormals, finalVertices);
                            ProcessFaceVertex(faceVertices[1], tempPositions, tempTexCoords, tempNormals, finalVertices);
                            ProcessFaceVertex(faceVertices[2], tempPositions, tempTexCoords, tempNormals, finalVertices);

                            ProcessFaceVertex(faceVertices[0], tempPositions, tempTexCoords, tempNormals, finalVertices);
                            ProcessFaceVertex(faceVertices[2], tempPositions, tempTexCoords, tempNormals, finalVertices);
                            ProcessFaceVertex(faceVertices[3], tempPositions, tempTexCoords, tempNormals, finalVertices);
                        }

                        else
                        {
                            Utils.ColoredWriteLine($"[OBJ Loader] Warning: Polygon with {vertexCount} vertices is skipped.", ConsoleColor.Red);
                        }
                        break;
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error while loading OBJ-File: {ex.Message}");
        }

        return finalVertices;
    }

    private static void ProcessFaceVertex(string vertexData,
                                            List<Vector3> pos,
                                            List<Vector2> tex,
                                            List<Vector3> norm,
                                            List<float> final)
    {
        string[] indices = vertexData.Split('/');

        int posIndex = int.Parse(indices[0]) - 1;
        int texIndex = int.Parse(indices[1]) - 1;
        int normIndex = int.Parse(indices[2]) - 1;

        final.Add(pos[posIndex].X);
        final.Add(pos[posIndex].Y);
        final.Add(pos[posIndex].Z);

        final.Add(tex[texIndex].X);
        final.Add(tex[texIndex].Y);

        final.Add(norm[normIndex].X);
        final.Add(norm[normIndex].Y);
        final.Add(norm[normIndex].Z);
    }
}