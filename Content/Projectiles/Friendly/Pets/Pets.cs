using Microsoft.Xna.Framework;
using System;
using System.Linq;

using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace Consolaria.Content.Projectiles.Friendly.Pets {
    public class WormData : ModPlayer {
        public bool IsWormPetActive { get; internal set; }

        public override void ResetEffects()
            => IsWormPetActive = false;
    }

    public class OldLadyData : ModPlayer {
        public bool IsOldLadyPetActive { get; internal set; }

        public override void ResetEffects()
            => IsOldLadyPetActive = false;
    }

    public class WormSoundManager {
        public enum WormSoundType {
            Boring,
            Bye,
            Grenade,
            Incoming,
            Jump,
            Victory
        }

        private const int IDLE_MAXTIME = 200;

        public const WormSoundType BORING = WormSoundType.Boring;
        public const WormSoundType BYE = WormSoundType.Bye;
        public const WormSoundType GRENADE = WormSoundType.Grenade;
        public const WormSoundType INCOMING = WormSoundType.Incoming;
        public const WormSoundType JUMP = WormSoundType.Jump;
        public const WormSoundType VICTORY = WormSoundType.Victory;

        private readonly Projectile _itself;

        private int _idleTimer, _cdBetweenQuotesTimer;
        private bool _eventStarted, _incomingSoundPlayed;

        public static WormSoundManager Instance { get; private set; }

        public Player Player
            => Main.player[_itself.owner];

        public int GetBoredTimer {
            get => _idleTimer;
            set {
                if (_itself.velocity.Length() <= 1f) {
                    _idleTimer = value;

                    const int maxTime = IDLE_MAXTIME;
                    if (_idleTimer >= maxTime) {
                        PlayWormSound(WormSoundType.Boring, () => { _idleTimer = 0; });
                    }
                }
                else {
                    _idleTimer = 0;
                }
            }
        }

        public int CDBetweenQuotes {
            get => _cdBetweenQuotesTimer;
            set {
                if (value > 0) {
                    _cdBetweenQuotesTimer = value;
                }
            }
        }

        public bool CanSpellNextQuote
            => CDBetweenQuotes <= 1;

        public WormSoundManager(Projectile itself) {
            Instance = this;

            _itself = itself;
        }

        public void Update() {
            UpdateValues();

            PlayJumpSound();
            PlayPlayersDeathSound();
            PlayGrenadeSound();
            PlayIncomingSound();
        }

        private void UpdateValues() {
            CDBetweenQuotes--;
            GetBoredTimer++;
        }

        internal void PlayWormSound(WormSoundType soundType, Action whenPlayed = null, bool noQueue = false) {
            if ((!CanSpellNextQuote && !noQueue) || Main.netMode == NetmodeID.Server) {
                return;
            }

            SoundStyle style = new($"{nameof(Consolaria)}/Assets/Sounds/Worm/{soundType}");
            SoundEngine.PlaySound(style, _itself.Center);

            whenPlayed?.Invoke();

            CDBetweenQuotes = 300;
        }

        private void PlayJumpSound() {
            bool isInFlightState = _itself.ai[0] != 0f,
                 jumped = _itself.velocity.Y < -1f;
            if (isInFlightState || !jumped) {
                return;
            }
            PlayWormSound(JUMP);
        }

        private void PlayPlayersDeathSound() {
            if (!Player.dead) {
                return;
            }
            PlayWormSound(BYE, noQueue: true);
        }

        private void PlayGrenadeSound() {
            Projectile explosivePlayerOwnedProjectile = Main.projectile.FirstOrDefault(projectile => projectile.type == Player.inventory[Player.selectedItem].shoot && projectile.owner == Player.whoAmI && projectile.aiStyle == 16);
            bool arentThereAnyPlayerOwnedExplosiveProjectiles = explosivePlayerOwnedProjectile == null || explosivePlayerOwnedProjectile == new Projectile() || !explosivePlayerOwnedProjectile.active;
            if (arentThereAnyPlayerOwnedExplosiveProjectiles) {
                return;
            }
            if (!Player.ItemAnimationJustStarted) {
                return;
            }
            PlayWormSound(GRENADE);
        }

        private void PlayIncomingSound() {
            bool isThereAnyInvasion = Main.invasionProgress != -1;
            if (!Main.bloodMoon && !Main.eclipse && !isThereAnyInvasion) {
                _eventStarted = false;
            }

            if (Main.bloodMoon || Main.eclipse || isThereAnyInvasion) {
                _eventStarted = true;
            }

            if (!_eventStarted) {
                _incomingSoundPlayed = false;
                return;
            }

            if (_incomingSoundPlayed) {
                return;
            }

            PlayWormSound(INCOMING, () => { _incomingSoundPlayed = true; }, true);
        }
    }

    public class Worm : ConsolariaPet2 {
        private static WormSoundManager _soundManager;

        public override void SetStaticDefaults() {
            Main.projFrames[Type] = 8;

            base.SetStaticDefaults();
        }

        public override void SetDefaults() {
            DrawOriginOffsetY = -5;

            int width = 36, height = 40;
            Projectile.Size = new Vector2(width, height);

            base.SetDefaults();

            _soundManager = new WormSoundManager(Projectile);
        }

        public override bool PreAI() {
            Player player = Main.player[Projectile.owner];
            WormData wormData = player.GetModPlayer<WormData>();
            if (player.dead) {
                wormData.IsWormPetActive = false;
            }
            if (wormData.IsWormPetActive) {
                Projectile.timeLeft = 2;
            }
            else {
                Projectile.Kill();
            }

            return true;
        }

        public override void PostAI()
            => _soundManager.Update();

        private class CheckKilledBossesSystem : GlobalNPC {
            public override void OnKill(NPC npc) {
                if (!npc.boss) {
                    return;
                }
                _soundManager.PlayWormSound(WormSoundManager.VICTORY, noQueue: true);
            }
        }
    }

    public class OldLady : ConsolariaPet2 {
        public override int animateIdleMax { get; set; } = 0;
        public override int animateIdle { get; set; } = 0;
        public override int animateWalkMin { get; set; } = 1;
        public override int animateWalkMax { get; set; } = 8;
        public override int animateFlyMin { get; set; } = 9;
        public override int animateFlyMax { get; set; } = 10;
        public override int animateFall { get; set; } = 9;

        public override void SetStaticDefaults() {
            Main.projFrames[Type] = 11;

            base.SetStaticDefaults();
        }

        public override void SetDefaults() {
            DrawOriginOffsetY = 3;

            int width = 20, height = 52;
            Projectile.Size = new Vector2(width, height);

            base.SetDefaults();
        }

        public override bool PreAI() {
            Player player = Main.player[Projectile.owner];
            OldLadyData oldLadyDta = player.GetModPlayer<OldLadyData>();
            if (player.dead) {
                oldLadyDta.IsOldLadyPetActive = false;
            }
            if (oldLadyDta.IsOldLadyPetActive) {
                Projectile.timeLeft = 2;
            }
            else {
                Projectile.Kill();
            }

            return true;
        }
    }

    public abstract class ConsolariaPet2 : ModProjectile {
        private int animateIdleTimer;

        public virtual int animateIdleMax { get; set; } = 4;
        public virtual int animateIdle { get; set; } = 4;
        public virtual int animateFall { get; set; } = 2;
        public virtual int animateWalkMin { get; set; } = 3;
        public virtual int animateWalkMax { get; set; } = 8;
        public virtual int animateFlyMin { get; set; } = 0;
        public virtual int animateFlyMax { get; set; } = 1;

        public virtual float speedX { get; set; } = 0.075f;

        public override void SetStaticDefaults() {
            Main.projPet[Type] = true;
        }

        public override void SetDefaults () {
            Projectile.aiStyle = -1;
            Projectile.manualDirectionChange = true;
        }

        public sealed override void AI () {
            Player player = Main.player[Projectile.owner];
            if (!player.active) {
                Projectile.active = false;
            }
            else
            {
                bool flag1 = false;
                bool flag2 = false;
                bool flag3 = false;
                bool flag4 = false;
                int num1 = 85;
                if ((double)player.position.X + (double)(player.width / 2) < (double)Projectile.position.X + (double)(Projectile.width / 2) - (double)num1)
                    flag1 = true;
                else if ((double)player.position.X + (double)(player.width / 2) > (double)Projectile.position.X + (double)(Projectile.width / 2) + (double)num1)
                    flag2 = true;
                if ((double)Projectile.ai[1] == 0.0)
                {
                    int num2 = 500;
                    if (player.rocketDelay2 > 0)
                        Projectile.ai[0] = 1f;
                    Vector2 vector2 = new Vector2(Projectile.position.X + (float)Projectile.width * 0.5f, Projectile.position.Y + (float)Projectile.height * 0.5f);
                    double num3 = (double)player.position.X + (double)(player.width / 2) - (double)vector2.X;
                    float num4 = player.position.Y + (float)(player.height / 2) - vector2.Y;
                    float num5 = (float)Math.Sqrt(num3 * num3 + (double)num4 * (double)num4);
                    if ((double)num5 > 2000.0)
                    {
                        Projectile.position.X = player.position.X + (float)(player.width / 2) - (float)(Projectile.width / 2);
                        Projectile.position.Y = player.position.Y + (float)(player.height / 2) - (float)(Projectile.height / 2);
                    }
                    else if ((double)num5 > (double)num2 || (double)Math.Abs(num4) > 300.0 && (double)Projectile.localAI[0] <= 0.0)
                        Projectile.ai[0] = 1f;
                }
                if ((double)Projectile.ai[0] != 0.0)
                {
                    float num6 = 0.2f;
                    int num7 = 200;
                    Projectile.tileCollide = false;
                    Vector2 vector2 = new Vector2(Projectile.position.X + (float)Projectile.width * 0.5f, Projectile.position.Y + (float)Projectile.height * 0.5f);
                    float num8 = player.position.X + (float)(player.width / 2) - vector2.X;
                    float num9 = player.position.Y + (float)(player.height / 2) - vector2.Y;
                    float num10 = (float)Math.Sqrt((double)num8 * (double)num8 + (double)num9 * (double)num9);
                    float num11 = 10f;
                    if ((double)num10 < (double)num7 && (double)player.velocity.Y == 0.0 && (double)Projectile.position.Y + (double)Projectile.height <= (double)player.position.Y + (double)player.height && !Collision.SolidCollision(Projectile.position, Projectile.width, Projectile.height))
                    {
                        Projectile.ai[0] = 0.0f;
                        if ((double)Projectile.velocity.Y < -6.0)
                            Projectile.velocity.Y = -6f;
                    }
                    float num12;
                    float num13;
                    if ((double)num10 < 60.0)
                    {
                        num12 = Projectile.velocity.X;
                        num13 = Projectile.velocity.Y;
                    }
                    else
                    {
                        float num14 = num11 / num10;
                        num12 = num8 * num14;
                        num13 = num9 * num14;
                    }
                    if ((double)Projectile.velocity.X < (double)num12)
                    {
                        Projectile.velocity.X += num6;
                        if ((double)Projectile.velocity.X < 0.0)
                            Projectile.velocity.X += num6 * 1.5f;
                    }
                    if ((double)Projectile.velocity.X > (double)num12)
                    {
                        Projectile.velocity.X -= num6;
                        if ((double)Projectile.velocity.X > 0.0)
                            Projectile.velocity.X -= num6 * 1.5f;
                    }
                    if ((double)Projectile.velocity.Y < (double)num13)
                    {
                        Projectile.velocity.Y += num6;
                        if ((double)Projectile.velocity.Y < 0.0)
                            Projectile.velocity.Y += num6 * 1.5f;
                    }
                    if ((double)Projectile.velocity.Y > (double)num13)
                    {
                        Projectile.velocity.Y -= num6;
                        if ((double)Projectile.velocity.Y > 0.0)
                            Projectile.velocity.Y -= num6 * 1.5f;
                    }
                    ++Projectile.frameCounter;
                    if (Projectile.frameCounter > 4)
                    {
                        ++Projectile.frame;
                        Projectile.frameCounter = 0;
                    }
                    if (Projectile.frame < this.animateFlyMin || Projectile.frame > this.animateFlyMax)
                        Projectile.frame = this.animateFlyMin;
                    Projectile.rotation = 0.0f;
                    if (Math.Abs(Projectile.velocity.X) > 0.25f)
                    {
                        Projectile.direction = ((double)Projectile.velocity.X > 0.0).ToDirectionInt();
                        if (Projectile.direction == -1)
                            Projectile.spriteDirection = 1;
                        if (Projectile.direction != 1)
                            return;
                        Projectile.spriteDirection = -1;
                    }
                }
                else
                {
                    Vector2 zero = Vector2.Zero;
                    if ((double)Projectile.ai[1] != 0.0)
                    {
                        flag1 = false;
                        flag2 = false;
                    }
                    if (true)
                        Projectile.rotation = 0.0f;
                    float num15 = speedX;
                    float num16 = 75f;
                    Projectile.tileCollide = true;
                    if (flag1)
                    {
                        if ((double)Projectile.velocity.X > -3.5)
                            Projectile.velocity.X -= num15;
                        else
                            Projectile.velocity.X -= num15 * 0.25f;
                    }
                    else if (flag2)
                    {
                        if ((double)Projectile.velocity.X < 3.5)
                            Projectile.velocity.X += num15;
                        else
                            Projectile.velocity.X += num15 * 0.25f;
                    }
                    else
                    {
                        Projectile.velocity.X *= 0.9f;
                        if ((double)Projectile.velocity.X >= -(double)num15 && (double)Projectile.velocity.X <= (double)num15)
                            Projectile.velocity.X = 0.0f;
                    }
                    if (flag1 | flag2)
                    {
                        int num17 = (int)((double)Projectile.position.X + (double)(Projectile.width / 2)) / 16;
                        int j = (int)((double)Projectile.position.Y + (double)(Projectile.height / 2)) / 16;
                        if (flag1)
                            --num17;
                        if (flag2)
                            ++num17;
                        if (WorldGen.SolidTile(num17 + (int)Projectile.velocity.X, j))
                            flag4 = true;
                    }
                    if ((double)player.position.Y + (double)player.height - 8.0 > (double)Projectile.position.Y + (double)Projectile.height)
                        flag3 = true;
                    Collision.StepUp(ref Projectile.position, ref Projectile.velocity, Projectile.width, Projectile.height, ref Projectile.stepSpeed, ref Projectile.gfxOffY);
                    if ((double)Projectile.velocity.Y == 0.0)
                    {
                        if (!flag3 && ((double)Projectile.velocity.X < 0.0 || (double)Projectile.velocity.X > 0.0))
                        {
                            int i = (int)((double)Projectile.position.X + (double)(Projectile.width / 2)) / 16;
                            int j = (int)((double)Projectile.position.Y + (double)(Projectile.height / 2)) / 16 + 1;
                            if (flag1)
                                --i;
                            if (flag2)
                                ++i;
                            WorldGen.SolidTile(i, j);
                        }
                        if (flag4)
                        {
                            int num18 = (int)((double)Projectile.position.X + (double)(Projectile.width / 2)) / 16;
                            int num19 = (int)((double)Projectile.position.Y + (double)Projectile.height) / 16 + 1;
                            if (WorldGen.SolidTile(num18, num19) || Main.tile[num18, num19].IsHalfBlock || Main.tile[num18, num19].Slope > SlopeType.Solid)
                            {
                                try
                                {
                                    int num20 = (int)((double)Projectile.position.X + (double)(Projectile.width / 2)) / 16;
                                    int num21 = (int)((double)Projectile.position.Y + (double)(Projectile.height / 2)) / 16;
                                    if (flag1)
                                        --num20;
                                    if (flag2)
                                        ++num20;
                                    int i = num20 + (int)Projectile.velocity.X;
                                    if (!WorldGen.SolidTile(i, num21 - 1) && !WorldGen.SolidTile(i, num21 - 2))
                                        Projectile.velocity.Y = -5.1f;
                                    else if (!WorldGen.SolidTile(i, num21 - 2))
                                        Projectile.velocity.Y = -7.1f;
                                    else if (WorldGen.SolidTile(i, num21 - 5))
                                        Projectile.velocity.Y = -11.1f;
                                    else if (WorldGen.SolidTile(i, num21 - 4))
                                        Projectile.velocity.Y = -10.1f;
                                    else
                                        Projectile.velocity.Y = -9.1f;
                                }
                                catch
                                {
                                    Projectile.velocity.Y = -9.1f;
                                }
                            }
                        }
                    }
                    if ((double)Projectile.velocity.X > (double)num16)
                        Projectile.velocity.X = num16;
                    if ((double)Projectile.velocity.X < -(double)num16)
                        Projectile.velocity.X = -num16;
                    if ((double)Projectile.velocity.X != 0.0)
                        Projectile.direction = ((double)Projectile.velocity.X > 0.0).ToDirectionInt();
                    if ((double)Projectile.velocity.X > (double)num15 & flag2)
                        Projectile.direction = 1;
                    if ((double)Projectile.velocity.X < -(double)num15 & flag1)
                        Projectile.direction = -1;
                    if (Projectile.direction == -1)
                        Projectile.spriteDirection = 1;
                    if (Projectile.direction == 1)
                        Projectile.spriteDirection = -1;
                    if ((double)Projectile.velocity.Y == 0.0)
                    {
                        if ((double)Projectile.velocity.X == 0.0)
                        {
                            if (Projectile.frame < this.animateIdle)
                            {
                                Projectile.frameCounter += 2;
                                if (Projectile.frameCounter > 6)
                                {
                                    ++Projectile.frame;
                                    Projectile.frameCounter = 0;
                                }
                                if (Projectile.frame >= this.animateWalkMax)
                                    Projectile.frame = this.animateIdle;
                            }
                            else
                            {
                                ++this.animateIdleTimer;
                                if (this.animateIdleTimer < 300)
                                {
                                    Projectile.frame = this.animateIdle;
                                    Projectile.frameCounter = 0;
                                }
                                else
                                {
                                    ++Projectile.frameCounter;
                                    if (Projectile.frameCounter > 12)
                                    {
                                        ++Projectile.frame;
                                        if (Projectile.frame > this.animateIdleMax)
                                        {
                                            Projectile.frame = this.animateIdle + 1;
                                            if (Main.rand.NextBool(8))
                                                this.animateIdleTimer = 0;
                                        }
                                        Projectile.frameCounter = 0;
                                    }
                                }
                            }
                        }
                        else if ((double)Projectile.velocity.X < -0.8 || (double)Projectile.velocity.X > 0.8)
                        {
                            this.animateIdleTimer = 0;
                            Projectile.frameCounter += (int)Math.Abs((double)Projectile.velocity.X * 0.3f);
                            ++Projectile.frameCounter;
                            if (Projectile.frameCounter > 6)
                            {
                                ++Projectile.frame;
                                Projectile.frameCounter = 0;
                            }
                            if (Projectile.frame >= this.animateWalkMax || Projectile.frame < this.animateWalkMin)
                                Projectile.frame = this.animateWalkMin;
                        }
                        else if (Projectile.frame > 0)
                        {
                            Projectile.frame = this.animateIdle;
                            Projectile.frameCounter = 0;
                        }
                        else
                        {
                            Projectile.frame = this.animateIdle;
                            Projectile.frameCounter = 0;
                        }
                    }
                    else if ((double)Projectile.velocity.Y < 0.0 || (double)Projectile.velocity.Y > 0.0)
                    {
                        Projectile.frameCounter = 0;
                        Projectile.frame = this.animateFall;
                        this.animateIdleTimer = 0;
                    }
                    Projectile.velocity.Y += 0.4f;
                    if ((double)Projectile.velocity.Y <= 10.0)
                        return;
                    Projectile.velocity.Y = 10f;
                }
            }
        }
    }
}