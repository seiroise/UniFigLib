using UnityEngine;
using System;
using System.Collections;

namespace UniFigLib {
	/// <summary>
	/// 頂点情報とその点がなす角度とフラグ情報
	/// </summary>
	[Serializable]
	public class Vertex {

		private Vector3 _pos;       //頂点座標
		public Vector3 pos { get { return _pos; } set { _pos = value; } }
		private float _angle;       //前後の頂点とのなす角
		public float angle { get { return _angle; } set { _angle = value; } }
		private bool _isActive;   	//有効かどうか	
		public bool isActive { get { return _isActive; } set { _isActive = value; } }

		public Vertex(Vector3 pos, float angle) {
			this._pos = pos;
			this._angle = angle;
			this._isActive = true;
		}
	}
}