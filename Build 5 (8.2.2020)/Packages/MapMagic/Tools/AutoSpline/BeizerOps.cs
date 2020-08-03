using System;
using System.Collections.Generic;
using UnityEngine;

namespace Den.Tools.AutoSplines
{ 
	[Serializable] 
	public static class BeizerOPs
	{
		/*private struct Segment 
		{
			public Vector3 pos0;
			public Vector3 dir0;
			public Vector3 pos1;
			public Vector3 dir1;


			public Vector3 BeizerPosition (float p)
			/// Gets position at n segment and p percent
			/// Large overhead of getting tangents
			{
				float ip = 1f-p;
				return  
					ip*ip*ip*pos0 + 
					3*p*ip*ip*(pos0+dir0) + 
					3*p*p*ip*(pos1+dir1) + 
					p*p*p*pos1;
			}
		}*/

		public static Vector3 GetTangent (this Spline spline, int n, bool prev=false)
		/// Gets the auto-layouted direction of n node, looking towards next node
		/// Or prev node if prev enabled
		{
			//only two nodes
			if (spline.nodes.Length==2)
			{
				if (n==0) 
				{
					Vector3 dir = (spline.nodes[n+1] - spline.nodes[n]) * 0.333f;
					return !prev ? dir : -dir;
				}

				else
				{
					Vector3 dir = (spline.nodes[n] - spline.nodes[n-1]) * 0.333f;
					return !prev ? dir : -dir;
				}
			}

			//first
			else if (n == 0)
			{
				Vector3 nextTan = GetTangent(spline, 1, true);
				Vector3 dir = ((spline.nodes[n+1]+nextTan) - spline.nodes[n]) * 0.333f;
				//Vector3 dir = (spline.nodes[n+1] - spline.nodes[n]) * 0.333f;
				return !prev ? dir : -dir;
			}

			//last
			else  if (n == spline.nodes.Length-1)
			{
				Vector3 prevTan = GetTangent(spline, spline.nodes.Length-2, false);
				Vector3 dir = (spline.nodes[n] - (spline.nodes[n-1]+prevTan)) * 0.333f;
				//Vector3 dir = (spline.nodes[n-1] - spline.nodes[n]) * 0.333f;
				return !prev ? dir : -dir;
			}

			//common case
			else
			{
				Vector3 outDir = spline.nodes[n+1] - spline.nodes[n];  
				float outDirLength = outDir.magnitude;
				if (outDirLength > 0.00001f) //prevPos match with pos, usually on first segment
					outDir /= outDirLength;
				else
					outDir = new Vector3();

				Vector3 inDir = spline.nodes[n-1] - spline.nodes[n];
				float inDirLength = inDir.magnitude;
				if (inDirLength > 0.00001f)
					inDir /= inDirLength;
				else
					inDir = new Vector3();

				Vector3 newInDir = (inDir - outDir).normalized;
				Vector3 newOutDir = -newInDir; //(outDir - inDir).normalized;

				inDir = newInDir.normalized * inDirLength * 0.35f;
				outDir = newOutDir.normalized * outDirLength * 0.35f;

				return !prev ? outDir : inDir;
			}
		}


		public static Vector3 BeizerPosition (this Spline spline, int n, float p)
		/// Gets position at n segment and p percent
		/// Large overhead of getting tangents
		{
			Vector3 n0dir = spline.GetTangent(n);
			Vector3 n1dir = spline.GetTangent(n+1, true);

			float ip = 1f-p;
			return  
				ip*ip*ip*spline.nodes[n] + 
				3*p*ip*ip*(spline.nodes[n]+n0dir) + 
				3*p*p*ip*(spline.nodes[n+1]+n1dir) + 
				p*p*p*spline.nodes[n+1];
		}

		public static Vector3 BeizerPosition (Vector3 pos0, Vector3 dir0, Vector3 pos1, Vector3 dir1,
			float p)
		/// Gets position at n segment and p percent
		/// Large overhead of getting tangents
		{
			float ip = 1f-p;
			return  
				ip*ip*ip*pos0 + 
				3*p*ip*ip*(pos0+dir0) + 
				3*p*p*ip*(pos1+dir1) + 
				p*p*p*pos1;
		}


