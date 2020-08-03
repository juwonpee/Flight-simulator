using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Den.Tools.SceneEdit;

namespace Den.Tools.AutoSplines
{
	public static class SplineEditor
	{
		const float nodeSize = 15f;
		const float junctionSize = 5f;

		public static HashSet<(int,int)> selectedNodes = new HashSet<(int,int)>();
		public static HashSet<(int,int)> dispSelectedNodes = new HashSet<(int,int)>(); //knobs that are virtually selected when dragging the selection frame
		public static int[] selectionNumsBounds = new int[0]; //to clear selection if line number or num nodes in each line changed


		#region Draw

			public static void DrawSplineSys (Spline[] sys, Matrix4x4 trs,
				bool drawLines=true, bool drawBeizer=false, bool drawNodes=false)
			/// Drawing inactive (non-editable) system
			{
				foreach (Spline spline in sys)
					DrawSpline(spline, trs, drawLines, drawBeizer, drawNodes);
			}
			

			public static void DrawSpline (Spline spline, Matrix4x4 trs, 
				bool drawLines=true, bool drawBeizer=false, bool drawNodes=false)
			{
				//lines
				if (drawLines)
				{
					Handles.DrawPolyLine(spline.nodes);
				}

				//segments
				if (drawBeizer)
				{
					for (int n=0; n<spline.nodes.Length; n++)
					{
						Vector3 n0dir = spline.GetTangent(n);
						Vector3 n1dir = spline.GetTangent(n+1, true);

						Handles.DrawBezier(
							spline.nodes[n], 
							spline.nodes[n+1], 
							spline.nodes[n]+n0dir, 
							spline.nodes[n+1]+n1dir, 
							Handles.color, null, 2);
					}
				}

				//nodes
				if (drawNodes)
					for (int n=0; n<spline.nodes.Length; n++)
						Handles.DotHandleCap(0, spline.nodes[n], Quaternion.identity, HandleUtility.GetHandleSize(spline.nodes[n])/nodeSize, EventType.Repaint);
			}


		#endregion


		#region Edit

			
			public static bool EditSplineSys (Spline[] sys, Matrix4x4 trs, Object undoObject=null)
			/// Select, move, rotate, scale nodes (and draw selection/transform gizmos)
			/// Returns true if there was a change
			{
				bool added = false;
				bool removed = false;
				bool moved = false;

				//clearing selection if it's bounds changed
				if (!IsSplineNodeNumbersMatch(sys))
				{
					selectedNodes.Clear();
					dispSelectedNodes.Clear();
				}


				//adding/removing  points (before selection to remove selected)
				Event eventCurrent = Event.current;
				if (eventCurrent.type == EventType.MouseUp  &&  eventCurrent.button == 0  &&  !eventCurrent.alt  &&  !SceneEdit.Select.isFrame)  //releasing mouse with no frame
				{
					//adding
					if (eventCurrent.shift) 
						added = AddNode(sys, eventCurrent.mousePosition, undoObject);

					//removing
					else if (eventCurrent.control)
						removed = RemoveNode(sys, eventCurrent.mousePosition, undoObject);
				}


				//moving
				if (selectedNodes.Count != 0)
					moved = MoveSelectedNodes(sys, undoObject);


				//selecting nodes (after move, to avoid drawing frame instead of moving)
				SelectNodes(sys, trs);


				//re-drawing selected nodes
				if (selectedNodes.Count != 0)
				{
					Color hColor = Handles.color;
					Handles.color = new Color(1,1,0,1);

					foreach ((int l, int n) in selectedNodes)
						Handles.DotHandleCap(0, sys[l].nodes[n], Quaternion.identity, HandleUtility.GetHandleSize(sys[l].nodes[n])/nodeSize, EventType.Repaint);

					Handles.color = hColor;
				}

				return added || removed || moved;
			}


			private static bool IsSplineNodeNumbersMatch (Spline[] sys)
			/// Compares sys lines count (and number of nodes in each) with stored selectionNumsBounds array
			/// BTW Will modify selectionNumsBounds array if not match
			{
				bool match = true;

				if (selectionNumsBounds.Length != sys.Length) 
					{ match = false; selectionNumsBounds = new int[sys.Length]; }

				if (match)
					for (int l=0; l<sys.Length; l++)
					{
						int nodesCount = sys[l].nodes.Length;
						if (selectionNumsBounds[l] != nodesCount) 
							match = false;
						selectionNumsBounds[l] = nodesCount;
					}

				return match;
			}


