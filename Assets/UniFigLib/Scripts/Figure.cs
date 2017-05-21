using UnityEngine;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

namespace UniFigLib {

	/// <summary>
	/// 図形
	/// </summary>
	[Serializable]
	public class Figure {

		private Vertex[] _vertices;     //頂点情報
		private int[] _indices;       //三角形情報
		private Color[] _colors;        //色情報
		private int _polyNum;           //ポリゴン数

		public Vertex[] vertices { get { return _vertices; } }
		public Vector3[] positions { get { return _vertices.Select(v => v.pos).ToArray(); } }
		public int[] indices { get { return _indices; } }
		public Color[] colors { get { return _colors; } set { _colors = value; } }
		public int polyNum { get { return _polyNum; } }

		#region Constructor

		private Figure(Vertex[] vertices, int[] indices, Color[] colors) {
			this._vertices = vertices;
			this._colors = colors;
			this._indices = indices;
			this._polyNum = indices.Length / 3;
		}

		#endregion

		#region Static Function

		/// <summary>
		/// 頂点リストを指定して新しくFigureデータを作成する。
		/// 入力座標データは始点と終点つながっていない必要があり,
		/// 失敗した場合にも入力座標データは加工される
		/// </summary>
		public static Figure FromPositions(List<Vector3> positions, Color color) {
			Vector3 temp;
			//平面に落とし込む
			for(int i = 0; i < positions.Count; i++) {
				temp = positions[i];
				temp.z = 0f;
				positions[i] = temp;
			}
			//座標配列に連続して重複する点がないか確認する
			CheckRemovePosition(positions);
			//この時点で要素数が2以下か確認
			if(positions.Count <= 2)
				throw new ArgumentException("positions is no match");
			//座標配列の変換する
			var vertices = CreateVertices(positions.ToArray());
			//頂点配列の三角形分割
			var triangles = Triangulation(vertices);
			//色情報を頂点数分の配列に格納する
			Color[] colors = new Color[vertices.Length];
			for(int i = 0; i < colors.Length; ++i) colors[i] = color;
			return new Figure(vertices, triangles, colors);
		}

		/// <summary>
		/// 頂点リストを指定して新しくFigureデータを作成する。
		/// 入力座標データは始点と終点つながっていない必要がある。
		/// </summary>
		public static Figure FromPositions(Vector3[] positions, Color color) {
			return FromPositions(new List<Vector3>(positions), color);
		}

		/// <summary>
		/// 頂点リストを指定して新しくFigureデータを作成する。
		/// 入力座標データは始点と終点つながっていない必要がある。
		/// </summary>
		public static Figure FromPositions(List<Vector2> positions, Color color) {
			List<Vector3> temp = new List<Vector3>();
			for(int i = 0; i < positions.Count; ++i) {
				temp.Add(positions[i]);
			}
			return FromPositions(temp, color);
		}

		/// <summary>
		/// 頂点リストを指定して新しくFigureデータを作成する。
		/// 入力座標データは始点と終点つながっていない必要がある。
		/// </summary>
		public static Figure FromPositions(Vector2[] positions, Color color) {
			List<Vector3> temp = new List<Vector3>();
			for(int i = 0; i < positions.Length; ++i) {
				temp.Add(positions[i]);
			}
			return FromPositions(temp, color);
		}

		/// <summary>
		/// 座標リストの連続して重複する点を削除する。
		/// </summary>
		private static void CheckRemovePosition(List<Vector3> positions) {
			Vector3 p = positions[positions.Count - 1];
			for(int i = positions.Count - 2; i >= 0; i--) {
				//座標が同じか確認する
				if(p == positions[i]) {
					//同じだった場合はそのインデックスを削除する
					positions.RemoveAt(i);
				} else {
					//違う場合は新しく比較対象に追加する
					p = positions[i];
				}
			}
		}

