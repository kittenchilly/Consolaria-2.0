using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;

namespace Consolaria.Content.Items.Weapons.Summon
{
	public class TurkeyStuff : ModItem
	{
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Turkey Staff");
			Tooltip.SetDefault("Summons a weird turkey to fight for you");
			ItemID.Sets.GamepadWholeScreenUseRange[Item.type] = true;
			ItemID.Sets.LockOnIgnoresCollision[Item.type] = true;
			CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
		}

		public override void SetDefaults() {
			int width = 34; int height = 38;
			Item.Size = new Vector2(width, height);

			Item.useStyle = ItemUseStyleID.Swing;
			Item.useTime = Item.useAnimation = 30;

			Item.mana = 15;
			Item.noMelee = true;

			Item.DamageType = DamageClass.Summon;
			Item.damage = 30;
			Item.knockBack = 4f;

			Item.value = Item.sellPrice(gold: 1, silver: 15);
			Item.rare = ItemRarityID.Orange;
			Item.UseSound = SoundID.Item44;

			Item.shoot = ModContent.ProjectileType<Projectiles.Friendly.TurkeyHead>();
			Item.buffType = ModContent.BuffType<Buffs.WeirdTurkey>();
		}

		public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
			=> position = new Vector2(player.Center.X, player.Center.Y - 30);		

		public override bool Shoot(Player player, ProjectileSource_Item_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
			player.AddBuff(Item.buffType, 2);
			var projectile = Projectile.NewProjectileDirect(source, position, velocity, type, damage, knockback, Main.myPlayer);
			projectile.originalDamage = Item.damage;
			return false;
		}
	}
}
