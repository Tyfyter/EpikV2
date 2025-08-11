using System.IO;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace EpikV2.Projectiles {
	public class WhipScaleSupport : GlobalProjectile {
		public override bool InstancePerEntity => true;
		public float ScaleModifier { get; private set; } = 1f;
		public override bool AppliesToEntity(Projectile entity, bool lateInstantiation) => ProjectileID.Sets.IsAWhip[entity.type] && entity.ModProjectile?.Mod is EpikV2;
		public override void Load() {
			On_Projectile.GetWhipSettings += On_Projectile_GetWhipSettings;
		}
		public override void OnSpawn(Projectile projectile, IEntitySource source) {
			if (source is EntitySource_ItemUse itemUse) SetScaleModifier(projectile, itemUse.Player.GetAdjustedItemScale(itemUse.Item));
		}
		public override void SendExtraAI(Projectile projectile, BitWriter bitWriter, BinaryWriter binaryWriter) {
			binaryWriter.Write(ScaleModifier);
		}
		public override void ReceiveExtraAI(Projectile projectile, BitReader bitReader, BinaryReader binaryReader) {
			ScaleModifier = binaryReader.ReadSingle();
		}
		public void SetScaleModifier(Projectile projectile, float modifier) {
			projectile.scale = (projectile.scale / ScaleModifier) * modifier;
			ScaleModifier = modifier;
		}

		static void On_Projectile_GetWhipSettings(On_Projectile.orig_GetWhipSettings orig, Projectile proj, out float timeToFlyOut, out int segments, out float rangeMultiplier) {
			orig(proj, out timeToFlyOut, out segments, out rangeMultiplier);
			if (proj.TryGetGlobalProjectile(out WhipScaleSupport whip)) rangeMultiplier *= whip.ScaleModifier;
		}
	}
}
