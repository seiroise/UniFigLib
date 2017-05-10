using UnityEngine;
using System.Linq;
using System.Collections.Generic;

namespace UniFigLib {

	/// <summary>
	/// 図形を動かすための骨格テーだ
	/// </summary>
	public class FigureFrame {

		/// <summary>
		/// 探索時の一時データクラス
		/// </summary>
		private class TempExpData {
			public LinkedPolygon polygon;   //探索ポリゴン
			public int expIndex;            //探索先の隣接ポリゴン番号
			public PolygonBone bone;        //このデータに対応するボーン

			public TempExpData(LinkedPolygon polygon, int expIndex, PolygonBone relayBone) {
				this.polygon = polygon;
				this.expIndex = expIndex;
				this.bone = relayBone;
			}
		}

		/// <summary>
		/// 頂点へのボーンの紐付けを行う一時データ
		/// </summary>
		private class TempBoneData {
			private List<int> _boneIndices;     //ボーン番号

			public TempBoneData() {
				_boneIndices = new List<int>();
			}

			public void AddIndex(int index) {
				_boneIndices.Add(index);
			}

			public void DebugPrint() {
				Debug.LogWarning(string.Format("#### {0} ####", this));
				for(int i = 0; i < _boneIndices.Count; ++i) {
					Debug.Log(string.Format("{0}: bone {1}", i, _boneIndices[i]));
				}
			}

			/// <summary>
			/// BoneWeightクラスへの変換
			/// </summary>
			/// <returns>ボーンのウェイト</returns>
			public BoneWeight ToBoneWeight() {
				if(_boneIndices.Count < 1) {
					throw new System.Exception("ボーンのウェイト設定数が0です");
				}
				//同じボーンに対してのウェイトは取り除く
				_boneIndices = new List<int>(_boneIndices.Distinct());
				//単純にBoneWeightに置き換えるだけ
				if(_boneIndices.Count == 1) {
					return ToBoneWeight1();
				} else if(_boneIndices.Count == 2) {
					return ToBoneWeight2();
				} else if(_boneIndices.Count == 3) {
					return ToBoneWeight3();
				} else {
					return ToBoneWeight4();
				}
			}

			/// <summary>
			/// 一つの紐付けを持つ場合
			/// </summary>
			/// <returns>変換したBoneWeightクラス</returns>
			private BoneWeight ToBoneWeight1() {
				//単純にBoneWeightに置き換えるだけ
				var boneWeight = new BoneWeight();
				boneWeight.boneIndex0 = _boneIndices[0];
				//重みの合計を1にする
				boneWeight.weight0 = 1f;

				return boneWeight;
			}

			/// <summary>
			/// 二つの紐付けを持つ場合
			/// </summary>
			/// <returns>変換したBoneWeightクラス</returns>
			private BoneWeight ToBoneWeight2() {
				//単純にBoneWeightに置き換えるだけ
				var boneWeight = new BoneWeight();
				boneWeight.boneIndex0 = _boneIndices[0];
				boneWeight.boneIndex1 = _boneIndices[1];
				//重みの合計を1にする
				boneWeight.weight0 = 0.5f;
				boneWeight.weight1 = 0.5f;
				return boneWeight;
			}

			/// <summary>
			/// 三つの紐付けを持つ場合
			/// </summary>
			/// <returns>変換したBoneWeightクラス</returns>
			private BoneWeight ToBoneWeight3() {
				//単純にBoneWeightに置き換えるだけ
				var boneWeight = new BoneWeight();
				boneWeight.boneIndex0 = _boneIndices[0];
				boneWeight.boneIndex1 = _boneIndices[1];
				boneWeight.boneIndex2 = _boneIndices[2];
				//重みの合計を1にする
				boneWeight.weight0 = 0.33f;
				boneWeight.weight1 = 0.33f;
				boneWeight.weight2 = 0.33f;
				return boneWeight;
			}

