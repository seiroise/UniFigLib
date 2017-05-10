using UnityEngine;
using System.Collections.Generic;

namespace UniFigLib {

	/// <summary>
	/// 連結ポリゴン
	/// </summary>
	public class LinkedPolygon {

		private Figure _figure;             //所属している図形

		private int[] _indices;             //頂点番号
		public int[] indices { get { return _indices; } }

		private List<LinkedPolygon> _link;  //連結ポリゴン
		public List<LinkedPolygon> link { get { return _link; } }

		private Vector3 _baseCenter;        //基準重心
		public Vector3 baseCenter { get { return _baseCenter; } }

		public LinkedPolygon(Figure figure, int i1, int i2, int i3) {
			_figure = figure;
			_indices = new int[] { i1, i2, i3 };
			_link = new List<LinkedPolygon>();
			_baseCenter = figure.GetCenter(i1, i2, i3);
		}

		/// <summary>
		/// 指定したポリゴンが接続できるか(隣接しているか)確認する
		/// </summary>
		/// <returns>隣接しているか</returns>
		/// <param name="polygon">隣接確認を行うポリゴン</param>
		public bool IsLinkable(LinkedPolygon polygon) {
			int c = 0;
			for(int i = 0; i < _indices.Length; ++i) {
				for(int j = 0; j < polygon.indices.Length; ++j) {
					if(_indices[i] == polygon.indices[j]) {
						c++;
						break;
					}
				}
			}
			return c == 2;
		}

		/// <summary>
		/// 指定した頂点番号の頂点の座標を返す
		/// </summary>
		/// <returns>頂点座標</returns>
		/// <param name="i">頂点番号</param>
		public Vector3 VertexPosition(int i) {
			return _figure.vertices[_indices[i]].pos;
		}
	}
}