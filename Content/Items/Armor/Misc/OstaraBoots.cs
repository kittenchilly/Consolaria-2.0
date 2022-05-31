using Microsoft.Xna.Framework;
using Terraria;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;

namespace Consolaria.Content.Items.Armor.Misc
{
    [AutoloadEquip(EquipType.Legs)]
    public class OstaraBoots : ModItem
    {
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Boots of Ostara");
            Tooltip.SetDefault("5% increased movement speed" + "\nAllows the wearer to perform up to 5 bunny hops");

            CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
        }

        public override void SetDefaults() {
            int width = 22; int height = 18;
            Item.Size = new Vector2(width, height);

            Item.value = Item.sellPrice(gold: 1);
            Item.rare = ItemRarityID.Green;

            Item.defense = 4;
        }

        public override void UpdateEquip(Player player) { 
            player.moveSpeed += 0.05f;
            player.GetModPlayer<OstarasPlayer>().bunnyHop = true;
        }
    }
}