			/// <summary>
			/// 四つ以上の紐付けを持つ場合
			/// </summary>
			/// <returns>変換したBoneWeightクラス</returns>
			private BoneWeight ToBoneWeight4() {
				//単純にBoneWeightに置き換えるだけ
				var boneWeight = new BoneWeight();
				boneWeight.boneIndex0 = _boneIndices[0];
				boneWeight.boneIndex1 = _boneIndices[1];
				boneWeight.boneIndex2 = _boneIndices[2];
				boneWeight.boneIndex3 = _boneIndices[3];
				//重みの合計を1にする
				boneWeight.weight0 = 0.25f;
				boneWeight.weight1 = 0.25f;
				boneWeight.weight2 = 0.25f;
				boneWeight.weight3 = 0.25f;
				return boneWeight;
			}
		}

		/// <summary>
		/// ボーンのツリー
		/// </summary>
		private class BoneTree {

			public BoneTree parent;         //親
			public PolygonBone bone;        //ボーン
			public List<BoneTree> children; //子供

			public BoneTree(BoneTree parent, PolygonBone bone, List<BoneTree> children) {
				this.parent = parent;
				this.bone = bone;
				this.children = children;
			}

			/// <summary>
			/// ツリー構造の出力
			/// </summary>
			public void PrintTree() {
				Debug.Log("<<<< Tree Structure >>>>");
				RePrintTree("", this);
			}

			/// <summary>
			/// ツリー構造出力用の再帰関数
			/// </summary>
			/// <param name="parentStr">親からの文字列</param>
			private void RePrintTree(string parentStr, BoneTree tree) {
				string str = string.Format("{0}/{1}", parentStr, tree.bone.id);
				Debug.Log(str);
				foreach(var b in tree.children) {
					RePrintTree(str, b);
				}
			}
		}

		/// <summary>
		/// ツリー状のTransformを構築するための一時データ
		/// </summary>
		private class TempExpTree {
			public Transform parent;
			public BoneTree tree;

			public TempExpTree(Transform parent, BoneTree tree) {
				this.parent = parent;
				this.tree = tree;
			}
		}

		private Figure _figure;
		private Vector3[] _centers;
		public Vector3[] centers { get { return _centers; } }
		private LinkedPolygon[] _polygons;
		public LinkedPolygon[] polygons { get { return _polygons; } }
		private PolygonBone[] _bones;
		public PolygonBone[] bone { get { return _bones; } }
		private BoneTree _rootBone;

		private FigureFrame(Figure figure, Vector3[] centers, LinkedPolygon[] polygons, PolygonBone[] bones, BoneTree rootBone) {
			_figure = figure;
			_centers = centers;
			_polygons = polygons;
			_bones = bones;
			_rootBone = rootBone;
		}

		#region Static Function

		/// <summary>
		/// 図形データから骨格データを作成する
		/// </summary>
		/// <returns>骨格データ</returns>
		/// <param name="figure">骨格データの作成素図形データ</param>
		public static FigureFrame FromFigure(Figure figure) {
			Vector3[] centers = new Vector3[figure.polyNum];
			figure.IterateTriangle((i, t) => {
				centers[i] = t.GetCenter();
			});
			LinkedPolygon[] polygons = MakeLinkedPolygon(figure);
			PolygonBone[] bones = MakePolygonBones(polygons);
			var root = MakeBoneTree(FindBigBone(bones));
			return new FigureFrame(figure, centers, polygons, bones, root);
		}

		/// <summary>
		/// 連結ポリゴンの作成
		/// </summary>
		private static LinkedPolygon[] MakeLinkedPolygon(Figure figure) {
			LinkedPolygon[] polygons = new LinkedPolygon[figure.polyNum];
			figure.IterateIndices((index, i1, i2, i3) => {
				polygons[index] = new LinkedPolygon(figure, i1, i2, i3);
			});
			foreach(var p in polygons) {
				ConnectLink(p, polygons);
			}
			return polygons;
		}

