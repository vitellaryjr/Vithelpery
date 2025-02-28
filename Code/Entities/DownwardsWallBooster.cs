using Microsoft.Xna.Framework;
using Monocle;
using Celeste.Mod.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MonoMod.Utils;
using MonoMod.Cil;
using Mono.Cecil.Cil;

namespace Celeste.Mod.Vithelpery.Entities;

[CustomEntity("Vithelpery/DownwardsWallBooster")]
[TrackedAs(typeof(WallBooster))]
public class DownwardsWallBooster : WallBooster {
    public float speed;
    public DownwardsWallBooster(EntityData data, Vector2 offset) : base(data, offset) {
        speed = data.Float("speed");
        foreach (Sprite tile in tiles) {
            tile.FlipY = true;
        }
        float topY = tiles[0].Y;
        tiles[0].Y = tiles[tiles.Count - 1].Y;
        tiles[tiles.Count - 1].Y = topY;
    }

    public static void Load() {
        IL.Celeste.Player.ClimbUpdate += Player_ClimbUpdate;
    }

    public static void Unload() {
        IL.Celeste.Player.ClimbUpdate -= Player_ClimbUpdate;
    }

    private static void Player_ClimbUpdate(MonoMod.Cil.ILContext il) {
        ILCursor cursor = new ILCursor(il);

        int boosterIndex = 0;
        if (!cursor.TryGotoNext(MoveType.After,
            instr => instr.MatchCallOrCallvirt<Player>("WallBoosterCheck"),
            instr => instr.MatchStloc(out boosterIndex))) {
            Logger.Error("Vithelpery", "Couldn't MatchCallvirt for Player.WallBoosterCheck in Player.ClimbUpdate");
            return;
        }

        if (!cursor.TryGotoNext(MoveType.After,
            instr => instr.MatchLdcR4(-160f))) {
            Logger.Error("Vithelpery", "Couldn't MatchLdcR4 for 160f in Player.ClimbUpdate");
            return;
        }

        cursor.Emit(OpCodes.Ldloc, boosterIndex);
        cursor.EmitDelegate(ApplyDownwardsSpeed);
    }

    public static float ApplyDownwardsSpeed(float origSpeed, WallBooster booster) {
        if (booster is DownwardsWallBooster) {
            return (booster as DownwardsWallBooster).speed;
        }
        return origSpeed;
    }
}
