﻿using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;

namespace Lockstep.UI
{
	public class StatsBar : MonoBehaviour
	{
		[SerializeField]
		private BarElement
			_shield;
		[SerializeField]
		private BarElement
			_health;
		[SerializeField]
		private BarElement
			_energy;
		Vector3 Offset;

		public LSAgent TrackedAgent { get; private set; }

		void Awake ()
		{
			gameObject.SetActive (false);
		}

		public void Setup (LSAgent agent)
		{
			TrackedAgent = agent;
			GameObject.DontDestroyOnLoad (gameObject);
			Offset = agent.StatsBarOffset;
			this.gameObject.name = agent.ToString ();
		}

		private static StatType[] statTypes = (StatType[])Enum.GetValues (typeof(StatType));

		public void Initialize ()
		{
			gameObject.SetActive (true);
			foreach (StatType statType in statTypes) {
				SetFill (statType, 1f);
			}
			UpdatePos ();
			UpdateScale ();
		}

		public void Visualize ()
		{
			if (InterfaceManager.GUIManager.CameraChanged && TrackedAgent.IsVisible) {
				this.UpdatePos ();
				this.UpdateScale ();
			} else if (TrackedAgent.VisualPositionChanged) {
				this.UpdatePos ();
			}
		}

        static GUIManager GUIManager {get {return InterfaceManager.GUIManager;}}
		static Camera MainCam { get { return GUIManager.MainCam; } }

		static Vector3 screenPos;

		private void UpdatePos ()
		{
			if (GUIManager.CanHUD == false) return;
			screenPos = MainCam.WorldToScreenPoint (TrackedAgent.VisualCenter.position + Offset);
			transform.position = screenPos;
		}

		static Vector3 tempScale;

		private void UpdateScale ()
		{
			if (GUIManager.CanHUD == false) return;
			float scale = Mathf.Max (TrackedAgent.SelectionRadius / 1f, .1f);
			tempScale.x = GUIManager.CameraScale * scale;
			tempScale.y = GUIManager.CameraScale;
			transform.localScale = tempScale;
		}

		public void SetFill (StatType statType, float amount)
		{
			BarElement element = null;
			switch (statType) {
			case StatType.Shield:
				element = _shield;
				break;
			case StatType.Health:
				element = _health;
				break;
			case StatType.Energy:
				element = _energy;
				break;
			}
			if (TrackedAgent.IsVisible == false ||  (amount >= 1f && !GUIManager.ShowHealthWhenFull)) {

				element.gameObject.SetActiveIfNot (false);
				return;
			}
			{
				element.SetFill (amount);
				element.gameObject.SetActiveIfNot (true);
			}
		}

		public void Deactivate ()
		{
            if (gameObject == null) return;
				gameObject.SetActive (false);
		}
	}

	public enum StatType
	{
		Shield,
		Health,
		Energy
	}
}