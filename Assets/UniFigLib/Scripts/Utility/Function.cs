using UnityEngine;
using System.Collections.Generic;
using UniFigLib.Utility;

namespace UniFigLib {

	/// <summary>
	/// 便利なもの
	/// </summary>
	public class Function {

		/// <summary>
		/// 曲線の近似処理を行った頂点群を返す
		/// </summary>
		/// <returns>近似された曲線</returns>
		/// <param name="src">入力曲線</param>
		/// <param name="approxVertNum">近似後の頂点数。元々の頂点数以上または2より小さい値が指定された場合は何もしない</param>
		public static List<Vector3> DouglasPeuckerApprox(List<Vector3> src, int approxVertNum) {
			List<int> indices = DouglasPeuckerApproxIndex(src, approxVertNum);
			if (indices == null) return src;

			List<Vector3> dst = new List<Vector3>();
			for(int i = 0; i < indices.Count; ++i) {
				dst.Add(src[indices[i]]);
			}
			return dst;
		}

		/// <summary>
		/// 曲線の近似処理を行い近似した頂点番号を返す
		/// </summary>
		/// <returns>近似された曲線頂点番号</returns>
		/// <param name="src">入力曲線</param>
		/// <param name="approxVertNum">近似後の頂点数。元々の頂点数以上または2より小さい値が指定された場合は何もしない</param>
		public static List<int> DouglasPeuckerApproxIndex(List<Vector3> src, int approxVertNum) {
			if(approxVertNum < 2 || src.Count <= approxVertNum) return null;

			//準備
			List<int> indices = new List<int>();
			List<IntRange> ranges = new List<IntRange>();
			Vector3[] dst = new Vector3[approxVertNum];

			indices.Add(0);
			indices.Add(src.Count - 1);
			ranges.Add(new IntRange(0, src.Count - 1));

			while(indices.Count < approxVertNum) {
				for(int i = ranges.Count - 1; i >= 0; --i) {
					IntRange range = ranges[i];
					ranges.RemoveAt(i);
					//range.fromからrange.toへの線分
					var lineSeg = new LineSeg(src[range.from], src[range.to]);
					//線分から最も離れている点を求める
					float max = 0f;
					int maxI = -1;
					float temp = 0f;
					foreach(int j in range.Offset(1, 0)) {
						temp = lineSeg.Distance(src[j]);
						if(temp >= max) {
							maxI = j;
							max = temp;
						}
					}
					//最も離れている点を確定させる
					indices.Add(maxI);
					if(indices.Count >= approxVertNum) break;
					//最も離れている点で分割する(距離が1以上離れていない場合は何もしない)
					if(maxI - range.from > 1) ranges.Add(new IntRange(range.from, maxI));
					if(range.to - maxI > 1) ranges.Add(new IntRange(maxI, range.to));
				}
			}
			indices.Sort();
			for(int i = 0; i < indices.Count; ++i) {
				dst[i] = src[indices[i]];
			}
			return indices;
		}
	}
}