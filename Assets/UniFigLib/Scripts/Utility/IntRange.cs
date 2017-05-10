using UnityEngine;
using System;
using System.Collections;

namespace UniFigLib.Utility {

	/// <summary>
	/// 二値の範囲
	/// </summary>
	public class IntRange : IEnumerable {

		private int _from;
		public int from { get { return _from; } }
		private int _to;
		public int to { get { return _to; } }
		public int count { get { return Mathf.Abs(_to - _from); } }

		public IntRange(int from, int to) {
			this._from = from;
			this._to = to;
		}

		/// <summary>
		/// 対応する列挙子の取得
		/// </summary>
		/// <returns>列挙子</returns>
		public IEnumerator GetEnumerator() {
			return new IntRangeEnumerator(_from, _to);
		}

		/// <summary>
		/// 開始値と終了値をずらしたIntRangeの取得
		/// </summary>
		/// <returns>オフセット分ずらしたIntRange</returns>
		/// <param name="start">開始値のオフセット</param>
		/// <param name="end">終了値のオフセット</param>
		public IntRange Offset(uint start, uint end) {
			return new IntRange((int)(_from + start), (int)(_to - end));
		}


		/// <summary>
		/// IntRangeの列挙子
		/// </summary>
		private class IntRangeEnumerator : IEnumerator {

			private int _from;      //開始値
			private int _to;        //終了値
			private int _add;       //加算値
			private int _current;   //現在値

			public IntRangeEnumerator(int from, int to) {
				_from = from;
				_to = to;
				_add = from < to ? 1 : -1;
				_current = 0;

				Reset();
			}

			public object Current {
				get {
					return _current;
				}
			}

			public bool MoveNext() {
				_current += _add;
				return !(_current == _to);
			}

			public void Reset() {
				_current = _from - _add;
			}
		}
	}
}