		/// <summary>
		/// 指定したポリゴンに接続するポリゴンを探す
		/// </summary>
		private static void ConnectLink(LinkedPolygon p, LinkedPolygon[] polygons) {
			int c = 0;
			for(int i = 0; i < polygons.Length && c < 3; ++i) {
				if(p.IsLinkable(polygons[i])) {
					p.link.Add(polygons[i]);
					c++;
				}
			}
		}

		/// <summary>
		/// 連結ポリゴンを分岐ごとに分けたボーンの配列を返す
		/// </summary>
		private static PolygonBone[] MakePolygonBones(LinkedPolygon[] polygons) {
			//探索結果
			List<PolygonBone> bones = new List<PolygonBone>();
			//構築中の連結
			List<LinkedPolygon> chain;
			//探索済み
			HashSet<LinkedPolygon> exploered = new HashSet<LinkedPolygon>();
			//探索中(探索の始点ポリゴンと探索方向インデックス)
			Queue<TempExpData> exploring = new Queue<TempExpData>();
			//未探索隣接番号
			List<int> nonExpIndices = new List<int>();

			//始点の追加(分岐方向が一つしかないもの(末端)を選ぶ)(1以下にしているのはただの三角形を想定して)
			foreach(var p in polygons) {
				if(p.link.Count <= 1) {
					var b = new TempExpData(p, 0, new PolygonBone(System.Guid.NewGuid().ToString()));
					exploring.Enqueue(b);
					break;
				}
			}

			TempExpData start;
			while(exploring.Count > 0) {
				start = exploring.Dequeue();
				chain = new List<LinkedPolygon>();
				chain.Add(start.polygon);
				exploered.Add(start.polygon);
				//分岐を見つけるまで探索を行う
				LinkedPolygon exp = start.polygon.link[start.expIndex];
				while(true) {
					chain.Add(exp);
					exploered.Add(exp);
					//有効な隣接ポリゴン(探索済みでない)の数を数える
					int c = 0;
					nonExpIndices.Clear();
					for(int i = 0; i < exp.link.Count; ++i) {
						if(exploered.Contains(exp.link[i])) continue;
						++c;
						nonExpIndices.Add(i);
					}

					//分岐確認
					if(c == 0) {
						//端(ここで終了)
						var bone = start.bone;
						bone.SetPolygons(chain.ToArray());
						bones.Add(bone);
						break;
					} else if(c == 1) {
						//分岐なし(探索済みでない方向に進む)
						exp = exp.link[nonExpIndices[0]];
					} else {
						//分岐あり(それぞれの方向に伸びる。いままでのは一旦ここで終了)
						var bone = start.bone;
						bone.SetPolygons(chain.ToArray());
						bones.Add(bone);
						//接続関係の構築とキューへの追加
						var linkedBones = new PolygonBone[nonExpIndices.Count];
						for(int i = 0; i < linkedBones.Length; ++i) {
							linkedBones[i] = new PolygonBone(System.Guid.NewGuid().ToString());
							bone.AddLinkedBone(linkedBones[i]);
							exploring.Enqueue(new TempExpData(exp, nonExpIndices[i], linkedBones[i]));
						}
						//隣接ボーンの接続関係の構築
						for(int i = 0; i < linkedBones.Length; ++i) {
							linkedBones[i].AddLinkedBone(bone);
							for(int j = 0; j < linkedBones.Length; ++j) {
								if(i == j) continue;
								linkedBones[i].AddLinkedBone(linkedBones[j]);
							}
						}
						break;
					}
				}
			}
			return bones.ToArray();
		}

