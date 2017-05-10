using UnityEngine;
using System;

namespace UniFigLib {

	/// <summary>
	/// 三角形
	/// </summary>
	public class Triangle {

		private Vector3[] _positions;
		public Vector3[] posisions { get { return _positions; } }

		public Triangle(Vector3 a, Vector3 b, Vector3 c) {
			_positions = new Vector3[]{a, b, c};
		}

		/// <summary>
		/// 重心
		/// </summary>
		public Vector3 GetCenter() {
			return (_positions[0] + _positions[1] + _positions[2]) / 3f;
		}
	}
}