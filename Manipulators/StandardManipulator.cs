﻿using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.VR.Tools;
using UnityEditor.VR;
using UnityEngine.VR.Handles;
using UnityEngine.VR.Utilities;

public class StandardManipulator : MonoBehaviour, IManipulator
{
	[SerializeField]
	private Transform m_PlaneHandlesParent;

	[SerializeField]
	private List<BaseHandle> m_AllHandles;

	public bool dragging { get {  return m_Dragging; } }
	private bool m_Dragging;

	public Action<Vector3> translate { private get; set; }
	public Action<Quaternion> rotate { private get; set; }
	public Action<Vector3> scale { private get; set; }

	void OnEnable()
	{
		foreach (var h in m_AllHandles)
		{
			if (h is LinearHandle || h is PlaneHandle || h is SphereHandle)
				h.onHandleDrag += TranslateHandleOnDrag;

			if (h is RadialHandle)
				h.onHandleDrag += RotateHandleOnDrag;

			h.onHandleBeginDrag += HandleOnBeginDrag;
			h.onHandleEndDrag += HandleOnEndDrag;
		}
	}

	void OnDisable()
	{
		foreach (var h in m_AllHandles)
		{
			if (h is LinearHandle || h is PlaneHandle || h is SphereHandle)
				h.onHandleDrag -= TranslateHandleOnDrag;

			if (h is RadialHandle)
				h.onHandleDrag -= RotateHandleOnDrag;

			h.onHandleBeginDrag -= HandleOnBeginDrag;
			h.onHandleEndDrag -= HandleOnEndDrag;
		}
	}

	void Update()
	{
		if (!m_Dragging)
		{
			// Place the plane handles in a good location that is accessible to the user
			var viewerPosition = U.Camera.GetMainCamera().transform.position;
			foreach (Transform t in m_PlaneHandlesParent)
			{
				var localPos = t.localPosition;
				localPos.x = Mathf.Abs(localPos.x) * (transform.position.x < viewerPosition.x ? 1 : -1);
				localPos.y = Mathf.Abs(localPos.y) * (transform.position.y < viewerPosition.y ? 1 : -1);
				localPos.z = Mathf.Abs(localPos.z) * (transform.position.z < viewerPosition.z ? 1 : -1);
				t.localPosition = localPos;
			}
		}
	}

	private void TranslateHandleOnDrag(BaseHandle handle, HandleDragEventData eventData)
	{
		translate(eventData.deltaPosition);
	}

	private void RotateHandleOnDrag(BaseHandle handle, HandleDragEventData eventData)
	{
		rotate(eventData.deltaRotation);
	}

	private void HandleOnBeginDrag(BaseHandle handle, HandleDragEventData eventData)
	{
		foreach (var h in m_AllHandles)
			h.gameObject.SetActive(h == handle);

		m_Dragging = true;
	}

	private void HandleOnEndDrag(BaseHandle handle, HandleDragEventData eventData)
	{
		foreach (var h in m_AllHandles)
			h.gameObject.SetActive(true);

		m_Dragging = false;
	}
}