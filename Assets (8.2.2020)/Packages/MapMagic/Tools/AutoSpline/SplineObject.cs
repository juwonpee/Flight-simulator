using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Den.Tools.AutoSplines
{
	public class SplineObject : MonoBehaviour
	{
		public Spline[] splines = new Spline[] { new Spline( new Vector3(0,0,0), new Vector3(100,0,100) ) };

		public bool drawBeizer = false;
		public bool drawLines = true;
		public bool drawNodes = false;
	}
}