		public static IEnumerable<Vector3> BeizerPositions (this Spline spline, int n, int count)
		/// Iterates in a given number of positions
		/// Includes original point, but not includes final (n+1) point
		{
			if (n>=spline.nodes.Length-1)
				throw new Exception("Beizer positions should be calculated for n-1");

			Vector3 n0dir = spline.GetTangent(n);
			Vector3 n1dir = spline.GetTangent(n+1, true);

			yield return spline.nodes[n];

			for (int i=1; i<count; i++)
			{
				float p = (float)i / count;
				float ip = 1f-p;

				yield return  
					ip*ip*ip*spline.nodes[n] + 
					3*p*ip*ip*(spline.nodes[n]+n0dir) + 
					3*p*p*ip*(spline.nodes[n+1]+n1dir) + 
					p*p*p*spline.nodes[n+1];
			}
		}


		public static void Subdivide (Spline spline, int num)
		/// Splits each segment in several shorter segments
		{
			Vector3[] newNodes = new Vector3[(spline.nodes.Length-1)*num + 1];

			for (int n=0; n<spline.nodes.Length-1; n++)
			{
				Vector3 pos0 = spline.nodes[n];
				Vector3 dir0 = spline.GetTangent(n); 
				Vector3 pos1 = spline.nodes[n+1];
				Vector3 dir1 = spline.GetTangent(n+1, true);

				for (int i=0; i<num; i++)
				{
					float percent = (float)i / num;

					Vector3 split = BeizerPosition(pos0, dir0, pos1, dir1, percent);



					newNodes[n*num+i] = split;
					
					//float percent = 1f / (num-i);
					//(Segment,Segment) split = currSegment.GetSplitted(percent);
					//currSegment = split.Item2;
				}
			}

			newNodes[newNodes.Length-1] = spline.nodes[spline.nodes.Length-1];

			spline.nodes = newNodes;
		}


		public static void Optimize (Spline spline, float deviation)
		/// Removes those nodes that should not change the shape a lot
		/// Similar to Spline.Optimize, but takes tangents into account
		{
			int iterations = spline.nodes.Length-2; //in worst case should remove all but start/end
			if (iterations <= 0) return;
			for (int i=0; i<iterations; i++) //using recorded iterations since nodes count will change
			//for (int i=0; i<deviation; i++)
			{
				float minDeviation = float.MaxValue;
				int minN = -1;

				//Vector3 posM1 = spline.nodes[0];
				//Vector3 dirM1 = spline.GetTangent(0); 

				for (int n=1; n<spline.nodes.Length-1; n++)
				{
					Vector3 posM1 = spline.nodes[n-1];
					Vector3 dirM1 = spline.GetTangent(n-1);
					Vector3 pos0 = spline.nodes[n];
					Vector3 dir0 = spline.GetTangent(n); 
					Vector3 pos1 = spline.nodes[n+1];
					Vector3 dir1 = spline.GetTangent(n+1, true);

					//checking how far point placed from tangent-tangent line
					float currDistToLine = DistanceToLine(
						posM1 + dirM1, 
						pos1 + dir1, 
						pos0);
					//float currLine = ((nodes[n-1].pos+nodes[n-1].outDir) - (nodes[n+1].pos+nodes[n+1].inDir)).magnitude;
					//float currDeviation = (currDistToLine*currDistToLine) / currLine;
					float currDeviation = currDistToLine;

					if (currDeviation < minDeviation)
					{
						minN = n;
						minDeviation = currDeviation;
					}

					//posM1 = pos0;  dirM1 = dir0;
				}

				if (minDeviation > deviation) break;
				ArrayTools.RemoveAt(ref spline.nodes, minN);
			}
		}


