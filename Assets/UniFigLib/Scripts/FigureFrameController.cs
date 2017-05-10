using UnityEngine;
using System.Linq;
using System.Collections.Generic;

namespace UniFigLib {

	/// <summary>
	/// 図形骨格の操作盤
	/// </summary>
	public class FigureFrameController : MonoBehaviour {

		private FigureFrame _figureFrame;
		private Dictionary<string, Transform> _boneDict;
		private Transform[] _bones;
		private Transform _rootBone;
		private Transform[] _boundDitectors;

		#region Function

		/// <summary>
		/// 初期パラメータの設定
		/// </summary>
		/// <param name="figureFrame">図形骨格データ</param>
		/// <param name="boneDict">骨格辞書</param>
		/// <param name="boundDictectors">領域検出オブジェクト</param>
		public void InitParameter(FigureFrame figureFrame, Dictionary<string, Transform> boneDict, Transform rootBone, List<Transform> boundDictectors) {
			_figureFrame = figureFrame;
			_boneDict = boneDict;
			_bones = boneDict.Values.ToArray();
			_rootBone = rootBone;
			_boundDitectors = boundDictectors.ToArray();
		}

		/// <summary>
		/// 指定したboneIdのボーンをangleだけ回転させる
		/// </summary>
		/// <param name="boneId">ボーンID</param>
		/// <param name="angle">角度</param>
		public void RotateBone(string boneId, float angle) {
			var bone = _boneDict[boneId];
			bone.Rotate(Vector3.forward, angle);
		}

		/// <summary>
		/// すべてのボーンをanglesだけ回転させる
		/// </summary>
		/// <param name="angles">ボーンに対応した要素を持つ配列</param>
		public void RotateBones(float[] angles) {
			var axis = Vector3.forward;
			for(var i = 0; i < angles.Length; ++i) {
				_bones[i].Rotate(axis, angles[i]);
			}
		}

		/// <summary>
		/// この骨格を構成しているボーンの数を返す
		/// </summary>
		/// <returns>ボーン数</returns>
		public int GetBoneCount() {
			return _bones.Length;
		}

		/// <summary>
		/// このオブジェクトの重心を返す
		/// </summary>
		/// <returns>重心</returns>
		public Vector3 GetCenter() {
			Vector3 sum = Vector3.zero;
			foreach(var d in _boundDitectors) {
				sum += d.transform.position;
			}
			return sum / _boundDitectors.Length;
		}

		/// <summary>
		/// ボーンとの位置関係を保ったまま全体を指定した方向に動かす
		/// </summary>
		public void MoveBones(Vector3 movement) {
			transform.position += movement;
			_rootBone.position -= movement;
		}

		/// <summary>
		/// オブジェクトの重心とオブジェクトの座標のズレを調整する
		/// </summary>
		public void AdjustRootPosition() {
			var pos = transform.position;
			var center = GetCenter();
			var offset = center - pos;

			MoveBones(offset);
		}

		#endregion
	}
}
