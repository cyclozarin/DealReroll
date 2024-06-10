using MonoMod.Cil;
using Mono.Cecil.Cil;
using System;
using Photon.Pun;

namespace DealReroll.Patches
{
    public class SurfaceNetworkHandlerPatch
    {
        internal static void Init()
        {
            IL.SurfaceNetworkHandler.NewWeek += MMILHook_NewWeekPatch;
        }

        private static void MMILHook_NewWeekPatch(ILContext il)
        {
            ILCursor _cursor = new(il);
            _cursor.GotoNext(
                MoveType.After,
                x => x.MatchCall(typeof(PhotonNetwork), "get_" + nameof(PhotonNetwork.IsMasterClient)),
                x => x.MatchBrfalse(out _)
            ); // match checking if client who calling that is master and returning if no
            _cursor.Index -= 2; // position us before if (currentRun == 1) instruction
            _cursor.RemoveRange(3); // remove it
            _cursor.Emit(OpCodes.Ldarg_0); // load our condition on stack
            _cursor.EmitDelegate<Func<SurfaceNetworkHandler, bool>>((self) =>
            {
                Plugin.Logger.LogDebug($"active deal null: {NetworkDealBoss.activeDeal == null} current run bigger than/equal to 1: {SurfaceNetworkHandler.RoomStats.CurrentRun >= 1}");
                if (NetworkDealBoss.activeDeal == null && SurfaceNetworkHandler.RoomStats.CurrentRun >= 1)
                {
                    Plugin.Logger.LogDebug("NEW DEALS GO GO GO");
                    return true;
                }
                return false;
            });
        }
    }
}