			private static void SelectNodes (Spline[] sys, Matrix4x4 trs)
			/// Draws selection and selects new nodes (segment starts and junctions). Returns the list of currently selected segments.
			{
				//disabling selection
				HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));

				SceneEdit.Select.UpdateFrame();

				bool selectionChanged = false;

				for (int l=0; l<sys.Length; l++)
				{
					Spline line = sys[l];
					for (int n=0; n<line.nodes.Length; n++)
					{
						bool isSelected = selectedNodes.Contains((l,n));
						bool isDispSelected = dispSelectedNodes.Contains((l,n));

						bool newSelected = isSelected;
						bool newDispSelected = isDispSelected;

						SceneEdit.Select.CheckSelected(line.nodes[n], nodeSize/2, ref newSelected, ref newDispSelected);

						if (newSelected && !isSelected)
						{ 
							if (!SceneEdit.Select.isFrame)  selectedNodes.Clear(); //selecting only one (but de-selecting all)
							selectedNodes.Add((l,n)); selectionChanged=true; 
						}
						
						if (!newSelected && isSelected)
							{ selectedNodes.Remove((l,n)); selectionChanged=true; }

						if (newDispSelected && !isDispSelected)
							dispSelectedNodes.Add((l,n));

						if (!newDispSelected && isDispSelected)
							dispSelectedNodes.Remove((l,n));
					}
				}