		/// <summary>
		/// ボーン構造のルートツリーを探索する
		/// </summary>
		/// <returns>ルートツリー</returns>
		/// <param name="bones">入力ボーン</param>
		private static BoneTree FindRoot(PolygonBone[] bones) {
			//準備
			HashSet<PolygonBone> relays = new HashSet<PolygonBone>();
			HashSet<PolygonBone> relayTemps = new HashSet<PolygonBone>();
			HashSet<PolygonBone> exploerd = new HashSet<PolygonBone>();
			BoneTree root;

			//まずすべての端ボーンを見つけそれに隣接している中継ボーンのセットを作成する
			foreach(var b in bones) {
				if(b.type == PolygonBone.Type.End) {
					relays.Add(b);
					exploerd.Add(b);
				}
			}
			//中継ボーンが
			while(true) {
				Debug.Log(string.Format("############ relays {0} ############", relays.Count));
				//中継ボーンの接続しているボーンからさらに中継ボーンを検索する
				foreach(var b in relays) {
					Debug.Log("-------------------------------");
					Debug.Log(string.Format("children: {0}", b.id));
					exploerd.Add(b);
					foreach(var lb in b.linkedBone) {
						Debug.Log(string.Format("relay: {0}", lb.id));
						if(lb.type == PolygonBone.Type.Relay && !exploerd.Contains(lb)) {
							Debug.Log("○");
							relayTemps.Add(lb);
						} else {
							Debug.Log("✖︎");
						}
					}
				}
				if(relayTemps.Count == 0) {
					Debug.Log("## Break ##");
					break;
				} else {
					//検索したデータを次の一時データへ
					relays = new HashSet<PolygonBone>(relayTemps.Select((x => x)));
					relayTemps.Clear();
				}
			}

			//ルートツリーを作成する子データを設定する
			List<BoneTree> children = new List<BoneTree>();
			if(relays.Count == 1) {
				root = new BoneTree(null, new PolygonBone(System.Guid.NewGuid().ToString()), null);
				children.Add(new BoneTree(root, relays.ToArray()[0], new List<BoneTree>()));
				Debug.Log("Root Children: " + relays.ToArray()[0].id);
			} else {
				root = new BoneTree(null, new PolygonBone("three top root"), null);
				foreach(var b in relays) {
					children.Add(new BoneTree(root, b, new List<BoneTree>()));
					root.bone.linkedBone.Add(b);
					Debug.Log("Root Children: " + b.id);
				}
			}
			root.children = children;
			return root;
		}

		/// <summary>
		/// ボーン構造から最も大きい(長い)ボーンを取得する
		/// </summary>
		/// <returns>ルートボーン</returns>
		/// <param name="bones">入力ボーン</param>
		private static BoneTree FindBigBone(PolygonBone[] bones) {

			int maxDisIndex = 0;
			float maxDis = 0f;
			float dis = 0f;
			for(int i = 0; i < bones.Length; ++i) {
				dis = bones[i].Distance();
				if(dis > maxDis) {
					maxDis = dis;
					maxDisIndex = i;
				}
			}
			return new BoneTree(null, bones[maxDisIndex], new List<BoneTree>());
		}

		/// <summary>
		/// ボーンツリーを作成する
		/// </summary>
		/// <param name="root">ルートのボーン</param>
		private static BoneTree MakeBoneTree(BoneTree root) {
			//準備
			Queue<BoneTree> explored = new Queue<BoneTree>();
			explored.Enqueue(root);

			BoneTree expBone;
			while(explored.Count > 0) {
				expBone = explored.Dequeue();
				//expBoneの子Boneを設定
				foreach(var b in expBone.bone.linkedBone) {
					if(expBone.parent != null && (b == expBone.parent.bone || expBone.parent.bone.linkedBone.Contains(b))) continue;
					var child = new BoneTree(expBone, b, new List<BoneTree>());
					expBone.children.Add(child);
					explored.Enqueue(child);
				}
			}

			return root;
		}

		#endregion

		#region Function

		/// <summary>
		/// 骨格ごとの重心の集まりを取得する
		/// </summary>
		/// <returns>骨格名と重心配列の辞書</returns>
		public Dictionary<string, Vector3[]> GetBoneCenters() {
			Dictionary<string, Vector3[]> centers = new Dictionary<string, Vector3[]>();
			foreach(var bone in _bones) {
				centers.Add(bone.id, bone.Centers());
			}
			return centers;
		}

