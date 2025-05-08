using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Elements.Assets;
using Elements.Core;
using FrooxEngine;

namespace Obsidian;

public class MetaballShape : MeshXShape
{
    public List<MetaballPoint> Points;

    public MetaballShape(MeshX mesh, List<MetaballPoint> points) : base(mesh)
    {
        Points = points;
        mesh.Clear();
        Generate(mesh);
    }

    private void Generate(MeshX mesh)
    {
    }

    public override void Update()
    {
        Generate(Mesh);
        Mesh.RecalculateNormals(AllTriangles);
        Mesh.RecalculateTangents(AllTriangles);
    }
}
