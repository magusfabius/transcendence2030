using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChessVFXController : MonoBehaviour
{
	public Transform Transform;
	public float DistanceMinimum;
	public float DistanceMaximum;
	public float EmberAdditiveRadius;
	public float EmberStrength;
	public float EmberDistanceMinimum;
	public float EmberDistanceMaximum;

	[Range(0.0f, 1.0f)]
	public float GizmoAlpha;
}