		/// <summary>
		/// 骨格の設定
		/// </summary>
		/// <param name="root">骨格のルートオブジェクト</param>
		/// <param name="mat">描画用マテリアル</param>
		private Dictionary<string, Transform> SetFrame(Transform root, Material mat) {
			//準備
			//一時ボーンウェイト情報
			TempBoneData[] boneDatas = new TempBoneData[_figure.vertices.Length];
			//ボーンのオブジェクト
			Transform[] boneObjs = new Transform[_bones.Length];
			//ボーンオブジェクトの辞書
			Dictionary<string, Transform> boneDict = new Dictionary<string, Transform>();
			//探索キュー
			Queue<TempExpTree> explored = new Queue<TempExpTree>();

			//データ初期化
			for(int i = 0; i < boneDatas.Length; ++i) {
				boneDatas[i] = new TempBoneData();
			}
			explored.Enqueue(new TempExpTree(root, _rootBone));

			//処理
			int cnt = 0;
			while(explored.Count > 0) {
				var exp = explored.Dequeue();
				var boneObj = new GameObject(exp.tree.bone.id).transform;
				boneObj.SetParent(exp.parent, true);
				boneObj.position = exp.tree.bone.Center();
				boneObjs[cnt] = boneObj;
				boneDict.Add(boneObj.name, boneObj);

				//ボーンの含む頂点へのボーンの紐付けと重み付け
				foreach(var poly in exp.tree.bone.polygons) {
					for(int j = 0; j < poly.indices.Length; ++j) {
						boneDatas[poly.indices[j]].AddIndex(cnt);
					}
				}
				//子をキューに追加
				foreach(var c in exp.tree.children) {
					explored.Enqueue(new TempExpTree(boneObj, c));
				}
				cnt++;
			}

			//ボーンウェイトデータの変換
			BoneWeight[] bw = new BoneWeight[boneDatas.Length];
			for(int i = 0; i < bw.Length; ++i) {
				//boneDatas[i].DebugPrint();
				bw[i] = boneDatas[i].ToBoneWeight();
			}

			//レンダラとの関連付け
			var skin = root.GetComponent<SkinnedMeshRenderer>();
			if(!skin) skin = root.gameObject.AddComponent<SkinnedMeshRenderer>();
			var mesh = _figure.ToMesh();
			mesh.boneWeights = bw;
			skin.rootBone = root;
			skin.sharedMesh = mesh;
			skin.material = mat;
			skin.bones = boneObjs;
			var bps = new Matrix4x4[skin.bones.Length];
			for(int i = 0; i < skin.bones.Length; ++i) {
				bps[i] = skin.bones[i].worldToLocalMatrix * root.transform.localToWorldMatrix;
			}
			mesh.bindposes = bps;
			return boneDict;
		}

		/// <summary>
		/// 操作用コントローラへの変換
		/// </summary>
		/// <returns>作成したコントローラ</returns>
		public FigureFrameController ToController(GameObject figObj, Material mat) {
			var ffCon = figObj.GetComponent<FigureFrameController>();
			if(ffCon)
				throw new System.ArgumentException("FigureBoneController already attached");
			ffCon = figObj.AddComponent<FigureFrameController>();

			var boneDict = SetFrame(figObj.transform, mat);
			//端ボーンの外側の端に検出用のオブジェクトを埋め込む
			List<Transform> endBoneOutObjs = new List<Transform>();
			foreach(var b in _bones) {
				if(b.type == PolygonBone.Type.End) {
					foreach(var pos in b.GetEnds()) {
						var obj = new GameObject("Bounds Detector").transform;
						obj.SetParent(boneDict[b.id], true);
						obj.position = pos;
						endBoneOutObjs.Add(obj);
					}
				}
			}
			//パラメータの設定
			ffCon.InitParameter(this, boneDict, boneDict[_rootBone.bone.id], endBoneOutObjs);
			return ffCon;
		}

		/// <summary>
		/// ルートのボーンの識別IDを受け取る
		/// </summary>
		/// <returns>The root bone identifier.</returns>
		public string GetRootBoneID() {
			return _rootBone.bone.id;
		}

		#endregion
	}
}