using UnityEngine;
using System.Collections;

namespace UniFigLib.Utility {

	/// <summary>
	/// 線分
	/// </summary>
	public class LineSeg {

		private Vector3 _a; //点1
		public Vector3 a { get { return _a; } }
		private Vector3 _b; //点2
		public Vector3 b { get { return _b; } }
		private float _distance;	//ab間の距離
		public float distance { get { return _distance; } }

		public LineSeg(Vector3 a, Vector3 b) {
			Reset(a, b);
		}

		/// <summary>
		/// 指定した座標との最短距離を返す
		/// </summary>
		/// <param name="p">座標</param>
		public float Distance(Vector3 p) {
			Vector3 ap = p - _a;
			Vector3 ab = _b - _a;
			if(Vector3.Dot(ab, ap) < 0f) return Vector3.Distance(_a, p);
			if(Vector3.Dot(_a - _b, p - _b) < 0f) return Vector3.Distance(_b, p);
			return Vector3.Cross(ab, ap).magnitude / _distance;
		}

		/// <summary>
		/// 値の再設定を行う
		/// </summary>
		/// <param name="a">座標a</param>
		/// <param name="b">座標b</param>
		public void Reset(Vector3 a, Vector3 b) {
			_a = a;
			_b = b;
			_distance = Vector3.Distance(_a, _b);
		}
	}
}