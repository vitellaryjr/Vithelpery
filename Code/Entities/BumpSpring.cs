using Microsoft.Xna.Framework;
using Monocle;
using Celeste.Mod.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MonoMod.Utils;

namespace Celeste.Mod.Vithelpery.Entities;

[CustomEntity("Vithelpery/BumpSpring")]
public class BumpSpring : Entity {
    public enum Directions {
        Up,
        Right,
        Left,
        Down,
    }
    public Directions dir;
    private Vector2 movement;

    public enum BounceTypes {
        Bumper,
        BumperCancel,
        Spring,
        SpringBoost,
    }
    private BounceTypes bounce;

    private float cooldown;
    private bool attach;

    public Sprite sprite;
    private VertexLight light;
    private BloomPoint bloom;
    private StaticMover staticMover;

    private float respawnTimer;

    public BumpSpring(EntityData data, Vector2 offset) : base(data.Position + offset) {
        dir = data.Enum("direction", Directions.Up);
        bounce = data.Enum("bounceType", BounceTypes.Bumper);
        cooldown = data.Float("cooldown", 0.6f);
        attach = data.Bool("attach", false);

        Depth = -8501;
        Add(new PlayerCollider(OnPlayer));
        Add(sprite = GFX.SpriteBank.Create("vithelp_bumpSpring"));
        Add(light = new VertexLight(Color.Teal, 1f, 16, 32));
        Add(bloom = new BloomPoint(0.5f, 16f));

        if (attach) {
            staticMover = new StaticMover();
            staticMover.OnAttach = delegate (Platform p) {
                Depth = p.Depth + 1;
            };
            switch (dir) {
                case Directions.Up:
                    staticMover.SolidChecker = (Solid s) => CollideCheck(s, Position + Vector2.UnitY);
                    staticMover.JumpThruChecker = (JumpThru jt) => CollideCheck(jt, Position + Vector2.UnitY);
                    break;
                case Directions.Left:
                    staticMover.SolidChecker = (Solid s) => CollideCheck(s, Position + Vector2.UnitX);
                    staticMover.JumpThruChecker = (JumpThru jt) => CollideCheck(jt, Position + Vector2.UnitX);
                    break;
                case Directions.Right:
                    staticMover.SolidChecker = (Solid s) => CollideCheck(s, Position - Vector2.UnitX);
                    staticMover.JumpThruChecker = (JumpThru jt) => CollideCheck(jt, Position - Vector2.UnitX);
                    break;
                case Directions.Down:
                    staticMover.SolidChecker = (Solid s) => CollideCheck(s, Position - Vector2.UnitY);
                    staticMover.JumpThruChecker = (JumpThru jt) => CollideCheck(jt, Position - Vector2.UnitY);
                    break;
            }
            Add(staticMover);
        }

        respawnTimer = 0f;

        if (dir == Directions.Up) {
            Collider = new Hitbox(16f, 6f, -8f, -6f);
        } else if (dir == Directions.Right) {
            Collider = new Hitbox(6f, 16f, 0f, -8f);
            sprite.Rotation = (float)Math.PI / 2f;
        } else if (dir == Directions.Left) {
            Collider = new Hitbox(6f, 16f, -6f, -8f);
            sprite.Rotation = (float)-Math.PI / 2f;
        } else {
            Collider = new Hitbox(16f, 6f, -8f, 0f);
            sprite.Rotation = (float)Math.PI;
        }

        if (dir == Directions.Up) {
            movement = -Vector2.UnitY;
        } else if (dir == Directions.Right) {
            movement = Vector2.UnitX;
        } else if (dir == Directions.Left) {
            movement = -Vector2.UnitX;
        } else if (dir == Directions.Down) {
            movement = Vector2.UnitY;
        }
    }

    public override void Update() {
        base.Update();
        if (respawnTimer > 0f) {
            respawnTimer = Calc.Approach(respawnTimer, 0f, Engine.DeltaTime);
            if (respawnTimer == 0f) {
                Respawn();
            }
        } else if (respawnTimer == 0f && Scene.OnInterval(0.05f)) {
            float direction = movement.Angle() + (Calc.Random.NextFloat((float)Math.PI) - ((float)Math.PI / 2f)); // 90 degrees from its facing in either direction
            (Scene as Level).Particles.Emit(Bumper.P_Ambience, 1, Center + Calc.AngleToVector(direction, 4), Vector2.One * 2f, direction);
        }
    }