		/// <summary>
		/// 頂点座標リストに対して前後の頂点とのなす角度などのデータを付加する
		/// </summary>
		private static Vertex[] CreateVertices(Vector3[] positions) {
			//最も遠い頂点からの前後の点へのベクトルを用いて内向き外積の向きを求める
			int farthest = FindFarthestIndexFromOrigine(positions);
			int next, prev;
			FindPrevNextIndex(farthest, positions, out prev, out next);
			Vector3 prevVector, nextVector;
			prevVector = positions[prev] - positions[farthest];
			nextVector = positions[next] - positions[farthest];
			Vector3 farthestCross = Vector3.Cross(nextVector, prevVector);  //外積を求める

			//頂点を順に調べて前後の頂点とのなす角度を求め頂点配列に格納する
			var vertices = new List<Vertex>();
			for(int i = 0; i < positions.Length; i++) {
				FindPrevNextIndex(i, positions, out prev, out next);
				prevVector = positions[prev] - positions[i];
				nextVector = positions[next] - positions[i];
				Vector3 cross = Vector3.Cross(nextVector, prevVector);  //外積を求める

				//内積から角度を求める(nextVector * prevVector)
				//距離
				float prevVecDis = Vector3.Distance(positions[prev], positions[i]);
				float nextVecDis = Vector3.Distance(positions[next], positions[i]);
				//角度
				float cosS = Vector3.Dot(nextVector, prevVector) / (nextVecDis * prevVecDis);
				float degree = Mathf.Acos(cosS) * Mathf.Rad2Deg;

				//角度が180°の場合頂点としては意味を成さないので除外する
				if(degree != 180f) {
					//外積の向きが異なる場合360°から引く
					if(!CrossDirection(farthestCross, cross)) {
						degree = 360f - degree;
					}
					vertices.Add(new Vertex(positions[i], degree));
				}
			}
			return vertices.ToArray();
		}

		/// <summary>
		/// 指定した頂点群の作る多角形の三角形分割 その1
		/// </summary>
		private static int[] Triangulation(Vertex[] vertices) {
			//下準備
			int polyNum = vertices.Length - 2;              //生成物のポリゴン数
			int[] triangles = new int[polyNum * 3];         //何角形 * 3
			int farthest, prev, next;
			Vector3 prevVector, nextVector, prevPos, nextPos, farthestPos;
			Vector3 farthestCross, cross;
			bool exist;                                     //三角形内に頂点が存在

			for(int polyIndex = 0; polyIndex < polyNum; polyIndex++) {
				farthest = FindFarthestActiveIndexFromOrigin(vertices);

				FindPrevNextActiveIndex(farthest, vertices, out prev, out next);
				farthestPos = vertices[farthest].pos;
				prevPos = vertices[prev].pos;
				nextPos = vertices[next].pos;

				//前後の頂点となす三角形内に他の頂点が存在するか確認する
				exist = false;
				for(int j = 0; j < vertices.Length; j++) {
					if(j != prev && j != farthest && j != next) {
						if(PointOnTriangle(prevPos, farthestPos, nextPos, vertices[j].pos)) {
							exist = true;
							break;
						}
					}
				}
				if(!exist) {
					//三角形を構成するための順序付けを時計回り/反時計回りを考慮して配列に格納する
					if(CheckCW(prevPos, farthestPos, nextPos) > 0f) {
						triangles[polyIndex * 3 + 0] = next;
						triangles[polyIndex * 3 + 1] = farthest;
						triangles[polyIndex * 3 + 2] = prev;
					} else {
						triangles[polyIndex * 3 + 0] = prev;
						triangles[polyIndex * 3 + 1] = farthest;
						triangles[polyIndex * 3 + 2] = next;
					}
					vertices[farthest].isActive = false;
				} else {
					//外積を求める
					prevVector = prevPos - farthestPos;
					nextVector = nextPos - farthestPos;
					farthestCross = Vector3.Cross(nextVector, prevVector);
					for(int i = 0; i < polyNum;	++i) {
						//隣に移動
						farthest = next;
						//前後の頂点から外積を求める
						FindPrevNextActiveIndex(farthest, vertices, out prev, out next);
						farthestPos = vertices[farthest].pos;
						prevPos = vertices[prev].pos;
						nextPos = vertices[next].pos;
						prevVector = prevPos - farthestPos;
						nextVector = nextPos - farthestPos;
						cross = Vector3.Cross(nextVector, prevVector);

						//外積の向きがあっているか確認
						if(!CrossDirection(farthestCross, cross)) {
							continue;
						}

						//前後の頂点となす三角形内に他の頂点が存在するか確認する
						exist = false;
						for(int j = 0; j < vertices.Length; j++) {
							if(j != prev && j != farthest && j != next) {
								if(PointOnTriangle(prevPos, farthestPos, nextPos, vertices[j].pos)) {
									exist = true;
									break;
								}
							}
						}
						if(!exist) {
							//三角形を構成するための順序付けを時計回り/反時計回りを考慮して配列に格納する
							if(CheckCW(prevPos, farthestPos, nextPos) > 0f) {
								triangles[polyIndex * 3 + 0] = next;
								triangles[polyIndex * 3 + 1] = farthest;
								triangles[polyIndex * 3 + 2] = prev;
							} else {
								triangles[polyIndex * 3 + 0] = prev;
								triangles[polyIndex * 3 + 1] = farthest;
								triangles[polyIndex * 3 + 2] = next;
							}
							vertices[farthest].isActive = false;
							break;
						}
					}
				}
			}

			return triangles;
		}

