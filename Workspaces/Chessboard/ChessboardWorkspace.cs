﻿using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VR.Utilities;

public class ChessboardWorkspace : Workspace
{
	private const float kGridScale = 1f;						//Scale grid cells because workspace is smaller than world
	private const float kClipBoxYOffset = 0.1333333f;			//1/3 of initial initial Y bounds (0.4)
	private const float kClipBoxInitScale = 25;					//We want to see a big region by default
	private const float kMinScale = 0.1f;
	private const float kMaxScale = 35;

	//NOTE: since pretty much all workspaces will want a prefab, should this go in the base class?
	[SerializeField]
	private GameObject m_ContentPrefab;
	[SerializeField]
	private GameObject m_UIPrefab;

	private MiniWorld m_MiniWorld;
	private ChessboardPrefab m_ChessboardPrefab;
	private Material m_GridMaterial;
	
	private readonly Dictionary<Transform, ControlData> m_ControlDatas = new Dictionary<Transform, ControlData>();

	private class ControlData
	{
		public Vector3 rayOriginStart;
		public Vector3 referenceTransformStart;
		public int order;
	}

	public override void Setup()
	{
		base.Setup();
		U.Object.InstantiateAndSetActive(m_ContentPrefab, m_WorkspacePrefab.sceneContainer, false);
		//Set up MiniWorld
		m_MiniWorld = GetComponentInChildren<MiniWorld>();
		m_MiniWorld.referenceTransform.position = Vector3.up * kClipBoxYOffset;
		m_MiniWorld.referenceTransform.localScale = Vector3.one * kClipBoxInitScale;
		m_ChessboardPrefab = GetComponentInChildren<ChessboardPrefab>();
		m_GridMaterial = m_ChessboardPrefab.grid.sharedMaterial;

		//Set up ControlBox
		//ControlBox shouldn't move with miniWorld
		var controlBox = m_ChessboardPrefab.controlBox;
		controlBox.parent = m_WorkspacePrefab.sceneContainer;
		controlBox.localPosition = Vector3.down * controlBox.localScale.y * 0.5f;
		m_ChessboardPrefab.OnControlDragStart = ControlDragStart;
		m_ChessboardPrefab.OnControlDrag = ControlDrag;
		m_ChessboardPrefab.OnControlDragEnd = ControlDragEnd;

		//Set up UI
		var UI = U.Object.InstantiateAndSetActive(m_UIPrefab, m_WorkspacePrefab.frontPanel, false);
		var chessboardUI = UI.GetComponentInChildren<ChessboardUI>();
		chessboardUI.OnZoomSlider = OnZoomSlider;
		chessboardUI.zoomSlider.maxValue = kMaxScale;
		chessboardUI.zoomSlider.minValue = kMinScale;
		chessboardUI.zoomSlider.value = kClipBoxInitScale;
		OnBoundsChanged();
	}

	private void Update()
	{
		//Set grid height, deactivate if out of bounds
		float gridHeight = m_MiniWorld.referenceTransform.position.y / m_MiniWorld.referenceTransform.localScale.y;
		if (Mathf.Abs(gridHeight) < contentBounds.extents.y)
		{
			m_ChessboardPrefab.grid.gameObject.SetActive(true);
			m_ChessboardPrefab.grid.transform.localPosition = Vector3.down * gridHeight;
		}
		else
		{
			m_ChessboardPrefab.grid.gameObject.SetActive(false);
		}

		//Update grid material if ClipBox has moved
		m_GridMaterial.mainTextureScale = new Vector2(
			m_MiniWorld.referenceTransform.localScale.x * contentBounds.size.x,
			m_MiniWorld.referenceTransform.localScale.z * contentBounds.size.z) * kGridScale;
		m_GridMaterial.mainTextureOffset =
			Vector2.one * 0.5f //Center grid
			+ new Vector2(m_GridMaterial.mainTextureScale.x % 2, m_GridMaterial.mainTextureScale.y % 2) * -0.5f //Scaling offset
			+ new Vector2(m_MiniWorld.referenceTransform.position.x, m_MiniWorld.referenceTransform.position.z) * kGridScale; //Translation offset
	}

	protected override void OnBoundsChanged()
	{
		m_MiniWorld.transform.localPosition = Vector3.up * contentBounds.extents.y;
		m_MiniWorld.SetBounds(contentBounds);

		m_ChessboardPrefab.grid.transform.localScale = new Vector3(contentBounds.size.x, contentBounds.size.z, 1);

		var controlBox = m_ChessboardPrefab.controlBox;
		controlBox.transform.localScale = new Vector3(contentBounds.size.x, controlBox.localScale.y, contentBounds.size.z);
	}

	private void OnZoomSlider(float value)
	{
		m_MiniWorld.referenceTransform.localScale = Vector3.one * value;
	}

	public void ControlDragStart(Transform controlBox, Transform rayOrigin)
	{
		m_ControlDatas[rayOrigin] = new ControlData { rayOriginStart = rayOrigin.position, referenceTransformStart = m_MiniWorld.referenceTransform.position , order = m_ControlDatas.Count };
	}

	public void ControlDrag(Transform controlBox, Transform rayOrigin)
	{
		switch (m_ControlDatas.Count)
		{
			case 1:
				//Translate
				var controlData = m_ControlDatas[rayOrigin];
				m_MiniWorld.referenceTransform.position = controlData.referenceTransformStart + Vector3.Scale(controlData.rayOriginStart - rayOrigin.transform.position, m_MiniWorld.referenceTransform.localScale);
				break;
			case 2:
				//Translate/Scale
				break;
		}
	}

	public void ControlDragEnd(Transform controlBox, Transform rayOrigin)
	{
		m_ControlDatas.Remove(rayOrigin);
	}
}
