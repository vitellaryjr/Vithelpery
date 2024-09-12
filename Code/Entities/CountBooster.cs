using Microsoft.Xna.Framework;
using Monocle;
using Celeste.Mod.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MonoMod.Utils;
using static Celeste.GaussianBlur;

namespace Celeste.Mod.Vithelpery.Entities;

[CustomEntity("Vithelpery/CountBooster")]
public class CountBooster : Booster {
    public int count;
    public int max;
    public bool kill;

    public Sprite overlay;
    public float target;
    public float apparent;
    private ParticleType appearType;

    public CountBooster(EntityData data, Vector2 offset) : base(data.Position + offset, false) {
        Remove(sprite);
        Add(sprite = GFX.SpriteBank.Create("vithelp_countBooster"));
        particleType = new ParticleType {
            Source = GFX.Game["particles/blob"],
            Color = Calc.HexToColor("2c956e"),
            FadeMode = ParticleType.FadeModes.None,
            LifeMin = 0.5f,
            LifeMax = 0.8f,
            Size = 0.7f,
            SizeRange = 0.25f,
            ScaleOut = true,
            Direction = 4.712389f,
            DirectionRange = 0.17453292f,
            SpeedMin = 10f,
            SpeedMax = 20f,
            SpeedMultiplier = 0.01f,
            Acceleration = new Vector2(0f, 90f)
        };
        appearType = new ParticleType {
            Size = 1f,
            Color = Calc.HexToColor("4ACFC6"),
            DirectionRange = 0.10471976f,
            LifeMin = 0.6f,
            LifeMax = 1f,
            SpeedMin = 40f,
            SpeedMax = 50f,
            SpeedMultiplier = 0.25f,
            FadeMode = ParticleType.FadeModes.Late
        };

        max = data.Int("count", 1);
        count = max;

        kill = data.Bool("deadly", true);

        Add(overlay = new Sprite(GFX.Game, "objects/Vithelpery/countdownBooster/"));
        overlay.Add("main", "countdown");
        overlay.Play("main");
        overlay.CenterOrigin();

        target = 0f;
        apparent = 0f;

        UpdateSprite();
    }

    public static void Load() {
        On.Celeste.Booster.PlayerBoosted += Booster_PlayerBoosted;
        On.Celeste.Booster.Respawn += Booster_Respawn;
        On.Celeste.Booster.AppearParticles += Booster_AppearParticles;
    }

    public static void Unload() {
        On.Celeste.Booster.PlayerBoosted -= Booster_PlayerBoosted;
        On.Celeste.Booster.Respawn -= Booster_Respawn;
        On.Celeste.Booster.AppearParticles -= Booster_AppearParticles;
    }

    private static void Booster_PlayerBoosted(On.Celeste.Booster.orig_PlayerBoosted orig, Booster self, Player player, Vector2 direction) {
        if (!(self is CountBooster)) {
            orig(self, player, direction);
        } else {
            CountBooster boost = self as CountBooster;
            if (boost.count > 0) {
                boost.count--;
                Audio.Play("event:/game/04_cliffside/greenbooster_dash", boost.Position);
                boost.BoostingPlayer = true;
                boost.Tag = (Tags.Persistent | Tags.TransitionUpdate);
                boost.sprite.Play("spin", false, false);
                boost.sprite.FlipX = (player.Facing == Facings.Left);
                if (boost.count > 0 || boost.kill) {
                    boost.outline.Visible = true;
                } else {
                    boost.outline.Remove();
                }
                boost.wiggler.Start();
                boost.dashRoutine.Replace(boost.BoostRoutine(player, direction));
                boost.overlay.Visible = false;
                boost.UpdatePercent();
            } else {
                if (boost.kill) {
                    player.Die(-direction);
                }
            }
        }
    }

    private static void Booster_Respawn(On.Celeste.Booster.orig_Respawn orig, Booster self) {
        if (!(self is CountBooster)) {
            orig(self);
        } else {
            CountBooster boost = self as CountBooster;
            if (!boost.kill && boost.count == 0) {
                boost.Remove();
            } else {
                boost.overlay.Visible = true;
                boost.UpdateSprite();
                orig(self);
            }
        }
    }

    private static void Booster_AppearParticles(On.Celeste.Booster.orig_AppearParticles orig, Booster self) {
        if (!(self is CountBooster)) {
            orig(self);
        } else {
            CountBooster boost = self as CountBooster;
            ParticleType origType = Booster.P_Appear;
            Booster.P_Appear = boost.appearType;
            orig(self);
            Booster.P_Appear = origType;
        }
    }

    public override void Render() {
        base.Render();
        if (overlay.Visible) {
            Vector2 origin = sprite.RenderPosition;
            int used = max - count;
            for (int i = used; i < max; i++) {
                if (used > 0 && i == used) {
                    i++;
                    if (i == max) break;
                }
                float angle = ((float)i / max + 0.25f) * (float)(-Math.PI * 2f);
                float cos = (float)Math.Cos(angle);
                float sin = (float)Math.Sin(angle);
                Draw.Line(origin, origin + new Vector2(cos * 8f, sin * 7f), Color.Black);
            }
        }
    }

    public override void Update() {
        base.Update();
        overlay.Position = sprite.Position;
        overlay.Scale = sprite.Scale;
        if (target != apparent) {
            apparent = Calc.Approach(apparent, target, Engine.DeltaTime  * 4f);
            UpdateSprite();
        }
    }

    public void UpdatePercent() {
        target = 1f - (count / (float)max);
        if (count == 0)
            target = 1f;
    }

    public void UpdateSprite() {
        if (apparent < 0.5f) {
            sprite.Color = Color.Lerp(Calc.HexToColor("8cf7cf"), Calc.HexToColor("ffee83"), apparent * 2f);
        } else {
            sprite.Color = Color.Lerp(Calc.HexToColor("ffee83"), Calc.HexToColor("ff2222"), (apparent - 0.5f) * 2f);
        }
        particleType.Color = sprite.Color;
        appearType.Color = sprite.Color;
        overlay.SetAnimationFrame((int)Math.Round(48f * apparent));
    }
}
