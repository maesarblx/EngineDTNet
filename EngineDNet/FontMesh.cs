using MadWorldNL.EarCut.Logic;
using OpenTK.Mathematics;
using SixLabors.Fonts;
using SixLabors.Fonts.Unicode;
using SixLabors.ImageSharp.Drawing;

namespace EngineDNet;

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
        for (var k = 0; k < r.Count; k++)
        {
            var p1 = r[k];
            var p2 = r[(k + 1) % r.Count];
            a += p1.X * p2.Y - p2.X * p1.Y;
        }

        return a * 0.5f;
    }
    private static bool PointInPolygon(Vector2 pt, List<Vector2> poly)
    {
        var inside = false;
        for (int a = 0, b = poly.Count - 1; a < poly.Count; b = a++)
        {
            var pa = poly[a];
            var pb = poly[b];
            if (((pa.Y > pt.Y) != (pb.Y > pt.Y)) &&
                (pt.X < (pb.X - pa.X) * (pt.Y - pa.Y) / (pb.Y - pa.Y + 0.0f) + pa.X))
            {
                inside = !inside;
            }
        }
        return inside;
    }
    public FontMesh(string path)
    {
        var family = _collection.Add(path);

        const float pointSize = 1f;
        const float dpi = 256f;

        var font = family.CreateFont(pointSize, FontStyle.Regular);
        var options = new TextOptions(font) { Dpi = dpi };
        var glyphPaths = TextBuilder.GenerateGlyphs(TextGlyphs, options);

        var i = 0;
        foreach (var glyphPath in glyphPaths)
        {
            var rings = new List<List<Vector2>>();
            foreach (var simple in glyphPath.Flatten())
            {
                var ring = new List<Vector2>();
                foreach (var p in simple.Points.Span)
                {
                    ring.Add(new Vector2(p.X, p.Y));
                }
                if (ring.Count > 1 && ring[0] == ring[^1])
                    ring.RemoveAt(ring.Count - 1);
                if (ring.Count > 0) rings.Add(ring);
            }

            if (rings.Count == 0) { i++; continue; }

            var parent = new int[rings.Count];
            for (var idx = 0; idx < parent.Length; idx++) parent[idx] = -1;

            for (var a = 0; a < rings.Count; a++)
            {
                var testPt = rings[a][0];
                var bestParent = -1;
                var bestParentArea = float.MaxValue;
                for (var b = 0; b < rings.Count; b++)
                {
                    if (a == b) continue;
                    if (!PointInPolygon(testPt, rings[b])) continue;
                    var areaAbs = MathF.Abs(SignedArea(rings[b]));
                    if (!(areaAbs < bestParentArea)) continue;
                    bestParentArea = areaAbs;
                    bestParent = b;
                }
                parent[a] = bestParent;
            }

            var outers = new List<int>();
            var holesOf = new Dictionary<int, List<int>>();
            for (var r = 0; r < rings.Count; r++)
            {
                if (parent[r] != -1) continue;
                outers.Add(r);
                holesOf[r] = [];
            }
            for (var r = 0; r < rings.Count; r++)
            {
                if (parent[r] == -1) continue;
                var p = parent[r];
                while (p != -1 && parent[p] != -1) p = parent[p];
                if (p != -1 && holesOf.TryGetValue(p, out var value))
                    value.Add(r);
                else
                {
                    outers.Add(r);
                    holesOf[r] = new List<int>();
                }
            }

            foreach (var o in outers)
            {
                if (SignedArea(rings[o]) < 0) rings[o].Reverse();
                foreach (var h in holesOf[o].Where(h => SignedArea(rings[h]) > 0))
                {
                    rings[h].Reverse();
                }
            }

            var globalVertices = new List<float>();
            var globalIndices = new List<int>();
            var globalVertexOffset = 0;

            foreach (var outerIdx in outers)
            {
                var groupVertices = new List<float>();
                var groupHoleIndices = new List<int>();
                var localCount = 0;

                foreach (var p in rings[outerIdx])
                {
                    groupVertices.Add(p.X);
                    groupVertices.Add(p.Y);
                    localCount++;
                }

                foreach (var holeIdx in holesOf[outerIdx])
                {
                    groupHoleIndices.Add(localCount);
                    foreach (var p in rings[holeIdx])
                    {
                        groupVertices.Add(p.X);
                        groupVertices.Add(p.Y);
                        localCount++;
                    }
                }

                if (groupVertices.Count == 0) continue;

                var groupIndices = groupHoleIndices.Count > 0
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

            var ptsCount = globalVertices.Count / 2;
            var pts = new Vector2[ptsCount];
            for (var vi = 0; vi < ptsCount; vi++)
                pts[vi] = new Vector2(globalVertices[vi * 2], globalVertices[vi * 2 + 1]);

            float minX = float.MaxValue, minY = float.MaxValue, maxX = float.MinValue, maxY = float.MinValue;
            foreach (var p in pts)
            {
                if (p.X < minX) minX = p.X;
                if (p.Y < minY) minY = p.Y;
                if (p.X > maxX) maxX = p.X;
                if (p.Y > maxY) maxY = p.Y;
            }

            var cp = new CodePoint(TextGlyphs[i]);
            if (!font.TryGetGlyphs(cp, out var glyphs) || glyphs.Count == 0) { i++; continue; }
            var glyph = glyphs[0];
            var gm = glyph.GlyphMetrics;
            float unitsPerEm = gm.UnitsPerEm;
            const float pixelsPerEm = pointSize * dpi / 72f;
            var scale = pixelsPerEm / unitsPerEm;
            var widthPx = gm.Width * scale;
            var heightPx = gm.Height * scale;
            var leftPx = gm.LeftSideBearing * scale;
            var topPx = gm.TopSideBearing * scale;
            var advancePx = gm.AdvanceWidth * scale;

            var meshVertices = new Mesh2D.Vertex[pts.Length];
            for (var vi = 0; vi < pts.Length; vi++)
            {
                var x = pts[vi].X - minX;
                var y = pts[vi].Y + minY - topPx;
                meshVertices[vi] = new Mesh2D.Vertex(new Vector2(x, y), new Vector2(x, y));
            }

            var glyphMesh = new Mesh2D(meshVertices, globalIndices.ToArray());
            Glyphs[TextGlyphs[i]] = new GlyphMesh(glyphMesh, TextGlyphs[i], widthPx, heightPx, leftPx, topPx, advancePx);
            i++;
        }
    }

}