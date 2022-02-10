using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace Consolaria.Content.Buffs
{
	public class Zombie : ModBuff
	{
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Pet Zombie");
			Description.SetDefault("Eats brains");
			Main.buffNoTimeDisplay[Type] = true;
			Main.vanityPet[Type] = true;
		}

		public override void Update(Player player, ref int buffIndex){ 
			player.buffTime[buffIndex] = 18000;
			sbyte type = (sbyte)ModContent.ProjectileType<Projectiles.Friendly.Pets.Zombie>();
			if (player.whoAmI == Main.myPlayer && player.ownedProjectileCounts[type] <= 0)
				Projectile.NewProjectile(player.GetProjectileSource_Buff(buffIndex), player.Center, Vector2.Zero, type, 0, 0f, player.whoAmI);
		}
	}
}