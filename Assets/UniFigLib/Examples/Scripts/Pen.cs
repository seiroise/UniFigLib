using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace UniFigLib.Examples {

	/// <summary>
	/// クリックしている間のマウス座標に線を作成
	/// </summary>
	public class Pen : MonoBehaviour {

		[Header("Contour")]
		public Camera cam;
		public LineRenderer contourRenderer;
		public float minAddDist = 0.5f;

		[Header("Approx")]
		[Range(0.01f, 1f)]
		public float approxPer = 0.5f;
		public LineRenderer approxRenderer;

		[Header("Figure")]
		public Material figMat;

		[Header("Bone")]
		public Material boneMat;

		//LineDraw
		private List<Vector3> _positions;
		private Vector3 _prevPosition;

		//Figure
		private Dictionary<Figure, Transform> _figures;

		private void Awake() {
			_positions = new List<Vector3>();
			_figures = new Dictionary<Figure, Transform>();
		}

		private void Update() {
			if(Input.GetMouseButton(0)) {
				Draw();
			}
			if(Input.GetMouseButtonUp(0)) {
				StopDrawing(_positions);
			} else if(Input.GetMouseButtonDown(0)) {
				StartDrawing();
			} else if(Input.GetMouseButtonDown(1)) {
				EraseFigures();
				EraseLine();
			}
		}

		/// <summary>
		/// お絵描きはじめ
		/// </summary>
		private void StartDrawing() {
			_prevPosition = ToLinePosition();
			EraseLine();
		}

		/// <summary>
		/// お絵描きおわり
		/// </summary>
		private void StopDrawing(List<Vector3> positions) {
			if(positions.Count < 3) return;
			//曲線の近似
			var approx = Function.DouglasPeuckerApprox(positions, (int)(positions.Count * approxPer));
			approxRenderer.SetVertexCount(approx.Count);
			approxRenderer.SetPositions(approx.ToArray());
			//図形の作成
			var figure = Figure.FromPositions(approx, new Color(Random.Range(0.3f, 1f), Random.Range(0.3f, 1f), Random.Range(0.3f, 1f), Random.Range(0.5f, 1f)));
			var figObj = new GameObject("figure").transform;
			figObj.transform.SetParent(transform);
			figObj.transform.localPosition = Vector3.forward;
			_figures.Add(figure, figObj);
			var filter = figObj.gameObject.AddComponent<MeshFilter>();
			var renderer = figObj.gameObject.AddComponent<MeshRenderer>();
			renderer.material = figMat;
			StartCoroutine(figure.MeshAnimation(1f, filter, OnMeshAnimationEnd));
		}

		/// <summary>
		/// お絵描き
		/// </summary>
		private void Draw() {
			Vector3 pos = ToLinePosition();
			if(Vector3.Distance(pos, _prevPosition) > minAddDist) {
				AddLineVertex(pos);
				_prevPosition = pos;
			}
		}

		/// <summary>
		/// 線の頂点を追加
		/// </summary>
		private void AddLineVertex(Vector3 pos) {
			_positions.Add(pos);
			contourRenderer.SetVertexCount(_positions.Count);
			contourRenderer.SetPositions(_positions.ToArray());
		}

		/// <summary>
		/// 線を消す
		/// </summary>
		private void EraseLine() {
			_positions.Clear();
			contourRenderer.SetVertexCount(0);
			contourRenderer.SetPositions(new Vector3[] { });
			approxRenderer.SetVertexCount(0);
			approxRenderer.SetPositions(new Vector3[] { });
		}

		/// <summary>
		/// 図を消す
		/// </summary>
		private void EraseFigures() {
			_figures.Clear();
			foreach(Transform t in transform) {
				Destroy(t.gameObject);
			}
		}

		/// <summary>
		/// マウスの座標を何かしらの座標に変換
		/// </summary>
		private Vector3 ToLinePosition() {
			Vector3 pos = Input.mousePosition;
			return cam.ScreenToWorldPoint(pos) + (cam.transform.rotation * (Vector3.forward * 10f));
		}

		/// <summary>
		/// メッシュアニメーションの終了時イベント
		/// </summary>
		private void OnMeshAnimationEnd(Figure figure) {
			if(!_figures.ContainsKey(figure)) return;
			var figBone = FigureFrame.FromFigure(figure);

			//ボーンの描画
			var figObj = _figures[figure].gameObject;
			foreach(var centers in figBone.GetBoneCenters()) {
				var lineObj = new GameObject("Line: " + centers.Key);
				lineObj.transform.SetParent(figObj.transform);
				var line = lineObj.AddComponent<LineRenderer>();
				var approx = Function.DouglasPeuckerApprox(new List<Vector3>(centers.Value), 2);
				line.SetVertexCount(approx.Count);
				line.SetPositions(approx.ToArray());
				line.material = boneMat;
				var color = new Color(Random.Range(0f, 0.6f), Random.Range(0f, 0.6f), Random.Range(0f, 0.6f), 0.7f);
				line.SetColors(color, color);
				line.SetWidth(0.2f, 0.2f);
			}
		}
	}
}