using OrangeBugReloaded.Core.Tiles;
using System.Threading.Tasks;
using OrangeBugReloaded.Core;
using System.Collections;
using UnityEngine;

[ViewModel(typeof(TeleporterTile))]
public class TeleporterTileViewModel : TailoredViewModel<LocationViewModel, TeleporterTile>
{
    protected override void OnInitialize()
    {
        base.OnInitialize();
        Object.EntityDetaching += OnEntityDetachingAsync;
    }

    protected override void OnDispose()
    {
        Object.EntityDetaching -= OnEntityDetachingAsync;
        base.OnDispose();
    }

    private Task OnEntityDetachingAsync(Tile sender, EntityMoveContext e)
    {
        // If the teleporter has triggered a teleportation move, play sound and animate teleport
        if (e.Initiator is TeleporterTile)
        {
            Dispatcher.Run(() => StartCoroutine(AnimateTeleportation(e.CurrentMove.Target.Position)));
            return TaskEx.Delay(1200); // Because MoveToTeleportationTarget takes 1200 ms to complete
        }

        return OrangeBugGameObject.Done;
    }

    private IEnumerator AnimateTeleportation(Point targetPosition)
    {
        PlaySound("TeleporterTileTeleport");
        yield return new WaitForSeconds(.2f);

        var scaleCurve = new AnimationCurve(
            new Keyframe(0, 1),
            new Keyframe(.5f, 2),
            new Keyframe(1, 1));

        var entity = ViewModel.EntityVM.transform;

        var current = entity.localPosition;
        var target = new Vector3(targetPosition.X, targetPosition.Y, 0);

        var startTime = Time.time;
        var animationCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

        while (Time.time <= startTime + 1)
        {
            var t = (Time.time - startTime);
            entity.localPosition = Vector3.Lerp(current, target, animationCurve.Evaluate(t));
            entity.localScale = scaleCurve.Evaluate(t) * Vector3.one;
            yield return null;
        }

        entity.localPosition = target;
        entity.localScale = Vector3.one;
    }
}
