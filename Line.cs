using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;

namespace EpikV2 {
	public struct Line {
		public readonly Vector2 a;
		public readonly Vector2 b;
		public Vector2 this[float pos] => Vector2.Lerp(a, b, pos);
		public Line(Vector2 a, Vector2 b) {
			this.a = a;
			this.b = b;
		}
		public bool Intersects(Rectangle rect) {
			return Collision.CheckAABBvLineCollision2(rect.TopLeft(), rect.Size(), a, b);
		}
		public (Vector2 min, Vector2 max) GetBounds() {
			float minX = (int)Math.Min(a.X, b.X);
			float minY = (int)Math.Min(a.Y, b.Y);
			float maxX = (int)Math.Max(a.X, b.X);
			float maxY = (int)Math.Max(a.Y, b.Y);
			return (new Vector2(minX, minY), new Vector2(maxX, maxY));
		}
	}
}
