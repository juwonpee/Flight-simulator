using UnityEngine;
using System;
using System.Collections.Generic;
using UnityEngine.Profiling;

using Den.Tools;
using Den.Tools.Matrices;
using Den.Tools.GUI;


namespace Den.Tools.Matrices
{
	public class MatrixTextureGizmo
	{
		private Mesh mesh;
		private Texture2D texture;
		private byte[] bytes;
		private Material material;
		private Matrix4x4 tfm;


		public void SetMatrix (Matrix matrix)
		{
			//generating preview texture 
			if (texture == null || texture.width != matrix.rect.size.x || texture.height != matrix.rect.size.z)
			{
				texture = new Texture2D(matrix.rect.size.x, matrix.rect.size.z, TextureFormat.RFloat, false, true);
				texture.filterMode = FilterMode.Point;
			}

			matrix.ExportTextureRaw(texture);

			//checking mesh
			if (mesh==null)
			{
				mesh = new Mesh();

				mesh.vertices = new Vector3[] {
					new Vector3(0,0,0),
					new Vector3(0,0,1),
					new Vector3(1,0,0),
					new Vector3(1,0,1) };
				mesh.uv = new Vector2[] {
					new Vector3(0,0),
					new Vector3(0,1),
					new Vector3(1,0),
					new Vector3(1,1) };
				mesh.triangles = new int[] {					
					1, 2, 0,
					2, 1, 3 };
			}

			//material
			if (material==null) material = new Material(Shader.Find("Hidden/MapMagic/TexturePreview"));
			material.SetTexture("_MainTex", texture);
			material.SetInt("_Margins", 0);
		}


		public void SetOffsetSize (Vector2D worldOffset, Vector2D worldSize)
		{
			tfm = Matrix4x4.TRS((Vector3)worldOffset, Quaternion.identity, (Vector3)worldSize);
		}


		public void SetMatrixWorld (MatrixWorld matrix)
		{
			SetMatrix(matrix);
			SetOffsetSize((Vector2D)matrix.worldPos, (Vector2D)matrix.worldSize);
		}


		public void Draw (
			bool colorize=false, bool relief=false, 
			float min=0, float max=1,
			Transform parent=null)
		{
			material.SetFloat("_Colorize", colorize ? 1 : 0);
			material.SetFloat("_Relief", relief ? 1 : 0);
			material.SetFloat("_MinValue", min);
			material.SetFloat("_MaxValue", max);

			material.SetPass(0);
			Graphics.DrawMeshNow(mesh, tfm);
		}


		public static void DrawNow (Matrix matrix, Vector2D worldOffset, Vector2D worldSize)
		{
			MatrixTextureGizmo gizmo = new MatrixTextureGizmo();
			gizmo.SetMatrix(matrix);
			gizmo.SetOffsetSize(worldOffset, worldSize);
			gizmo.Draw();
		}
	}


	public class MatrixTerrainGizmo
	{
		private Mesh mesh;
		private Vector3[] vertices;
		private Vector2[] uv;
		private int[] tris;
		private Material material;
		private Matrix4x4 tfm;

		public enum ZMode { Occluded, Overlay, Both };


		public void SetMatrix (Matrix matrix)
		{
			SetMatrixInThread(matrix);
			SetMatrixApply();
		}


		public void SetMatrixInThread (Matrix matrix)
		{
			if (vertices == null || vertices.Length != matrix.rect.Count)
				MakePlane(matrix.rect.size.x-1); //resolution is a number of planes, not vertices

			for (int x=0; x<matrix.rect.size.x; x++)
				for (int z=0; z<matrix.rect.size.z; z++)
				{
					int pos = z*matrix.rect.size.x + x;
					vertices[pos].y = matrix.arr[pos];
				}
		}


		public void SetMatrixApply ()
		{
			//mesh
			if (mesh == null) { mesh = new Mesh(); mesh.MarkDynamic(); }
			
			mesh.vertices = vertices;
			mesh.uv = uv;
			mesh.triangles = tris;
			mesh.RecalculateNormals();

			//material
			if (material==null) 
			{
				material = new Material(Shader.Find("Standard"));
				material.SetColor("_Color", Color.gray);
			}
		}


		private void MakePlane (int resolution) 
		/// Fills verts,tris,uv with a plane data within 0-1 coordinate
		{
			float step = 1f / resolution;
		
			vertices = new Vector3[(resolution+1)*(resolution+1)];
			uv = new Vector2[vertices.Length];
			tris = new int[resolution*resolution*2*3];

			int vertCounter = 0;
			int triCounter = 0;
			for (float x=0; x<1.00001f; x+=step) //including max
				for (float z=0; z<1.00001f; z+=step)
			{
				vertices[vertCounter] = new Vector3(x, 0, z);
				uv[vertCounter] = new Vector2(x, z);
			
				if (x>0.00001f && z>0.00001f)
				{
					tris[triCounter] = vertCounter-(resolution+1);		tris[triCounter+1] = vertCounter-1;					tris[triCounter+2] = vertCounter-resolution-2;
					tris[triCounter+3] = vertCounter-1;					tris[triCounter+4] = vertCounter-(resolution+1);	tris[triCounter+5] = vertCounter;
					triCounter += 6;
				}

				vertCounter++;
			}
		}


		public void SetOffsetSize (Vector3 worldOffset, Vector3 worldSize)
		{
			tfm = Matrix4x4.TRS(worldOffset, Quaternion.identity, worldSize);
		}


		public void Draw (
			Material material=null,
			Transform parent=null)
		{
			Material currMat = material ?? this.material;
			currMat.SetPass(0);
			Graphics.DrawMeshNow(mesh, tfm);
		}


		public static void DrawNow (Matrix matrix, Vector3 worldOffset, Vector3 worldSize)
		{
			MatrixTerrainGizmo gizmo = new MatrixTerrainGizmo();
			gizmo.SetMatrix(matrix);
			gizmo.SetOffsetSize(worldOffset, worldSize);
			gizmo.Draw();
		}
	}
}
