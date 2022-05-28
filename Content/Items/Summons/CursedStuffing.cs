using Consolaria.Content.NPCs.Turkor;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;

namespace Consolaria.Content.Items.Summons
{
    public class CursedStuffing : ModItem
    {
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Cursed Stuffing");
            Tooltip.SetDefault("Summons Turkor the Ungrateful");
            CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 3;
            ItemID.Sets.SortingPriorityBossSpawns[Type] = 12;
        }

        public override void SetDefaults() {
            int width = 34; int height = 28;
            Item.Size = new Vector2(width, height);

            Item.maxStack = 20;

            Item.value = Item.sellPrice(0, 0, 1, 0);
            Item.rare = ItemRarityID.Blue;

            Item.useAnimation = 30;
            Item.useTime = 30;

            Item.useStyle = ItemUseStyleID.HoldUp;
            Item.consumable = true;
        }

        public override bool CanUseItem(Player player)
         => !NPC.AnyNPCs(ModContent.NPCType<TurkortheUngrateful>());
        
        public override bool? UseItem(Player player) {
            if (player.whoAmI == Main.myPlayer) {
                SoundEngine.PlaySound(SoundID.Roar, player.position);

                int type = ModContent.NPCType<TurkortheUngrateful>();
                if (Main.netMode != NetmodeID.MultiplayerClient)               
                    NPC.SpawnOnPlayer(player.whoAmI, type);              
                else                
                    NetMessage.SendData(MessageID.SpawnBoss, number: player.whoAmI, number2: type);                
            }
            return true;
        }
    }
}