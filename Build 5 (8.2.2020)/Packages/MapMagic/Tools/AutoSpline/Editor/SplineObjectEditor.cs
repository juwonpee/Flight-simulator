using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

using Den.Tools.GUI;

namespace Den.Tools.AutoSplines
{
	[CustomEditor(typeof(SplineObject))]
	public partial class SplineObjectInspector : Editor
	{
		[DrawGizmo(GizmoType.NonSelected | GizmoType.Selected)]
		static void DrawInactiveGizmo (SplineObject obj, GizmoType gizmoType)
		{
			//drawing selected in OnSceneGUI - otherwise will be erased by matrix gizmo drawn in tester's OnSceneGUI

			if (gizmoType.HasFlag(GizmoType.NonSelected))
			{
				Handles.color = new Color(0.75f, 0.25f, 0, 1);
				SplineEditor.DrawSplineSys(obj.splines, obj.transform.localToWorldMatrix,
					drawLines:obj.drawLines, drawBeizer:obj.drawBeizer, drawNodes:obj.drawNodes);
			}
		}

		private void OnSceneGUI () 
		{
			SplineObject obj = (SplineObject)target;

			Handles.color = new Color(1, 0.5f, 0, 1);
			SplineEditor.DrawSplineSys(obj.splines, obj.transform.localToWorldMatrix,
				drawLines:obj.drawLines, drawBeizer:obj.drawBeizer, drawNodes:obj.drawNodes);

			SplineEditor.EditSplineSys(obj.splines, obj.transform.localToWorldMatrix, obj);
		}


		UI ui = new UI();
		public override void OnInspectorGUI () 
			{ ui.Draw(DrawGUI); }

		private void DrawGUI ()
		{
			SplineObject splineObj = (SplineObject)target;
			SplineInspector.DrawSpline(splineObj);


		}
	}
}