		/// <summary>
		/// 前後の座標の番号を返す
		/// </summary>
		private static void FindPrevNextIndex(int now, Vector3[] positions, out int prev, out int next) {
			prev = next = now;
			if(positions.Length == 0)
				throw new ArgumentException("array is 0 elems");

			int len = positions.Length;
			prev = (now + len - 1) % len;
			next = (now + 1) % len;
		}

		/// <summary>
		/// 前後の有効な頂点の番号を返す
		/// </summary>
		private static void FindPrevNextActiveIndex(int now, Vertex[] vertices, out int prev, out int next) {
			prev = next = now;
			if(vertices.Length == 0)
				throw new ArgumentException("array is 0 elems");

			int len = vertices.Length;
			int n = now;
			for(int i = 0; i < len - 1; ++i) {
				n = (n + len - 1) % len;
				if(vertices[n].isActive) {
					prev = n;
					break;
				}
			}
			n = now;
			for(int i = 0; i < len - 1; ++i) {
				n = (n + 1) % len;
				if(vertices[n].isActive) {
					next = n;
					break;
				}
			}
		}

		/// <summary>
		/// 次の有効な頂点の番号を返す
		/// </summary>
		private static int FindNextActiveIndex(int now, Vertex[] vertices) {
			if(vertices.Length == 0)
				throw new ArgumentException("array is 0 elems");

			int len = vertices.Length;
			int n = now;
			for(int i = 0; i < len; ++i) {
				if(vertices[n = (n + i) % len].isActive) return n;
			}
			throw new ArgumentException("all vertices is nonActive");
		}

		/// <summary>
		/// 原点から最も遠い座標の番号を求める
		/// </summary>
		private static int FindFarthestIndexFromOrigine(Vector3[] positions) {
			if(positions.Length == 0)
				throw new ArgumentException("array is 0 elems");
			int index = -1;             //一番遠い頂点の番号
			float aDis = 0f, bDis = 0f; //一番遠い頂点までの距離を記憶する
			for(int i = 0; i < positions.Length; i++) {
				bDis = Vector3.Distance(Vector3.zero, positions[i]);
				if(bDis > aDis) {
					aDis = bDis;
					index = i;
				}
			}
			return index;
		}

		/// <summary>
		/// 原点から最も遠い有効な頂点の番号を求める
		/// </summary>
		private static int FindFarthestActiveIndexFromOrigin(Vertex[] vertices) {
			if(vertices.Length == 0)
				throw new ArgumentException("array is 0 elems");
			int index = -1;             //一番遠い頂点の番号
			float aDis = 0f, bDis = 0f; //一番遠い頂点までの距離を記憶する
			for(int i = 0; i < vertices.Length; i++) {
				if(!vertices[i].isActive) continue;
				bDis = Vector3.Distance(Vector3.zero, vertices[i].pos);
				if(bDis > aDis) {
					aDis = bDis;
					index = i;
				}
			}
			return index;
		}

		/// <summary>
		/// 外積の方向が正しいか判定する
		/// </summary>
		private static bool CrossDirection(Vector3 c1, Vector3 c2) {
			//おそらくだけどこれはz方向の確認だけでいいと思う
			//x方向
			return (
				(c1.x > 0 && c2.x > 0 || c1.x <= 0 && c2.x <= 0) &&
				(c1.y > 0 && c2.y > 0 || c1.y <= 0 && c2.y <= 0) &&
				(c1.z > 0 && c2.z > 0 || c1.z <= 0 && c2.z <= 0)
			);
		}

