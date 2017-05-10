using UnityEngine;
using System.Collections.Generic;

namespace UniFigLib {

	/// <summary>
	/// ポリゴンのつながりで表されたボーン
	/// これ一つで一つのボーン
	/// </summary>
	public class PolygonBone {

		public enum Type {
			End,        //端点
			Relay       //中継点
		}

		private string _id;
		public string id { get { return _id; } }

		private Type _type;     //種類
		public Type type { get { return _type; } }

		private LinkedPolygon[] _polygons; //ポリゴンのつながり
		public LinkedPolygon[] polygons { get { return _polygons; } }

		private List<PolygonBone> _linkedBone;  //接続しているボーン
		public List<PolygonBone> linkedBone { get { return _linkedBone; } }

		public PolygonBone(string id) {
			_id = id;
			_linkedBone = new List<PolygonBone>();
		}

		#region Function

		/// <summary>
		/// ポリゴンのつながりの設定
		/// </summary>
		/// <param name="polygons">ポリゴンのつながり</param>
		public void SetPolygons(LinkedPolygon[] polygons) {
			_type = (polygons[0].link.Count == 1 || polygons[polygons.Length - 1].link.Count == 1) ? Type.End : Type.Relay;
			_polygons = polygons;
		}

		/// <summary>
		/// 隣接しているボーンの追加
		/// </summary>
		/// <param name="bone">隣接しているボーン</param>
		public void AddLinkedBone(PolygonBone bone) {
			_linkedBone.Add(bone);
		}

		/// <summary>
		/// ボーンの重心を返す
		/// </summary>
		/// <returns>重心</returns>
		public Vector3 Center() {
			var sum = Vector3.zero;
			sum += _polygons[0].baseCenter;
			sum += _polygons[_polygons.Length - 1].baseCenter;
			return sum / 2;
		}

		/// <summary>
		/// 関節を構成するポリゴンの重心の配列を返す
		/// </summary>
		/// <returns>重心の配列</returns>
		public Vector3[] Centers() {
			Vector3[] centers = new Vector3[_polygons.Length];
			for(int i = 0; i < _polygons.Length; ++i) {
				centers[i] = _polygons[i].baseCenter;
			}
			return centers;
		}

		/// <summary>
		/// 始点から終点までの距離を返す
		/// </summary>
		public float Distance() {
			return Vector3.Distance(_polygons[0].baseCenter, _polygons[_polygons.Length - 1].baseCenter);
		}

		/// <summary>
		/// 外側の端点の座標を返す
		/// </summary>
		/// <returns>外側の端点のリスト</returns>
		public List<Vector3> GetEnds() {
			if(type != Type.End) return null;
			List<Vector3> ends = new List<Vector3>();
			if(_polygons[0].link.Count < 3) {
				ends.Add(_polygons[0].baseCenter);
			}
			if(_polygons[_polygons.Length - 1].link.Count < 3) {
				ends.Add(_polygons[_polygons.Length - 1].baseCenter);
			}
			return ends;
		}

		#endregion
	}
}