    public void OnPlayer(Player player) {
        if (respawnTimer > 0f) return;

        Vector2 offset = Vector2.Zero;
        if (bounce == BounceTypes.Bumper || bounce == BounceTypes.BumperCancel) {
            float dashTimer = player.dashCooldownTimer;
            offset = player.ExplodeLaunch(player.Center - movement, false, false);
            if (bounce == BounceTypes.BumperCancel) player.dashCooldownTimer = dashTimer;
        } else if (bounce == BounceTypes.Spring || bounce == BounceTypes.SpringBoost) {
            if (dir == Directions.Up) {
                player.SuperBounce(Top);
            } else if (dir == Directions.Right) {
                player.SideBounce(1, Right, CenterY);
            } else if (dir == Directions.Left) {
                player.SideBounce(-1, Left, CenterY);
            } else if (dir == Directions.Down) { // taken from Brokemia Helper Dash Spring
                player.SuperBounce(Bottom + player.Height);
                player.varJumpSpeed = player.Speed.Y = 185f;
            }
            if (bounce == BounceTypes.SpringBoost) {
                if (Input.MoveX.Value == Math.Sign(player.Speed.X)) {
                    player.Speed.X *= 1.4f;
                } else { // check again in 1 frame (after freeze frames)
                    player.Add(Alarm.Create(Alarm.AlarmMode.Oneshot, () => {
                        if (Input.MoveX.Value == Math.Sign(player.Speed.X)) {
                            player.Speed.X *= 1.4f;
                        }
                    }, 0.01f, true));
                }
            }
            offset = player.Speed;

            Celeste.Freeze(0.1f);
        }
        respawnTimer = cooldown;
        BounceAnimate();

        (Scene as Level).DirectionalShake(offset, 0.15f);
        (Scene as Level).Displacement.AddBurst(Center, 0.3f, 8f, 32f, 0.8f, null, null);
        (Scene as Level).Particles.Emit(Bumper.P_Launch, 12, Center + offset * 12f, Vector2.One * 3f, offset.Angle());
    }

    private void BounceAnimate() {
        Audio.Play("event:/game/06_reflection/pinballbumper_hit", Position);
        sprite.Play("bounce");
        light.Visible = bloom.Visible = false;
        if (attach) {
            staticMover.TriggerPlatform();
        }
    }

    private void Respawn() {
        Audio.Play("event:/game/06_reflection/pinballbumper_reset", Position);
        sprite.Play("reform");
        light.Visible = bloom.Visible = true;
    }

    public static void Load() {
        On.Celeste.FloatySpaceBlock.OnStaticMoverTrigger += FloatySpaceBlock_OnStaticMoverTrigger;
    }

    public static void Unload() {
        On.Celeste.FloatySpaceBlock.OnStaticMoverTrigger -= FloatySpaceBlock_OnStaticMoverTrigger;
    }

    private static void FloatySpaceBlock_OnStaticMoverTrigger(On.Celeste.FloatySpaceBlock.orig_OnStaticMoverTrigger orig, FloatySpaceBlock self, StaticMover sm) {
        orig(self, sm);
        if (sm.Entity is BumpSpring) {
            switch ((sm.Entity as BumpSpring).dir) {
                case BumpSpring.Directions.Up:
                    self.sinkTimer = 0.5f;
                    return;
                case BumpSpring.Directions.Left:
                    self.dashEase = 1f;
                    self.dashDirection = Vector2.UnitX;
                    return;
                case BumpSpring.Directions.Right:
                    self.dashEase = 1f;
                    self.dashDirection = -Vector2.UnitX;
                    break;
                case BumpSpring.Directions.Down:
                    self.dashEase = 1f;
                    self.dashDirection = -Vector2.UnitY;
                    break;
                default:
                    return;
            }
        }
    }
}
