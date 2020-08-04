using System;
using System.Collections.Generic;
using UnityEngine;

namespace Den.Tools.AutoSplines
{ 
	[Serializable] 
	public class Spline
	{
		public Vector3[] nodes;
		public int count;

		public Spline () { nodes = new Vector3[0]; count = 0; }
		public Spline (Spline src) { nodes = src.nodes; count = src.count; }
		public Spline (Vector3 start, Vector3 end) { nodes = new Vector3[2] {start,end}; count = 2; }
		public Spline (Vector3[] nodes) { this.nodes = nodes; count = nodes.Length; }

		public Vector3 GetPoint (int n, float p)
		{
			return nodes[n]*(1-p) + nodes[n+1]*p;
		}

		public void Split (int n, float p)
		{
			Vector3 point = GetPoint(n,p);
			ArrayTools.Insert(ref nodes, n+1, point); //new node position will be n+1
		}

		public void RemoveNode (int n) => ArrayTools.RemoveAt(ref nodes, n);


		public void Relax (float blur, int iterations) 
		/// Moves nodes to make the spline smooth
		/// Works with auto-tangents only
		{
			for (int i=0; i<iterations; i++)
				Relax(blur);
		}

		public void Relax (float blur)
		/// Moves nodes to make the spline smooth
		/// Works with auto-tangents only
		{
			throw new NotImplementedException();
			/*
			for (int n=1; n<segments.Length; n++)
			{
				Vector3 midPos = (segments[n-1].start.pos + segments[n].end.pos)/2;
				segments[n].start.pos = midPos*blur/2f + segments[n].start.pos*(1-blur/2f);
				segments[n-1].end.pos = segments[n].start.pos;
			}*/
		}


		public void Optimize (float deviation)
		/// Removes those nodes that should not change the shape a lot
		{
			throw new NotImplementedException();
			/*
			int iterations = segments.Length-1; //in worst case should remove all but start/end
			if (iterations <= 0) return;
			for (int i=0; i<iterations; i++) //using recorded itterations since nodes count will change
			//for (int i=0; i<deviation; i++)
			{
				float minDeviation = float.MaxValue;
				int minN = -1;

				for (int s=1; s<segments.Length; s++)
				{
					//checking how far point placed from tangent-tangent line
					float currDistToLine = DistanceToLine(
						segments[s-1].start.pos + segments[s-1].start.dir, 
						segments[s].end.pos + segments[s].end.dir, 
						segments[s].start.pos);
					//float currLine = ((nodes[n-1].pos+nodes[n-1].outDir) - (nodes[n+1].pos+nodes[n+1].inDir)).magnitude;
					//float currDeviation = (currDistToLine*currDistToLine) / currLine;
					float currDeviation = currDistToLine;

					if (currDeviation < minDeviation)
					{
						minN = s;
						minDeviation = currDeviation;
					}
				}

				if (minDeviation > deviation) break;

				segments[minN-1].end = segments[minN].end;
				ArrayTools.RemoveAt(ref segments, minN);

				UpdateTangents(); //(minN-1);
			}
			*/
		}

		#region Distance To Line

		public (int n, float p, float dist) ClosestToPoint (Vector3 pos)
		/// Returns the coordinate of a point on the spline that is closest to pos
		{
			int cn = -1;
			float cp = 0;
			float cDist = float.MaxValue;

			for (int n=0; n<nodes.Length-1; n++)
			{
				//skipping if pos is out of bounds+dist
				Vector3 segMin = Vector3.Min(nodes[n], nodes[n+1]);
				Vector3 segMax = Vector3.Max(nodes[n], nodes[n+1]);

				if (segMin.x > pos.x+cDist  ||  segMin.y > pos.y+cDist  ||  segMin.z > pos.z+cDist  ||
					segMax.x < pos.x-cDist  ||  segMax.x < pos.x-cDist  ||  segMax.x < pos.x-cDist)
						continue;

				Vector3 closestPoint = PointNearestToPos(pos, nodes[n], nodes[n+1]);
				closestPoint = ClampPointToSegment(closestPoint, nodes[n], nodes[n+1]);

				float dist = (closestPoint-pos).magnitude;
				if (dist<cDist)
				{
					cn = n;
					cp = (closestPoint-nodes[n]).magnitude / (nodes[n+1]-nodes[n]).magnitude;
					cDist = dist;
				}
			}

			return (cn, cp, cDist);
		}