		private static float DistanceToLine (Vector3 lineStart, Vector3 lineEnd, Vector3 point)
		/// Just helper fn for optimize, isn't related with beizer
		{
			Vector3 lineDir = lineStart-lineEnd;
			float lineLengthSq = lineDir.x*lineDir.x + lineDir.y*lineDir.y + lineDir.z*lineDir.z;
			float lineLength = Mathf.Sqrt(lineLengthSq);
			float startDistance = (lineStart-point).magnitude;
			float endDistance = (lineEnd-point).magnitude;

			//finding height of triangle 
			float halfPerimeter = (startDistance + endDistance + lineLength) / 2;
			float square = Mathf.Sqrt( halfPerimeter*(halfPerimeter-endDistance)*(halfPerimeter-startDistance)*(halfPerimeter-lineLength) );
			float height = 2/lineLength * square;

			//dealing with out of line cases
			float distFromStartSq = startDistance*startDistance - height*height;
			float distFromEndSq = endDistance*endDistance - height*height;

			if (distFromStartSq > lineLengthSq && distFromStartSq > distFromEndSq) return endDistance;
			else if (distFromEndSq > lineLengthSq) return startDistance; 
			else return height;
		}




		#region Distance Ops

		#endregion


		#region Approximate Length Ops

			// Measures line length (and calculates lut) by evaluating line points and measuring distance between them
			// Faster than integral length ops, but less precise
			// Useful when tangents lay on the line: 
			//		float linearLength = (start.pos-end.pos).magnitude;
			//		float tangentLength = start.dir.magnitude + ((start.dir+start.pos) - (end.dir+end.pos)).magnitude + end.dir.magnitude;
			//		if (linearLength > tangentLength-tangentLength/100 && linearLength < tangentLength+tangentLength/100)


			public static float ApproxLengthBeizer (this Spline spline)
			/// Calculates the length of all curve by evaluating split segments lengths
			{
				float length = 0;
				for (int n=0; n<spline.nodes.Length-1; n++)
					length += ApproxLengthBeizer(spline, n, 0,1, 32);

				return length;
			}

			public static float ApproxLengthBeizer (this Spline spline, int n, float pStart=0, float pEnd=1, int iterations=32) =>
				ApproxLengthBeizer(spline.nodes[n], spline.GetTangent(n), spline.nodes[n+1], spline.GetTangent(n+1, true), pStart, pEnd, iterations);

			public static float ApproxLengthBeizer (Vector3 pos0, Vector3 dir0, Vector3 pos1, Vector3 dir1, 
				float pStart=0, float pEnd=1, int iterations=32)
			/// Splits the curve, calcs distance between points, measures the approximate length of the segment part from pStart to pEnd
			{
				float length = 0;
				float pDelta = pEnd - pStart;
				float pStep = pDelta / (iterations-1);

				float prevPercent = 0;
				for (int i=1; i<iterations; i++)
				{
					float percent = pStart + pStep*i;
					length += ArcLinearDistance(pos0, dir0, pos1, dir1, prevPercent, percent);
					prevPercent = percent;
				}

				return length;
			}


			private static float ArcLinearDistance (Vector3 pos0, Vector3 dir0, Vector3 pos1, Vector3 dir1, 
				float a, float b)
			/// Calculates the line distance between two points on beizer curve
			/// Iteration of ApproxLengthBeizer
			{
				float ia = 1f-a;
				float ib = 1f-b;

				Vector3 delta = 
					(3*a*ia*ia - 3*b*ib*ib)*(dir0) + 
					(3*a*a*ia - 3*b*b*ib)*(pos1+dir1-pos0) + 
					(a*a*a - b*b*b)*(pos1-pos0);

				return delta.magnitude;
			}


			public static void LengthToPercentLut (this Spline spline, int n, float[] lengths, float[] lut) =>
				LengthToPercentLut(spline.nodes[n], spline.GetTangent(n), spline.nodes[n+1], spline.GetTangent(n+1, true), lengths, lut);