				if (selectionChanged) 
				{
					Editor[] allEditors = Resources.FindObjectsOfTypeAll<Editor>();
					for (int e=0; e<allEditors.Length; e++)
						allEditors[e].Repaint();
				}
			}


			/*private static bool MoveNode (ref Knob knob)
			/// Draws transform gizmo for the node, and transform it with it's tangents (i.e. it could be rotated and scale)
			{
				//hiding the defaul tool
				UnityEditor.Tools.hidden = true;

				//gathering all positions
				Vector3[] allPositions = new Vector3[] { 
					knob.pos, 
					knob.pos + knob.outDir,
					knob.pos + knob.inDir };

				//transforming
				bool changed = MoveRotateScale.Update(allPositions, knob.pos);

				//setting back positions
				if (changed)
				{
					knob.pos = allPositions[0];
					knob.outDir = allPositions[1] - knob.pos;
					knob.inDir = allPositions[2] - knob.pos;
				}

				return changed;
			}


			private static bool EditTangents (ref Knob knob)
			{
				bool change = false;

				//moving tans
				if (knob.type == Node.TangentType.broken || knob.type == Node.TangentType.correlated)
				{
					if (knob.outDefined)
					{
						Vector3 outTanPos = knob.pos+knob.outDir;
						Vector3 newOutTanPos = Handles.PositionHandle(outTanPos, Quaternion.identity);
						if ((outTanPos - newOutTanPos).sqrMagnitude != 0)
						{ 
							knob.outDir = newOutTanPos - knob.pos; 
							if (knob.type == Node.TangentType.correlated) knob.inDir = Node.CorrelatedInTangent(knob.inDir, knob.outDir);
							change = true; 
						}
					}

					if (knob.inDefined)
					{
						Vector3 inTanPos = knob.pos+knob.inDir;
						Vector3 newInTanPos = Handles.PositionHandle(inTanPos, Quaternion.identity);
						if ((inTanPos - newInTanPos).sqrMagnitude != 0)
						{ 
							knob.inDir = newInTanPos - knob.pos;
							if (knob.type == Node.TangentType.correlated) knob.outDir = Node.CorrelatedOutTangent(knob.inDir, knob.outDir);
							change = true; 
						}
					}
				}

				return change;
			}*/


			private static bool MoveSelectedNodes (Spline[] sys, Object undoObject=null)
			/// Draws currently selected tool gizmo and translates the selected nodes
			/// Not only moves, but rotates and scales
			{
				//hiding the defaul tool
				UnityEditor.Tools.hidden = true;

				//gathering all positions
				//Vector3[] allPositions = new Vector3[knobs.Length*3]; //for each node 3 positions: 1 pos and 2 tangents
				Vector3[] allPositions = new Vector3[selectedNodes.Count];
				
				int i=0;
				foreach ((int l, int n) in selectedNodes)
				{
					allPositions[i] = sys[l].nodes[n];
					i++;
				}

				//positions center
				Vector3 pivot = new Vector3(0,0,0);
				for (i=0; i<allPositions.Length; i++)
					pivot += allPositions[i];
				pivot /= selectedNodes.Count;

				//transforming
				bool changed = MoveRotateScale.Update(allPositions, pivot);

				//setting back positions
				if (changed)
				{
					if (undoObject!=null)
						Undo.RecordObject(undoObject, "Spline Nodes Move");

					i=0;
					foreach ((int l, int n) in selectedNodes)
					{
						sys[l].nodes[n] = allPositions[i];
						i++;
					}
				}

				return changed;
			}


			private static bool AddNode (Spline[] sys, Vector2 mousePos, Object undoObject=null)
			{
				if (SceneView.currentDrawingSceneView?.camera == null) return false;

				bool change = false;

				Ray mouseRay = HandleUtility.GUIPointToWorldRay(mousePos); 
				//SceneView.currentDrawingSceneView.camera.ScreenPointToRay(mousePos); //gives coord offseted 20 pixels up

				(Spline cSpline, int cn, float cp, float cDist) = GetSplineDistance(sys, mouseRay);

				Vector3 worldPos = cSpline.GetPoint(cn, cp);
				Vector2 screenPos = HandleUtility.WorldToGUIPoint(worldPos);
				//SceneView.currentDrawingSceneView.camera.WorldToScreenPoint(worldPos);
				
				if ((mousePos-screenPos).magnitude < 10) //10 pixels from line on screen
				{
					//avoiding adding a node instead selection
					(Spline tmpSpline, int tmpn, float minNodeDist) = GetNodeDistance(sys, mousePos);
					if (minNodeDist < nodeSize/2)
						return false;

					if (undoObject!=null)
						Undo.RecordObject(undoObject, "Spline Node Add");

					cSpline.Split(cn, cp);

					change = true;
					EditorWindow.focusedWindow?.Repaint(); 
				}

				return change;
			}


			private static bool RemoveNode (Spline[] sys, Vector2 mousePos, Object undoObject)
			{
				bool change = false;

				(Spline cSpline, int cn, float cDist) = GetNodeDistance(sys, mousePos);

				if (cDist < nodeSize/2)
				{
					if (undoObject!=null)
						Undo.RecordObject(undoObject, "Spline Node Remove");

					cSpline.RemoveNode(cn);

					change = true;
					EditorWindow.focusedWindow?.Repaint(); 
				}

				return change;
			}


			private static (Spline cSpline, int cn, float cp, float cDist) GetSplineDistance (Spline[] sys, Ray mouseRay)
			/// Gets a distance from mouse cursor to closest spline in a system, and returns closest data
			{
				Spline cSpline = null;
				int cn = -1;
				float cp = 0;
				float cDist = float.MaxValue;

				(int n, float p, float dist)[] perSplineDists = new (int n, float p, float dist)[sys.Length];
				for (int s=0; s<sys.Length; s++)
				{
					(int n, float p, float dist) = sys[s].ClosestToRay(mouseRay);
					if (dist < cDist)
					{
						cSpline = sys[s];
						cn = n;
						cp = p;
						cDist = dist;
					}
				}

				return (cSpline, cn, cp, cDist);
			}


			private static  (Spline cSpline, int cn, float cDist) GetNodeDistance (Spline[] sys, Vector2 mousePos)
			/// Gets the closest node in screen-space, returns the distance to this node
			{
				float cDist = float.MaxValue;
				Spline cSpline = null;
				int cn = 0;

				for (int l=0; l<sys.Length; l++) 
				{
					Spline spline = sys[l];
					for (int n=0; n<spline.nodes.Length; n++)
					{
						Vector2 nodeScreenPos = HandleUtility.WorldToGUIPoint(spline.nodes[n]);
						float dist = (nodeScreenPos-mousePos).magnitude;
						if (dist < cDist) 
						{ 
							cSpline = spline; 
							cn = n;
							cDist = dist;
						}
					}
				}

				return (cSpline, cn, cDist);
			}

		#endregion
	}
}