		public (int n, float p, float dist) ClosestToRay (Ray ray)
		/// Returns the coordinate of a point on the spline that is closest to infinite ray
		/// Ray should be normalized
		{
			int cn = 0;
			float cp = 0;
			float cDist = float.MaxValue;

			for (int n=0; n<nodes.Length-1; n++)
			{
				Vector3 closestPoint = PointNearestToRay(ray, nodes[n], nodes[n+1]);
				closestPoint = ClampPointToSegment(closestPoint, nodes[n], nodes[n+1]);

				//and finding distance from closest point back to line
				Vector3 backPoint = PointNearestToPos(closestPoint, ray.origin, ray.origin+ray.direction);
				if (((backPoint-ray.origin).normalized - ray.direction).sqrMagnitude > 1) 
					backPoint = ray.origin;

				//DebugGizmos.DrawLine("Yellow " + n, backPoint, closestPoint, Color.yellow);

				float dist = (closestPoint-backPoint).magnitude;
				if (dist<cDist)
				{
					cn = n;
					cp = (closestPoint-nodes[n]).magnitude / (nodes[n+1]-nodes[n]).magnitude;
					cDist = dist;
				}
			}

			return (cn, cp, cDist);
		}


		private Vector3 PointNearestToPos (Vector3 pos, Vector3 segStart, Vector3 segEnd)
		/// Finds a point on a segment that is nearest to the given pos
		{
			Vector3 segVec = segStart - segEnd;
			Vector3 posVec = pos - segStart;
			float segVecMagnitude = segVec.magnitude;

			float percent = Vector3.Dot(posVec, segVec) / (segVecMagnitude*segVecMagnitude);
			
			return segVec*percent + segStart;
		}

		private Vector3 PointNearestToRay (Ray ray, Vector3 segStart, Vector3 segEnd)
		/// Finds a point on a segment that is nearest to the given infinite ray
		/// Ray should be normalized
		/// Borrowed from stackoverflow, I'll promise to return it back when it's not needed. Thanks 16807
		{
			Vector3 lineVec = ray.direction; //(lineEnd-lineStart).normalized;
			Vector3 segVec = (segEnd-segStart).normalized;
			Vector3 deltaVec = segStart-ray.origin; //-lineStart;

			Vector3 crossVec = Vector3.Cross(segVec,lineVec).normalized;
			Vector3 proj = Vector3.Dot(deltaVec, lineVec) * lineVec;
			Vector3 rej = deltaVec 
				- Vector3.Dot(deltaVec, lineVec) * lineVec 
				- Vector3.Dot(deltaVec, crossVec) * crossVec;
			float rejMagnitude = rej.magnitude;
			return segStart - segVec*rejMagnitude / Vector3.Dot(segVec,rej/rejMagnitude);
		}
		
		private Vector3 ClampPointToSegment (Vector3 point, Vector3 segStart, Vector3 segEnd)
		/// If pointNearest is located before the segment start or after segment end resturing it to start or to end
		{
			Vector3 segVec = segEnd - segStart;
			float segVecMagnitude = segVec.magnitude;
			Vector3 segVecNormalized = segVec / segVecMagnitude;

			float percentFromStart = (segStart-point).magnitude / segVecMagnitude;
			if (percentFromStart > 1) return segEnd;

			float percentFromEnd = (segEnd-point).magnitude / segVecMagnitude;
			if (percentFromEnd > 1) return segStart;

			return point;
		}

		#endregion

		#region Cut

			public enum CutAxis { AxisX, AxisZ }
			public enum CutSide { Negative=-1, Positive=1 }

