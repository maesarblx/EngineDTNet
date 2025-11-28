using MadWorldNL.EarCut.Logic;
using System.Numerics;
using SixLabors.Fonts;
using SixLabors.Fonts.Unicode;
using SixLabors.ImageSharp.Drawing;

namespace EngineDNet.Assets.Meshes;

public class FontMesh
{
    private static readonly string TextGlyphs = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz!@#$%^&*()-_=+[]{}|;:'\"\\,.<>?/`~0123456789АБВГДЕЁЖЗИЙКЛМНОПРСТУФХЦЧШЩЪЫЬЭЮЯабвгдеёжзийклмнопрстуфхцчшщъыьэюя";
    public Dictionary<char, GlyphMesh> Glyphs { get; private set; } = new();
    private readonly FontCollection _collection = new();

    public struct GlyphMesh(Mesh2D mesh, char character, float width, float height, float bearingX, float bearingY, float advance)
    {
        public readonly Mesh2D? Mesh = mesh;
        public readonly char Character = character;
        public readonly float Width = width;
        public readonly float Height = height;
        public readonly float BearingX = bearingX;
        public readonly float BearingY = bearingY;
        public readonly float Advance = advance;
    };

    private static float SignedArea(IList<Vector2> r)
    {
        float a = 0;
        for (int k = 0; k < r.Count; k++)
        {
            Vector2 p1 = r[k];
            Vector2 p2 = r[(k + 1) % r.Count];
            a += p1.X * p2.Y - p2.X * p1.Y;
        }

        return a * 0.5f;
    }
    private static bool PointInPolygon(Vector2 pt, List<Vector2> poly)
    {
        bool inside = false;
        for (int a = 0, b = poly.Count - 1; a < poly.Count; b = a++)
        {
            Vector2 pa = poly[a];
            Vector2 pb = poly[b];
            if (pa.Y > pt.Y != pb.Y > pt.Y &&
                pt.X < (pb.X - pa.X) * (pt.Y - pa.Y) / (pb.Y - pa.Y + 0.0f) + pa.X)
            {
                inside = !inside;
            }
        }
        return inside;
    }
    public FontMesh(string path)
    {
        FontFamily family = _collection.Add(path);

        const float pointSize = 1f;
        const float dpi = 256f;

        Font font = family.CreateFont(pointSize, FontStyle.Regular);
        TextOptions options = new(font) { Dpi = dpi };
        IPathCollection glyphPaths = TextBuilder.GenerateGlyphs(TextGlyphs, options);

        int i = 0;
        foreach (IPath glyphPath in glyphPaths)
        {
            List<List<Vector2>> rings = new();
            foreach (var simple in glyphPath.Flatten())
            {
                List<Vector2> ring = new();
                foreach (Vector2 p in simple.Points.Span)
                {
                    ring.Add(p);
                }
                if (ring.Count > 1 && ring[0] == ring[^1])
                    ring.RemoveAt(ring.Count - 1);
                if (ring.Count > 0) rings.Add(ring);
            }

            if (rings.Count == 0) { i++; continue; }

            int[] parent = new int[rings.Count];
            for (int idx = 0; idx < parent.Length; idx++) parent[idx] = -1;

            for (int a = 0; a < rings.Count; a++)
            {
                Vector2 testPt = rings[a][0];
                int bestParent = -1;
                float bestParentArea = float.MaxValue;
                for (int b = 0; b < rings.Count; b++)
                {
                    if (a == b) continue;
                    if (!PointInPolygon(testPt, rings[b])) continue;
                    float areaAbs = MathF.Abs(SignedArea(rings[b]));
                    if (!(areaAbs < bestParentArea)) continue;
                    bestParentArea = areaAbs;
                    bestParent = b;
                }
                parent[a] = bestParent;
            }

            List<int> outers = new();
            Dictionary<int, List<int>> holesOf = new();
            for (int r = 0; r < rings.Count; r++)
            {
                if (parent[r] != -1) continue;
                outers.Add(r);
                holesOf[r] = [];
            }
            for (int r = 0; r < rings.Count; r++)
            {
                if (parent[r] == -1) continue;
                int p = parent[r];
                while (p != -1 && parent[p] != -1) p = parent[p];
                if (p != -1 && holesOf.TryGetValue(p, out List<int>? value))
                    value.Add(r);
                else
                {
                    outers.Add(r);
                    holesOf[r] = new();
                }
            }

            foreach (int o in outers)
            {
                if (SignedArea(rings[o]) < 0) rings[o].Reverse();
                foreach (int h in holesOf[o].Where(h => SignedArea(rings[h]) > 0))
                {
                    rings[h].Reverse();
                }
            }

            List<float> globalVertices = new();
            List<int> globalIndices = new();
            int globalVertexOffset = 0;

            foreach (int outerIdx in outers)
            {
                List<float> groupVertices = new();
                List<int> groupHoleIndices = new();
                int localCount = 0;

                foreach (Vector2 p in rings[outerIdx])
                {
                    groupVertices.Add(p.X);
                    groupVertices.Add(p.Y);
                    localCount++;
                }

                foreach (int holeIdx in holesOf[outerIdx])
                {
                    groupHoleIndices.Add(localCount);
                    foreach (Vector2 p in rings[holeIdx])
                    {
                        groupVertices.Add(p.X);
                        groupVertices.Add(p.Y);
                        localCount++;
                    }
                }

                if (groupVertices.Count == 0) continue;

                int[]? groupIndices = groupHoleIndices.Count > 0
                    ? EarCut.Tessellate(groupVertices.ToArray(), groupHoleIndices.ToArray()).ToArray()
                    : EarCut.Tessellate(groupVertices.ToArray()).ToArray();

                globalVertices.AddRange(groupVertices);

                globalIndices.AddRange(groupIndices.Select(idxLocal => idxLocal + globalVertexOffset));
                globalVertexOffset += groupVertices.Count / 2;
            }

            if (globalVertices.Count == 0 || globalIndices.Count == 0)
            {
                i++;
                continue;
            }

            int ptsCount = globalVertices.Count / 2;
            Vector2[] pts = new Vector2[ptsCount];
            for (int vi = 0; vi < ptsCount; vi++)
                pts[vi] = new Vector2(globalVertices[vi * 2], globalVertices[vi * 2 + 1]);

            float minX = float.MaxValue, minY = float.MaxValue, maxX = float.MinValue, maxY = float.MinValue;
            foreach (Vector2 p in pts)
            {
                if (p.X < minX) minX = p.X;
                if (p.Y < minY) minY = p.Y;
                if (p.X > maxX) maxX = p.X;
                if (p.Y > maxY) maxY = p.Y;
            }

            CodePoint cp = new(TextGlyphs[i]);
            if (!font.TryGetGlyphs(cp, out IReadOnlyList<Glyph>? glyphs) || glyphs.Count == 0) { i++; continue; }
            Glyph glyph = glyphs[0];
            GlyphMetrics gm = glyph.GlyphMetrics;
            float unitsPerEm = gm.UnitsPerEm;
            const float pixelsPerEm = pointSize * dpi / 72f;
            float scale = pixelsPerEm / unitsPerEm;
            float widthPx = gm.Width * scale;
            float heightPx = gm.Height * scale;
            float leftPx = gm.LeftSideBearing * scale;
            float topPx = gm.TopSideBearing * scale;
            float advancePx = gm.AdvanceWidth * scale;

            Mesh2D.Vertex[] meshVertices = new Mesh2D.Vertex[pts.Length];
            for (var vi = 0; vi < pts.Length; vi++)
            {
                var x = pts[vi].X - minX;
                var y = pts[vi].Y + minY - topPx;
                meshVertices[vi] = new Mesh2D.Vertex(new Vector2(x, y), new Vector2(x, y));
            }

            Mesh2D glyphMesh = new(meshVertices, globalIndices.ToArray());
            Glyphs[TextGlyphs[i]] = new GlyphMesh(glyphMesh, TextGlyphs[i], widthPx, heightPx, leftPx, topPx, advancePx);
            i++;
        }
    }

}