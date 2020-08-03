using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

using Den.Tools;
using Den.Tools.GUI;

namespace Den.Tools.AutoSplines
{
	//[CustomEditor(typeof(SplineObject))]
	public static class SplineInspector// : Editor
	{ 
		public static bool guiLines = true;
		public static bool guiKnobs = true;
		public static bool guiDisplay = true;

		/*UI ui = new UI();
		SplineObject so;

		public void OnSceneGUI ()
		{	
			if (so == null) so = (SplineObject)target;
			ui.DrawInspector(() => DrawSpline(so.splineSys));
		}*/


		public static void DrawSpline (SplineObject obj)
		{
			using (Cell.LineStd)
				using (new Draw.FoldoutGroup(ref guiDisplay, "Display", isLeft:true))
					if (guiDisplay)
					{
						using (Cell.LineStd) Draw.ToggleLeft(ref obj.drawNodes, "Draw Nodes");
						using (Cell.LineStd) Draw.ToggleLeft(ref obj.drawLines, "Draw Segments");
						using (Cell.LineStd) Draw.ToggleLeft(ref obj.drawBeizer, "Draw Beizer");

						if (Cell.current.valChanged)
							SceneView.lastActiveSceneView?.Repaint();
					}

			Cell.EmptyLinePx(4);
			using (Cell.LineStd)
				using (new Draw.FoldoutGroup(ref guiLines, "Splines", isLeft:true))
					if (guiLines)
					{
						using (Cell.LineStd) Draw.DualLabel("Lines Count", obj.splines.Length.ToString());

						if (SplineEditor.selectedNodes.Count == 1)
						{
							(int l, int n) = SplineEditor.selectedNodes.Any();
							Spline spline = obj.splines[l];

							//using (Cell.LineStd) Draw.DualLabel("Length", spline.length.ToString());
							using (Cell.LineStd) Draw.DualLabel("Node Counts", spline.nodes.Length.ToString());
							//using (Cell.LineStd) Draw.Toggle(ref spline.looped, "Looped");

							using (Cell.LineStd)
								if (Draw.Button("Remove"))
								{
									ArrayTools.RemoveAt(ref obj.splines, l);
									SplineEditor.selectedNodes.Clear();
									SplineEditor.dispSelectedNodes.Clear();
								}
						}

						using (Cell.LineStd)
							if (Draw.Button("Add"))
							{
								Spline newSpline = new Spline(
									SceneView.lastActiveSceneView.pivot - new Vector3(SceneView.lastActiveSceneView.cameraDistance/10,0,0),
									SceneView.lastActiveSceneView.pivot + new Vector3(SceneView.lastActiveSceneView.cameraDistance/10,0,0) );
								newSpline.nodes[0].y = 0;
								newSpline.nodes[1].y = 0;

								ArrayTools.Add(ref obj.splines, newSpline);
							}
					}

			Cell.EmptyLinePx(4);
			using (Cell.LineStd)
				using (new Draw.FoldoutGroup(ref guiKnobs, "Nodes", isLeft:true))
					if (guiKnobs)
					{
						using (Cell.LineStd) Draw.DualLabel("Selected", SplineEditor.selectedNodes.Count.ToString());
						
						if (SplineEditor.selectedNodes.Count == 1)
						{
							Cell.EmptyLinePx(4);
							(int l, int n) = SplineEditor.selectedNodes.Any();
							using (Cell.LinePx(0)) DrawNode (obj.splines, l, n);
						}
					}

			/*Cell.EmptyLinePx(4);
			using (Cell.LineStd)
				if (Draw.Button("Update"))
				{
					sys.Update();
					SceneView.lastActiveSceneView?.Repaint();
				}*/
		}

		public static void DrawNode (Spline[] sys, int l, int n)
		{
			using (Cell.LineStd) Draw.DualLabel("Number", "Line:" + l.ToString() + ", Node:" + n.ToString());
			using (Cell.LineStd) Draw.Toggle(n==0, "Is First");
			using (Cell.LineStd) Draw.Toggle(n==sys[l].nodes.Length-1, "Is Last");

			Spline spline = sys[l];

			using (Cell.LinePx(0))
			{
				using (Cell.LineStd) Draw.Field(ref spline.nodes[n], "Position");
				//using (Cell.LineStd) Draw.DualLabel("In Dir", knob.inDir.ToString());
				//using (Cell.LineStd) Draw.DualLabel("Out Dir", knob.outDir.ToString());
				//using (Cell.LineStd) Draw.Field(ref knob.inDir, "In Dir");
				//using (Cell.LineStd) Draw.Field(ref knob.outDir, "Out Dir");
				//using (Cell.LineStd) Draw.Field(ref knob.type, "Type");

				if (Cell.current.valChanged)
				{
					SceneView.lastActiveSceneView?.Repaint();
				}
			}
		}
	}
}