		/// <summary>
		/// 3点t1,t2,t3のなす三角形の内部に点pが存在するか確認する
		/// </summary>
		private static bool PointOnTriangle(Vector3 t1, Vector3 t2, Vector3 t3, Vector3 p) {
			float z1, z2, z3;
			z1 = (t3.x - t2.x) * (p.y - t2.y) - (t3.y - t2.y) * (p.x - t2.x);
			z2 = (t1.x - t3.x) * (p.y - t3.y) - (t1.y - t3.y) * (p.x - t3.x);
			z3 = (t2.x - t1.x) * (p.y - t1.y) - (t2.y - t1.y) * (p.x - t1.x);
			//同じ方向(正負を向いているか確認する),(=をつけると線分上の点の場合もかぶっているかの判定になる)
			return (z1 >= 0 && z2 >= 0 && z3 >= 0 || z1 <= 0 && z2 <= 0 && z3 <= 0);
		}

		/// <summary>
		/// 2軸(xy)の外積
		/// </summary>
		private static float Cross(Vector3 v1, Vector3 v2) {
			return v1.x * v2.y - v2.x * v1.y;
		}

		/// <summary>
		/// p1 -> p2 -> p3の時計/反時計周り判定
		/// 戻り値が正で時計回り?
		/// </summary>
		private static float CheckCW(Vector3 p1, Vector3 p2, Vector3 p3) {
			return Cross(p2 - p1, p3 - p2);
		}

		#endregion

		#region Function

		/// <summary>
		/// Meshに変換
		/// </summary>
		public Mesh ToMesh() {
			var mesh = new Mesh();
			mesh.vertices = positions;
			mesh.colors = _colors;
			mesh.SetIndices(indices, MeshTopology.Triangles, 0);

			mesh.Optimize();
			mesh.RecalculateNormals();
			mesh.RecalculateBounds();

			return mesh;
		}

		/// <summary>
		/// 指定した番号のポリゴンまでを格納したメッシュに変換
		/// </summary>
		public Mesh ToMesh(int i) {
			if(i <= 0 && i > _polyNum)
				throw new ArgumentOutOfRangeException("i is 0 < i <= polyNum");

			var mesh = new Mesh();
			mesh.vertices = positions;
			mesh.colors = _colors;
			mesh.SetIndices(indices.Take(i * 3).ToArray(), MeshTopology.Triangles, 0);

			mesh.Optimize();
			mesh.RecalculateNormals();
			mesh.RecalculateBounds();

			return mesh;
		}

		/// <summary>
		/// 三角形情報へのイテレータ
		/// </summary>
		public void IterateTriangle(Action<int, Triangle> action) {
			for(int i = 0, index = 0; i < _indices.Length; i += 3, ++index) {
				action(index, new Triangle(
					_vertices[_indices[i]].pos,
					_vertices[_indices[i + 1]].pos,
					_vertices[_indices[i + 2]].pos));
			}
		}

		/// <summary>
		/// 順序情報へのイテレータ
		/// actionの引数はポリゴン番号, 頂点番号1, 頂点番号2, 頂点番号3 の順番
		/// </summary>
		public void IterateIndices(Action<int, int, int, int> action) {
			for(int i = 0, p = 0; i < _indices.Length; i += 3, ++p) {
				action(p, _indices[i], _indices[i + 1], _indices[i + 2]);
			}
		}

		/// <summary>
		/// 順々に表示
		/// </summary>
		public IEnumerator MeshAnimation(float time, MeshFilter filter, Action<Figure> onEnd = null) {
			float interval = time / _polyNum;
			for(int i = 1; i <= _polyNum; ++i) {
				filter.mesh = ToMesh(i);
				yield return new WaitForSeconds(interval);
			}
			onEnd(this);
		}

		/// <summary>
		/// 指定した番号の三角形を構成している頂点番号を取得する
		/// </summary>
		public int[] GetTriangleIndices(int i) {
			return new int[]{
				_indices[i * 3],
				_indices[i * 3 + 1],
				_indices[i * 3 + 2]};
		}

		/// <summary>
		/// 指定した頂点番号から構成される三角形の重心を返す
		/// </summary>
		public Vector3 GetCenter(int i1, int i2, int i3) {
			return (positions[i1] + positions[i2] + positions[i3]) / 3f;
		}

		#endregion
	}
}