			public static void LengthToPercentLut (Vector3 pos0, Vector3 dir0, Vector3 pos1, Vector3 dir1,
				float[] lengths, float[] lut)
			/// Fills lut array with length data 
			{
				float totalLength = 0;

				//calculating lengths
				for (int p=0; p<lengths.Length; p++)
				{
					float startPercent = 1f * p / lengths.Length;
					float endPercent = 1f * (p+1) / lengths.Length;
					
					float length = ArcLinearDistance(pos0, dir0, pos1, dir1, startPercent, endPercent);
					lengths[p] = totalLength + length;
					totalLength += length;
				}

				//transforming all lengths to normalized
				if (totalLength >= 0)
					for (int p=0; p<lengths.Length; p++)
						lengths[p] /= totalLength;

				//filling length-to-percent lut
				for (int i=0; i<lut.Length; i++)
				{
					float normLength = (float)(i+1) / (lut.Length+1);

					int startNum = 0;
					for (int l=0; l<lengths.Length; l++)
						if (lengths[l]>normLength) { startNum = l; break; }
					int endNum = startNum+1;
					
					float stEndPercent = (normLength-lengths[startNum]) / (lengths[endNum]-lengths[startNum]);
					float segPercent = (startNum*(1-stEndPercent) + endNum*stEndPercent) / (lengths.Length+1);
					lut[i] = segPercent;
				}
			}

		#endregion


		#region Integral Length
		/*
					public float IntegralLength (float t=1, int iterations=8)
					/// A variation of Legendre-Gauss solution from Primer (https://pomax.github.io/bezierinfo/#arclength)
					{
						float z = t / 2;
						float sum = 0;

						double[] abscissaeLutArr = abscissaeLut[iterations];
						double[] weightsLutArr = weightsLut[iterations];

						float correctedT;
						for (int i=0; i<iterations; i++) 
						{
							correctedT = z * (float)abscissaeLutArr[i] + z;

							Vector3 derivative = GetDerivative(correctedT);
							float b = Mathf.Sqrt(derivative.x*derivative.x + derivative.z*derivative.z);
							sum += (float)weightsLutArr[i] * b;
						}

						return z * sum;
					}


					public void FillIntegralLengthLut (float fullLength=-1, int iterations=32)
					/// Fills the distance for each length (i.e. if lengthPercents is 4 then finds dist for 0.25,0.5,075,1)
					{
						if (fullLength < 0) fullLength = IntegralLength();

						float pHalf = LengthToPercent(fullLength*0.5f, iterations:iterations);
						float p025 = LengthToPercent(fullLength*0.25f, pFrom:0, pTo:pHalf, iterations:iterations/2);
						float p075 = LengthToPercent(fullLength*0.75f, pFrom:pHalf, pTo:1, iterations:iterations/2);
						float p0125 = LengthToPercent(fullLength*0.125f, pFrom:0, pTo:p025, iterations:iterations/4);
						float p0375 = LengthToPercent(fullLength*0.375f, pFrom:p025, pTo:pHalf, iterations:iterations/4);
						float p0625 = LengthToPercent(fullLength*0.625f, pFrom:pHalf, pTo:p075, iterations:iterations/4);
						float p0875 = LengthToPercent(fullLength*0.875f, pFrom:p075, pTo:1, iterations:iterations/4);

						lengthToPercentLut.b0 = (byte)(p0125*255 + 0.5f);
						lengthToPercentLut.b1 = (byte)(p025*255 + 0.5f);
						lengthToPercentLut.b2 = (byte)(p0375*255 + 0.5f);
						lengthToPercentLut.b3 = (byte)(pHalf*255 + 0.5f);
						lengthToPercentLut.b4 = (byte)(p0625*255 + 0.5f);
						lengthToPercentLut.b5 = (byte)(p075*255 + 0.5f);
						lengthToPercentLut.b6 = (byte)(p0875*255 + 0.5f);
					}


					public float LengthToPercent (float length, float pFrom=0, float pTo=1, int iterations=8)
					/// Converts world length to segments percent without using segment length LUT. Used to fill that LUT.
					{
						float pMid = (pFrom+pTo)/2;
						float lengthMid = IntegralLength(pMid);

						if (length < lengthMid)
						{
							if (iterations <= 1) return (pFrom+pMid) /2;
							else return LengthToPercent(length, pFrom:pFrom, pTo:pMid, iterations:iterations-1);
						}

						else
						{
							if (iterations <= 1) return (pMid+pTo) /2;
							else return LengthToPercent(length, pFrom:pMid, pTo:pTo, iterations:iterations-1);
						}
					}
		*/
		#endregion


