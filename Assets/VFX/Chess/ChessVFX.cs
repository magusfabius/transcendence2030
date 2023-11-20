using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class ChessVFX : MonoBehaviour
{
	private readonly int MAXIMUM_CONTROLLERS = 4;

	public ChessVFXController[] Controllers = null;
	public Material Material = null;

	private Vector4[] SpheresBurntPosition = null;
	private Vector4[] SpheresBurntProperties = null;
	private Vector4[] SpheresEmberProperties = null;

	/// <summary>
	/// Unity OnDrawGizmos.
	/// </summary>
	private void OnDrawGizmos()
	{
		if (Controllers != null)
		{
			for (int i = 0; i < Controllers.Length; ++i)
			{
				if (Controllers[i].Transform != null && Controllers[i].Transform.gameObject.activeSelf)
				{
					Color colorBurnt = i >= MAXIMUM_CONTROLLERS ? Color.red : Color.black;
					Color colorEmber = i >= MAXIMUM_CONTROLLERS ? Color.red : Color.yellow;
					colorBurnt.a = Controllers[i].GizmoAlpha;
					colorEmber.a = Controllers[i].GizmoAlpha;

					float radius = Mathf.Max(Controllers[i].Transform.localScale.x, Controllers[i].Transform.localScale.y, Controllers[i].Transform.localScale.z);
					Gizmos.color = colorBurnt;
					Gizmos.DrawWireSphere(Controllers[i].Transform.position, radius);
					Gizmos.color = colorEmber;
					Gizmos.DrawWireSphere(Controllers[i].Transform.position, radius + Controllers[i].EmberAdditiveRadius);

					colorBurnt.a *= 0.5f;
					colorEmber.a *= 0.5f;
					Gizmos.color = colorBurnt;
					Gizmos.DrawSphere(Controllers[i].Transform.position, radius);
					Gizmos.color = colorEmber;
					Gizmos.DrawSphere(Controllers[i].Transform.position, radius + Controllers[i].EmberAdditiveRadius);
				}
			}
		}
	}

	/// <summary>
	/// Unity Update.
	/// </summary>
	private void Update()
    {
		if (Material != null)
		{
			if (Controllers != null)
			{
				// TODO: Use structured buffer and material property block
				if (SpheresBurntPosition == null || SpheresBurntPosition.Length != MAXIMUM_CONTROLLERS)
				{ SpheresBurntPosition = new Vector4[MAXIMUM_CONTROLLERS]; }
				if (SpheresBurntProperties == null || SpheresBurntProperties.Length != MAXIMUM_CONTROLLERS)
				{ SpheresBurntProperties = new Vector4[MAXIMUM_CONTROLLERS]; }
				if (SpheresEmberProperties == null || SpheresEmberProperties.Length != MAXIMUM_CONTROLLERS)
				{ SpheresEmberProperties = new Vector4[MAXIMUM_CONTROLLERS]; }

				int count = 0;
				// TODO: Filling array during runtime is not needed
				for (int i = 0; i < Controllers.Length; ++i)
				{
					if (i >= MAXIMUM_CONTROLLERS)
					{ break; }

					if (Controllers[i].Transform != null && Controllers[i].Transform.gameObject.activeSelf)
					{
						SpheresBurntPosition[i] = Controllers[i].Transform.position;
						float radius = Mathf.Max(Controllers[i].Transform.localScale.x, Controllers[i].Transform.localScale.y, Controllers[i].Transform.localScale.z);
						SpheresBurntProperties[i] = new Vector4(radius, Controllers[i].DistanceMinimum, Controllers[i].DistanceMaximum, 0.0f);

						radius += Controllers[i].EmberAdditiveRadius;
						SpheresEmberProperties[i] = new Vector4(radius, Controllers[i].EmberStrength, Controllers[i].EmberDistanceMinimum, Controllers[i].EmberDistanceMaximum);
					}
					else
					{
						SpheresBurntPosition[i] = Vector3.zero;
						SpheresBurntProperties[i] = new Vector4(0.0f, 0.0f, 0.0f, 0.0f);

					}
					count++;
				}
				Material.SetFloat("_BurntSpheresCount", count);
				Material.SetVectorArray("_BurntSpheresPosition", SpheresBurntPosition);
				Material.SetVectorArray("_BurntSpheresProperties1", SpheresBurntProperties);
				Material.SetVectorArray("_BurntSpheresProperties2", SpheresEmberProperties);
			}
		}
	}
}