			/*public void CutByRect (Vector3 pos, Vector3 size)
			/// Splits all segments so that each intersection with AABB rect has a node
			{			
				for (int i=0; i<13; i++) //spline could be divided in 12 parts maximum
				{
					List<Vector3> newNodes = new List<Vector3>();

					for (int n=0; n<nodes.Length-1; n++)
					{
						//early check - if inside/outside rect
						Vector3 min = Vector3.Min(nodes[n], nodes[n+1]);
						Vector3 max = Vector3.Max(nodes[n], nodes[n+1]);

						if (max.x < pos.x  ||  min.x > pos.x+size.x ||
							max.z < pos.z  ||  min.z > pos.z+size.z) 
								{ newNodes.Add(nodes[n+1]); continue; } //fully outside
						if (min.x > pos.x  &&  max.x < pos.x+size.x &&
							min.z > pos.z  &&  max.z < pos.z+size.z) 
								{ newNodes.Add(nodes[n+1]); continue; } //fully inside

						//splitting
						float sp = segments[s].IntersectRect(pos, size);
						if (sp < 0.0001f  ||  sp > 0.999f) 
							{ newSegments.Add(segments[s]); continue; }  //no intersection

						(Segment s1, Segment s2) = segments[s].GetSplitted(sp);
						newSegments.Add(s1);
						newSegments.Add(s2);
					}

					bool segemntsCountChanged = segments.Length != newSegments.Count;
					segments = newSegments.ToArray();
					if (!segemntsCountChanged) break; //if no nodes added - exiting 12 iterations
				}
			}*/

			public void CutAA (float val, CutAxis axis)
			/// Cuts the line creating points on horizontal line with X coordinate
			{
				List<Vector3> newNodes = new List<Vector3>(capacity: nodes.Length) {
					nodes[0] };

				for (int n=0; n<nodes.Length-1; n++)
				{
					float pos0 = axis==CutAxis.AxisX ? nodes[n].x : nodes[n].z;
					float pos1 = axis==CutAxis.AxisX ? nodes[n+1].x : nodes[n+1].z;

					// early check - on one side only
					if (pos0 < val  && pos1 < val) { newNodes.Add(nodes[n+1]); continue; }
					if (pos0 > val  &&  pos1 > val) { newNodes.Add(nodes[n+1]); continue; }

					// cutting
					float percent = (pos0 - val) / (pos0 - pos1);
					newNodes.Add( new Vector3(
						nodes[n].x*(1-percent) + nodes[n+1].x*percent,
						nodes[n].y*(1-percent) + nodes[n+1].y*percent,
						nodes[n].z*(1-percent) + nodes[n+1].z*percent ) );

					newNodes.Add(nodes[n+1]);
				}

				nodes = newNodes.ToArray();
			}


			public Spline[] RemoveOuter (float val, CutAxis axis, CutSide side)
			/// Removes segment if any of the segment nodes is less than x
			/// Will split spline in several, returns new splitted splines
			/// Subtract a bit (or add if side is positive) from x when using together wit Cut:  +0.0001f*(int)side
			{
				List<Spline> newSplines = new List<Spline>();

				List<Vector3> currSpline = null;

				for (int n=0; n<nodes.Length; n++)
				{
					bool isOuter = axis==CutAxis.AxisX ? nodes[n].x > val : nodes[n].z > val;
					if (side == CutSide.Negative) isOuter = !isOuter;

					// starting new spline
					if (currSpline == null)
					{
						if (!isOuter)
						{
							currSpline = new List<Vector3>();
							currSpline.Add(nodes[n]);
						}

						//ignoring if under x
					}

					else
					{
						//ending spline
						if (isOuter)
						{
							newSplines.Add( new Spline(currSpline.ToArray()) );
							currSpline = null;
						}

						//adding node
						else
							currSpline.Add(nodes[n]);
					}		
				}

				if (currSpline!=null)
					newSplines.Add( new Spline(currSpline.ToArray()) );

				return newSplines.ToArray();
			}

		#endregion
	}
}