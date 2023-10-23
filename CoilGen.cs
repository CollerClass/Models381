//============================================================================
// SpringModel
//============================================================================
using Godot;
using System;
using System.Collections.Generic;

public partial class CoilGen : MeshInstance3D
{
	float R;       // major radius of coil
	float r;       // minor radius of coil
	float L;       // natural length of coil
	float nt;      // number of turns in the coil

	int nx;        // number of steps in the axial direction
	int nd;        // number of steps in resolving the wire

	bool initialized;

	//------------------------------------------------------------------------
	// _Ready: Called when the node enters the scene tree for the first time.
	//------------------------------------------------------------------------
	public override void _Ready()
	{
		GD.Print("CoilGen _Ready");
		initialized = false;
	}

	//------------------------------------------------------------------------
	// Gen: Generate the coil
	//------------------------------------------------------------------------
	public void Gen(float R, float r, float L, float nt,
		int nx = 31, int nd = 10)
	{
		if(initialized){
			GD.PrintErr("CoilGen:Gen: Coil already initialized.");
			return;
		}

		if(R < 0.01f){
			GD.PrintErr("CoilGen:Gen: Major radius too small");
			return;
		}

		if(r < 0.005f || r > 0.5f*R)
		{
			GD.PrintErr("CoilGen:Gen: Minor radius in within range.");
			return;
		}

		if(L < 0.01f)
		{
			GD.PrintErr("CoilGen:Gen: Length too small.");
			return;
		}

		if(nt < 0.5f){
			GD.PrintErr("CoilGen:Gen: Must have at least a half turn.");
			return;
		}

		if(nx < 6){
			GD.PrintErr("CoilGen:Gen: Must have at least 6 axial steps.");
			return;
		}

		if(nd < 3){
			GD.PrintErr("CoilGen:Gen: Must have at least 3 steps around wire");
			return;
		}

		GD.Print("Generating COIL");
		var surfaceArray = new Godot.Collections.Array();
		surfaceArray.Resize((int)Mesh.ArrayType.Max);

		// Create lists to hold gometry info
		var verts = new List<Vector3>();
		var normals = new List<Vector3>();
		var uvs = new List<Vector2>();

		float ds = 1.0f/(1.0f*(nx - 1));
		GD.Print(ds + "  ****");
		Vector3 axVec = new Vector3(1.0f, 0.0f, 0.0f);
		Vector3 velRaw0 = new Vector3(L, 0.0f, 0.0f);
		Vector3 velRaw1 = new Vector3(L, 0.0f, 0.0f);
		Vector3 ptc0 = new Vector3();
		Vector3 ptc1 = new Vector3();
		Vector3 dumVec;

		int i,j;
		float[] phi = new float[nd + 1];
		float dPhi = Mathf.Tau/nd;
		for(j=0; j<nd; ++j){
			phi[j] = -Mathf.Pi + dPhi*j;
		}
		phi[nd] = Mathf.Pi;

		for(i=0; i<nx; ++i){
			float s0 = i*ds;
			ptc0.X = -0.5f*L + L*s0;
			ptc0.Y = -R*Mathf.Cos(2.0f*nt*Mathf.Pi*s0);
			ptc0.Z = -R*Mathf.Sin(2.0f*nt*Mathf.Pi*s0);
			velRaw0.Y = 2.0f*nt*Mathf.Pi*Mathf.Sin(2.0f*nt*Mathf.Pi*s0);
			velRaw0.Z = -2.0f*nt*Mathf.Pi*Mathf.Cos(2.0f*nt*Mathf.Pi*s0);
			Vector3 velNorm0 = velRaw0.Normalized();
			dumVec = velNorm0.Cross(axVec);
			Vector3 e1_0 = dumVec.Normalized();
			Vector3 e2_0 = velNorm0.Cross(e1_0);
			// GD.Print(e1_0);
			// GD.Print(e2_0);
			// GD.Print(e2_0.Dot(e2_0));

			float s1 = s0 + ds;
			ptc1.X = -0.5f*L + L*s1;
			ptc1.Y = -R*Mathf.Cos(2.0f*nt*Mathf.Pi*s1);
			ptc1.Z = -R*Mathf.Sin(2.0f*nt*Mathf.Pi*s1);
			velRaw1.Y = 2.0f*nt*Mathf.Pi*Mathf.Sin(2.0f*nt*Mathf.Pi*s1);
			velRaw1.Z = -2.0f*nt*Mathf.Pi*Mathf.Cos(2.0f*nt*Mathf.Pi*s1);
			Vector3 velNorm1 = velRaw1.Normalized();
			dumVec = velNorm1.Cross(axVec);
			Vector3 e1_1 = dumVec.Normalized();
			Vector3 e2_1 = velNorm1.Cross(e1_1);
			// GD.Print(e1_1);
			// GD.Print(e2_1);
			// GD.Print(e2_1.Dot(e2_1));

			for(j=0; j<nd; ++j){
				Vector3 nrml0A = Mathf.Cos(phi[j])*e1_0 + Mathf.Sin(phi[j])*e2_0;
				Vector3 nrml0B = Mathf.Cos(phi[j+1])*e1_0 + Mathf.Sin(phi[j+1])*e2_0;
				Vector3 nrml1A = Mathf.Cos(phi[j])*e1_1 + Mathf.Sin(phi[j])*e2_1;
				Vector3 nrml1B = Mathf.Cos(phi[j+1])*e1_1 + Mathf.Sin(phi[j+1])*e2_1;

				Vector3 vtx0A = ptc0 + r*nrml0A;
				Vector3 vtx0B = ptc0 + r*nrml0B;
				Vector3 vtx1A = ptc1 + r*nrml1A;
				Vector3 vtx1B = ptc1 + r*nrml1B;

				verts.Add(vtx0A);  verts.Add(vtx1B);  verts.Add(vtx0B);
				normals.Add(nrml0A);   normals.Add(nrml1B);  normals.Add(nrml0B);

				verts.Add(vtx0A);  verts.Add(vtx1A);  verts.Add(vtx1B);
				normals.Add(nrml0A);   normals.Add(nrml1A);  normals.Add(nrml1B); 
				// GD.Print(vtx0A);
				// GD.Print(vtx0B);
				// GD.Print(vtx1A);
				// GD.Print(vtx1B);
				// GD.Print(ptc0);
				// GD.Print(ptc1);
			}
		}

		// Convert lists to arrays and asign to surface array
		surfaceArray[(int)Mesh.ArrayType.Vertex] = verts.ToArray();
		surfaceArray[(int)Mesh.ArrayType.Normal] = normals.ToArray();

		var arrMesh = Mesh as ArrayMesh;

		if(arrMesh != null){
			arrMesh.AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles,
				surfaceArray);
		}

		initialized = true;
	}

	public bool ReadyToGo{
		get{
			return initialized;
		}
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