		#region Uniform Split
		
			// Operations related with the real spline distances (not distorted by beizer)
			// Requires length to percent lut - it's either approximate or linear LengthToPercentLut

			public static float NormLengthToPercent (Spline spline, int n, float normLength, float[] lengthToPercentLut) =>
				NormLengthToPercent(spline.nodes[n], spline.GetTangent(n), spline.nodes[n+1], spline.GetTangent(n+1, true), normLength, lengthToPercentLut);

			public static float NormLengthToPercent (Vector3 pos0, Vector3 dir0, Vector3 pos1, Vector3 dir1, 
				float normLength, float[] lengthToPercentLut)
			/// Converts relative length (range 0-1) to segment percent
			{
				int numIntervals = lengthToPercentLut.Length + 1;  //8 points, do not include 0 and 1 => 9 intervals

				int prevMark = (int)(normLength*9);  
				float markPercent = (normLength - prevMark/9f)*9f;

				float prevPercent = 0;
				float nextPercent = 1;

				//for (int i=1; i<lengthToPercentLut.Length; i++)

				if (prevMark>0) prevPercent=lengthToPercentLut[prevMark-1];
				if (prevMark<lengthToPercentLut.Length) nextPercent = lengthToPercentLut[prevMark];

				return (prevPercent*(1-markPercent) + nextPercent*markPercent);
			}


			public static IEnumerable<Vector3> UniformPositions (Spline spline, int n, int count)
			/// Iterates in a given number of positions
			/// Includes original point, but not includes final (n+1) point
			{
				if (n>=spline.nodes.Length-1)
					throw new Exception("Beizer positions should be calculated for n-1");

				Vector3 pos0 = spline.nodes[n];
				Vector3 dir0 = spline.GetTangent(n); 
				Vector3 pos1 = spline.nodes[n+1];
				Vector3 dir1 = spline.GetTangent(n+1, true);

				float[] lengths = new float[64];
				float[] lengthToPercentLut = new float[8];

				LengthToPercentLut(pos0, dir0, pos1, dir1, lengths, lengthToPercentLut);

				for (int i=0; i<count; i++)
				{
					float l = (float)i / count;
					float p = NormLengthToPercent(pos0, dir0, pos1, dir1, l, lengthToPercentLut);
					yield return BeizerPosition(pos0, dir0, pos1, dir1, p);
				}
			}


			public static Vector3[] GetAllPoints (Spline spline, float resPerUnit=0.1f, int minRes=3, int maxRes=20)
			/// Converts line into array of points to draw polyline
			/// Requires length updated
			/// Maybe rename to version of Subdivide?
			{
				//calculating lengths
				float[] lengths = new float[spline.nodes.Length-1];
				for (int n=0; n<lengths.Length; n++)
					lengths[n] = ApproxLengthBeizer(spline, n);

				//calculating number of points
				int numPoints = 0;
				for (int n=0; n<lengths.Length; n++)
				{
					int modRes = (int)( lengths[n] * resPerUnit );
					if (modRes < minRes) modRes = minRes;
					if (modRes > maxRes) modRes = maxRes;

					numPoints += modRes;
				}

				Vector3[] points = new Vector3[numPoints + 1];
			
				int i=0;
				for (int n=0; n<lengths.Length; n++)
				{
					int modRes = (int)( lengths[n] * resPerUnit );
					if (modRes < minRes) modRes = minRes;
					if (modRes > maxRes) modRes = maxRes;

					Vector3 pos0 = spline.nodes[n];
					Vector3 dir0 = spline.GetTangent(n); 
					Vector3 pos1 = spline.nodes[n+1];
					Vector3 dir1 = spline.GetTangent(n+1, true);

					for (int p=0; p<modRes; p++)
					{
						float percent = 1f*p / modRes;
						points[i] = BeizerPosition(pos0, dir0, pos1, dir1, percent);
						i++;
					}
				}

				//the last one
				points[points.Length-1] = spline.nodes[spline.nodes.Length-1];

				return points; 
			}

		#endregion
	}
}